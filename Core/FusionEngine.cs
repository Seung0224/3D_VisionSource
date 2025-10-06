using System;
using System.IO;
using SharpDX;
using System.Drawing;
using OpenCvSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media.Media3D;
using System.Linq;
using HT = HelixToolkit.Wpf.SharpDX;
using MediaColor = System.Windows.Media.Color;

namespace _3D_VisionSource
{
    public class FusionParams
    {
        public float Sx { get; set; } = 0.05f;     // mm/px
        public float Sy { get; set; } = 0.05f;     // mm/px
        public float ZScale { get; set; } = 0.001f; // mm/level
        public float ZOffset { get; set; } = 0f;    // mm
        public byte InvalidZ { get; set; } = 0;     // 8bit invalid
        public ushort InvalidZ16 { get; set; } = 0; // 16bit invalid
        public bool CenterOrigin { get; set; } = true;
    }
    public class PointCloudResult
    {
        public Point3D[] Points;
        public MediaColor[] Colors;
        public int Width;
        public int Height;
    }
    public class HoleComponent
    {
        public int Label { get; set; }
        public int AreaPx { get; set; }
        public double AreaMm2 { get; set; }
        public OpenCvSharp.Rect BBox { get; set; }
        public OpenCvSharp.Point CentroidPx { get; set; }
    }
    public class HoleDetectionResult
    {
        public Mat HoleMask;                 // 8UC1, 0/255
        public List<OpenCvSharp.Point[]> ContoursPx;  // 외곽선 픽셀 좌표들
        public List<HoleComponent> Components;        // 라벨/면적 등
        public Bitmap Overlay2D;             // Intensity 위 오버레이 (선택)
    }

    public static class FusionEngine
    {
        public static float[,] LoadZRawFromFile(string zPath, out bool is16bit)
        {
            using (var mat = Cv2.ImRead(zPath, ImreadModes.Unchanged))
            {
                if (mat.Empty()) throw new InvalidOperationException("ZMap read failed: " + zPath);
                if (mat.Channels() != 1) throw new NotSupportedException("ZMap must be 1-channel.");

                if (mat.Type() == MatType.CV_16UC1)
                {
                    is16bit = true;
                    int h = mat.Rows, w = mat.Cols;
                    var outArr = new float[h, w];
                    var idx = mat.GetGenericIndexer<ushort>();
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                            outArr[y, x] = idx[y, x]; // 0..65535
                    return outArr;
                }
                if (mat.Type() == MatType.CV_8UC1)
                {
                    is16bit = false;
                    int h = mat.Rows, w = mat.Cols;
                    var outArr = new float[h, w];
                    var idx = mat.GetGenericIndexer<byte>();
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                            outArr[y, x] = idx[y, x]; // 0..255
                    return outArr;
                }
                throw new NotSupportedException("ZMap must be CV_8UC1 or CV_16UC1.");
            }
        }

        public static PointCloudResult BuildPointCloudFromFiles(string intensityPath, string zPath, FusionParams p)
        {
            // 1) 입력 로드
            Mat im = Cv2.ImRead(intensityPath, ImreadModes.Unchanged);   // 원본 그대로 (8UC1/8UC3/8UC4/16UC1 등)
            if (im.Empty()) throw new InvalidOperationException("Intensity read failed: " + intensityPath);

            bool is16;
            var zRaw = LoadZRawFromFile(zPath, out is16);

            int h = Math.Min(im.Rows, zRaw.GetLength(0));
            int w = Math.Min(im.Cols, zRaw.GetLength(1));

            // 2) 무효 마스크
            var mask = new bool[h, w];
            int validCount = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float v = zRaw[y, x];
                    bool ok = is16 ? (v != p.InvalidZ16) : (v != p.InvalidZ);
                    mask[y, x] = ok;
                    if (ok) validCount++;
                }
            if (validCount == 0) throw new InvalidOperationException("유효한 Z가 없습니다.");

            // 3) 좌표/색 생성
            double cx = p.CenterOrigin ? w / 2.0 : 0.0;
            double cy = p.CenterOrigin ? h / 2.0 : 0.0;

            var pts = new Point3D[validCount];
            var cols = new MediaColor[validCount];

            // Intensity를 색으로 꺼내기 위한 인덱서들
            int ch = im.Channels();
            Mat im8; // 색상 접근은 8비트로 통일
            if (ch == 1)
            {
                // 8UC1이면 그대로, 16UC1이면 8UC1로 스케일링
                if (im.Type() == MatType.CV_8UC1) im8 = im;
                else
                {
                    im8 = new Mat();
                    if (im.Type() == MatType.CV_16UC1)
                    {
                        // 0..65535 -> 0..255
                        im.ConvertTo(im8, MatType.CV_8U, 1.0 / 256.0);
                    }
                    else
                    {
                        // 예외 케이스는 안전하게 8UC1로 변환
                        im.ConvertTo(im8, MatType.CV_8U);
                    }
                }
            }
            else if (ch == 3 || ch == 4)
            {
                // 3/4채널은 8비트 보장(일반 이미지). 4채널이면 그대로 쓰고 A는 무시.
                im8 = im;
            }
            else
            {
                // 예상치 못한 포맷은 8UC3로 변환
                im8 = new Mat();
                im.ConvertTo(im8, MatType.CV_8UC3);
                ch = 3;
            }

            // 인덱서 준비
            var idx1 = (ch == 1) ? im8.GetGenericIndexer<byte>() : null;
            var idx3 = (ch == 3) ? im8.GetGenericIndexer<Vec3b>() : null;
            var idx4 = (ch == 4) ? im8.GetGenericIndexer<Vec4b>() : null;

            int k = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (!mask[y, x]) continue;

                    // 월드 좌표
                    double X = (x - cx) * p.Sx;
                    double Y = -(y - cy) * p.Sy;
                    double Z = p.ZOffset + p.ZScale * zRaw[y, x];
                    pts[k] = new Point3D(X, Y, Z);


                    byte R, G, B;

                    if (ch == 1)
                    {
                        byte g = idx1[y, x];
                        R = g; G = g; B = g;
                    }
                    else if (ch == 3)
                    {
                        var bgr = idx3[y, x];
                        // 항상 회색조로: 0.114*B + 0.587*G + 0.299*R (ITU-R BT.601)
                        byte g = (byte)Math.Min(255,
                            (int)Math.Round(0.114 * bgr.Item0 + 0.587 * bgr.Item1 + 0.299 * bgr.Item2));
                        R = g; G = g; B = g;
                    }
                    else // ch == 4
                    {
                        var bgra = idx4[y, x];
                        byte g = (byte)Math.Min(255,
                            (int)Math.Round(0.114 * bgra.Item0 + 0.587 * bgra.Item1 + 0.299 * bgra.Item2));
                        R = g; G = g; B = g;
                    }

                    cols[k] = MediaColor.FromRgb(R, G, B);
                    k++;
                }

            return new PointCloudResult { Points = pts, Colors = cols, Width = w, Height = h };
        }

        /// <summary>
        /// Z의 invalid 기반 Hole 검출 + 2D 오버레이 생성
        /// - roiMaskPath: 선택(흑백, 0/255). null이면 전체.
        /// - minAreaMm2: 이보다 작은 결손은 무시.
        /// - return: HoleDetectionResult (마스크/컨투어/성분/2D 오버레이)
        /// </summary>
        public static HoleDetectionResult DetectHolesAndMakeOverlay(
            string intensityPath,
            string zPath,
            FusionParams p,
            string roiMaskPath = null,
            double minAreaMm2 = 1.0,
            bool drawOverlay = true)
        {
            // 0) 입력
            var imRaw = Cv2.ImRead(intensityPath, ImreadModes.Unchanged);
            if (imRaw.Empty()) throw new InvalidOperationException("Intensity read failed: " + intensityPath);

            bool is16;
            var zRaw = LoadZRawFromFile(zPath, out is16);       // float[,] (0..255 or 0..65535)
            int H = imRaw.Rows, W = imRaw.Cols;
            H = Math.Min(H, zRaw.GetLength(0));
            W = Math.Min(W, zRaw.GetLength(1));
            var im = imRaw[0, H, 0, W];

            // 1) ROI
            Mat roi;
            if (!string.IsNullOrEmpty(roiMaskPath) && File.Exists(roiMaskPath))
            {
                roi = Cv2.ImRead(roiMaskPath, ImreadModes.Grayscale);
                if (roi.Empty()) throw new InvalidOperationException("ROI read failed: " + roiMaskPath);
                if (roi.Rows != H || roi.Cols != W) Cv2.Resize(roi, roi, new OpenCvSharp.Size(W, H), 0, 0, InterpolationFlags.Nearest);
                Cv2.Threshold(roi, roi, 127, 255, ThresholdTypes.Binary);
            }
            else
            {
                // validZ 만들고, 그 중 가장 큰 연결성분만 ROI로 사용
                var valid2 = new Mat(H, W, MatType.CV_8UC1);
                unsafe
                {
                    byte* vp = (byte*)valid2.Data;
                    for (int y = 0; y < H; y++)
                        for (int x = 0; x < W; x++)
                        {
                            float v = zRaw[y, x];
                            bool ok = is16 ? (v != p.InvalidZ16) : (v != p.InvalidZ);
                            vp[y * valid2.Step() + x] = ok ? (byte)255 : (byte)0;
                        }
                }
                // 구멍 메우기 + 외곽 정리
                var k5 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
                Cv2.MorphologyEx(valid2, valid2, MorphTypes.Close, k5);
                Cv2.MorphologyEx(valid2, valid2, MorphTypes.Open, k5);

                // 가장 큰 외곽선만 남김
                Cv2.FindContours(valid2, out var cnts2, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                roi = new Mat(H, W, MatType.CV_8UC1, Scalar.All(0));
                if (cnts2.Length > 0)
                {
                    int best = 0; double bestA = 0;
                    for (int i = 0; i < cnts2.Length; i++)
                    {
                        double a = Cv2.ContourArea(cnts2[i]);
                        if (a > bestA) { bestA = a; best = i; }
                    }
                    Cv2.DrawContours(roi, new[] { cnts2[best] }, -1, Scalar.All(255), thickness: -1);
                }
            }
            // 경계 오검출 완화: ROI를 살짝 안쪽으로
            Cv2.Erode(roi, roi, Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3)));


            // 2) valid Z mask
            var valid = new Mat(H, W, MatType.CV_8UC1);
            unsafe
            {
                byte* vp = (byte*)valid.Data;
                for (int y = 0; y < H; y++)
                {
                    for (int x = 0; x < W; x++)
                    {
                        float v = zRaw[y, x];
                        bool ok = is16 ? (v != p.InvalidZ16) : (v != p.InvalidZ);
                        vp[y * valid.Step() + x] = ok ? (byte)255 : (byte)0;
                    }
                }
            }

            // 3) hole = ROI AND (NOT valid)
            var hole = new Mat();
            var notValid = new Mat();
            Cv2.BitwiseNot(valid, notValid);
            Cv2.BitwiseAnd(notValid, roi, hole);

            // 4) 소거/보정 (열기 후 닫기)
            var k3 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.MorphologyEx(hole, hole, MorphTypes.Open, k3);
            Cv2.MorphologyEx(hole, hole, MorphTypes.Close, k3);

            // 5) 연결요소/면적 필터
            var labels = new Mat();
            var stats = new Mat();
            var cents = new Mat();
            int n = Cv2.ConnectedComponentsWithStats(hole, labels, stats, cents, PixelConnectivity.Connectivity8, MatType.CV_32S);

            // 픽셀->mm²
            double pxAreaToMm2 = p.Sx * p.Sy;
            var keepMask = new Mat(H, W, MatType.CV_8UC1, Scalar.All(0));
            var comps = new List<HoleComponent>();

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

                // 라벨 픽셀 차집합으로 keepMask 구성
                var eq = labels.InRange(i, i);
                Cv2.BitwiseOr(keepMask, eq, keepMask);

                var cx = cents.Get<double>(i, 0);
                var cy = cents.Get<double>(i, 1);

                comps.Add(new HoleComponent
                {
                    Label = i,
                    AreaPx = areaPx,
                    AreaMm2 = areaMm2,
                    BBox = bb,
                    CentroidPx = new OpenCvSharp.Point((int)Math.Round(cx), (int)Math.Round(cy))
                });
            }

            // 최종 hole 마스크(필터링 반영)
            hole = keepMask;

            // 6) 컨투어(외곽선) 추출
            var contours = new List<OpenCvSharp.Point[]>();
            Cv2.FindContours(hole, out var cnts, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            contours.AddRange(cnts);

            // 7) 2D 오버레이 (선택)
            Bitmap overlayBmp = null;
            if (drawOverlay)
            {
                // im → imColor 를 반드시 8UC3(BGR)로 통일
                Mat imColor;
                {
                    Mat base8;
                    if (im.Type() != MatType.CV_8UC1 && im.Type() != MatType.CV_8UC3 && im.Type() != MatType.CV_8UC4)
                    {
                        double scale = (im.Type() == MatType.CV_16UC1) ? 1.0 / 256.0 : 1.0;
                        base8 = new Mat();
                        im.ConvertTo(base8, MatType.CV_8U, scale);
                    }
                    else
                    {
                        base8 = im;
                    }

                    if (base8.Channels() == 1)           // GRAY -> BGR
                        Cv2.CvtColor(base8, imColor = new Mat(), ColorConversionCodes.GRAY2BGR);
                    else if (base8.Channels() == 4)      // BGRA -> BGR
                        Cv2.CvtColor(base8, imColor = new Mat(), ColorConversionCodes.BGRA2BGR);
                    else                                  // 3채널이면 그대로
                        imColor = base8.Clone();
                }

                // fill 은 반드시 imColor 와 동일 타입/크기
                var fill = new Mat(imColor.Size(), imColor.Type(), new Scalar(0, 0, 255)); // BGR 빨강

                // 1) 전체 블렌드 이미지 생성
                var blended = new Mat();
                Cv2.AddWeighted(imColor, 1.0, fill, 0.35, 0, blended);   // << 타입/채널 동일하므로 OK

                // 2) hole 내부만 덮어쓰기 (hole: 8UC1, 0/255)
                blended.CopyTo(imColor, hole);

                // 외곽선 + 면적 라벨
                foreach (var c in contours)
                    Cv2.Polylines(imColor, new[] { c }, true, new Scalar(0, 255, 255), 2);

                foreach (var comp in comps)
                {
                    string txt = comp.AreaMm2.ToString("F1", CultureInfo.InvariantCulture) + " mm^2";
                    Cv2.PutText(imColor, txt, comp.CentroidPx, HersheyFonts.HersheySimplex, 0.5,
                                new Scalar(0, 0, 0), 2, LineTypes.AntiAlias);
                    Cv2.PutText(imColor, txt, comp.CentroidPx, HersheyFonts.HersheySimplex, 0.5,
                                new Scalar(0, 255, 255), 1, LineTypes.AntiAlias);
                }

                overlayBmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imColor);
            }

            return new HoleDetectionResult
            {
                HoleMask = hole,
                ContoursPx = contours,
                Components = comps,
                Overlay2D = overlayBmp
            };
        }

        /// <summary>
        /// 2D 컨투어를 3D 라인 루프로 변환 (Z는 이웃 유효 Z 평균 샘플링)
        /// </summary>
        public static List<System.Windows.Media.Media3D.Point3D[]> Make3DContourLoops(
            List<OpenCvSharp.Point[]> contoursPx,
            float[,] zRaw,
            FusionParams p,
            bool is16bit,
            int neighbor = 2)
        {
            int H = zRaw.GetLength(0), W = zRaw.GetLength(1);
            double cx = p.CenterOrigin ? W / 2.0 : 0.0;
            double cy = p.CenterOrigin ? H / 2.0 : 0.0;

            var loops = new List<System.Windows.Media.Media3D.Point3D[]>();

            foreach (var c in contoursPx)
            {
                var loop = new System.Windows.Media.Media3D.Point3D[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    int x = ClampInt(c[i].X, 0, W - 1);
                    int y = ClampInt(c[i].Y, 0, H - 1);

                    // 주변 유효 Z 평균
                    double sum = 0; int cnt = 0;
                    for (int dy = -neighbor; dy <= neighbor; dy++)
                        for (int dx = -neighbor; dx <= neighbor; dx++)
                        {
                            int xx = x + dx, yy = y + dy;
                            if (xx < 0 || yy < 0 || xx >= W || yy >= H) continue;
                            float v = zRaw[yy, xx];
                            bool ok = is16bit ? (v != p.InvalidZ16) : (v != p.InvalidZ);
                            if (ok) { sum += v; cnt++; }
                        }
                    double zVal = (cnt > 0) ? (sum / cnt) : (is16bit ? p.InvalidZ16 : p.InvalidZ);

                    double X = (x - cx) * p.Sx;
                    double Y = -(y - cy) * p.Sy;
                    double Z = p.ZOffset + p.ZScale * zVal;
                    loop[i] = new System.Windows.Media.Media3D.Point3D(X, Y, Z);
                }
                loops.Add(loop);
            }
            return loops;
        }

        // HelixToolkit 메쉬를 반환하도록 명시
        public static HT.MeshGeometry3D[] Make3DFilledMeshes(
            List<OpenCvSharp.Point[]> contoursPx,
            float[,] zRaw,
            FusionParams p,
            bool is16bit,
            int neighbor = 2,
            double approxEpsPx = 1.5)
        {
            if (contoursPx == null || contoursPx.Count == 0) return Array.Empty<HT.MeshGeometry3D>();

            int H = zRaw.GetLength(0), W = zRaw.GetLength(1);
            double cx = p.CenterOrigin ? W / 2.0 : 0.0;
            double cy = p.CenterOrigin ? H / 2.0 : 0.0;

            var list = new List<HT.MeshGeometry3D>();

            foreach (var c0 in contoursPx)
            {
                if (c0 == null || c0.Length < 3) continue;

                // OpenCvSharp 올바른 API 형태 (out 매개변수 X)
                var c = (approxEpsPx > 0) ? Cv2.ApproxPolyDP(c0, approxEpsPx, true) : c0;
                if (c.Length < 3) continue;

                var tris = TriangulateSimplePolygon(c); // List<int[3]>

                var positions = new HT.Vector3Collection();
                var indices = new HT.IntCollection();
                var idxMap = new Dictionary<int, int>();

                int Map(int i)
                {
                    if (idxMap.TryGetValue(i, out var outIdx)) return outIdx;

                    int x = Math.Max(0, Math.Min(W - 1, c[i].X));
                    int y = Math.Max(0, Math.Min(H - 1, c[i].Y));

                    double zVal = SampleZWithNeighbor(zRaw, x, y, is16bit ? p.InvalidZ16 : p.InvalidZ, neighbor);
                    double X = (x - cx) * p.Sx;
                    double Y = -(y - cy) * p.Sy;
                    double Z = p.ZOffset + p.ZScale * zVal;

                    outIdx = positions.Count;
                    positions.Add(new SharpDX.Vector3((float)X, (float)Y, (float)Z));
                    idxMap[i] = outIdx;
                    return outIdx;
                }

                foreach (var t in tris)
                {
                    int i0 = Map(t[0]), i1 = Map(t[1]), i2 = Map(t[2]);
                    indices.Add(i0); indices.Add(i1); indices.Add(i2);
                }

                list.Add(new HT.MeshGeometry3D { Positions = positions, Indices = indices });
            }

            return list.ToArray();
        }


        // ====== 삼각분할 & Z 샘플 헬퍼 ======

        private static List<int[]> TriangulateSimplePolygon(OpenCvSharp.Point[] poly)
        {
            int n = poly.Length;
            var idx = Enumerable.Range(0, n).ToList(); // ← using System.Linq 필요
            var tris = new List<int[]>();

            if (SignedArea(poly) < 0) idx.Reverse(); // CW면 CCW로

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

        private static double SignedArea(OpenCvSharp.Point[] poly)
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

        private static bool IsConvex(OpenCvSharp.Point a, OpenCvSharp.Point b, OpenCvSharp.Point c)
        {
            long cross = (long)(b.X - a.X) * (c.Y - a.Y) - (long)(b.Y - a.Y) * (c.X - a.X);
            return cross > 0; // CCW
        }

        private static bool PointInTriangle(OpenCvSharp.Point p, OpenCvSharp.Point a, OpenCvSharp.Point b, OpenCvSharp.Point c)
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

        private static double SampleZWithNeighbor(float[,] zRaw, int x, int y, float invalidZ, int neighbor)
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


        private static int ClampInt(int v, int lo, int hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }




    }
}
