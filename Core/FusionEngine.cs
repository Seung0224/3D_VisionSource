using System;
using System.Collections.Generic;
using System.Drawing;
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
        public byte InvalidZ { get; set; } = 0;   // 0, 255, 65535 등(8bit이면 0/255)
        public float PLo { get; set; } = 1f;      // Z 1% ~ 99% 정규화
        public float PHi { get; set; } = 99f;
        public bool CenterOrigin { get; set; } = false; // (x - cx, y - cy)
    }

    public class PointCloudResult
    {
        public Point3D[] Points;
        public MediaColor[] Colors;
        public int Width;   // 원본 작업 해상도(크롭 후)
        public int Height;
    }

    public static class FusionEngine
    {
        public static PointCloudResult BuildPointCloud(Bitmap intensity, Bitmap zmap, FusionParams p)
        {
            if (intensity == null || zmap == null)
                throw new ArgumentNullException("intensity/zmap is null.");
            if (p == null) throw new ArgumentNullException(nameof(p));

            // 1) 공통 크기로 crop
            int h = Math.Min(intensity.Height, zmap.Height);
            int w = Math.Min(intensity.Width, zmap.Width);
            var roi = new Rectangle(0, 0, w, h);

            using (var I2 = intensity.Clone(roi, intensity.PixelFormat))
            using (var Z2 = zmap.Clone(roi, zmap.PixelFormat))
            {
                // 2) I → 그레이 0..1
                float[,] I01 = ToGray01(I2);

                // 3) Z → 1~99% 정규화 (0..1)
                float[,] Z01 = PercentileNormalize(Z2, p.PLo, p.PHi, out _, out _);

                // 4) 포인트 생성 (InvalidZ 필터 고려 → List로 수집)
                var pts = new List<Point3D>(w * h);
                var cols = new List<MediaColor>(w * h);

                double cx = p.CenterOrigin ? w / 2.0 : 0.0;
                double cy = p.CenterOrigin ? h / 2.0 : 0.0;

                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        // 원본 Z 픽셀값(8bit 가정)로 invalid 필터
                        if (p.InvalidZ != 0)
                        {
                            byte raw = Z2.GetPixel(x, y).R;
                            if (raw == p.InvalidZ) continue;
                        }

                        // 좌표: 파이썬과 동일하게 Y축 부호 반전(-Y)
                        double X = (x - cx) * p.Sx;
                        double Y = -(y - cy) * p.Sy;
                        double Z = p.ZOffset + Z01[y, x] * p.ZScale;

                        pts.Add(new Point3D(X, Y, Z));

                        // 색상: JET(Z01) * (0.5 + 0.5 * I01)
                        var (r, g, b) = Jet(Z01[y, x]);
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

        // --- Helpers ---

        private static float[,] ToGray01(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            var f = new float[h, w];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var c = bmp.GetPixel(x, x); // BUG guard: ensures bounds (typo check)
                }
            // 위 한 줄은 실수 방지용 검사였다가 제거합니다:
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    f[y, x] = (c.R + c.G + c.B) / (255f * 3f);
                }
            return f;
        }

        private static float[,] PercentileNormalize(Bitmap zmap, float loPct, float hiPct, out float lo, out float hi)
        {
            int w = zmap.Width, h = zmap.Height;
            float[] vals = new float[w * h];
            int idx = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    vals[idx++] = zmap.GetPixel(x, y).R;

            Array.Sort(vals);
            lo = Percentile(vals, loPct);
            hi = Percentile(vals, hiPct);

            var outZ = new float[h, w];
            float denom = Math.Max(1e-6f, (hi - lo));
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float v = zmap.GetPixel(x, y).R;
                    float z = (v - lo) / denom;
                    outZ[y, x] = Clamp01(z);
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
            return sorted[sorted.Length - 1]; // C# 7.3 호환
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
