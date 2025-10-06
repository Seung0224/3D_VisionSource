using System;
using SharpDX;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using HelixToolkit.Wpf.SharpDX;
using Media3D = System.Windows.Media.Media3D;
using DxCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace _3D_VisionSource.Viewer
{
    public partial class Viewer3DControl : UserControl
    {
        public Viewer3DControl()
        {
            InitializeComponent();
            Viewport.EffectsManager = new DefaultEffectsManager();
            Viewport.Background = Brushes.White;
        }

        private void FitCameraToPoints(Media3D.Point3D[] pts)
        {
            if (pts == null || pts.Length == 0) return;

            var cx = pts.Average(p => p.X);
            var cy = pts.Average(p => p.Y);
            var cz = pts.Average(p => p.Z);
            var center = new Media3D.Point3D(cx, cy, cz);

            double r2 = 0.0;
            foreach (var p in pts)
            {
                var dx = p.X - cx;
                var dy = p.Y - cy;
                var dz = p.Z - cz;
                var d2 = dx * dx + dy * dy + dz * dz;
                if (d2 > r2) r2 = d2;
            }
            var r = Math.Sqrt(r2);

            var cam = Viewport.Camera as DxCamera;
            if (cam == null)
            {
                cam = new DxCamera();
                Viewport.Camera = cam;
            }

            var dir = new Media3D.Vector3D(0, 1, -0.7);
            dir.Normalize();

            double fovRad = cam.FieldOfView * Math.PI / 180.0;
            double dist = r / Math.Max(1e-3, Math.Tan(fovRad * 0.5)) + r * 0.2;

            var look = dir * dist;
            cam.Position = center - look;
            cam.LookDirection = look;
            cam.UpDirection = new Media3D.Vector3D(0, 0, 1);

            cam.NearPlaneDistance = Math.Max(0.001, r * 0.01);
            cam.FarPlaneDistance = Math.Max(10.0, r * 20.0);
        }

        public void LoadPoints(System.Windows.Media.Media3D.Point3D[] pts, System.Windows.Media.Color[] cols, double pointSize = 2.0)
        {
            if (pts == null || cols == null || pts.Length == 0 || cols.Length != pts.Length)
                return;

            var positions = new Vector3Collection(
                pts.Select(p => new Vector3((float)p.X, (float)p.Y, (float)p.Z)));

            var colors = new Color4Collection(
                cols.Select(c => new Color4(c.R / 255f, c.G / 255f, c.B / 255f, 1f)));

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

            // 곱연산용 상수색은 White로 (버텍스 컬러를 그대로 보이게)
            var colorProp = model.GetType().GetProperty("Color", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (colorProp != null)
            {
                var t = colorProp.PropertyType;
                if (t == typeof(SharpDX.Color4)) colorProp.SetValue(model, new Color4(1f, 1f, 1f, 1f), null);
                else if (t == typeof(System.Windows.Media.Color)) colorProp.SetValue(model, System.Windows.Media.Colors.White, null);
            }

            // 크기 설정 (버전별 속성명 호환)
            var sizeProp = model.GetType().GetProperty("Size");
            if (sizeProp != null)
            {
                var t = sizeProp.PropertyType;
                if (t == typeof(double)) sizeProp.SetValue(model, pointSize, null);
                else if (t == typeof(float)) sizeProp.SetValue(model, (float)pointSize, null);
                else if (t == typeof(System.Windows.Size)) sizeProp.SetValue(model, new System.Windows.Size(pointSize, pointSize), null);
            }
            else
            {
                var pointSizeProp = model.GetType().GetProperty("PointSize");
                if (pointSizeProp != null)
                {
                    if (pointSizeProp.PropertyType == typeof(double)) pointSizeProp.SetValue(model, pointSize, null);
                    else if (pointSizeProp.PropertyType == typeof(float)) pointSizeProp.SetValue(model, (float)pointSize, null);
                }
            }

            var oitProp = model.GetType().GetProperty("EnableOIT");
            if (oitProp != null && oitProp.CanWrite) oitProp.SetValue(model, true, null);

            Viewport.Items.Clear();
            Viewport.Items.Add(model);

            FitCameraToPoints(pts);
            Viewport.ZoomExtents();
        }

        public void OverlayLineLoops(System.Windows.Media.Media3D.Point3D[][] loops, System.Windows.Media.Color color, float thickness = 10f)
        {
            if (loops == null || loops.Length == 0) return;

            var positions = new Vector3Collection();
            var indices = new IntCollection();
            int baseIdx = 0;

            foreach (var loop in loops)
            {
                if (loop == null || loop.Length < 2) continue;
                for (int i = 0; i < loop.Length; i++)
                    positions.Add(new Vector3((float)loop[i].X, (float)loop[i].Y, (float)loop[i].Z));

                for (int i = 0; i < loop.Length; i++)
                {
                    int a = baseIdx + i;
                    int b = baseIdx + ((i + 1) % loop.Length);
                    indices.Add(a);
                    indices.Add(b);
                }
                baseIdx += loop.Length;
            }

            var lineGeom = new LineGeometry3D
            {
                Positions = positions,
                Indices = indices
            };

            var m = new LineGeometryModel3D
            {
                Geometry = lineGeom,
                Color = color,
                Thickness = thickness,
                IsHitTestVisible = false
            };

            Viewport.Items.Add(m);
        }
        public void OverlayFillMeshes(HelixToolkit.Wpf.SharpDX.MeshGeometry3D[] meshes,
                              System.Windows.Media.Color color,
                              float opacity = 0.6f)   // 조금 진하게 시작
        {
            if (meshes == null || meshes.Length == 0) return;

            // 1) 컬러 준비
            var col4 = new SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, opacity);

            // 2) 머티리얼: Unlit + Emissive + Diffuse 알파는 0이 아닌 값으로
            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial
            {
                // 조명 영향 제거 + 색 고정
                DiffuseColor = new SharpDX.Color4(1f, 1f, 1f, opacity), // 알파 0 금지!
                AmbientColor = new SharpDX.Color4(0, 0, 0, 0),
                EmissiveColor = col4,
                SpecularColor = new SharpDX.Color4(0, 0, 0, 0),
                SpecularShininess = 1f
            };

            // 버전별: Unlit 지원 시 켜기
            var unlitProp = mat.GetType().GetProperty("EnableUnLit");
            if (unlitProp != null && unlitProp.CanWrite) unlitProp.SetValue(mat, true, null);

            foreach (var g in meshes)
            {
                if (g == null || g.Indices == null || g.Indices.Count < 3) continue; // 삼각형 없으면 skip

                var m = new HelixToolkit.Wpf.SharpDX.MeshGeometryModel3D
                {
                    Geometry = g,
                    Material = mat,
                    IsHitTestVisible = false,
                    CullMode = SharpDX.Direct3D11.CullMode.None, // 양면
                    DepthBias = -1                             // 너무 큰 음수는 피함
                };

                // 투명도 혼합(OIT) 지원시 켜기
                var oitProp = m.GetType().GetProperty("EnableOIT");
                if (oitProp != null && oitProp.CanWrite) oitProp.SetValue(m, true, null);

                // 정렬/렌더순서(투명물 위로 오게)
                var roProp = m.GetType().GetProperty("RenderOrder");
                if (roProp != null && roProp.CanWrite) roProp.SetValue(m, 1000, null);

                Viewport.Items.Add(m);
            }
        }

    }
}