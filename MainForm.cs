using System;
using System.Drawing;
using System.Windows.Forms;
using Sunny.UI;

namespace _3D_VisionSource
{
    public partial class MainForm : UIForm
    {
        #region constants
        public const int RightSideBarWidth         = 220;
        public const int RightSideBarButtonsLeft   = 12;
        public const int RightSideBarButtonsTop    = 12;
        public const int RightSideBarButtonsRight  = 12;
        public const int RightSideBarButtonsBottom = 12;
        public const int RightSideBarButtonSpacing = 8;
        public const int RightSideBarTotalButton   = 3;
        #endregion

        #region fields
        private UIPanel rightsideBar, content;
        private UISymbolButton btnFusion, btnOpenIntensity, btnOpenZMap;
        private Bitmap intensityBuffer, zmapBuffer;

        public Bitmap IntensityBuffer => intensityBuffer;
        public Bitmap ZMapBuffer => zmapBuffer;
        #endregion

        public MainForm()
        {
            InitalizeMainUI();
            this.Style = UIStyle.Blue;
            this.Text = "3D Vision Source";
            this.titleForeColor = Color.Black;
            // SunnyUI UIForm 전용
            this.ShowIcon = false;
            this.WindowState = FormWindowState.Maximized;
            this.TitleFont = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void InitalizeMainUI()
        {
            #region Right Side Bar
            rightsideBar = new UIPanel();
            rightsideBar.Dock = DockStyle.Right;
            rightsideBar.Width = RightSideBarWidth;
            rightsideBar.FillColor = Color.FromArgb(245, 248, 255);
            rightsideBar.RectColor = Color.FromArgb(220, 230, 250);
            this.Controls.Add(rightsideBar);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 0,
                Padding = new Padding(RightSideBarButtonsLeft, RightSideBarButtonsTop, RightSideBarButtonsRight, RightSideBarButtonsBottom),
                AutoScroll = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rightsideBar.Controls.Add(table);

            btnOpenIntensity = new UISymbolButton();
            btnOpenIntensity.Dock = DockStyle.None;
            btnOpenIntensity.Text = "Intensity Image Open";
            btnOpenIntensity.Width = table.Width;
            btnOpenIntensity.Radius = 4;
            btnOpenIntensity.Symbol = 61664;
            btnOpenIntensity.Margin = new Padding(0, 0, 0, RightSideBarButtonSpacing);
            btnOpenIntensity.Click += OnClickOpenIntensity;
            ApplySidebarButtonTheme(btnOpenIntensity, 0, RightSideBarTotalButton);
            table.Controls.Add(btnOpenIntensity, 0, table.RowCount++);

            btnOpenZMap = new UISymbolButton();
            btnOpenZMap.Dock = DockStyle.None;
            btnOpenZMap.Text = "Zmap Image Open";
            btnOpenZMap.Width = table.Width;
            btnOpenZMap.Radius = 4;
            btnOpenZMap.Symbol = 61664;
            btnOpenZMap.Margin = new Padding(0, 0, 0, RightSideBarButtonSpacing);
            btnOpenZMap.Click += OnClickOpenZMap;
            ApplySidebarButtonTheme(btnOpenZMap, 1, RightSideBarTotalButton);
            table.Controls.Add(btnOpenZMap, 0, table.RowCount++);

            btnFusion = new UISymbolButton();
            btnFusion.Dock = DockStyle.None;
            btnFusion.Text = "Fusion";
            btnFusion.Width = table.Width;
            btnFusion.Radius = 4;
            btnFusion.Symbol = 61515;
            btnFusion.Margin = new Padding(0, 0, 0, RightSideBarButtonSpacing);
            btnFusion.Click += OnClickFusion;
            ApplySidebarButtonTheme(btnFusion, 2, RightSideBarTotalButton);
            table.Controls.Add(btnFusion, 0, table.RowCount++);
            #endregion

            content = new UIPanel();
            content.Dock = DockStyle.Fill;
            content.FillColor = Color.White;
            content.RectColor = Color.FromArgb(230, 235, 250);
            this.Controls.Add(content);
        }

        #region Buttons and Events

        #region Buttons Event Handlers
        private void OnClickOpenIntensity(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Intensity Image";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All Files|*.*";
                ofd.CheckFileExists = true;
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    intensityBuffer?.Dispose();
                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        intensityBuffer = new Bitmap(img);
                    }
                    UIMessageTip.ShowOk("Intensity image loaded to buffer.");
                }
            }
        }
        private void OnClickOpenZMap(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Zmap Image";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All Files|*.*";
                ofd.CheckFileExists = true;
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    zmapBuffer?.Dispose();
                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        zmapBuffer = new Bitmap(img);
                    }
                    UIMessageTip.ShowOk("Zmap image loaded to buffer.");
                }
            }
        }
        private void OnClickFusion(object sender, EventArgs e)
        {
            UIMessageTip.ShowOk("Fusion Run.");
            // 실행 함수 추가
        }
        #endregion

        #region Buttons Color Change
        private static Color LerpColor(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            int r = (int)(a.R + (b.R - a.R) * t);
            int g = (int)(a.G + (b.G - a.G) * t);
            int bch = (int)(a.B + (b.B - a.B) * t);
            return Color.FromArgb(r, g, bch);
        }

        private void ApplySidebarButtonTheme(UISymbolButton btn, int index, int total)
        {
            Color start = Color.FromArgb(210, 230, 255); // 매우 연한 파랑
            Color end = Color.FromArgb(45, 100, 200); // 기존 진한 파랑

            float t = (total <= 1) ? 1f : (float)index / (float)(total - 1);

            Color baseFill = LerpColor(start, end, t);
            Color baseRect = LerpColor(baseFill, end, 0.35f);

            btn.FillColor = baseFill;
            btn.RectColor = baseRect;

            btn.FillHoverColor = LerpColor(baseFill, end, 0.25f);
            btn.RectHoverColor = LerpColor(baseRect, end, 0.25f);
            btn.FillPressColor = LerpColor(baseFill, end, 0.45f);
            btn.RectPressColor = LerpColor(baseRect, end, 0.45f);

            btn.ForeColor = Color.Black;
        }
        #endregion

        #endregion

        #region Form Events
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            intensityBuffer?.Dispose();
            intensityBuffer = null;
            zmapBuffer?.Dispose();
            zmapBuffer = null;
        }
        #endregion
    }
}
