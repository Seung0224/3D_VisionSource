using OpenCvSharp;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using HT = HelixToolkit.Wpf.SharpDX;
using MediaColor = System.Windows.Media.Color;

namespace _3D_VisionSource
{
    // 이 Parameter를 비롯한 Calibration 값들을 정확히 넣어야 정확한 값 산출 가능
    public class InspectionParams
    {
        public float Sx = 0.05f;
        public float Sy = 0.05f;
        public float ZScale = 0.001f;
        public float ZOffset = 0f;
        public byte InvalidZ = 0;
        public ushort InvalidZ16 = 0;
        public bool CenterOrigin = true;
        public double MinAreaMm2 = 0.001;
        public double OverlayAlpha = 0.35;
    }

    public class InspectionResults
    {
        public Point3D[] Points;
        public MediaColor[] Colors;
        public int Width;
        public int Height;

        public Mat HoleMask;
        public List<OpenCvSharp.Point[]> ContoursPx;

        public List<int> CompLabels;
        public List<int> CompAreaPx;
        public List<double> CompAreaMm2;
        public List<OpenCvSharp.Rect> CompBBox;
        public List<OpenCvSharp.Point> CompCentroidPx;

        public Bitmap Overlay2D;
    }

    public static class FusionEngine
    {
        static readonly InspectionParams P = new InspectionParams();

        public enum ArgbByteIndex : int { B = 0, G = 1, R = 2, A = 3 }

        // 16비트 값의 '하위 8비트'에 쓸 채널(기본값: B 채널)
        // 16비트 값의 '상위 8비트'에 쓸 채널(기본값: G 채널)
        // 바이트 결합 순서: false=리틀엔디안, true=빅엔디안
        // 이미지 저장 형식의따라서 B와 G값 이 바뀔수도있음 그래서 bigEndian 옵션도 추가
        // 결국 여기에서 반환되는값이 깊이 값이고 BG채널이 깊이값으로 사용됨

        public static float[,] LoadZ16FromArgb32(Mat zMat)
        {
            ArgbByteIndex lowByte = ArgbByteIndex.B;
            ArgbByteIndex highByte = ArgbByteIndex.G;
            bool bigEndian = false;

            int h = zMat.Rows, w = zMat.Cols;
            var outArr = new float[h, w];

            var idx = zMat.GetGenericIndexer<Vec4b>();
            int lo = (int)lowByte, hi = (int)highByte;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var px = idx[y, x]; // BGRA
                    byte bLow = px[lo];
                    byte bHigh = px[hi];

                    ushort v = bigEndian
                        ? (ushort)((bLow << 8) | bHigh)
                        : (ushort)(bLow | (bHigh << 8));

                    outArr[y, x] = v;
                }
            }
            return outArr;
        }
        public static float[,] LoadZ16(Mat zMat)
        {
            int h = zMat.Rows, w = zMat.Cols;
            var outArr = new float[h, w];
            var idx = zMat.GetGenericIndexer<ushort>();
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    outArr[y, x] = idx[y, x];
            return outArr;
        }
        public static float[,] LoadZ8(Mat zMat)
        {
            int h = zMat.Rows, w = zMat.Cols;
            var outArr = new float[h, w];
            var idx = zMat.GetGenericIndexer<byte>();
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    outArr[y, x] = idx[y, x];
            return outArr;
        }

        public static InspectionResults Inspect(Mat intensityMat, float[,] zRaw, string roiMaskPath = null, bool drawOverlay = true)
        {
            try
            {
                // Intensity와 Z 크기가 다를 수 있으니, 두 쪽의 공통 영역(H, W)만 사용
                int H = Math.Min(intensityMat.Rows, zRaw.GetLength(0));
                int W = Math.Min(intensityMat.Cols, zRaw.GetLength(1));
                // intensityMat에서 [0..H), [0..W) 영역을 잘라서 사용(복사 아님, 뷰/슬라이스)
                var im = intensityMat[0, H, 0, W];

                // Z의 유효/무효(Invalid) 여부를 표시할 마스크(메모리상 true/false)
                var mask = new bool[H, W];
                int validCount = 0; // 유효 픽셀 개수(포인트클라우드 생성 시 배열 크기 산출용)
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        float v = zRaw[y, x];          // 이 좌표의 Z 원시값(0~65535 또는 Invalid)
                        bool ok = (v != P.InvalidZ16); // 16-bit Z의 무효값(예: -1 또는 특정 상수)과 비교
                        mask[y, x] = ok;               // 유효/무효 기록
                        if (ok) validCount++;          // 유효 픽셀 카운트 증가
                    }
                if (validCount == 0) throw new InvalidOperationException("No valid Z."); // 유효 Z가 하나도 없으면 중단

                // 좌표계 원점 설정: CenterOrigin이면 영상 중앙을 (0,0)으로, 아니면 좌상단을 (0,0)으로
                double cx = P.CenterOrigin ? W / 2.0 : 0.0;
                double cy = P.CenterOrigin ? H / 2.0 : 0.0;

                // 유효 픽셀 수만큼 포인트/색상 배열을 미리 할당(성능상 이득)
                var pts = new Point3D[validCount];    // 3D 포인트(X,Y,Z) 모음
                var cols = new MediaColor[validCount];// 각 포인트의 색(그레이로 통일 예정)

                // Intensity의 채널/비트심도는 다양할 수 있으니, 아래에서 8비트 그레이 기준으로 정규화할 준비
                int ch = im.Channels();
                // 각 채널 수에 맞는 픽셀 인덱서를 준비(성능 좋고 사용 간편)
                var idx = (ch == 4) ? im.GetGenericIndexer<Vec4b>() : null;

                int k = 0; // 유효 픽셀을 pts/cols에 채울 때 사용할 인덱스
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        if (!mask[y, x]) continue; // 무효 Z는 건너뜀

                        // 픽셀 좌표(x,y)를 mm 단위의 X,Y로 변환 (픽셀 간격 Sx,Sy, 원점 이동 반영)
                        double X = (x - cx) * P.Sx;
                        double Y = -(y - cy) * P.Sy;           // 영상 좌표는 아래로 증가 → 3D Y는 위로 증가시키려 음수
                        double Z = P.ZOffset + P.ZScale * zRaw[y, x]; // Z 스케일/오프셋 적용해 물리 단위로
                        pts[k] = new Point3D(X, Y, Z);         // 포인트클라우드에 추가

                        // 포인트 색상(그레이스케일): 채널 수에 따라 회색값 g를 계산
                        byte R, G, B;
                            var bgra = idx[y, x];          // B,G,R,A
                            byte g = (byte)Math.Min(255, (int)Math.Round(0.114 * bgra.Item0 + 0.587 * bgra.Item1 + 0.299 * bgra.Item2));
                            R = g; G = g; B = g;
                        
                        cols[k] = MediaColor.FromRgb(R, G, B); // 이 포인트의 색 저장
                        k++;                                   // 다음 유효 픽셀로 인덱스 이동
                    }

                // ROI 생성: 외부 마스크 파일이 있으면 병합, 없으면 Z 유효 영역 기반으로 자동 생성
                Mat roi = BuildRoiAutoOrFromMask(zRaw, H, W, roiMaskPath);

                // ROI를 살짝 깎아서(침식) 경계 잡음 제거
                Cv2.Erode(roi, roi, Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3)));

                // valid: 유효 Z(255)/무효 Z(0)를 표현하는 8비트 마스크(Mat)
                var valid = new Mat(H, W, MatType.CV_8UC1);
                unsafe
                {
                    // valid.Data를 직접 써서 빠르게 채움
                    byte* vp = (byte*)valid.Data;
                    for (int y = 0; y < H; y++)
                        for (int x = 0; x < W; x++)
                        {
                            float v = zRaw[y, x];
                            bool ok = (v != P.InvalidZ16);
                            // Mat의 한 줄 stride는 valid.Step(), 픽셀은 1바이트
                            vp[y * valid.Step() + x] = ok ? (byte)255 : (byte)0;
                        }
                }

                // hole = ROI 안에서 "유효하지 않은 영역"만 선택 (즉, ROI ∧ ¬valid)
                var hole = new Mat();
                var notValid = new Mat();
                Cv2.BitwiseNot(valid, notValid); // valid를 반전 → 무효=255, 유효=0
                Cv2.BitwiseAnd(notValid, roi, hole); // 무효 ∧ ROI

                // hole에 대해 오프닝/클로징(열기/닫기) 연산으로 작은 잡음 제거 & 경계 매끈하게
                var k3 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
                Cv2.MorphologyEx(hole, hole, MorphTypes.Open, k3);
                Cv2.MorphologyEx(hole, hole, MorphTypes.Close, k3);

                // hole에서 연결성분 레이블링(각각의 결함 영역 추출)
                var labels = new Mat(); // 각 픽셀의 레이블 ID 저장
                var stats = new Mat(); // 레이블별 통계(면적, bbox 등)
                var cents = new Mat(); // 레이블별 무게중심
                int n = Cv2.ConnectedComponentsWithStats(
                    hole, labels, stats, cents,
                    PixelConnectivity.Connectivity8, MatType.CV_32S);

                // 픽셀 면적을 mm^2로 바꾸기 위한 변환(픽셀 크기 Sx,Sy 사용)
                double pxAreaToMm2 = P.Sx * P.Sy;

                // 최종적으로 유지할 영역(면적 기준 필터링 후 OR로 합칠 마스크)
                var keepMask = new Mat(H, W, MatType.CV_8UC1, Scalar.All(0));

                // 결과 요약 저장용 리스트들(라벨, 면적, 바운딩 박스, 중심 등)
                var compLabels = new List<int>();
                var compAreaPx = new List<int>();
                var compAreaMm2 = new List<double>();
                var compBBox = new List<OpenCvSharp.Rect>();
                var compCentroidPx = new List<OpenCvSharp.Point>();

                // 레이블 0은 배경, 1부터가 실제 컴포넌트들
                for (int i = 1; i < n; i++)
                {
                    int areaPx = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area); // 픽셀 개수
                    double areaMm2 = areaPx * pxAreaToMm2;                              // 물리 면적
                    if (areaMm2 < P.MinAreaMm2) continue;                              // 너무 작으면 스킵(노이즈 제거)

                    // 레이블 i의 바운딩박스 정보 추출
                    int x = stats.Get<int>(i, (int)ConnectedComponentsTypes.Left);
                    int y = stats.Get<int>(i, (int)ConnectedComponentsTypes.Top);
                    int w = stats.Get<int>(i, (int)ConnectedComponentsTypes.Width);
                    int h = stats.Get<int>(i, (int)ConnectedComponentsTypes.Height);
                    var bb = new OpenCvSharp.Rect(x, y, w, h);

                    // labels == i인 픽셀만 255인 마스크(eq)를 만들어 keepMask에 OR로 누적
                    var eq = labels.InRange(i, i);
                    Cv2.BitwiseOr(keepMask, eq, keepMask);

                    // 무게중심(소수) 좌표를 정수 근사로 변환해 표시용으로 사용
                    var cxPx = cents.Get<double>(i, 0);
                    var cyPx = cents.Get<double>(i, 1);

                    compLabels.Add(i);
                    compAreaPx.Add(areaPx);
                    compAreaMm2.Add(areaMm2);
                    compBBox.Add(bb);
                    compCentroidPx.Add(new OpenCvSharp.Point((int)Math.Round(cxPx), (int)Math.Round(cyPx)));
                }

                // hole을 최종 유지 마스크로 치환(면적 기준 필터링 결과)
                hole = keepMask;

                // hole 경계선(외곽)들을 추출 → 이후 2D/3D 오버레이에 사용
                var contours = new List<OpenCvSharp.Point[]>();
                Cv2.FindContours(hole, out var cnts, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                contours.AddRange(cnts);

                Bitmap overlayBmp = null; // 2D 오버레이 비트맵(옵션)
                if (drawOverlay)
                {
                    Mat imColor;
                    {
                        Mat base8;
                        // im이 8U(1/3/4채널)이 아니면 8U로 변환(16U → 1/256 스케일)
                        if (im.Type() != MatType.CV_8UC1 && im.Type() != MatType.CV_8UC3 && im.Type() != MatType.CV_8UC4)
                        {
                            double scale = (im.Type() == MatType.CV_16UC1) ? 1.0 / 256.0 : 1.0;
                            base8 = new Mat();
                            im.ConvertTo(base8, MatType.CV_8U, scale);
                        }
                        else base8 = im;

                        // 컬러 3채널(BGR)로 맞춤(1채널 Gray → BGR, 4채널 BGRA → BGR)
                        if (base8.Channels() == 1) Cv2.CvtColor(base8, imColor = new Mat(), ColorConversionCodes.GRAY2BGR);
                        else if (base8.Channels() == 4) Cv2.CvtColor(base8, imColor = new Mat(), ColorConversionCodes.BGRA2BGR);
                        else imColor = base8.Clone(); // 이미 3채널이면 복사본 생성
                    }

                    // 결함 영역(hole)을 붉게 채워 보이게 하기 위한 베이스
                    var fill = new Mat(imColor.Size(), imColor.Type(), new Scalar(0, 0, 255)); // 순수 빨강(BGR = (0,0,255))
                    var blended = new Mat();
                    // 원본*1.0 + 빨강*OverlayAlpha → 빨간 기운이 입혀진 이미지
                    Cv2.AddWeighted(imColor, 1.0, fill, P.OverlayAlpha, 0, blended);
                    // hole(255인 곳)에만 blended 복사 → 즉, 결함 영역만 붉게 표시
                    blended.CopyTo(imColor, hole);

                    // 결함 외곽선(노란색) 덧그리기
                    foreach (var c in contours) Cv2.Polylines(imColor, new[] { c }, true, new Scalar(0, 255, 255), 2);

                    // 각 컴포넌트의 면적(mm^2)을 중심 근처에 라벨로 표기
                    for (int i = 0; i < compLabels.Count; i++)
                    {
                        string txt = compAreaMm2[i].ToString("F1", CultureInfo.InvariantCulture) + " mm^2";
                        // 가독성을 위해 검정 테두리 + 노란 본문을 두 번 찍음
                        Cv2.PutText(imColor, txt, compCentroidPx[i], HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 0), 2, LineTypes.AntiAlias);
                        Cv2.PutText(imColor, txt, compCentroidPx[i], HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 255, 255), 1, LineTypes.AntiAlias);
                    }

                    // 최종 2D 오버레이(Mat)를 Bitmap으로 변환해 UI에 띄울 수 있게 함
                    overlayBmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imColor);
                }

                // 모든 결과를 하나의 구조체(InspectionResults)로 묶어서 반환
                return new InspectionResults
                {
                    Points = pts,                 // 3D 포인트(유효 Z들만)
                    Colors = cols,                // 각 포인트의 색상(그레이)
                    Width = W,                    // 처리 영역 가로
                    Height = H,                   // 처리 영역 세로
                    HoleMask = hole,              // 결함 영역 마스크(Mat, 8UC1)
                    ContoursPx = contours,        // 결함 외곽선 픽셀 좌표들
                    CompLabels = compLabels,      // 남겨둔 컴포넌트 라벨 ID들
                    CompAreaPx = compAreaPx,      // 컴포넌트 면적(픽셀)
                    CompAreaMm2 = compAreaMm2,    // 컴포넌트 면적(mm^2)
                    CompBBox = compBBox,          // 컴포넌트 바운딩 박스
                    CompCentroidPx = compCentroidPx, // 컴포넌트 중심(픽셀 좌표)
                    Overlay2D = overlayBmp        // 2D 오버레이 비트맵(옵션)
                };
            }
            catch
            {
                // 상위에서 메시지 처리/로깅을 하도록 예외는 재던짐
                throw;
            }
        }



        /// 2D 컨투어를 3D 라인 루프로 변환
        public static List<Point3D[]> Make3DContourLoops(InspectionResults res, float[,] zRaw, int neighbor = 2)
        {
            if (res == null || res.ContoursPx == null || res.ContoursPx.Count == 0) return new List<Point3D[]>();

            int H = zRaw.GetLength(0), W = zRaw.GetLength(1);
            double cx = P.CenterOrigin ? W / 2.0 : 0.0;
            double cy = P.CenterOrigin ? H / 2.0 : 0.0;

            var loops = new List<Point3D[]>();
            foreach (var c in res.ContoursPx)
            {
                var loop = new Point3D[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    int x = ClampInt(c[i].X, 0, W - 1);
                    int y = ClampInt(c[i].Y, 0, H - 1);
                    double zVal = SampleZWithNeighbor(zRaw, x, y, P.InvalidZ16, neighbor); // 16-bit 전용
                    double X = (x - cx) * P.Sx;
                    double Y = -(y - cy) * P.Sy;
                    double Z = P.ZOffset + P.ZScale * zVal;
                    loop[i] = new Point3D(X, Y, Z);
                }
                loops.Add(loop);
            }
            return loops;
        }

        // ROI를 파일(있으면) 또는 Z 유효영역의 최대성분으로 생성
        private static OpenCvSharp.Mat BuildRoiAutoOrFromMask(float[,] zRaw, int H, int W, string roiMaskPath)
        {
            if (!string.IsNullOrEmpty(roiMaskPath) && File.Exists(roiMaskPath))
            {
                var roi = Cv2.ImRead(roiMaskPath, ImreadModes.Grayscale);
                if (roi.Empty()) throw new InvalidOperationException("ROI read failed: " + roiMaskPath);
                if (roi.Rows != H || roi.Cols != W)
                    Cv2.Resize(roi, roi, new OpenCvSharp.Size(W, H), 0, 0, InterpolationFlags.Nearest);
                Cv2.Threshold(roi, roi, 127, 255, ThresholdTypes.Binary);
                return roi;
            }

            var valid = new Mat(H, W, MatType.CV_8UC1);
            unsafe
            {
                byte* vp = (byte*)valid.Data;
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        float v = zRaw[y, x];
                        bool ok = (v != P.InvalidZ16); // 16-bit 전용
                        vp[y * valid.Step() + x] = ok ? (byte)255 : (byte)0;
                    }
            }

            var k5 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(valid, valid, MorphTypes.Close, k5);
            Cv2.MorphologyEx(valid, valid, MorphTypes.Open, k5);

            Cv2.FindContours(valid, out var cnts, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            var roiAuto = new Mat(H, W, MatType.CV_8UC1, Scalar.All(0));
            if (cnts != null && cnts.Length > 0)
            {
                int best = 0; double bestA = 0;
                for (int i = 0; i < cnts.Length; i++)
                {
                    double a = Cv2.ContourArea(cnts[i]);
                    if (a > bestA) { bestA = a; best = i; }
                }
                Cv2.DrawContours(roiAuto, new[] { cnts[best] }, -1, Scalar.All(255), -1);
            }
            return roiAuto;
        }

        /// 결손 영역을 3D 채움 메쉬로 생성
        // === [REPLACE] 3D 채움 메쉬: is16 제거 ===
        public static HT.MeshGeometry3D[] Make3DFilledMeshes(InspectionResults res, float[,] zRaw, int neighbor = 2, double approxEpsPx = 1.5)
        {
            if (res == null || res.ContoursPx == null || res.ContoursPx.Count == 0) return Array.Empty<HT.MeshGeometry3D>();

            int H = zRaw.GetLength(0), W = zRaw.GetLength(1);
            double cx = P.CenterOrigin ? W / 2.0 : 0.0;
            double cy = P.CenterOrigin ? H / 2.0 : 0.0;

            var list = new List<HT.MeshGeometry3D>();

            foreach (var c0 in res.ContoursPx)
            {
                if (c0 == null || c0.Length < 3) continue;

                var c = (approxEpsPx > 0) ? Cv2.ApproxPolyDP(c0, approxEpsPx, true) : c0;
                if (c.Length < 3) continue;

                var tris = TriangulateSimplePolygon(c);

                var positions = new HT.Vector3Collection();
                var indices = new HT.IntCollection();
                var map = new Dictionary<int, int>();

                for (int t = 0; t < tris.Count; t++)
                {
                    int[] tri = tris[t];
                    int i0 = MapIndex(c, tri[0], map, positions, W, H, cx, cy, zRaw, neighbor);
                    int i1 = MapIndex(c, tri[1], map, positions, W, H, cx, cy, zRaw, neighbor);
                    int i2 = MapIndex(c, tri[2], map, positions, W, H, cx, cy, zRaw, neighbor);
                    indices.Add(i0); indices.Add(i1); indices.Add(i2);
                }

                list.Add(new HT.MeshGeometry3D { Positions = positions, Indices = indices });
            }

            return list.ToArray();
        }

        static int MapIndex(OpenCvSharp.Point[] c, int i, Dictionary<int, int> map, HT.Vector3Collection pos,
                            int W, int H, double cx, double cy, float[,] zRaw, int neighbor)
        {
            if (map.TryGetValue(i, out int outIdx)) return outIdx;

            int x = Math.Max(0, Math.Min(W - 1, c[i].X));
            int y = Math.Max(0, Math.Min(H - 1, c[i].Y));

            double zVal = SampleZWithNeighbor(zRaw, x, y, P.InvalidZ16, neighbor); // 16-bit 전용
            double X = (x - cx) * P.Sx;
            double Y = -(y - cy) * P.Sy;
            double Z = P.ZOffset + P.ZScale * zVal;

            outIdx = pos.Count;
            pos.Add(new Vector3((float)X, (float)Y, (float)Z));
            map[i] = outIdx;
            return outIdx;
        }

        /// 다각형을 귀자르기 삼각분할
        static List<int[]> TriangulateSimplePolygon(OpenCvSharp.Point[] poly)
        {
            int n = poly.Length;
            var idx = Enumerable.Range(0, n).ToList();
            var tris = new List<int[]>();

            if (SignedArea(poly) < 0) idx.Reverse();

            int guard = 0;
            while (idx.Count > 2 && guard++ < 100000)
            {
                bool earFound = false;
                for (int k = 0; k < idx.Count; k++)
                {
                    int i0 = idx[(k - 1 + idx.Count) % idx.Count];
                    int i1 = idx[k];
                    int i2 = idx[(k + 1) % idx.Count];

                    var a = poly[i0]; var b = poly[i1]; var c = poly[i2];
                    if (!IsConvex(a, b, c)) continue;

                    bool anyInside = false;
                    for (int m = 0; m < idx.Count; m++)
                    {
                        int im = idx[m];
                        if (im == i0 || im == i1 || im == i2) continue;
                        if (PointInTriangle(poly[im], a, b, c)) { anyInside = true; break; }
                    }
                    if (anyInside) continue;

                    tris.Add(new[] { i0, i1, i2 });
                    idx.RemoveAt(k);
                    earFound = true;
                    break;
                }
                if (!earFound) break;
            }
            return tris;
        }
        /// 삼각형 꼭짓점 인덱스를 위치/인덱스로 매핑
        static int MapIndex(OpenCvSharp.Point[] c, int i, Dictionary<int, int> map, HT.Vector3Collection pos, int W, int H, double cx, double cy, float[,] zRaw, bool is16, int neighbor)
        {
            int outIdx;
            if (map.TryGetValue(i, out outIdx)) return outIdx;

            int x = Math.Max(0, Math.Min(W - 1, c[i].X));
            int y = Math.Max(0, Math.Min(H - 1, c[i].Y));

            double zVal = SampleZWithNeighbor(zRaw, x, y, is16 ? P.InvalidZ16 : P.InvalidZ, neighbor);
            double X = (x - cx) * P.Sx;
            double Y = -(y - cy) * P.Sy;
            double Z = P.ZOffset + P.ZScale * zVal;

            outIdx = pos.Count;
            pos.Add(new Vector3((float)X, (float)Y, (float)Z));
            map[i] = outIdx;
            return outIdx;
        }
        /// 다각형 부호 있는 면적
        static double SignedArea(OpenCvSharp.Point[] poly)
        {
            double s = 0;
            for (int i = 0; i < poly.Length; i++)
            {
                var p = poly[i];
                var q = poly[(i + 1) % poly.Length];
                s += (double)p.X * q.Y - (double)p.Y * q.X;
            }
            return s * 0.5;
        }
        /// CCW 볼록성 판정
        static bool IsConvex(OpenCvSharp.Point a, OpenCvSharp.Point b, OpenCvSharp.Point c)
        {
            long cross = (long)(b.X - a.X) * (c.Y - a.Y) - (long)(b.Y - a.Y) * (c.X - a.X);
            return cross > 0;
        }
        /// 점의 삼각형 포함 판정
        static bool PointInTriangle(OpenCvSharp.Point p, OpenCvSharp.Point a, OpenCvSharp.Point b, OpenCvSharp.Point c)
        {
            double v0x = c.X - a.X, v0y = c.Y - a.Y;
            double v1x = b.X - a.X, v1y = b.Y - a.Y;
            double v2x = p.X - a.X, v2y = p.Y - a.Y;
            double dot00 = v0x * v0x + v0y * v0y;
            double dot01 = v0x * v1x + v0y * v1y;
            double dot02 = v0x * v2x + v0y * v2y;
            double dot11 = v1x * v1x + v1y * v1y;
            double dot12 = v1x * v2x + v1y * v2y;
            double inv = 1.0 / Math.Max(1e-12, (dot00 * dot11 - dot01 * dot01));
            double u = (dot11 * dot02 - dot01 * dot12) * inv;
            double v = (dot00 * dot12 - dot01 * dot02) * inv;
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }
        /// 근방 평균으로 Z 샘플링(폴백 링 확장)
        static double SampleZWithNeighbor(float[,] zRaw, int x, int y, float invalidZ, int neighbor)
        {
            int H = zRaw.GetLength(0), W = zRaw.GetLength(1);
            double sum = 0; int cnt = 0;

            for (int dy = -neighbor; dy <= neighbor; dy++)
                for (int dx = -neighbor; dx <= neighbor; dx++)
                {
                    int xx = x + dx, yy = y + dy;
                    if (xx < 0 || yy < 0 || xx >= W || yy >= H) continue;
                    float v = zRaw[yy, xx];
                    if (v != invalidZ) { sum += v; cnt++; }
                }
            if (cnt > 0) return sum / cnt;

            for (int r = neighbor + 1; r <= neighbor + 6; r++)
            {
                sum = 0; cnt = 0;
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Math.Abs(dx) != r && Math.Abs(dy) != r) continue;
                        int xx = x + dx, yy = y + dy;
                        if (xx < 0 || yy < 0 || xx >= W || yy >= H) continue;
                        float v = zRaw[yy, xx];
                        if (v != invalidZ) { sum += v; cnt++; }
                    }
                if (cnt > 0) return sum / cnt;
            }
            return invalidZ;
        }
        /// 정수 클램프
        static int ClampInt(int v, int lo, int hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }
    }
}
