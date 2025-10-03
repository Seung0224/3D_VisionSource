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
        // ✅ Bitmap 대신 파일 경로를 보관
        private string _intensityPath;
        private string _zmapPath;

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
                    _intensityPath = ofd.FileName;
                    UIMessageTip.ShowOk("Intensity loaded.");
                }
            }
        }

        private void btnOpenZMap_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select ZMap Image (8/16-bit GRAY)";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All Files|*.*";
                ofd.CheckFileExists = true;
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    _zmapPath = ofd.FileName;
                    UIMessageTip.ShowOk("ZMap loaded.");
                }
            }
        }

        private void btnFusion_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_intensityPath) || string.IsNullOrEmpty(_zmapPath))
                {
                    UIMessageTip.ShowWarning("먼저 Intensity/ZMap 이미지를 로드하세요.");
                    return;
                }

                var fp = new FusionParams
                {
                    Sx = 0.05f,
                    Sy = 0.05f,
                    ZScale = 0.001f,
                    ZOffset = 0f,
                    InvalidZ = 0,
                    InvalidZ16 = 0,
                    CenterOrigin = true
                };

                var sw = Stopwatch.StartNew();
                var result = FusionEngine.BuildPointCloudFromFiles(_intensityPath, _zmapPath, fp);
                sw.Stop();

                Trace.WriteLine($"Fusion: pts={result.Points?.Length ?? 0}, time={sw.ElapsedMilliseconds} ms");

                _viewer.LoadPoints(result.Points, result.Colors, pointSize: 2.0);
                UIMessageTip.ShowOk($"Fusion OK ({result.Points?.Length ?? 0} pts, {sw.ElapsedMilliseconds} ms)");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                UIMessageBox.ShowError($"Fusion 실패\n{ex.Message}");
            }
        }
        #endregion

        #region Form Events
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ✅ 파일 경로만 보관하므로 Dispose 불필요
            _intensityPath = null;
            _zmapPath = null;
        }
        #endregion
    }
}
