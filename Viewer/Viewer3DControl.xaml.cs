using SharpDX;                
using System;
using System.Linq;
using System.Windows;         
using System.Windows.Controls;
using System.Windows.Media;   
using HelixToolkit.Wpf.SharpDX;
using WpfColor = System.Windows.Media.Color;
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

            // 중심/반경
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


        public void LoadPoints(Media3D.Point3D[] points, WpfColor[] colors, double pointSize = 2.0)
        {
            var positions = new Vector3Collection(points.Select(p => new SharpDX.Vector3((float)p.X, (float)p.Y, (float)p.Z)));

            Color4Collection color4s =
                (colors != null && colors.Length == points.Length)
                ? new Color4Collection(colors.Select(c => new Color4(c.R / 255f, c.G / 255f, c.B / 255f, 1f)))
                : new Color4Collection(Enumerable.Repeat(new Color4(0.1f, 0.6f, 1f, 1f), positions.Count));

            var geom = new PointGeometry3D { Positions = positions, Colors = color4s };

            PointModel.Geometry = new PointGeometry3D { Positions = positions, Colors = color4s };
            PointModel.Size = new System.Windows.Size(pointSize, pointSize);

            FitCameraToPoints(points);
        }
    }
}
