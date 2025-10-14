using OpenCvSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Media3D;
using MediaColor = System.Windows.Media.Color;

namespace _3D_VisionSource
{
    public class InspectionParams
    {
        public float Sx = 0f;
        public float Sy = 0f;
        public float ZScale = 0f;
        public float ZOffset = 0f;
        public byte InvalidZ = 0;
        public ushort InvalidZ16 = 0;
        public bool CenterOrigin = true;
        public double MinAreaMm2 = 0;
        public double OverlayAlpha = 0;
        public bool Centinal = false;
        public bool UseMinPixel = false;
        public int MinPxKernel = 3;

        public InspectionParams Clone() => (InspectionParams)this.MemberwiseClone();
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
}
