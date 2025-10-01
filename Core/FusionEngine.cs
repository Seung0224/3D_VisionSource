using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using MediaColor = System.Windows.Media.Color;
using System.Windows.Media.Media3D;

namespace _3D_VisionSource
{
    public class FusionParams
    {
        public float Sx { get; set; } = 0.05f;
        public float Sy { get; set; } = 0.05f;
        public float ZScale { get; set; } = 0.001f;
        public float ZOffset { get; set; } = 0f;
        public byte InvalidZ { get; set; } = 0;      // 8-bit 무효
        public ushort InvalidZ16 { get; set; } = 0;  // 16-bit 무효
        public float PLo { get; set; } = 1f;
        public float PHi { get; set; } = 99f;
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
        public static PointCloudResult BuildPointCloud(Bitmap intensity, Bitmap zmap, FusionParams p)
        {
            if (intensity == null || zmap == null) throw new ArgumentNullException();
            if (p == null) throw new ArgumentNullException(nameof(p));

            int h = Math.Min(intensity.Height, zmap.Height);
            int w = Math.Min(intensity.Width, zmap.Width);
            var roi = new Rectangle(0, 0, w, h);

            using (var I2 = intensity.Clone(roi, intensity.PixelFormat))
            using (var Z2 = zmap.Clone(roi, zmap.PixelFormat))
            {
                var I01 = ToGray01(I2);
                var zRaw = ReadZRaw(Z2, out bool is16bit);

                var mask = new bool[h, w];
                int validCount = 0;
                if (is16bit)
                {
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                        {
                            ushort v = (ushort)zRaw[y, x];
                            bool ok = v != p.InvalidZ16;
                            mask[y, x] = ok;
                            if (ok) validCount++;
                        }
                }
                else
                {
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                        {
                            byte v = (byte)zRaw[y, x];
                            bool ok = v != p.InvalidZ;
                            mask[y, x] = ok;
                            if (ok) validCount++;
                        }
                }
                if (validCount == 0) throw new InvalidOperationException("유효한 Z 포인트가 없습니다.");

                var pts = new List<Point3D>(validCount);
                var cols = new List<MediaColor>(validCount);

                double cx = p.CenterOrigin ? w / 2.0 : 0.0;
                double cy = p.CenterOrigin ? h / 2.0 : 0.0;

                var Zn = PercentileNormalize(zRaw, p.PLo, p.PHi); // 색상용

                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        if (!mask[y, x]) continue;

                        double X = (x - cx) * p.Sx;
                        double Y = -(y - cy) * p.Sy; // 파이썬과 동일하게 -Y
                        double Z = p.ZOffset + zRaw[y, x] * p.ZScale; // 지오메트리는 raw 사용

                        pts.Add(new Point3D(X, Y, Z));

                        var (r, g, b) = Jet(Clamp01(Zn[y, x]));
                        float wgt = 0.5f + 0.5f * I01[y, x];
                        byte R = (byte)Math.Round(Clamp01(r * wgt) * 255);
                        byte G = (byte)Math.Round(Clamp01(g * wgt) * 255);
                        byte B = (byte)Math.Round(Clamp01(b * wgt) * 255);
                        cols.Add(MediaColor.FromRgb(R, G, B));
                    }

                return new PointCloudResult
                {
                    Points = pts.ToArray(),
                    Colors = cols.ToArray(),
                    Width = w,
                    Height = h
                };
            }
        }

        private static float[,] ToGray01(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            var f = new float[h, w];

            // 가능한 빠르게 처리하려면 LockBits 권장(여기는 간단 버전)
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    f[y, x] = (c.R + c.G + c.B) / (255f * 3f);
                }
            return f;
        }

        // zRaw: 8bit면 [0..255], 16bit면 [0..65535]
        private static float[,] ReadZRaw(Bitmap z, out bool is16bit)
        {
            int w = z.Width, h = z.Height;
            var pfBits = Image.GetPixelFormatSize(z.PixelFormat);
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
                var data = z.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, z.PixelFormat);
                try
                {
                    int stride = data.Stride;
                    IntPtr scan0 = data.Scan0;
                    int bytesPerPixel = 2; // 16bpp Gray

                    for (int y = 0; y < h; y++)
                    {
                        IntPtr rowPtr = scan0 + y * stride;
                        for (int x = 0; x < w; x++)
                        {
                            ushort v = (ushort)Marshal.ReadInt16(rowPtr, x * bytesPerPixel);
                            a[y, x] = v; // 0..65535
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

        // 입력: zRaw(8/16bit 범위). 출력: 1~99 분위수로 0..1 정규화
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

        private static float Percentile(float[] sorted, float p)
        {
            if (sorted == null || sorted.Length == 0) return 0f;
            double rank = (p / 100.0) * (sorted.Length - 1);
            int i = (int)Math.Floor(rank);
            double frac = rank - i;
            if (i + 1 < sorted.Length)
                return (float)(sorted[i] * (1 - frac) + sorted[i + 1] * frac);
            return sorted[sorted.Length - 1];
        }

        private static (float r, float g, float b) Jet(float t)
        {
            t = Clamp01(t);
            float r = Clamp01(1.5f - Math.Abs(4 * t - 3));
            float g = Clamp01(1.5f - Math.Abs(4 * t - 2));
            float b = Clamp01(1.5f - Math.Abs(4 * t - 1));
            return (r, g, b);
        }

        private static float Clamp01(float v) => v < 0 ? 0 : (v > 1 ? 1 : v);
    }
}
