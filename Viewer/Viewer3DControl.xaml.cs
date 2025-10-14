using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using HelixToolkit.Wpf.SharpDX;
using Media3D = System.Windows.Media.Media3D;
using SharpDX;
using DxCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace _3D_VisionSource.Viewer
{
    public partial class Viewer3DControl : UserControl
    {
        // ========= 뷰 프리셋 =========
        public enum ViewPreset { Front, Back, Left, Right, Top, Bottom, Iso }

        public Viewer3DControl()
        {
            InitializeComponent();
            Viewport.EffectsManager = new DefaultEffectsManager();
            Viewport.Background = Brushes.White;
        }

        #region ==== Camera helpers ====
        private static void CalcCenterAndRadius(Media3D.Point3D[] pts, out Media3D.Point3D center, out double r)
        {
            double cx = pts.Average(p => p.X);
            double cy = pts.Average(p => p.Y);
            double cz = pts.Average(p => p.Z);
            center = new Media3D.Point3D(cx, cy, cz);

            double r2 = 0.0;
            foreach (var p in pts)
            {
                double dx = p.X - cx, dy = p.Y - cy, dz = p.Z - cz;
                double d2 = dx * dx + dy * dy + dz * dz;
                if (d2 > r2) r2 = d2;
            }
            r = Math.Sqrt(r2);
            if (r < 1e-9) r = 1.0;
        }
        private void ReplaceCamera(Media3D.Point3D center, Media3D.Vector3D dir, Media3D.Vector3D up, double distance)
        {
            if (dir.LengthSquared < 1e-12) dir = new Media3D.Vector3D(0, 1, -0.7);
            if (up.LengthSquared < 1e-12) up = new Media3D.Vector3D(0, 0, 1);
            dir.Normalize(); up.Normalize();

            var old = Viewport.Camera as DxCamera;
            double fov = old?.FieldOfView ?? 45.0;
            double near = old?.NearPlaneDistance ?? 0.1;
            double far = old?.FarPlaneDistance ?? 10000.0;

            var look = dir * distance;
            var pos = center - look;

            var cam = new DxCamera
            {
                Position = pos,
                LookDirection = look,
                UpDirection = up,
                FieldOfView = fov,
                NearPlaneDistance = near,
                FarPlaneDistance = far
            };
            Viewport.Camera = cam;
            Viewport.InvalidateRender();
        }
        private static void GetPreset(ViewPreset preset, out Media3D.Vector3D dir, out Media3D.Vector3D up)
        {
            switch (preset)
            {
                // ViewCube 스타일(Front=-Z, Up=+Y). 필요하면 여기만 바꾸면 됨.
                case ViewPreset.Front: dir = new Media3D.Vector3D(0, 0, -1); up = new Media3D.Vector3D(0, 1, 0); break;
                case ViewPreset.Back: dir = new Media3D.Vector3D(0, 0, +1); up = new Media3D.Vector3D(0, 1, 0); break;
                case ViewPreset.Left: dir = new Media3D.Vector3D(-1, 0, 0); up = new Media3D.Vector3D(0, 0, 1); break;
                case ViewPreset.Right: dir = new Media3D.Vector3D(+1, 0, 0); up = new Media3D.Vector3D(0, 0, 1); break;
                case ViewPreset.Top: dir = new Media3D.Vector3D(0, 0, -1); up = new Media3D.Vector3D(0, 1, 0); break;
                case ViewPreset.Bottom: dir = new Media3D.Vector3D(0, 0, +1); up = new Media3D.Vector3D(0, -1, 0); break;
                case ViewPreset.Iso:
                default:
                    dir = new Media3D.Vector3D(0.8, 0.8, -0.6); dir.Normalize();
                    up = new Media3D.Vector3D(0, 0, 1); break;
            }
        }
        private void ApplyPresetToPoints(Media3D.Point3D[] pts, ViewPreset preset)
        {
            CalcCenterAndRadius(pts, out var center, out var r);
            var cam = Viewport.Camera as DxCamera ?? new DxCamera();
            Viewport.Camera = cam;

            double fovRad = cam.FieldOfView * Math.PI / 180.0;
            double dist = r / Math.Max(1e-6, Math.Tan(fovRad * 0.5)) + r * 0.2;

            GetPreset(preset, out var dir, out var up);
            ReplaceCamera(center, dir, up, dist);

            cam.NearPlaneDistance = Math.Max(0.001, r * 0.01);
            cam.FarPlaneDistance = Math.Max(10.0, r * 20.0);
        }
        #endregion

        #region ==== Model helpers ====
        private static PointGeometryModel3D CreatePointModel(Media3D.Point3D[] pts, System.Windows.Media.Color[] cols, double pointSize)
        {
            var positions = new Vector3Collection(pts.Select(p => new Vector3((float)p.X, (float)p.Y, (float)p.Z)));
            var colors = new Color4Collection(cols.Select(c => new Color4(c.R / 255f, c.G / 255f, c.B / 255f, 1f)));

            var geom = new PointGeometry3D { Positions = positions, Colors = colors };
            var model = new PointGeometryModel3D { Geometry = geom, IsHitTestVisible = false };

            // 버텍스 컬러 그대로 보이도록 상수색 White
            var colorProp = model.GetType().GetProperty("Color");
            if (colorProp != null)
            {
                if (colorProp.PropertyType == typeof(Color4)) colorProp.SetValue(model, new Color4(1, 1, 1, 1), null);
                else if (colorProp.PropertyType == typeof(System.Windows.Media.Color)) colorProp.SetValue(model, Colors.White, null);
            }

            // 포인트 크기(버전 호환)
            var sizeProp = model.GetType().GetProperty("Size");
            if (sizeProp != null)
            {
                if (sizeProp.PropertyType == typeof(double)) sizeProp.SetValue(model, pointSize, null);
                else if (sizeProp.PropertyType == typeof(float)) sizeProp.SetValue(model, (float)pointSize, null);
                else if (sizeProp.PropertyType == typeof(System.Windows.Size)) sizeProp.SetValue(model, new System.Windows.Size(pointSize, pointSize), null);
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
            return model;
        }
        private static MeshGeometryModel3D CreateMeshModel(HelixToolkit.Wpf.SharpDX.MeshGeometry3D geom, System.Windows.Media.Color color, float opacity)
        {
            var mat = new PhongMaterial
            {
                DiffuseColor = new Color4(1f, 1f, 1f, opacity),
                AmbientColor = new Color4(0, 0, 0, 0),
                EmissiveColor = new Color4(color.R / 255f, color.G / 255f, color.B / 255f, opacity),
                SpecularColor = new Color4(0, 0, 0, 0),
                SpecularShininess = 1f
            };
            var unlitProp = mat.GetType().GetProperty("EnableUnLit");
            if (unlitProp != null && unlitProp.CanWrite) unlitProp.SetValue(mat, true, null);

            var m = new MeshGeometryModel3D
            {
                Geometry = geom,
                Material = mat,
                IsHitTestVisible = false,
                CullMode = SharpDX.Direct3D11.CullMode.None,
                DepthBias = -1
            };
            var oitProp = m.GetType().GetProperty("EnableOIT");
            if (oitProp != null && oitProp.CanWrite) oitProp.SetValue(m, true, null);
            var roProp = m.GetType().GetProperty("RenderOrder");
            if (roProp != null && roProp.CanWrite) roProp.SetValue(m, 1000, null);
            return m;
        }
        public void ClearScene()
        {
            Viewport.Items.Clear();
        }
        #endregion

        #region ==== Public APIs ====
        /// <summary>
        /// 한 번의 호출로 포인트클라우드 + 메쉬 오버레이 + 카메라 프리셋 적용까지 수행.
        /// </summary>
        /// <param name="pts">Point cloud positions</param>
        /// <param name="cols">Per-vertex colors (same length as pts)</param>
        /// <param name="meshes">Optional overlay meshes</param>
        /// <param name="preset">Camera view preset</param>
        /// <param name="pointSize">Point size in pixels</param>
        /// <param name="meshColor">Mesh color (default: Red)</param>
        /// <param name="meshOpacity">Mesh opacity (0~1, default: 0.35f)</param>
        /// <param name="clearBefore">True면 기존 아이템 모두 제거</param>
        public void RenderScene(Media3D.Point3D[] pts, System.Windows.Media.Color[] cols, MeshGeometry3D[] meshes = null, ViewPreset preset = ViewPreset.Front, double pointSize = 3.0, System.Windows.Media.Color? meshColor = null, float meshOpacity = 0.35f, bool clearBefore = true)
        {
            if (pts == null || cols == null || pts.Length == 0 || cols.Length != pts.Length) return;

            if (clearBefore) Viewport.Items.Clear();

            // 1) 포인트클라우드
            var pointModel = CreatePointModel(pts, cols, pointSize);
            Viewport.Items.Add(pointModel);

            // 2) 메쉬 오버레이(있으면)
            if (meshes != null && meshes.Length > 0)
            {
                var mc = meshColor ?? Colors.Red;
                foreach (var g in meshes)
                {
                    if (g == null || g.Indices == null || g.Indices.Count < 3) continue;
                    Viewport.Items.Add(CreateMeshModel(g, mc, meshOpacity));
                }
            }

            // 3) 카메라 프리셋
            ApplyPresetToPoints(pts, preset);
        }
        #endregion
    }
}
