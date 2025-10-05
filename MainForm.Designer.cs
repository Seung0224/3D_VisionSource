namespace _3D_VisionSource
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainUITLP = new Sunny.UI.UITableLayoutPanel();
            this.ButtonUITLP = new Sunny.UI.UITableLayoutPanel();
            this.uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            this.BTN_ZMAP_PATH_SEARCH = new Sunny.UI.UIButton();
            this.PNL_ZMAP_PATH = new Sunny.UI.UIPanel();
            this.TLP_INTENSITY = new Sunny.UI.UITableLayoutPanel();
            this.PNL_INTENSITY_PATH = new Sunny.UI.UIPanel();
            this.BTN_INENSITY_PATH_SEARCH = new Sunny.UI.UIButton();
            this.BTN_IMAGE_OPEN = new Sunny.UI.UISymbolButton();
            this.BTN_IMAGE_FUSION = new Sunny.UI.UISymbolButton();
            this.MainUIPanel = new Sunny.UI.UIPanel();
            this.ViewerHost = new System.Windows.Forms.Integration.ElementHost();
            this.MainUITLP.SuspendLayout();
            this.ButtonUITLP.SuspendLayout();
            this.uiTableLayoutPanel1.SuspendLayout();
            this.TLP_INTENSITY.SuspendLayout();
            this.MainUIPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainUITLP
            // 
            this.MainUITLP.ColumnCount = 3;
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 346F));
            this.MainUITLP.Controls.Add(this.ButtonUITLP, 2, 0);
            this.MainUITLP.Controls.Add(this.MainUIPanel, 0, 0);
            this.MainUITLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainUITLP.Location = new System.Drawing.Point(0, 35);
            this.MainUITLP.Name = "MainUITLP";
            this.MainUITLP.RowCount = 1;
            this.MainUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainUITLP.Size = new System.Drawing.Size(1291, 820);
            this.MainUITLP.TabIndex = 0;
            this.MainUITLP.TagString = null;
            // 
            // ButtonUITLP
            // 
            this.ButtonUITLP.AutoSize = true;
            this.ButtonUITLP.ColumnCount = 1;
            this.ButtonUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonUITLP.Controls.Add(this.uiTableLayoutPanel1, 0, 2);
            this.ButtonUITLP.Controls.Add(this.TLP_INTENSITY, 0, 1);
            this.ButtonUITLP.Controls.Add(this.BTN_IMAGE_OPEN, 0, 0);
            this.ButtonUITLP.Controls.Add(this.BTN_IMAGE_FUSION, 0, 3);
            this.ButtonUITLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonUITLP.Location = new System.Drawing.Point(944, 0);
            this.ButtonUITLP.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.ButtonUITLP.Name = "ButtonUITLP";
            this.ButtonUITLP.RowCount = 5;
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.Size = new System.Drawing.Size(345, 820);
            this.ButtonUITLP.TabIndex = 0;
            this.ButtonUITLP.TagString = null;
            // 
            // uiTableLayoutPanel1
            // 
            this.uiTableLayoutPanel1.ColumnCount = 2;
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.34101F));
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.65899F));
            this.uiTableLayoutPanel1.Controls.Add(this.BTN_ZMAP_PATH_SEARCH, 1, 0);
            this.uiTableLayoutPanel1.Controls.Add(this.PNL_ZMAP_PATH, 0, 0);
            this.uiTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTableLayoutPanel1.Location = new System.Drawing.Point(3, 83);
            this.uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            this.uiTableLayoutPanel1.RowCount = 1;
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.uiTableLayoutPanel1.Size = new System.Drawing.Size(339, 34);
            this.uiTableLayoutPanel1.TabIndex = 12;
            this.uiTableLayoutPanel1.TagString = null;
            // 
            // BTN_ZMAP_PATH_SEARCH
            // 
            this.BTN_ZMAP_PATH_SEARCH.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_ZMAP_PATH_SEARCH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_ZMAP_PATH_SEARCH.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_ZMAP_PATH_SEARCH.Location = new System.Drawing.Point(268, 3);
            this.BTN_ZMAP_PATH_SEARCH.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_ZMAP_PATH_SEARCH.Name = "BTN_ZMAP_PATH_SEARCH";
            this.BTN_ZMAP_PATH_SEARCH.Size = new System.Drawing.Size(68, 28);
            this.BTN_ZMAP_PATH_SEARCH.TabIndex = 13;
            this.BTN_ZMAP_PATH_SEARCH.Text = "...";
            this.BTN_ZMAP_PATH_SEARCH.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_ZMAP_PATH_SEARCH.Click += new System.EventHandler(this.BTN_PICK_ZMAP_Click);
            // 
            // PNL_ZMAP_PATH
            // 
            this.PNL_ZMAP_PATH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PNL_ZMAP_PATH.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.PNL_ZMAP_PATH.Location = new System.Drawing.Point(4, 5);
            this.PNL_ZMAP_PATH.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PNL_ZMAP_PATH.MinimumSize = new System.Drawing.Size(1, 1);
            this.PNL_ZMAP_PATH.Name = "PNL_ZMAP_PATH";
            this.PNL_ZMAP_PATH.Size = new System.Drawing.Size(257, 24);
            this.PNL_ZMAP_PATH.TabIndex = 11;
            this.PNL_ZMAP_PATH.Text = null;
            this.PNL_ZMAP_PATH.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TLP_INTENSITY
            // 
            this.TLP_INTENSITY.ColumnCount = 2;
            this.TLP_INTENSITY.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.34101F));
            this.TLP_INTENSITY.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.65899F));
            this.TLP_INTENSITY.Controls.Add(this.PNL_INTENSITY_PATH, 0, 0);
            this.TLP_INTENSITY.Controls.Add(this.BTN_INENSITY_PATH_SEARCH, 1, 0);
            this.TLP_INTENSITY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TLP_INTENSITY.Location = new System.Drawing.Point(3, 43);
            this.TLP_INTENSITY.Name = "TLP_INTENSITY";
            this.TLP_INTENSITY.RowCount = 1;
            this.TLP_INTENSITY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TLP_INTENSITY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_INTENSITY.Size = new System.Drawing.Size(339, 34);
            this.TLP_INTENSITY.TabIndex = 11;
            this.TLP_INTENSITY.TagString = null;
            // 
            // PNL_INTENSITY_PATH
            // 
            this.PNL_INTENSITY_PATH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PNL_INTENSITY_PATH.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PNL_INTENSITY_PATH.Location = new System.Drawing.Point(4, 5);
            this.PNL_INTENSITY_PATH.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PNL_INTENSITY_PATH.MinimumSize = new System.Drawing.Size(1, 1);
            this.PNL_INTENSITY_PATH.Name = "PNL_INTENSITY_PATH";
            this.PNL_INTENSITY_PATH.Size = new System.Drawing.Size(257, 24);
            this.PNL_INTENSITY_PATH.TabIndex = 11;
            this.PNL_INTENSITY_PATH.Text = null;
            this.PNL_INTENSITY_PATH.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BTN_INENSITY_PATH_SEARCH
            // 
            this.BTN_INENSITY_PATH_SEARCH.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_INENSITY_PATH_SEARCH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_INENSITY_PATH_SEARCH.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_INENSITY_PATH_SEARCH.Location = new System.Drawing.Point(268, 3);
            this.BTN_INENSITY_PATH_SEARCH.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_INENSITY_PATH_SEARCH.Name = "BTN_INENSITY_PATH_SEARCH";
            this.BTN_INENSITY_PATH_SEARCH.Size = new System.Drawing.Size(68, 28);
            this.BTN_INENSITY_PATH_SEARCH.TabIndex = 12;
            this.BTN_INENSITY_PATH_SEARCH.Text = "...";
            this.BTN_INENSITY_PATH_SEARCH.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_INENSITY_PATH_SEARCH.Click += new System.EventHandler(this.BTN_PICK_INTENSITY_Click);
            // 
            // BTN_IMAGE_OPEN
            // 
            this.BTN_IMAGE_OPEN.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_IMAGE_OPEN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_IMAGE_OPEN.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(230)))), ((int)(((byte)(255)))));
            this.BTN_IMAGE_OPEN.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMAGE_OPEN.ForeColor = System.Drawing.Color.Black;
            this.BTN_IMAGE_OPEN.Location = new System.Drawing.Point(3, 3);
            this.BTN_IMAGE_OPEN.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_IMAGE_OPEN.Name = "BTN_IMAGE_OPEN";
            this.BTN_IMAGE_OPEN.Size = new System.Drawing.Size(339, 34);
            this.BTN_IMAGE_OPEN.Symbol = 61893;
            this.BTN_IMAGE_OPEN.SymbolColor = System.Drawing.Color.Black;
            this.BTN_IMAGE_OPEN.TabIndex = 7;
            this.BTN_IMAGE_OPEN.Text = "Image Open";
            this.BTN_IMAGE_OPEN.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMAGE_OPEN.Click += new System.EventHandler(this.BTN_IMAGE_OPEN_Click);
            // 
            // BTN_IMAGE_FUSION
            // 
            this.BTN_IMAGE_FUSION.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_IMAGE_FUSION.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_IMAGE_FUSION.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(230)))), ((int)(((byte)(255)))));
            this.BTN_IMAGE_FUSION.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMAGE_FUSION.ForeColor = System.Drawing.Color.Black;
            this.BTN_IMAGE_FUSION.Location = new System.Drawing.Point(3, 123);
            this.BTN_IMAGE_FUSION.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_IMAGE_FUSION.Name = "BTN_IMAGE_FUSION";
            this.BTN_IMAGE_FUSION.Size = new System.Drawing.Size(339, 34);
            this.BTN_IMAGE_FUSION.Symbol = 559469;
            this.BTN_IMAGE_FUSION.SymbolColor = System.Drawing.Color.Black;
            this.BTN_IMAGE_FUSION.TabIndex = 5;
            this.BTN_IMAGE_FUSION.Text = "3D Fusion";
            this.BTN_IMAGE_FUSION.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMAGE_FUSION.Click += new System.EventHandler(this.BTN_IMAGE_FUSION_Click);
            // 
            // MainUIPanel
            // 
            this.MainUITLP.SetColumnSpan(this.MainUIPanel, 2);
            this.MainUIPanel.Controls.Add(this.ViewerHost);
            this.MainUIPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainUIPanel.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.MainUIPanel.Location = new System.Drawing.Point(4, 3);
            this.MainUIPanel.Margin = new System.Windows.Forms.Padding(4, 3, 0, 3);
            this.MainUIPanel.MinimumSize = new System.Drawing.Size(1, 1);
            this.MainUIPanel.Name = "MainUIPanel";
            this.MainUIPanel.Padding = new System.Windows.Forms.Padding(3);
            this.MainUIPanel.Size = new System.Drawing.Size(940, 814);
            this.MainUIPanel.TabIndex = 2;
            this.MainUIPanel.Text = "MainUIPanel";
            this.MainUIPanel.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ViewerHost
            // 
            this.ViewerHost.BackColor = System.Drawing.Color.Transparent;
            this.ViewerHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ViewerHost.Location = new System.Drawing.Point(3, 3);
            this.ViewerHost.Margin = new System.Windows.Forms.Padding(0);
            this.ViewerHost.Name = "ViewerHost";
            this.ViewerHost.Size = new System.Drawing.Size(934, 808);
            this.ViewerHost.TabIndex = 0;
            this.ViewerHost.Text = "elementHost1";
            this.ViewerHost.Child = null;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1291, 855);
            this.Controls.Add(this.MainUITLP);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "MainForm";
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 883, 641);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.MainUITLP.ResumeLayout(false);
            this.MainUITLP.PerformLayout();
            this.ButtonUITLP.ResumeLayout(false);
            this.uiTableLayoutPanel1.ResumeLayout(false);
            this.TLP_INTENSITY.ResumeLayout(false);
            this.MainUIPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITableLayoutPanel MainUITLP;
        private Sunny.UI.UITableLayoutPanel ButtonUITLP;
        private Sunny.UI.UIPanel MainUIPanel;
        private System.Windows.Forms.Integration.ElementHost ViewerHost;
        private Sunny.UI.UISymbolButton BTN_IMAGE_OPEN;
        private Sunny.UI.UISymbolButton BTN_IMAGE_FUSION;
        private Sunny.UI.UITableLayoutPanel TLP_INTENSITY;
        private Sunny.UI.UIPanel PNL_INTENSITY_PATH;
        private Sunny.UI.UIButton BTN_INENSITY_PATH_SEARCH;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UIButton BTN_ZMAP_PATH_SEARCH;
        private Sunny.UI.UIPanel PNL_ZMAP_PATH;
    }
}

