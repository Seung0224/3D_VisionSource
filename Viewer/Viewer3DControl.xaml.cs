using SharpDX;
using System;
using System.Linq;
using System.Windows;
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

        public void LoadPoints(System.Windows.Media.Media3D.Point3D[] pts,
                               System.Windows.Media.Color[] cols,
                               double pointSize = 2.0)
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

            // ★ 여기서 더 이상 colors를 덮어쓰지 않는다 ★

            Viewport.Items.Clear();
            Viewport.Items.Add(model);

            FitCameraToPoints(pts);
            Viewport.ZoomExtents();
        }
    }
}