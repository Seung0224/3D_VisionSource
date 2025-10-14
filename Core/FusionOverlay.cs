// FusionOverlay.cs  — 2D/3D Overlay 전담 (그리기 담당), 엔진은 계산만 수행
using _3D_VisionSource.Viewer;
using Cyotek.Windows.Forms;
using HelixToolkit.Wpf.SharpDX;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using HT = HelixToolkit.Wpf.SharpDX;
using Media3D = System.Windows.Media.Media3D;

namespace _3D_VisionSource
{
    public static class FusionOverlay
    {
        // ==== 기본값(표시 전용 상수) ====
        private const int NEIGHBOR_RADIUS = 2;            // 경계 보간 반경(삼각화 꼭짓점 Z 샘플링)
        private const double APPROX_EPS_PX = 1.5;         // 컨투어 단순화 허용 오차(px)
        private const double POINT_SIZE = 3.0;            // 포인트 사이즈
        private const float MESH_OPACITY = 0.35f;         // 메시 불투명도
        private static readonly System.Windows.Media.Color MESH_COLOR = System.Windows.Media.Colors.Red;

        /// <summary>
        /// 한 번 호출로 2D/3D 모두 렌더. (2D는 intensityMat 기반으로 즉석 생성)
        /// </summary>
        public static void Render(
            InspectionResults res,
            float[,] zRaw,
            Viewer3DControl viewer,
            ImageBox imageBox,
            InspectionParams p,
            Mat intensityMat,
            Viewer3DControl.ViewPreset preset = Viewer3DControl.ViewPreset.Front)
        {
            if (res == null) return;

            // ----- 2D : intensity + 결과 -> Overlay 비트맵 생성 -----
            if (imageBox != null && intensityMat != null)
            {
                var overlayBmp = BuildOverlay2DFromIntensity(intensityMat, res, overlayAlpha: p.OverlayAlpha);
                Apply2D(imageBox, overlayBmp);
            }

            // ----- 3D : 포인트 + 선택 메시 렌더 -----
            if (viewer != null && res.Points != null && res.Colors != null)
            {
                var meshes = Make3DFilledMeshes(
                    res, zRaw,
                    sx: p.Sx, sy: p.Sy, zScale: p.ZScale, zOffset: p.ZOffset,
                    centerOrigin: p.CenterOrigin, invalidZ: p.InvalidZ,
                    neighbor: NEIGHBOR_RADIUS, approxEpsPx: APPROX_EPS_PX);

                Render3D(
                    viewer,
                    pts: res.Points,
                    cols: res.Colors,
                    meshes: meshes,
                    preset: preset,
                    pointSize: POINT_SIZE,
                    meshColor: MESH_COLOR,
                    meshOpacity: MESH_OPACITY);
            }
            else
            {
                viewer?.ClearScene();
            }
        }

        // ===================== 2D =====================
        public static void Apply2D(ImageBox box, Bitmap overlay2D)
        {
            if (box == null) return;
            box.Image = overlay2D;
            if (overlay2D != null) box.ZoomToFit();
        }

        /// <summary>
        /// Intensity Mat과 Inspect 결과(마스크/컨투어/컴포넌트)를 이용해 Overlay 비트맵 생성
        /// </summary>
        public static Bitmap BuildOverlay2DFromIntensity(Mat im, InspectionResults res, double overlayAlpha)
        {
            if (im == null || im.Empty() || res == null) return null;

            // 1) im → 8U BGR
            Mat imColor;
            {
                Mat base8;
                if (im.Type() != MatType.CV_8UC1 && im.Type() != MatType.CV_8UC3 && im.Type() != MatType.CV_8UC4)
                {
                    // 16U 등은 8U로 스케일 다운
                    double scale = (im.Type() == MatType.CV_16UC1) ? 1.0 / 256.0 : 1.0;
                    base8 = new Mat();
                    im.ConvertTo(base8, MatType.CV_8U, scale);
                }
                else base8 = im;

                if (base8.Channels() == 1) Cv2.CvtColor(base8, imColor = new Mat(), ColorConversionCodes.GRAY2BGR);
                else if (base8.Channels() == 4) Cv2.CvtColor(base8, imColor = new Mat(), ColorConversionCodes.BGRA2BGR);
                else imColor = base8.Clone();
            }

            // 2) hole 채우기(빨간색 블렌딩)
            if (res.HoleMask != null && !res.HoleMask.Empty())
            {
                using (var fill = new Mat(imColor.Size(), imColor.Type(), new Scalar(0, 0, 255)))
                using (var blended = new Mat())
                {
                    Cv2.AddWeighted(imColor, 1.0, fill, overlayAlpha, 0, blended);
                    blended.CopyTo(imColor, res.HoleMask);
                }
            }

            // 3) 컨투어 윤곽선
            if (res.ContoursPx != null)
                foreach (var c in res.ContoursPx)
                    Cv2.Polylines(imColor, new[] { c }, true, new Scalar(0, 255, 255), 2);

            // 4) 라벨/면적 표기 (엔진이 계산한 리스트 사용)
            if (res.CompLabels != null)
            {
                int n = res.CompLabels.Count;
                for (int i = 0; i < n; i++)
                {
                    var bb = res.CompBBox[i];
                    var area = res.CompAreaMm2[i];
                    string txt = FormatAreaLabel(i + 1, area);

                    // 라벨 위치(프레임 밖으로 밀리지 않게 보정)
                    int x = bb.Right + 4;
                    int y = Math.Max(12, Math.Min(imColor.Rows - 4, bb.Top + 12));
                    if (x > imColor.Cols - 40)
                    {
                        x = Math.Max(0, bb.Left);
                        y = Math.Min(imColor.Rows - 4, bb.Bottom + 12);
                    }

                    PutLabelThin(imColor, txt, new OpenCvSharp.Point(x, y), scale: 0.4, thickness: 1);
                }
            }

            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imColor);
        }

        private static void PutLabelThin(Mat img, string text, OpenCvSharp.Point org, double scale, int thickness)
        {
            // 외곽선(검정) + 본색(노랑)
            Cv2.PutText(img, text, org, HersheyFonts.HersheySimplex, scale, new Scalar(0, 0, 0), thickness + 1, LineTypes.AntiAlias);
            Cv2.PutText(img, text, org, HersheyFonts.HersheySimplex, scale, new Scalar(0, 255, 255), thickness, LineTypes.AntiAlias);
        }

        private static string FormatAreaLabel(int idx, double areaMm2)
        {
            if (areaMm2 < 1e-3) return $"{idx}:{(areaMm2 * 1e6):F0} µm^2";
            if (areaMm2 < 1) return $"{idx}:{areaMm2:F3} mm^2";
            if (areaMm2 < 10) return $"{idx}:{areaMm2:F2} mm^2";
            return $"{idx}:{areaMm2:F1} mm^2";
        }

        // ===================== 3D =====================
        public static void Render3D(
            Viewer3DControl viewer,
            Media3D.Point3D[] pts,
            System.Windows.Media.Color[] cols,
            HT.MeshGeometry3D[] meshes,
            Viewer3DControl.ViewPreset preset,
            double pointSize,
            System.Windows.Media.Color meshColor,
            float meshOpacity)
        {
            if (viewer == null || pts == null || cols == null || pts.Length == 0 || cols.Length != pts.Length)
            {
                viewer?.ClearScene();
                return;
            }

            viewer.RenderScene(
                pts: pts,
                cols: cols,
                meshes: (meshes != null && meshes.Length > 0) ? meshes : null,
                preset: preset,
                pointSize: pointSize,
                meshColor: meshColor,
                meshOpacity: meshOpacity,
                clearBefore: true);
        }

        // ===================== Mesh Builder =====================
        public static HT.MeshGeometry3D[] Make3DFilledMeshes(
            InspectionResults res, float[,] zRaw,
            double sx, double sy, double zScale, double zOffset, bool centerOrigin, float invalidZ,
            int neighbor, double approxEpsPx)
        {
            if (res == null || res.ContoursPx == null || res.ContoursPx.Count == 0 || zRaw == null)
                return Array.Empty<HT.MeshGeometry3D>();

            int H = zRaw.GetLength(0), W = zRaw.GetLength(1);
            double cx = centerOrigin ? W / 2.0 : 0.0;
            double cy = centerOrigin ? H / 2.0 : 0.0;

            var list = new List<HT.MeshGeometry3D>(res.ContoursPx.Count);

            foreach (var contour in res.ContoursPx)
            {
                if (contour == null || contour.Length < 3) continue;

                // 다각형 단순화(옵션)
                var poly = (approxEpsPx > 0) ? Cv2.ApproxPolyDP(contour, approxEpsPx, true) : contour;
                if (poly.Length < 3) continue;

                // 삼각분할
                var tris = TriangulateSimplePolygon(poly);
                if (tris == null || tris.Count == 0) continue;

                var positions = new HT.Vector3Collection();
                var indices = new HT.IntCollection();
                var map = new Dictionary<int, int>(); // poly index → vertex index

                for (int t = 0; t < tris.Count; t++)
                {
                    var tri = tris[t];
                    int i0 = MapIndex(poly, tri[0], map, positions, W, H, cx, cy, zRaw, sx, sy, zScale, zOffset, invalidZ, neighbor);
                    int i1 = MapIndex(poly, tri[1], map, positions, W, H, cx, cy, zRaw, sx, sy, zScale, zOffset, invalidZ, neighbor);
                    int i2 = MapIndex(poly, tri[2], map, positions, W, H, cx, cy, zRaw, sx, sy, zScale, zOffset, invalidZ, neighbor);
                    indices.Add(i0); indices.Add(i1); indices.Add(i2);
                }

                list.Add(new HT.MeshGeometry3D { Positions = positions, Indices = indices });
            }

            return list.ToArray();
        }

        private static int MapIndex(
            OpenCvSharp.Point[] poly, int k, Dictionary<int, int> map, HT.Vector3Collection positions,
            int W, int H, double cx, double cy, float[,] zRaw,
            double sx, double sy, double zScale, double zOffset, float invalidZ, int neighbor)
        {
            if (map.TryGetValue(k, out int existed)) return existed;

            var p = poly[k];
            int x = ClampInt(p.X, 0, W - 1);
            int y = ClampInt(p.Y, 0, H - 1);

            double z = SampleZWithNeighbor(zRaw, x, y, invalidZ, neighbor);
            double X = (x - cx) * sx;
            double Y = -(y - cy) * sy;
            double Z = zOffset + zScale * z;

            positions.Add(new SharpDX.Vector3((float)X, (float)Y, (float)Z));
            int idx = positions.Count - 1;
            map[k] = idx;
            return idx;
        }

        private static int ClampInt(int v, int lo, int hi) => (v < lo) ? lo : (v > hi ? hi : v);

        // 폴리곤 꼭짓점의 Z가 INVALID일 경우, 주변 이웃(상하좌우+확장)에서 첫 유효 Z를 샘플
        private static double SampleZWithNeighbor(float[,] zRaw, int x, int y, float invalid, int neighbor)
        {
            int H = zRaw.GetLength(0), W = zRaw.GetLength(1);
            float z = zRaw[y, x];
            if (z != invalid) return z;

            for (int r = 1; r <= neighbor; r++)
            {
                int x0 = ClampInt(x - r, 0, W - 1);
                int x1 = ClampInt(x + r, 0, W - 1);
                int y0 = ClampInt(y - r, 0, H - 1);
                int y1 = ClampInt(y + r, 0, H - 1);

                for (int xx = x0; xx <= x1; xx++) { float a = zRaw[y0, xx]; if (a != invalid) return a; float b = zRaw[y1, xx]; if (b != invalid) return b; }
                for (int yy = y0; yy <= y1; yy++) { float a = zRaw[yy, x0]; if (a != invalid) return a; float b = zRaw[yy, x1]; if (b != invalid) return b; }
            }
            return 0.0; // 끝까지 못 찾으면 0으로
        }

        // 단순 폴리곤 삼각분할(ear clipping)
        private static List<int[]> TriangulateSimplePolygon(OpenCvSharp.Point[] poly)
        {
            int n = poly.Length;
            var result = new List<int[]>();
            if (n < 3) return result;

            var V = new List<int>(n);
            for (int i = 0; i < n; i++) V.Add(i);

            // 방향(CCW 여부)
            double area = 0;
            for (int i = 0, j = n - 1; i < n; j = i++)
                area += (double)poly[j].X * poly[i].Y - (double)poly[i].X * poly[j].Y;
            bool ccw = area > 0;

            bool IsEar(int iPrev, int iCurr, int iNext)
            {
                var A = poly[iPrev]; var B = poly[iCurr]; var C = poly[iNext];
                double cross = ((B.X - A.X) * (C.Y - A.Y)) - ((B.Y - A.Y) * (C.X - A.X));
                if (ccw && cross <= 0) return false;
                if (!ccw && cross >= 0) return false;

                for (int k = 0; k < V.Count; k++)
                {
                    int idx = V[k];
                    if (idx == iPrev || idx == iCurr || idx == iNext) continue;
                    if (PointInTriangle(poly[idx], A, B, C)) return false;
                }
                return true;
            }

            while (V.Count >= 3)
            {
                bool clipped = false;
                int m = V.Count;
                for (int s = 0; s < m; s++)
                {
                    int iPrev = V[(s - 1 + m) % m];
                    int iCurr = V[s];
                    int iNext = V[(s + 1) % m];

                    if (IsEar(iPrev, iCurr, iNext))
                    {
                        result.Add(new[] { iPrev, iCurr, iNext });
                        V.RemoveAt(s);
                        clipped = true;
                        break;
                    }
                }
                if (!clipped) break; // 비정상 폴리곤(자가교차 등)
            }
            return result;
        }

        private static bool PointInTriangle(OpenCvSharp.Point P, OpenCvSharp.Point A, OpenCvSharp.Point B, OpenCvSharp.Point C)
        {
            double s1 = Cross(P, A, B), s2 = Cross(P, B, C), s3 = Cross(P, C, A);
            bool hasNeg = (s1 < 0) || (s2 < 0) || (s3 < 0), hasPos = (s1 > 0) || (s2 > 0) || (s3 > 0);
            return !(hasNeg && hasPos);
        }

        private static double Cross(OpenCvSharp.Point p, OpenCvSharp.Point a, OpenCvSharp.Point b)
            => (b.X - a.X) * (double)(p.Y - a.Y) - (b.Y - a.Y) * (double)(p.X - a.X);
    }
}
