using System;
using SharpDX;
using System.IO;
using System.Linq;
using OpenCvSharp;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media.Media3D;
using HT = HelixToolkit.Wpf.SharpDX;
using MediaColor = System.Windows.Media.Color;

namespace _3D_VisionSource
{
    public class InspectionParams
    {
        public float Sx = 0.05f;
        public float Sy = 0.05f;
        public float ZScale = 0.001f;
        public float ZOffset = 0f;
        public byte InvalidZ = 0;
        public ushort InvalidZ16 = 0;
        public bool CenterOrigin = true;
        public double MinAreaMm2 = 3.0;
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
        /// Z맵을 float[,]로 로드(CV_8UC1/CV_16UC1 지원)
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
                            outArr[y, x] = idx[y, x];
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
                            outArr[y, x] = idx[y, x];
                    return outArr;
                }
                throw new NotSupportedException("ZMap must be CV_8UC1 or CV_16UC1.");
            }
        }
        /// Intensity/ZMap에서 포인트클라우드 생성 + 홀검출 + 2D 오버레이 생성
        public static InspectionResults Inspect(string intensityPath, string zPath, string roiMaskPath = null, bool drawOverlay = true)
        {
            var imRaw = Cv2.ImRead(intensityPath, ImreadModes.Unchanged);
            if (imRaw.Empty()) throw new InvalidOperationException("Intensity read failed: " + intensityPath);

            bool is16;
            var zRaw = LoadZRawFromFile(zPath, out is16);

            int H = Math.Min(imRaw.Rows, zRaw.GetLength(0));
            int W = Math.Min(imRaw.Cols, zRaw.GetLength(1));
            var im = imRaw[0, H, 0, W];

            var mask = new bool[H, W];
            int validCount = 0;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float v = zRaw[y, x];
                    bool ok = is16 ? (v != P.InvalidZ16) : (v != P.InvalidZ);
                    mask[y, x] = ok;
                    if (ok) validCount++;
                }
            if (validCount == 0) throw new InvalidOperationException("No valid Z.");

            double cx = P.CenterOrigin ? W / 2.0 : 0.0;
            double cy = P.CenterOrigin ? H / 2.0 : 0.0;

            var pts = new Point3D[validCount];
            var cols = new MediaColor[validCount];

            int ch = im.Channels();
            Mat im8;
            if (ch == 1)
            {
                if (im.Type() == MatType.CV_8UC1) im8 = im;
                else
                {
                    im8 = new Mat();
                    if (im.Type() == MatType.CV_16UC1) im.ConvertTo(im8, MatType.CV_8U, 1.0 / 256.0);
                    else im.ConvertTo(im8, MatType.CV_8U);
                }
            }
            else if (ch == 3 || ch == 4)
            {
                im8 = im;
            }
            else
            {
                im8 = new Mat();
                im.ConvertTo(im8, MatType.CV_8UC3);
                ch = 3;
            }

            var idx1 = (ch == 1) ? im8.GetGenericIndexer<byte>() : null;
            var idx3 = (ch == 3) ? im8.GetGenericIndexer<Vec3b>() : null;
            var idx4 = (ch == 4) ? im8.GetGenericIndexer<Vec4b>() : null;

            int k = 0;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    if (!mask[y, x]) continue;

                    double X = (x - cx) * P.Sx;
                    double Y = -(y - cy) * P.Sy;
                    double Z = P.ZOffset + P.ZScale * zRaw[y, x];
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
                        byte g = (byte)Math.Min(255, (int)Math.Round(0.114 * bgr.Item0 + 0.587 * bgr.Item1 + 0.299 * bgr.Item2));
                        R = g; G = g; B = g;
                    }
                    else
                    {
                        var bgra = idx4[y, x];
                        byte g = (byte)Math.Min(255, (int)Math.Round(0.114 * bgra.Item0 + 0.587 * bgra.Item1 + 0.299 * bgra.Item2));
                        R = g; G = g; B = g;
                    }
                    cols[k] = MediaColor.FromRgb(R, G, B);
                    k++;
                }

            Mat roi = BuildRoiAutoOrFromMask(zRaw, H, W, is16, roiMaskPath);

            Cv2.Erode(roi, roi, Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3)));

            var valid = new Mat(H, W, MatType.CV_8UC1);
            unsafe
            {
                byte* vp = (byte*)valid.Data;
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        float v = zRaw[y, x];
                        bool ok = is16 ? (v != P.InvalidZ16) : (v != P.InvalidZ);
                        vp[y * valid.Step() + x] = ok ? (byte)255 : (byte)0;
                    }
            }

            var hole = new Mat();
            var notValid = new Mat();
            Cv2.BitwiseNot(valid, notValid);
            Cv2.BitwiseAnd(notValid, roi, hole);

            var k3 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.MorphologyEx(hole, hole, MorphTypes.Open, k3);
            Cv2.MorphologyEx(hole, hole, MorphTypes.Close, k3);

            var labels = new Mat();
            var stats = new Mat();
            var cents = new Mat();
            int n = Cv2.ConnectedComponentsWithStats(hole, labels, stats, cents, PixelConnectivity.Connectivity8, MatType.CV_32S);

            double pxAreaToMm2 = P.Sx * P.Sy;
            var keepMask = new Mat(H, W, MatType.CV_8UC1, Scalar.All(0));

            var compLabels = new List<int>();
            var compAreaPx = new List<int>();
            var compAreaMm2 = new List<double>();
            var compBBox = new List<OpenCvSharp.Rect>();
            var compCentroidPx = new List<OpenCvSharp.Point>();

            for (int i = 1; i < n; i++)
            {
                int areaPx = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area);
                double areaMm2 = areaPx * pxAreaToMm2;
                if (areaMm2 < P.MinAreaMm2) continue;

                int x = stats.Get<int>(i, (int)ConnectedComponentsTypes.Left);
                int y = stats.Get<int>(i, (int)ConnectedComponentsTypes.Top);
                int w = stats.Get<int>(i, (int)ConnectedComponentsTypes.Width);
                int h = stats.Get<int>(i, (int)ConnectedComponentsTypes.Height);
                var bb = new OpenCvSharp.Rect(x, y, w, h);

                var eq = labels.InRange(i, i);
                Cv2.BitwiseOr(keepMask, eq, keepMask);

                var cxPx = cents.Get<double>(i, 0);
                var cyPx = cents.Get<double>(i, 1);

                compLabels.Add(i);
                compAreaPx.Add(areaPx);
                compAreaMm2.Add(areaMm2);
                compBBox.Add(bb);
                compCentroidPx.Add(new OpenCvSharp.Point((int)Math.Round(cxPx), (int)Math.Round(cyPx)));
            }

            hole = keepMask;

            var contours = new List<OpenCvSharp.Point[]>();
            Cv2.FindContours(hole, out var cnts, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            contours.AddRange(cnts);

            Bitmap overlayBmp = null;
            if (drawOverlay)
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

                var fill = new Mat(imColor.Size(), imColor.Type(), new Scalar(0, 0, 255));
                var blended = new Mat();
                Cv2.AddWeighted(imColor, 1.0, fill, P.OverlayAlpha, 0, blended);
                blended.CopyTo(imColor, hole);

                foreach (var c in contours) Cv2.Polylines(imColor, new[] { c }, true, new Scalar(0, 255, 255), 2);

                for (int i = 0; i < compLabels.Count; i++)
                {
                    string txt = compAreaMm2[i].ToString("F1", CultureInfo.InvariantCulture) + " mm^2";
                    Cv2.PutText(imColor, txt, compCentroidPx[i], HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 0), 2, LineTypes.AntiAlias);
                    Cv2.PutText(imColor, txt, compCentroidPx[i], HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 255, 255), 1, LineTypes.AntiAlias);
                }

                overlayBmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imColor);
            }

            return new InspectionResults
            {
                Points = pts,
                Colors = cols,
                Width = W,
                Height = H,
                HoleMask = hole,
                ContoursPx = contours,
                CompLabels = compLabels,
                CompAreaPx = compAreaPx,
                CompAreaMm2 = compAreaMm2,
                CompBBox = compBBox,
                CompCentroidPx = compCentroidPx,
                Overlay2D = overlayBmp
            };
        }
        /// 2D 컨투어를 3D 라인 루프로 변환
        public static List<Point3D[]> Make3DContourLoops(InspectionResults res, float[,] zRaw, bool is16bit, int neighbor = 2)
        {
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
                    double zVal = SampleZWithNeighbor(zRaw, x, y, is16bit ? P.InvalidZ16 : P.InvalidZ, neighbor);
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
        private static OpenCvSharp.Mat BuildRoiAutoOrFromMask(float[,] zRaw, int H, int W, bool is16bit, string roiMaskPath)
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
                {
                    for (int x = 0; x < W; x++)
                    {
                        float v = zRaw[y, x];
                        bool ok = is16bit ? (v != (float)P.InvalidZ16) : (v != (float)P.InvalidZ);
                        vp[y * valid.Step() + x] = ok ? (byte)255 : (byte)0;
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
        /// 결손 영역을 3D 채움 메쉬로 생성
        public static HT.MeshGeometry3D[] Make3DFilledMeshes(InspectionResults res, float[,] zRaw, bool is16bit, int neighbor = 2, double approxEpsPx = 1.5)
        {
            if (res == null || res.ContoursPx == null || res.ContoursPx.Count == 0) return new HT.MeshGeometry3D[0];

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
                    int i0 = MapIndex(c, tri[0], map, positions, W, H, cx, cy, zRaw, is16bit, neighbor);
                    int i1 = MapIndex(c, tri[1], map, positions, W, H, cx, cy, zRaw, is16bit, neighbor);
                    int i2 = MapIndex(c, tri[2], map, positions, W, H, cx, cy, zRaw, is16bit, neighbor);
                    indices.Add(i0); indices.Add(i1); indices.Add(i2);
                }

                list.Add(new HT.MeshGeometry3D { Positions = positions, Indices = indices });
            }

            return list.ToArray();
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
