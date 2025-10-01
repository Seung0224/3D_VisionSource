using Sunny.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using _3D_VisionSource.Viewer;

namespace _3D_VisionSource
{
    public partial class MainForm : UIForm
    {
        #region fields
        private Bitmap IntensityBuffer, ZmapBuffer;
        private Viewer3DControl _viewer;
        #endregion

        public MainForm()
        {
            InitializeComponent();
            InitializeMainUI();
            Initialize3DViwerUI();
        }
        private void InitializeMainUI()
        {
            this.Style = UIStyle.Blue;
            this.Text = "3D Vision Source";
            this.titleForeColor = Color.Black;
            this.ShowIcon = false;
            this.WindowState = FormWindowState.Maximized;
            this.TitleFont = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void Initialize3DViwerUI()
        {
            _viewer = new Viewer3DControl();
            ViewerHost.Child = _viewer;
        }

        #region Buttons Event Handlers
        private void btnOpenIntensity_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Intensity Image";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All Files|*.*";
                ofd.CheckFileExists = true;
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    IntensityBuffer?.Dispose();
                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        IntensityBuffer = new Bitmap(img);
                    }
                }
            }
        }

        private void btnOpenZMap_Click(object sender, EventArgs e)
        {

            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Zmap Image";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All Files|*.*";
                ofd.CheckFileExists = true;
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    ZmapBuffer?.Dispose();
                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        ZmapBuffer = new Bitmap(img);
                    }
                }
            }
        }

        private void btnFusion_Click(object sender, EventArgs e)
        {
            UIMessageTip.ShowOk("Fusion Run.");
            if (IntensityBuffer == null || ZmapBuffer == null)
            {
                UIMessageTip.ShowWarning("먼저 Intensity/ZMap 이미지를 로드하세요.");
                return;
            }

            var result = FusionEngine.BuildPointCloud(IntensityBuffer, ZmapBuffer, new FusionParams());
            Trace.WriteLine($"pts={result.Points?.Length ?? 0}");
            _viewer.LoadPoints(result.Points, result.Colors, pointSize: 2.0);
        }

        private void MainUITLP_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MainUIPanel_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region Form Events
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IntensityBuffer?.Dispose();
            IntensityBuffer = null;
            ZmapBuffer?.Dispose();
            ZmapBuffer = null;
        }
        #endregion
    }
}
