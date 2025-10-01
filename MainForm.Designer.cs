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
            this.btnFusion = new Sunny.UI.UISymbolButton();
            this.btnOpenZMap = new Sunny.UI.UISymbolButton();
            this.MainUIPanel = new Sunny.UI.UIPanel();
            this.btnOpenIntensity = new Sunny.UI.UISymbolButton();
            this.ViewerHost = new System.Windows.Forms.Integration.ElementHost();
            this.MainUITLP.SuspendLayout();
            this.ButtonUITLP.SuspendLayout();
            this.MainUIPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainUITLP
            // 
            this.MainUITLP.ColumnCount = 3;
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 228F));
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
            this.MainUITLP.Paint += new System.Windows.Forms.PaintEventHandler(this.MainUITLP_Paint);
            // 
            // ButtonUITLP
            // 
            this.ButtonUITLP.AutoSize = true;
            this.ButtonUITLP.ColumnCount = 1;
            this.ButtonUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonUITLP.Controls.Add(this.btnFusion, 0, 2);
            this.ButtonUITLP.Controls.Add(this.btnOpenZMap, 0, 1);
            this.ButtonUITLP.Controls.Add(this.btnOpenIntensity, 0, 0);
            this.ButtonUITLP.Dock = System.Windows.Forms.DockStyle.Right;
            this.ButtonUITLP.Location = new System.Drawing.Point(1066, 0);
            this.ButtonUITLP.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.ButtonUITLP.Name = "ButtonUITLP";
            this.ButtonUITLP.RowCount = 4;
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ButtonUITLP.Size = new System.Drawing.Size(223, 820);
            this.ButtonUITLP.TabIndex = 0;
            this.ButtonUITLP.TagString = null;
            // 
            // btnFusion
            // 
            this.btnFusion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFusion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFusion.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(230)))), ((int)(((byte)(255)))));
            this.btnFusion.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnFusion.ForeColor = System.Drawing.Color.Black;
            this.btnFusion.Location = new System.Drawing.Point(3, 83);
            this.btnFusion.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnFusion.Name = "btnFusion";
            this.btnFusion.Size = new System.Drawing.Size(217, 34);
            this.btnFusion.Symbol = 559469;
            this.btnFusion.SymbolColor = System.Drawing.Color.Black;
            this.btnFusion.TabIndex = 2;
            this.btnFusion.Text = "3D Fusion";
            this.btnFusion.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnFusion.Click += new System.EventHandler(this.btnFusion_Click);
            // 
            // btnOpenZMap
            // 
            this.btnOpenZMap.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpenZMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenZMap.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(230)))), ((int)(((byte)(255)))));
            this.btnOpenZMap.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnOpenZMap.ForeColor = System.Drawing.Color.Black;
            this.btnOpenZMap.Location = new System.Drawing.Point(3, 43);
            this.btnOpenZMap.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOpenZMap.Name = "btnOpenZMap";
            this.btnOpenZMap.Radius = 3;
            this.btnOpenZMap.Size = new System.Drawing.Size(217, 34);
            this.btnOpenZMap.Symbol = 361893;
            this.btnOpenZMap.SymbolColor = System.Drawing.Color.Black;
            this.btnOpenZMap.TabIndex = 1;
            this.btnOpenZMap.Text = "ZMap Open";
            this.btnOpenZMap.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnOpenZMap.Click += new System.EventHandler(this.btnOpenZMap_Click);
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
            this.MainUIPanel.Size = new System.Drawing.Size(1058, 814);
            this.MainUIPanel.TabIndex = 2;
            this.MainUIPanel.Text = "MainUIPanel";
            this.MainUIPanel.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.MainUIPanel.Click += new System.EventHandler(this.MainUIPanel_Click);
            // 
            // btnOpenIntensity
            // 
            this.btnOpenIntensity.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpenIntensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenIntensity.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(230)))), ((int)(((byte)(255)))));
            this.btnOpenIntensity.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnOpenIntensity.ForeColor = System.Drawing.Color.Black;
            this.btnOpenIntensity.Location = new System.Drawing.Point(3, 3);
            this.btnOpenIntensity.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnOpenIntensity.Name = "btnOpenIntensity";
            this.btnOpenIntensity.Size = new System.Drawing.Size(217, 34);
            this.btnOpenIntensity.Symbol = 61893;
            this.btnOpenIntensity.SymbolColor = System.Drawing.Color.Black;
            this.btnOpenIntensity.TabIndex = 0;
            this.btnOpenIntensity.Text = "Intensity Open";
            this.btnOpenIntensity.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnOpenIntensity.Click += new System.EventHandler(this.btnOpenIntensity_Click);
            // 
            // ViewerHost
            // 
            this.ViewerHost.BackColor = System.Drawing.Color.Transparent;
            this.ViewerHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ViewerHost.Location = new System.Drawing.Point(3, 3);
            this.ViewerHost.Margin = new System.Windows.Forms.Padding(0);
            this.ViewerHost.Name = "ViewerHost";
            this.ViewerHost.Size = new System.Drawing.Size(1052, 808);
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
            this.MainUITLP.ResumeLayout(false);
            this.MainUITLP.PerformLayout();
            this.ButtonUITLP.ResumeLayout(false);
            this.MainUIPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITableLayoutPanel MainUITLP;
        private Sunny.UI.UITableLayoutPanel ButtonUITLP;
        private Sunny.UI.UISymbolButton btnFusion;
        private Sunny.UI.UISymbolButton btnOpenZMap;
        private Sunny.UI.UIPanel MainUIPanel;
        private Sunny.UI.UISymbolButton btnOpenIntensity;
        private System.Windows.Forms.Integration.ElementHost ViewerHost;
    }
}

