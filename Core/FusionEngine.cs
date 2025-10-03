// FusionEngine.cs  (C# 7.3)
// NuGet: OpenCvSharp4, OpenCvSharp4.runtime.win, HelixToolkit.Wpf.SharpDX

using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using OpenCvSharp;

using HelixToolkit.Wpf.SharpDX;
using SharpDX; // Vector3, Color4

using System.Windows.Media.Media3D;
using MediaColor = System.Windows.Media.Color;
using SDRect = System.Drawing.Rectangle;   // 충돌 방지: System.Drawing.Rectangle 별칭

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

    public static class FusionEngine
    {
        /// <summary>
        /// Intensity(원본 색) / Z 경로를 받아 포인트클라우드 생성.
        /// 색은 Intensity 이미지의 **원본 픽셀 색**을 그대로 사용한다.
        /// </summary>
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
        /// HelixToolkit PointGeometryModel3D로 변환 (버텍스 컬러 그대로 표시)
        /// </summary>
        public static PointGeometryModel3D ToPointModel(PointCloudResult pc, double pointSize = 1.5)
        {
            var positions = new Vector3Collection(pc.Points.Select(p => new Vector3((float)p.X, (float)p.Y, (float)p.Z)));
            var colors = new Color4Collection(pc.Colors.Select(c => new Color4(c.R / 255f, c.G / 255f, c.B / 255f, 1f)));

            var geom = new PointGeometry3D
            {
                Positions = positions,
                Colors = colors
            };

            var model = new PointGeometryModel3D
            {
                Geometry = geom,
                IsHitTestVisible = false
            };

            SetPointSize(model, pointSize);

            var oitProp = typeof(PointGeometryModel3D).GetProperty("EnableOIT");
            if (oitProp != null && oitProp.CanWrite) oitProp.SetValue(model, true, null);

            return model;
        }

        private static void SetPointSize(PointGeometryModel3D model, double s)
        {
            var sizeProp = model.GetType().GetProperty("Size");
            if (sizeProp != null)
            {
                var t = sizeProp.PropertyType;
                if (t == typeof(double)) sizeProp.SetValue(model, s, null);
                else if (t == typeof(float)) sizeProp.SetValue(model, (float)s, null);
                else if (t == typeof(System.Windows.Size)) sizeProp.SetValue(model, new System.Windows.Size(s, s), null);
                return;
            }
            var pointSizeProp = model.GetType().GetProperty("PointSize");
            if (pointSizeProp != null)
            {
                if (pointSizeProp.PropertyType == typeof(double)) pointSizeProp.SetValue(model, s, null);
                else if (pointSizeProp.PropertyType == typeof(float)) pointSizeProp.SetValue(model, (float)s, null);
            }
        }

        // ---------- OpenCV 로더 ----------

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

        // ---------- Helpers (현재 색에는 사용하지 않지만 남겨둠) ----------

        private static float[,] PercentileNormalize(float[,] zRaw, float loPct, float hiPct)
        {
            int h = zRaw.GetLength(0), w = zRaw.GetLength(1);
            int n = w * h;
            var buf = new float[n];
            int k = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    buf[k++] = zRaw[y, x];

            Array.Sort(buf);
            float lo = Percentile(buf, loPct);
            float hi = Percentile(buf, hiPct);
            float denom = Math.Max(1e-6f, hi - lo);

            var outZ = new float[h, w];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float t = (zRaw[y, x] - lo) / denom;
                    outZ[y, x] = Clamp01(t);
                }
            return outZ;
        }

        private static float Percentile(float[] sortedAsc, float p)
        {
            if (sortedAsc == null || sortedAsc.Length == 0) return 0f;
            double rank = (p / 100.0) * (sortedAsc.Length - 1);
            int i = (int)Math.Floor(rank);
            double frac = rank - i;
            if (i + 1 < sortedAsc.Length)
                return (float)(sortedAsc[i] * (1 - frac) + sortedAsc[i + 1] * frac);
            return sortedAsc[sortedAsc.Length - 1];
        }

        private static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }

        // (참고) GDI+ 16bpp Gray 로더 예시
        private static float[,] ReadZRaw(Bitmap z, out bool is16bit)
        {
            int w = z.Width, h = z.Height;
            int pfBits = Image.GetPixelFormatSize(z.PixelFormat);
            is16bit = (pfBits == 16);

            if (!is16bit)
            {
                var a = new float[h, w];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        a[y, x] = z.GetPixel(x, y).R;
                return a;
            }
            else
            {
                var a = new float[h, w];
                var data = z.LockBits(new SDRect(0, 0, w, h), ImageLockMode.ReadOnly, z.PixelFormat);
                try
                {
                    int stride = data.Stride;
                    IntPtr scan0 = data.Scan0;
                    int bytesPerPixel = 2;

                    for (int y = 0; y < h; y++)
                    {
                        IntPtr rowPtr = scan0 + y * stride;
                        for (int x = 0; x < w; x++)
                        {
                            ushort v = (ushort)Marshal.ReadInt16(rowPtr, x * bytesPerPixel);
                            a[y, x] = v;
                        }
                    }
                }
                finally
                {
                    z.UnlockBits(data);
                }
                return a;
            }
        }
    }
}
