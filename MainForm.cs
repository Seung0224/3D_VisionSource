using System;
using System.Drawing;
using System.Windows.Forms;
using Sunny.UI;

namespace _3D_VisionSource
{
    public partial class MainForm : UIForm
    {
        public const int RightSideBarWidth         = 220;
        public const int RightSideBarButtonsLeft   = 5;
        public const int RightSideBarButtonsTop    = 5;
        public const int RightSideBarButtonsRight  = 5;
        public const int RightSideBarButtonsBottom = 5;
        public const int RightSideBarButtonFusion  = 5;
        public const int RightSideBarButtonSpacing = 5;

        private UIPanel sideBar, content;
        private UISymbolButton btnFusion;

        public MainForm()
        {
            InitalizeMainUI();
            this.Style = UIStyle.Blue;
            this.Text = "3D Vision Source";
            // SunnyUI UIForm 전용
            this.TitleFont = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Point);
            this.WindowState = FormWindowState.Maximized;
        }

        private void InitalizeMainUI()
        {
            // 우측 사이드바
            sideBar = new UIPanel();
            sideBar.Dock = DockStyle.Right;
            sideBar.Width = RightSideBarWidth;
            sideBar.FillColor = Color.FromArgb(245, 248, 255);
            sideBar.RectColor = Color.FromArgb(220, 230, 250);
            this.Controls.Add(sideBar);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 0,
                Padding = new Padding(RightSideBarButtonsLeft, RightSideBarButtonsTop, RightSideBarButtonsRight, RightSideBarButtonsBottom),
                AutoScroll = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            sideBar.Controls.Add(table);

            // 합성 버튼 
            btnFusion = new UISymbolButton();
            btnFusion.Dock = DockStyle.None;
            btnFusion.Text = "Fusion";
            btnFusion.Width = table.Width;
            btnFusion.Radius = 4;
            btnFusion.Symbol = 61515;
            btnFusion.FillColor = Color.FromArgb(45, 100, 200);
            btnFusion.RectColor = Color.FromArgb(45, 100, 200);
            btnFusion.ForeColor = Color.White;
            btnFusion.Margin = new Padding(0, 0, 0, RightSideBarButtonSpacing);
            btnFusion.Click += delegate { UIMessageTip.ShowOk("Fusion Run."); };

            table.Controls.Add(btnFusion, 0, table.RowCount++);

            // Main Display 영역
            content = new UIPanel();
            content.Dock = DockStyle.Fill;
            content.FillColor = Color.White;
            content.RectColor = Color.FromArgb(230, 235, 250);
            this.Controls.Add(content);
        }
    }
}
