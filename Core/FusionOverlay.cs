// 파일: FusionOverlay.cs
using _3D_VisionSource.Viewer;          // Viewer3DControl
using Cyotek.Windows.Forms;              // ImageBox
using HelixToolkit.Wpf.SharpDX;         // MeshGeometry3D
using OpenCvSharp;                       // Contour/ApproxPolyDP
using System;
using System.Collections.Generic;
using System.Drawing;
using HT = HelixToolkit.Wpf.SharpDX;
using Media3D = System.Windows.Media.Media3D;

namespace _3D_VisionSource
{
    /// <summary>
    /// 인스턴스 없이 호출하는 2D/3D 오버레이 유틸(심플 API).
    /// Render(...) 한 번으로 2D/3D 모두 표시.
    /// </summary>
    public static class FusionOverlay
    {
        // ===== 내부 기본값(필요 시 여기만 바꾸면 전체 반영) =====
        private const bool CENTER_ORIGIN = true;            // 영상 중심 원점
        private const float INVALID_Z = 0f;              // zRaw 무효값
        private const int NEIGHBOR_RADIUS = 2;               // Z 보간용 이웃 탐색 반경
        private const double APPROX_EPS_PX = 1.5;             // 컨투어 단순화 에psilon(px)
        private const double POINT_SIZE = 3.0;             // 포인트 크기
        private const float MESH_OPACITY = 0.35f;           // 메쉬 투명도
        private static readonly System.Windows.Media.Color MESH_COLOR = System.Windows.Media.Colors.Red;

        /// <summary>
        /// 2D/3D 오버레이를 한 번에 렌더(간단 API).
        /// </summary>
        public static void Render(
            InspectionResults res,
            float[,] zRaw,
            Viewer3DControl viewer,
            ImageBox imageBox,
            InspectionParams p,
            Viewer3DControl.ViewPreset preset = Viewer3DControl.ViewPreset.Front)
        {
            if (res == null) return;

            // 2D
            Apply2D(imageBox, res.Overlay2D);

            // 3D
            if (viewer != null && res.Points != null && res.Colors != null)
            {
                var meshes = Make3DFilledMeshes(
                    res, zRaw,
                    sx: p.Sx, sy: p.Sy, zScale: p.ZScale, zOffset: p.ZOffset,
                    centerOrigin: CENTER_ORIGIN, invalidZ: INVALID_Z,
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

        // ---------- 2D ----------
        public static void Apply2D(ImageBox box, Bitmap overlay2D)
        {
            if (box == null) return;
            if (overlay2D != null)
            {
                box.Image = overlay2D;
                box.ZoomToFit();
            }
            else
            {
                box.Image = null;
            }
        }

        // ---------- 3D ----------
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

        // ---------- Mesh Builder ----------
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

                var poly = (approxEpsPx > 0) ? Cv2.ApproxPolyDP(contour, approxEpsPx, true) : contour;
                if (poly.Length < 3) continue;

                var tris = TriangulateSimplePolygon(poly);
                if (tris == null || tris.Count == 0) continue;

                var positions = new HT.Vector3Collection();
                var indices = new HT.IntCollection();
                var map = new Dictionary<int, int>(); // poly index -> positions index

                for (int t = 0; t < tris.Count; t++)
                {
                    var tri = tris[t]; // [i0,i1,i2] : poly의 인덱스
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
            OpenCvSharp.Point[] poly, int k,
            Dictionary<int, int> map,
            HT.Vector3Collection positions,
            int W, int H, double cx, double cy, float[,] zRaw,
            double sx, double sy, double zScale, double zOffset, float invalidZ, int neighbor)
        {
            if (map.TryGetValue(k, out int existed)) return existed;

            var p = poly[k];
            int x = ClampInt(p.X, 0, W - 1);
            int y = ClampInt(p.Y, 0, H - 1);

            double z = SampleZWithNeighbor(zRaw, x, y, invalidZ, neighbor);

            // 픽셀 -> 월드
            double X = (x - cx) * sx;
            double Y = -(y - cy) * sy;      // 화면 Y+ 아래 → 월드 Y+ 위
            double Z = zOffset + zScale * z;

            positions.Add(new SharpDX.Vector3((float)X, (float)Y, (float)Z));
            int idx = positions.Count - 1;
            map[k] = idx;
            return idx;
        }

        private static int ClampInt(int v, int lo, int hi)
        {
            if (v < lo) return lo;
            if (v > hi) return hi;
            return v;
        }

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

                for (int xx = x0; xx <= x1; xx++)
                {
                    float a = zRaw[y0, xx]; if (a != invalid) return a;
                    float b = zRaw[y1, xx]; if (b != invalid) return b;
                }
                for (int yy = y0; yy <= y1; yy++)
                {
                    float a = zRaw[yy, x0]; if (a != invalid) return a;
                    float b = zRaw[yy, x1]; if (b != invalid) return b;
                }
            }
            return 0.0;
        }

        // ---- 아주 단순한 ear-clipping 삼각분할 (구멍 없음, 단순 폴리곤 가정) ----
        private static List<int[]> TriangulateSimplePolygon(OpenCvSharp.Point[] poly)
        {
            int n = poly.Length;
            var result = new List<int[]>();
            if (n < 3) return result;

            var V = new List<int>(n);
            for (int i = 0; i < n; i++) V.Add(i);

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
                if (!clipped) break;
            }
            return result;
        }

        private static bool PointInTriangle(OpenCvSharp.Point P, OpenCvSharp.Point A, OpenCvSharp.Point B, OpenCvSharp.Point C)
        {
            double s1 = Cross(P, A, B);
            double s2 = Cross(P, B, C);
            double s3 = Cross(P, C, A);
            bool hasNeg = (s1 < 0) || (s2 < 0) || (s3 < 0);
            bool hasPos = (s1 > 0) || (s2 > 0) || (s3 > 0);
            return !(hasNeg && hasPos);
        }

        private static double Cross(OpenCvSharp.Point p, OpenCvSharp.Point a, OpenCvSharp.Point b)
            => (b.X - a.X) * (double)(p.Y - a.Y) - (b.Y - a.Y) * (double)(p.X - a.X);
    }
}
