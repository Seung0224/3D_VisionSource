using OpenCvSharp;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using HT = HelixToolkit.Wpf.SharpDX;
using MediaColor = System.Windows.Media.Color;

namespace _3D_VisionSource
{
    public static class FusionEngine
    {
        static InspectionParams P = new InspectionParams();

        public enum ArgbByteIndex : int { B = 0, G = 1, R = 2, A = 3 }

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

        #region Public API: Inspect (refactored)
        public static InspectionResults Inspect(Mat intensityMat, float[,] zRaw, InspectionParams p, System.Drawing.RectangleF? roiRectImg = null, bool draw2DOverlay = true)
        {
            P = p.Clone();

            var sw = Stopwatch.StartNew();
            long last = 0L;
            Action<string> lap = name =>
            {
                long t = sw.ElapsedMilliseconds;
                Log("{0}: {1} ms", name, t - last);
                last = t;
            };

            try
            {
                Log("Inspect start");

                // 1) 입력 슬라이스: Intensity 이미지와 Zmap 의 이미지 사이즈가 동일하지 않을 수도 있기때문에 맞춰주는 작업이 필요함
                Mat im;
                int H, W;
                SliceInputs(intensityMat, zRaw, out im, out H, out W);
                lap("Slice");

                // 2) 유효 Z 마스크(bool) & 카운트
                //  - zRaw에서 신뢰 가능한 깊이(센티넬/NaN/Inf 제외)만 true로 표시하고 개수를 집계한다.
                //  - 센티넬(P.InvalidZ16)은 “이 픽셀의 깊이는 신뢰할 수 없어 비워둔 값”을 의미한다.

                bool[,] validMaskBool;
                int validCount;
                BuildValidMaskBool(zRaw, H, W, out validMaskBool, out validCount, P.Centinal);
                
                if (!P.Centinal && validCount == 0)
                    throw new InvalidOperationException("No valid Z.");
                lap("Valid Z Calc");

                // 3) 포인트클라우드 + 색 생성
                //  - 유효 픽셀(validMaskBool=true)만 사용해 3D 좌표(X,Y,Z)와 회색톤 색상을 계산한다.
                //  - (x,y,zRaw) → (X,Y,Z) 변환 시 스케일·오프셋(P.Sx, Sy, ZScale, ZOffset) 적용.
                //  - CenterOrigin=true면 영상 중심을 (0,0)으로, false면 좌상단을 원점으로 둔다.
                //  - 결과: pts[], cols[]에 유효 포인트가 연속 저장되어 3D 시각화·측정에 사용된다.
                //  - 배열 참조로 넘겨주기때문에 ref, out이 없어도 pts, cols값을 그대로 InspectionResults로 전달

                int totalForArrays = P.Centinal ? (H * W) : validCount;
                var pts = new Point3D[totalForArrays];
                var cols = new MediaColor[totalForArrays];
                ComputePointCloudAndColorsFast(im, zRaw, validMaskBool, H, W, pts, cols, P.Centinal);
                lap("pointcloud+color");

                // 4) ROI 마스크 만들기 (사각 or 자동)
                Mat roi = BuildRoiMask(roiRectImg, zRaw, H, W);
                lap(roiRectImg.HasValue ? "ROI Mode On" : "A-ROI Mode On");

                #region Rule-Based 기반 검사 비전 파이프 라인
                // 5) 유효 Z 8U 마스크
                // - OpenCV에서 검사를 하기 위해서 유효한 값을 뽑은 MASK를 MAT 형태로 변환하는 과정
                var valid8u = BoolMaskTo8U(validMaskBool, H, W);
                lap("valid-mat8u");

                // 6) hole = ROI ∧ ¬valid
                // 5)에서 만든 ZMap과 ROI 마스크에서 빈곳만 255로 채움 (5번 결과값을 반전해서 구함)
                var hole = BuildHoleMask(valid8u, roi);
                lap("hole-build");

                // 7) 형태학적 정리
                // 너무 작은 검출 결과들은 전처리 과정을 통해서 없애버림 (Cognex Blob MinPixel과 같은 개념의 느낌)
                MorphCleanInPlace(hole, ksize: P.MinPxKernel, P.UseMinPixel);
                lap("morph-open-close");

                // 8) 라벨링 + 필터 (MinAreaMm2)
                // "hole 이미지 안에서 각각의 구멍(덩어리)을 찾아내고, 너무 작은 건 버리고, 남은 구멍들의 위치·크기·중심좌표를 정리하는 함수" 입니다.
                // 여기 이 함수 내에서 minAreaMm2 변수 값을 통해서 조절함
                // 이 함수 = “구멍 찾는 카운터 + 줄자” 구멍이 몇 개인지 세고 너무 작은 먼지 구멍은 무시하고 남은 구멍의 크기, 위치, 중심을 기록하는 과정이에요.
                ComponentResult comps = LabelAndFilterComponents(hole, P.Sx * P.Sy, P.MinAreaMm2);
                lap("cc-label+filter");

                // 최종 hole = keepMask
                hole = comps.KeepMask;

                // 9) 컨투어
                // 위에서 찾은 최종 Hole 들에 대해서 외곽선 그리기 위한 함수
                var contours = FindContoursFromMask(hole);
                lap("find-contours");

                // 10) 2D 오버레이
                Bitmap overlayBmp = null;
                if (draw2DOverlay)
                {
                    overlayBmp = RenderOverlay2D(im, hole, contours, comps);
                    lap("overlay-bitmap");
                }

                Log("Inspect done. total {0} ms", sw.ElapsedMilliseconds);
                #endregion

                return new InspectionResults
                {
                    Points = pts,
                    Colors = cols,
                    Width = W,
                    Height = H,
                    HoleMask = hole,
                    ContoursPx = contours,
                    CompLabels = comps.Labels,
                    CompAreaPx = comps.AreaPx,
                    CompAreaMm2 = comps.AreaMm2,
                    CompBBox = comps.BBox,
                    CompCentroidPx = comps.CentroidPx,
                    Overlay2D = overlayBmp
                };
            }
            catch
            {
                Log("Inspect ERROR after {0} ms", sw.ElapsedMilliseconds);
                throw;
            }
        }
        #endregion

        #region Inspect helpers

        /// <summary>
        /// 두 입력(intensityMat, zRaw)을 동일한 좌표계·해상도(H×W)로 강제로 맞춰서 이후 파이프라인이 픽셀 1:1로 정합된 상태에서 동작하도록 보장하기 위함입니다.
        /// </summary>
        /// <param name="intensityMat"></param>
        /// <param name="zRaw"></param>
        /// <param name="im"></param>
        /// <param name="H"></param>
        /// <param name="W"></param>
        private static void SliceInputs(Mat intensityMat, float[,] zRaw, out Mat im, out int H, out int W)
        {
            H = Math.Min(intensityMat.Rows, zRaw.GetLength(0));
            W = Math.Min(intensityMat.Cols, zRaw.GetLength(1));
            // intensityMat[0, H, 0, W]는 (row: 0 ~ H-1, col: 0 ~ W-1)
            im = intensityMat[0, H, 0, W];
        }

        private static void BuildValidMaskBool(float[,] zRaw, int H, int W, out bool[,] mask, out int validCount, bool countAllIfCentinal)
        {
            mask = new bool[H, W];
            int cnt = 0;

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float v = zRaw[y, x];
                    bool ok = (v != P.InvalidZ16); // 필요 시 NaN/Inf 배제 추가
                    mask[y, x] = ok;
                    if (ok) cnt++;
                }

            validCount = countAllIfCentinal ? (H * W) : cnt;
        }
        public static int ComputePointCloudAndColorsFast(Mat im, float[,] zRaw, bool[,] validMask, int H, int W, Point3D[] pts, MediaColor [] cols, bool includeInvalid = false)
        {
            if (im.Type() != MatType.CV_8UC4)
                throw new ArgumentException("im must be CV_8UC4 (BGRA). Convert beforehand.");

            // ---- 상수 캐시 & LUT 준비 ----
            double cx = P.CenterOrigin ? W / 2.0 : 0.0;
            double cy = P.CenterOrigin ? H / 2.0 : 0.0;
            double Sx = P.Sx, Sy = P.Sy, Zs = P.ZScale, Zo = P.ZOffset;

            // x, y LUT (double) — Point3D가 double이라 여기서 만들어두면 덜 계산함
            var xLut = new double[W];
            for (int x = 0; x < W; x++) xLut[x] = (x - cx) * Sx;
            var yLut = new double[H];
            for (int y = 0; y < H; y++) yLut[y] = -(y - cy) * Sy;

            // ---- 비압축 경로 (includeInvalid == true) ----
            if (includeInvalid)
            {
                int total = H * W;

                unsafe
                {
                    byte* basePtr = (byte*)im.Data;
                    long step = im.Step();

                    Parallel.For(0, H, y =>
                    {
                        byte* rowPtr = basePtr + y * step;
                        int rowBase = y * W;
                        double Y = yLut[y];

                        for (int x = 0; x < W; x++)
                        {
                            int i = rowBase + x;

                            // BGRA 읽기
                            int off = x << 2; // x*4
                            byte B = rowPtr[off + 0];
                            byte G = rowPtr[off + 1];
                            byte R = rowPtr[off + 2];
                            // byte A = rowPtr[off + 3]; // 사용 안함

                            // 좌표/색
                            double X = xLut[x];
                            double Z = Zo + Zs * zRaw[y, x];

                            pts[i] = new Point3D(X, Y, Z);
                            // invalid도 검정으로 만들고 싶으면 validMask로 분기 가능
                            cols[i] = validMask[y, x] ? MediaColor.FromArgb(255, R, G, B) : MediaColor.FromArgb(255, (byte)0, (byte)0, (byte)0);
                        }
                    });
                }

                return total;
            }

            // ---- 압축 경로 (includeInvalid == false) ----
            // 1) 행별 유효 개수 카운트
            var rowCounts = new int[H];
            int totalValid = 0;
            for (int y = 0; y < H; y++)
            {
                int c = 0;
                for (int x = 0; x < W; x++)
                    if (validMask[y, x]) c++;
                rowCounts[y] = c;
                totalValid += c;
            }

            // 2) prefix sum → 행 오프셋 계산
            var rowOffsets = new int[H];
            int acc = 0;
            for (int y = 0; y < H; y++)
            {
                rowOffsets[y] = acc;
                acc += rowCounts[y];
            }

            // pts/cols 길이 체크(부족하면 예외 or 필요한 만큼만 쓴다고 가정)
            // if (pts.Length < totalValid || cols.Length < totalValid) throw new ArgumentException("pts/cols too small");

            // 3) 병렬로 각 행을 자신의 구간에 race-free 채우기
            unsafe
            {
                byte* basePtr = (byte*)im.Data;
                long step = im.Step();

                Parallel.For(0, H, y =>
                {
                    int baseIndex = rowOffsets[y];
                    int local = 0;

                    byte* rowPtr = basePtr + y * step;
                    double Y = yLut[y];

                    for (int x = 0; x < W; x++)
                    {
                        if (!validMask[y, x]) continue;

                        int i = baseIndex + local;

                        int off = x << 2;
                        byte B = rowPtr[off + 0];
                        byte G = rowPtr[off + 1];
                        byte R = rowPtr[off + 2];

                        double X = xLut[x];
                        double Z = Zo + Zs * zRaw[y, x];

                        pts[i] = new Point3D(X, Y, Z);
                        cols[i] = MediaColor.FromArgb(255, R, G, B);

                        local++;
                    }
                });
            }

            return totalValid;
        }
        private static Mat BuildRoiMask(System.Drawing.RectangleF? roiRectImg, float[,] zRaw, int H, int W)
        {
            if (roiRectImg.HasValue)
                return BuildRoiFromRect(roiRectImg.Value, H, W);

            var roi = BuildRoiAuto(zRaw, H, W);
            Cv2.Erode(roi, roi, Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3))); // 경계 잡음 약화
            return roi;
        }

        private static Mat BoolMaskTo8U(bool[,] mask, int H, int W)
        {
            var m = new OpenCvSharp.Mat(H, W, OpenCvSharp.MatType.CV_8UC1);
            unsafe
            {
                byte* p = (byte*)m.Data;
                int step = (int)m.Step();
                for (int y = 0; y < H; y++)
                {
                    int row = y * step;
                    for (int x = 0; x < W; x++)
                        p[row + x] = mask[y, x] ? (byte)255 : (byte)0;
                }
            }
            return m;
        }

        private static Mat BuildHoleMask(Mat valid8u, Mat roi8u)
        {
            var hole = new Mat();
            using (var notValid = new Mat())
            {
                Cv2.BitwiseNot(valid8u, notValid);
                Cv2.BitwiseAnd(notValid, roi8u, hole);
            }
            return hole;
        }

        private static void MorphCleanInPlace(Mat mask, int ksize, bool UseMinPixel)
        {
            if (UseMinPixel)
            {
                var k = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(ksize, ksize));
                Cv2.MorphologyEx(mask, mask, MorphTypes.Open, k);
                Cv2.MorphologyEx(mask, mask, MorphTypes.Close, k);
            }
        }

        private struct ComponentResult
        {
            public Mat KeepMask;
            public List<int> Labels;
            public List<int> AreaPx;
            public List<double> AreaMm2;
            public List<OpenCvSharp.Rect> BBox;
            public List<OpenCvSharp.Point> CentroidPx;
        }

        private static ComponentResult LabelAndFilterComponents(Mat mask, double pxAreaToMm2, double minAreaMm2)
        {
            var labels = new Mat();
            var stats = new Mat();
            var cents = new Mat();

            int n = Cv2.ConnectedComponentsWithStats(mask, labels, stats, cents, PixelConnectivity.Connectivity8, MatType.CV_32S);

            var keepMask = new Mat(mask.Rows, mask.Cols, MatType.CV_8UC1, Scalar.All(0));
            var compLabels = new List<int>();
            var compAreaPx = new List<int>();
            var compAreaMm2 = new List<double>();
            var compBBox = new List<OpenCvSharp.Rect>();
            var compCentroidPx = new List<OpenCvSharp.Point>();

            for (int i = 1; i < n; i++)
            {
                int areaPx = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area);
                double areaMm2 = areaPx * pxAreaToMm2;
                if (areaMm2 < minAreaMm2) continue;

                int x = stats.Get<int>(i, (int)ConnectedComponentsTypes.Left);
                int y = stats.Get<int>(i, (int)ConnectedComponentsTypes.Top);
                int w = stats.Get<int>(i, (int)ConnectedComponentsTypes.Width);
                int h = stats.Get<int>(i, (int)ConnectedComponentsTypes.Height);
                var bb = new OpenCvSharp.Rect(x, y, w, h);

                using (var eq = labels.InRange(i, i))
                    Cv2.BitwiseOr(keepMask, eq, keepMask);

                var cxPx = cents.Get<double>(i, 0);
                var cyPx = cents.Get<double>(i, 1);

                compLabels.Add(i);
                compAreaPx.Add(areaPx);
                compAreaMm2.Add(areaMm2);
                compBBox.Add(bb);
                compCentroidPx.Add(new OpenCvSharp.Point((int)Math.Round(cxPx), (int)Math.Round(cyPx)));
            }

            labels.Dispose(); stats.Dispose(); cents.Dispose();

            return new ComponentResult
            {
                KeepMask = keepMask,
                Labels = compLabels,
                AreaPx = compAreaPx,
                AreaMm2 = compAreaMm2,
                BBox = compBBox,
                CentroidPx = compCentroidPx
            };
        }

        private static List<OpenCvSharp.Point[]> FindContoursFromMask(Mat binMask)
        {
            var contours = new List<OpenCvSharp.Point[]>();
            Cv2.FindContours(binMask, out var cnts, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            if (cnts != null && cnts.Length > 0) contours.AddRange(cnts);
            return contours;
        }

        private static Bitmap RenderOverlay2D(Mat im, Mat holeMask, List<OpenCvSharp.Point[]> contours, ComponentResult comps)
        {
            Mat imColor;
            {
                Mat base8;
                if (im.Type() != MatType.CV_8UC1 && im.Type() != MatType.CV_8UC3 && im.Type() != MatType.CV_8UC4)
                {
                    double scale = (im.Type() == MatType.CV_16UC1) ? 1.0 / 256.0 : 1.0;
                    base8 = new Mat();
                    im.ConvertTo(base8, MatType.CV_8U, scale);
                }
                else base8 = im;

                if (base8.Channels() == 1) Cv2.CvtColor(base8, imColor = new Mat(), ColorConversionCodes.GRAY2BGR);
                else if (base8.Channels() == 4) Cv2.CvtColor(base8, imColor = new Mat(), ColorConversionCodes.BGRA2BGR);
                else imColor = base8.Clone();
            }

            using (var fill = new Mat(imColor.Size(), imColor.Type(), new Scalar(0, 0, 255)))
            using (var blended = new Mat())
            {
                Cv2.AddWeighted(imColor, 1.0, fill, P.OverlayAlpha, 0, blended);
                blended.CopyTo(imColor, holeMask);
            }

            foreach (var c in contours)
                Cv2.Polylines(imColor, new[] { c }, true, new Scalar(0, 255, 255), 2);

            for (int i = 0; i < comps.Labels.Count; i++)
            {
                var bb = comps.BBox[i];
                var txt = $"{i + 1}:{FormatArea(comps.AreaMm2[i])}";

                int x = bb.Right + 4;
                int y = Math.Max(12, Math.Min(imColor.Rows - 4, bb.Top + 12));

                if (x > imColor.Cols - 40)
                {
                    x = Math.Max(0, bb.Left);
                    y = Math.Min(imColor.Rows - 4, bb.Bottom + 12);
                }

                DrawLabelThin(imColor, txt, new OpenCvSharp.Point(x, y), scale: 0.4, thickness: 1);
            }

            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imColor);
        }
        #endregion


        #region Public API: 3D conversions
        public static List<Point3D[]> Make3DContourLoops(InspectionResults res, float[,] zRaw, int neighbor = 2)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (res == null || res.ContoursPx == null || res.ContoursPx.Count == 0)
                {
                    Log("Make3DContourLoops: no contours");
                    return new List<Point3D[]>();
                }

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
                        double zVal = SampleZWithNeighbor(zRaw, x, y, P.InvalidZ16, neighbor);
                        double X = (x - cx) * P.Sx;
                        double Y = -(y - cy) * P.Sy;
                        double Z = P.ZOffset + P.ZScale * zVal;
                        loop[i] = new Point3D(X, Y, Z);
                    }
                    loops.Add(loop);
                }
                Log("Make3DContourLoops done in {0} ms (loops={1})", sw.ElapsedMilliseconds, loops.Count);
                return loops;
            }
            catch
            {
                Log("Make3DContourLoops ERROR after {0} ms", sw.ElapsedMilliseconds);
                throw;
            }
        }

        public static HT.MeshGeometry3D[] Make3DFilledMeshes(InspectionResults res, float[,] zRaw, int neighbor = 2, double approxEpsPx = 1.5)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (res == null || res.ContoursPx == null || res.ContoursPx.Count == 0)
                {
                    Log("Make3DFilledMeshes: no contours");
                    return Array.Empty<HT.MeshGeometry3D>();
                }

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
            catch
            {
                throw;
            }
        }
        #endregion

        #region ROI Builders
        static OpenCvSharp.Mat BuildRoiFromRect(System.Drawing.RectangleF rImg, int H, int W)
        {
            var roi = new Mat(H, W, MatType.CV_8UC1, Scalar.All(0));

            int x = Math.Max(0, Math.Min(W - 1, (int)Math.Floor(rImg.X)));
            int y = Math.Max(0, Math.Min(H - 1, (int)Math.Floor(rImg.Y)));
            int rw = Math.Max(0, Math.Min(W - x, (int)Math.Ceiling(rImg.Width)));
            int rh = Math.Max(0, Math.Min(H - y, (int)Math.Ceiling(rImg.Height)));

            if (rw > 0 && rh > 0)
                Cv2.Rectangle(roi, new OpenCvSharp.Rect(x, y, rw, rh), Scalar.All(255), -1, LineTypes.Link8);

            return roi;
        }

        static OpenCvSharp.Mat BuildRoiAuto(float[,] zRaw, int H, int W)
        {
            var valid = new OpenCvSharp.Mat(H, W, OpenCvSharp.MatType.CV_8UC1);
            unsafe
            {
                byte* vp = (byte*)valid.Data;
                int step = (int)valid.Step();
                for (int y = 0; y < H; y++)
                {
                    int row = y * step;
                    for (int x = 0; x < W; x++)
                    {
                        float v = zRaw[y, x];
                        bool ok = (v != P.InvalidZ16);
                        vp[row + x] = ok ? (byte)255 : (byte)0;
                    }
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
        #endregion

        #region Helpers
        static int MapIndex(OpenCvSharp.Point[] c, int i, Dictionary<int, int> map,
                            HT.Vector3Collection pos, int W, int H, double cx, double cy,
                            float[,] zRaw, int neighbor)
        {
            if (map.TryGetValue(i, out int outIdx)) return outIdx;

            int x = Math.Max(0, Math.Min(W - 1, c[i].X));
            int y = Math.Max(0, Math.Min(H - 1, c[i].Y));

            double zVal = SampleZWithNeighbor(zRaw, x, y, P.InvalidZ16, neighbor);
            double X = (x - cx) * P.Sx;
            double Y = -(y - cy) * P.Sy;
            double Z = P.ZOffset + P.ZScale * zVal;

            outIdx = pos.Count;
            pos.Add(new Vector3((float)X, (float)Y, (float)Z));
            map[i] = outIdx;
            return outIdx;
        }

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

        static bool IsConvex(OpenCvSharp.Point a, OpenCvSharp.Point b, OpenCvSharp.Point c)
        {
            long cross = (long)(b.X - a.X) * (c.Y - a.Y) - (long)(b.Y - a.Y) * (c.X - a.X);
            return cross > 0;
        }

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

        static int ClampInt(int v, int lo, int hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }

        static void DrawLabelThin(Mat img, string text, OpenCvSharp.Point org, double scale = 0.4, int thickness = 1)
        {
            Cv2.PutText(img, text, org, HersheyFonts.HersheySimplex, scale, new Scalar(0, 0, 0), thickness + 1, LineTypes.AntiAlias);
            Cv2.PutText(img, text, org, HersheyFonts.HersheySimplex, scale, new Scalar(0, 255, 255), thickness, LineTypes.AntiAlias);
        }

        static string FormatArea(double a)
        {
            var inv = CultureInfo.InvariantCulture;
            if (a < 1e-3) return (a * 1e6).ToString("F0", inv) + " µm^2";
            if (a < 1) return a.ToString("F3", inv) + " mm^2";
            if (a < 10) return a.ToString("F2", inv) + " mm^2";
            return a.ToString("F1", inv) + " mm^2";
        }
        #endregion

        #region Logging
        public interface IFusionLogger { void Log(string message); }
        public static IFusionLogger LogSink { get; set; }
        static void Log(string format, params object[] args)
        {
            var sink = LogSink;
            if (sink == null) return;
            var msg = (args == null || args.Length == 0)
                ? format
                : string.Format(CultureInfo.InvariantCulture, format, args);
            sink.Log(msg);
        }
        #endregion
    }
}
