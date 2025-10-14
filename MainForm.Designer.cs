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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainUITLP = new Sunny.UI.UITableLayoutPanel();
            this.TWODImageBox = new Cyotek.Windows.Forms.ImageBox();
            this.ZMapImageBox = new Cyotek.Windows.Forms.ImageBox();
            this.THREEDPanelUI = new Sunny.UI.UIPanel();
            this.TWODPanelUI = new Sunny.UI.UIPanel();
            this.ZMapPanelUI = new Sunny.UI.UIPanel();
            this.ButtonUITLP = new Sunny.UI.UITableLayoutPanel();
            this.BTN_3D_VISION_LOG_DATA_CLEAR = new Sunny.UI.UISymbolButton();
            this.BTN_3D_VISION_LOG_CLEAR = new Sunny.UI.UISymbolButton();
            this.BTN_SHOW_ROI = new Sunny.UI.UISymbolButton();
            this.BTN_SET_ROI = new Sunny.UI.UISymbolButton();
            this.uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            this.BTN_ZMAP_PATH_SEARCH = new Sunny.UI.UIButton();
            this.PNL_ZMAP_PATH = new Sunny.UI.UIPanel();
            this.TLP_INTENSITY = new Sunny.UI.UITableLayoutPanel();
            this.PNL_INTENSITY_PATH = new Sunny.UI.UIPanel();
            this.BTN_INENSITY_PATH_SEARCH = new Sunny.UI.UIButton();
            this.BTN_IMAGE_FUSION = new Sunny.UI.UISymbolButton();
            this.LB_3D_VISION_LOG = new Sunny.UI.UIListBox();
            this.GV_3D_VISION_LOG = new Sunny.UI.UIDataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.UP_Overlay = new Sunny.UI.UIPanel();
            this.UP_Spec = new Sunny.UI.UIPanel();
            this.UP_Zoff = new Sunny.UI.UIPanel();
            this.UP_Zscale = new Sunny.UI.UIPanel();
            this.UP_Sy = new Sunny.UI.UIPanel();
            this.UP_Sx = new Sunny.UI.UIPanel();
            this.uiPanel7 = new Sunny.UI.UIPanel();
            this.uiPanel6 = new Sunny.UI.UIPanel();
            this.uiPanel5 = new Sunny.UI.UIPanel();
            this.uiPanel4 = new Sunny.UI.UIPanel();
            this.uiPanel3 = new Sunny.UI.UIPanel();
            this.uiPanel2 = new Sunny.UI.UIPanel();
            this.uiPanel1 = new Sunny.UI.UIPanel();
            this.UP_Centinal = new Sunny.UI.UISwitch();
            this.MainUIPanel = new Sunny.UI.UIPanel();
            this.ViewerHost = new System.Windows.Forms.Integration.ElementHost();
            this.BTN_IMAGE_OPEN = new Sunny.UI.UISymbolButton();
            this.IntensityPanelUI = new Sunny.UI.UIPanel();
            this.IntensityImageBox = new Cyotek.Windows.Forms.ImageBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.uiPanel20 = new Sunny.UI.UIPanel();
            this.UP_Minpixel = new Sunny.UI.UISwitch();
            this.uiPanel8 = new Sunny.UI.UIPanel();
            this.UP_MinpxKernel = new Sunny.UI.UIPanel();
            this.MainUITLP.SuspendLayout();
            this.ButtonUITLP.SuspendLayout();
            this.uiTableLayoutPanel1.SuspendLayout();
            this.TLP_INTENSITY.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GV_3D_VISION_LOG)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.MainUIPanel.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainUITLP
            // 
            this.MainUITLP.ColumnCount = 5;
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.MainUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 346F));
            this.MainUITLP.Controls.Add(this.TWODImageBox, 2, 1);
            this.MainUITLP.Controls.Add(this.ZMapImageBox, 1, 1);
            this.MainUITLP.Controls.Add(this.THREEDPanelUI, 3, 0);
            this.MainUITLP.Controls.Add(this.TWODPanelUI, 2, 0);
            this.MainUITLP.Controls.Add(this.ZMapPanelUI, 1, 0);
            this.MainUITLP.Controls.Add(this.ButtonUITLP, 4, 1);
            this.MainUITLP.Controls.Add(this.MainUIPanel, 3, 1);
            this.MainUITLP.Controls.Add(this.BTN_IMAGE_OPEN, 4, 0);
            this.MainUITLP.Controls.Add(this.IntensityPanelUI, 0, 0);
            this.MainUITLP.Controls.Add(this.IntensityImageBox, 0, 1);
            this.MainUITLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainUITLP.Location = new System.Drawing.Point(0, 35);
            this.MainUITLP.Name = "MainUITLP";
            this.MainUITLP.RowCount = 2;
            this.MainUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.MainUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainUITLP.Size = new System.Drawing.Size(1920, 1045);
            this.MainUITLP.TabIndex = 0;
            this.MainUITLP.TagString = null;
            // 
            // TWODImageBox
            // 
            this.TWODImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TWODImageBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TWODImageBox.Location = new System.Drawing.Point(789, 43);
            this.TWODImageBox.Name = "TWODImageBox";
            this.TWODImageBox.Size = new System.Drawing.Size(387, 999);
            this.TWODImageBox.TabIndex = 14;
            // 
            // ZMapImageBox
            // 
            this.ZMapImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZMapImageBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZMapImageBox.Location = new System.Drawing.Point(396, 43);
            this.ZMapImageBox.Name = "ZMapImageBox";
            this.ZMapImageBox.Size = new System.Drawing.Size(387, 999);
            this.ZMapImageBox.TabIndex = 13;
            // 
            // THREEDPanelUI
            // 
            this.THREEDPanelUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.THREEDPanelUI.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.THREEDPanelUI.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.THREEDPanelUI.Location = new System.Drawing.Point(1182, 3);
            this.THREEDPanelUI.MinimumSize = new System.Drawing.Size(1, 1);
            this.THREEDPanelUI.Name = "THREEDPanelUI";
            this.THREEDPanelUI.Size = new System.Drawing.Size(387, 34);
            this.THREEDPanelUI.TabIndex = 11;
            this.THREEDPanelUI.Text = "3D";
            this.THREEDPanelUI.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TWODPanelUI
            // 
            this.TWODPanelUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TWODPanelUI.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.TWODPanelUI.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TWODPanelUI.Location = new System.Drawing.Point(789, 3);
            this.TWODPanelUI.MinimumSize = new System.Drawing.Size(1, 1);
            this.TWODPanelUI.Name = "TWODPanelUI";
            this.TWODPanelUI.Size = new System.Drawing.Size(387, 34);
            this.TWODPanelUI.TabIndex = 10;
            this.TWODPanelUI.Text = "2D";
            this.TWODPanelUI.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ZMapPanelUI
            // 
            this.ZMapPanelUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZMapPanelUI.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.ZMapPanelUI.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ZMapPanelUI.Location = new System.Drawing.Point(396, 3);
            this.ZMapPanelUI.MinimumSize = new System.Drawing.Size(1, 1);
            this.ZMapPanelUI.Name = "ZMapPanelUI";
            this.ZMapPanelUI.Size = new System.Drawing.Size(387, 34);
            this.ZMapPanelUI.TabIndex = 9;
            this.ZMapPanelUI.Text = "Z-Map";
            this.ZMapPanelUI.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ButtonUITLP
            // 
            this.ButtonUITLP.AutoSize = true;
            this.ButtonUITLP.ColumnCount = 1;
            this.ButtonUITLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ButtonUITLP.Controls.Add(this.tableLayoutPanel2, 0, 5);
            this.ButtonUITLP.Controls.Add(this.BTN_3D_VISION_LOG_DATA_CLEAR, 0, 10);
            this.ButtonUITLP.Controls.Add(this.BTN_3D_VISION_LOG_CLEAR, 0, 8);
            this.ButtonUITLP.Controls.Add(this.BTN_SHOW_ROI, 0, 2);
            this.ButtonUITLP.Controls.Add(this.BTN_SET_ROI, 0, 3);
            this.ButtonUITLP.Controls.Add(this.uiTableLayoutPanel1, 0, 1);
            this.ButtonUITLP.Controls.Add(this.TLP_INTENSITY, 0, 0);
            this.ButtonUITLP.Controls.Add(this.BTN_IMAGE_FUSION, 0, 6);
            this.ButtonUITLP.Controls.Add(this.LB_3D_VISION_LOG, 0, 7);
            this.ButtonUITLP.Controls.Add(this.GV_3D_VISION_LOG, 0, 9);
            this.ButtonUITLP.Controls.Add(this.tableLayoutPanel1, 0, 4);
            this.ButtonUITLP.Location = new System.Drawing.Point(1572, 40);
            this.ButtonUITLP.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.ButtonUITLP.Name = "ButtonUITLP";
            this.ButtonUITLP.RowCount = 11;
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.ButtonUITLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.ButtonUITLP.Size = new System.Drawing.Size(344, 910);
            this.ButtonUITLP.TabIndex = 0;
            this.ButtonUITLP.TagString = null;
            // 
            // BTN_3D_VISION_LOG_DATA_CLEAR
            // 
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_3D_VISION_LOG_DATA_CLEAR.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_3D_VISION_LOG_DATA_CLEAR.ForeColor = System.Drawing.Color.Black;
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Location = new System.Drawing.Point(3, 888);
            this.BTN_3D_VISION_LOG_DATA_CLEAR.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Name = "BTN_3D_VISION_LOG_DATA_CLEAR";
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Size = new System.Drawing.Size(338, 19);
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Symbol = 558684;
            this.BTN_3D_VISION_LOG_DATA_CLEAR.SymbolColor = System.Drawing.Color.Black;
            this.BTN_3D_VISION_LOG_DATA_CLEAR.TabIndex = 18;
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Text = "Data Clear";
            this.BTN_3D_VISION_LOG_DATA_CLEAR.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_3D_VISION_LOG_DATA_CLEAR.Click += new System.EventHandler(this.BTN_3D_VISION_LOG_DATA_CLEAR_Click);
            // 
            // BTN_3D_VISION_LOG_CLEAR
            // 
            this.BTN_3D_VISION_LOG_CLEAR.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_3D_VISION_LOG_CLEAR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_3D_VISION_LOG_CLEAR.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            this.BTN_3D_VISION_LOG_CLEAR.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_3D_VISION_LOG_CLEAR.ForeColor = System.Drawing.Color.Black;
            this.BTN_3D_VISION_LOG_CLEAR.Location = new System.Drawing.Point(3, 613);
            this.BTN_3D_VISION_LOG_CLEAR.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_3D_VISION_LOG_CLEAR.Name = "BTN_3D_VISION_LOG_CLEAR";
            this.BTN_3D_VISION_LOG_CLEAR.Size = new System.Drawing.Size(338, 19);
            this.BTN_3D_VISION_LOG_CLEAR.Symbol = 557913;
            this.BTN_3D_VISION_LOG_CLEAR.SymbolColor = System.Drawing.Color.Black;
            this.BTN_3D_VISION_LOG_CLEAR.TabIndex = 16;
            this.BTN_3D_VISION_LOG_CLEAR.Text = "Log Clear";
            this.BTN_3D_VISION_LOG_CLEAR.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_3D_VISION_LOG_CLEAR.Click += new System.EventHandler(this.BTN_3D_VISION_LOG_CLEAR_Click);
            // 
            // BTN_SHOW_ROI
            // 
            this.BTN_SHOW_ROI.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_SHOW_ROI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_SHOW_ROI.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            this.BTN_SHOW_ROI.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SHOW_ROI.ForeColor = System.Drawing.Color.Black;
            this.BTN_SHOW_ROI.Location = new System.Drawing.Point(3, 83);
            this.BTN_SHOW_ROI.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_SHOW_ROI.Name = "BTN_SHOW_ROI";
            this.BTN_SHOW_ROI.Size = new System.Drawing.Size(338, 34);
            this.BTN_SHOW_ROI.Symbol = 559445;
            this.BTN_SHOW_ROI.SymbolColor = System.Drawing.Color.Black;
            this.BTN_SHOW_ROI.TabIndex = 14;
            this.BTN_SHOW_ROI.Text = "Show ROI";
            this.BTN_SHOW_ROI.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SHOW_ROI.Click += new System.EventHandler(this.BTN_SHOW_ROI_Click);
            // 
            // BTN_SET_ROI
            // 
            this.BTN_SET_ROI.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_SET_ROI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_SET_ROI.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            this.BTN_SET_ROI.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SET_ROI.ForeColor = System.Drawing.Color.Black;
            this.BTN_SET_ROI.Location = new System.Drawing.Point(3, 123);
            this.BTN_SET_ROI.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_SET_ROI.Name = "BTN_SET_ROI";
            this.BTN_SET_ROI.Size = new System.Drawing.Size(338, 34);
            this.BTN_SET_ROI.Symbol = 559483;
            this.BTN_SET_ROI.SymbolColor = System.Drawing.Color.Black;
            this.BTN_SET_ROI.TabIndex = 13;
            this.BTN_SET_ROI.Text = "Set ROI";
            this.BTN_SET_ROI.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SET_ROI.Click += new System.EventHandler(this.BTN_SET_ROI_Click);
            // 
            // uiTableLayoutPanel1
            // 
            this.uiTableLayoutPanel1.ColumnCount = 2;
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.34101F));
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.65899F));
            this.uiTableLayoutPanel1.Controls.Add(this.BTN_ZMAP_PATH_SEARCH, 1, 0);
            this.uiTableLayoutPanel1.Controls.Add(this.PNL_ZMAP_PATH, 0, 0);
            this.uiTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTableLayoutPanel1.Location = new System.Drawing.Point(3, 43);
            this.uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            this.uiTableLayoutPanel1.RowCount = 1;
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.uiTableLayoutPanel1.Size = new System.Drawing.Size(338, 34);
            this.uiTableLayoutPanel1.TabIndex = 12;
            this.uiTableLayoutPanel1.TagString = null;
            // 
            // BTN_ZMAP_PATH_SEARCH
            // 
            this.BTN_ZMAP_PATH_SEARCH.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_ZMAP_PATH_SEARCH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_ZMAP_PATH_SEARCH.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_ZMAP_PATH_SEARCH.Location = new System.Drawing.Point(267, 3);
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
            this.PNL_ZMAP_PATH.Size = new System.Drawing.Size(256, 24);
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
            this.TLP_INTENSITY.Location = new System.Drawing.Point(3, 3);
            this.TLP_INTENSITY.Name = "TLP_INTENSITY";
            this.TLP_INTENSITY.RowCount = 1;
            this.TLP_INTENSITY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TLP_INTENSITY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_INTENSITY.Size = new System.Drawing.Size(338, 34);
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
            this.PNL_INTENSITY_PATH.Size = new System.Drawing.Size(256, 24);
            this.PNL_INTENSITY_PATH.TabIndex = 11;
            this.PNL_INTENSITY_PATH.Text = null;
            this.PNL_INTENSITY_PATH.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BTN_INENSITY_PATH_SEARCH
            // 
            this.BTN_INENSITY_PATH_SEARCH.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_INENSITY_PATH_SEARCH.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_INENSITY_PATH_SEARCH.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_INENSITY_PATH_SEARCH.Location = new System.Drawing.Point(267, 3);
            this.BTN_INENSITY_PATH_SEARCH.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_INENSITY_PATH_SEARCH.Name = "BTN_INENSITY_PATH_SEARCH";
            this.BTN_INENSITY_PATH_SEARCH.Size = new System.Drawing.Size(68, 28);
            this.BTN_INENSITY_PATH_SEARCH.TabIndex = 12;
            this.BTN_INENSITY_PATH_SEARCH.Text = "...";
            this.BTN_INENSITY_PATH_SEARCH.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_INENSITY_PATH_SEARCH.Click += new System.EventHandler(this.BTN_PICK_INTENSITY_Click);
            // 
            // BTN_IMAGE_FUSION
            // 
            this.BTN_IMAGE_FUSION.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_IMAGE_FUSION.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_IMAGE_FUSION.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            this.BTN_IMAGE_FUSION.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMAGE_FUSION.ForeColor = System.Drawing.Color.Black;
            this.BTN_IMAGE_FUSION.Location = new System.Drawing.Point(3, 323);
            this.BTN_IMAGE_FUSION.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_IMAGE_FUSION.Name = "BTN_IMAGE_FUSION";
            this.BTN_IMAGE_FUSION.Size = new System.Drawing.Size(338, 34);
            this.BTN_IMAGE_FUSION.Symbol = 559469;
            this.BTN_IMAGE_FUSION.SymbolColor = System.Drawing.Color.Black;
            this.BTN_IMAGE_FUSION.TabIndex = 5;
            this.BTN_IMAGE_FUSION.Text = "Fusion Run";
            this.BTN_IMAGE_FUSION.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMAGE_FUSION.Click += new System.EventHandler(this.BTN_IMAGE_FUSION_Click);
            // 
            // LB_3D_VISION_LOG
            // 
            this.LB_3D_VISION_LOG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LB_3D_VISION_LOG.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LB_3D_VISION_LOG.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.LB_3D_VISION_LOG.ItemSelectForeColor = System.Drawing.Color.White;
            this.LB_3D_VISION_LOG.Location = new System.Drawing.Point(4, 365);
            this.LB_3D_VISION_LOG.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LB_3D_VISION_LOG.MinimumSize = new System.Drawing.Size(1, 1);
            this.LB_3D_VISION_LOG.Name = "LB_3D_VISION_LOG";
            this.LB_3D_VISION_LOG.Padding = new System.Windows.Forms.Padding(2);
            this.LB_3D_VISION_LOG.ShowText = false;
            this.LB_3D_VISION_LOG.Size = new System.Drawing.Size(336, 240);
            this.LB_3D_VISION_LOG.TabIndex = 15;
            this.LB_3D_VISION_LOG.Text = null;
            // 
            // GV_3D_VISION_LOG
            // 
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.GV_3D_VISION_LOG.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.GV_3D_VISION_LOG.BackgroundColor = System.Drawing.Color.White;
            this.GV_3D_VISION_LOG.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.GV_3D_VISION_LOG.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.GV_3D_VISION_LOG.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.GV_3D_VISION_LOG.DefaultCellStyle = dataGridViewCellStyle3;
            this.GV_3D_VISION_LOG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GV_3D_VISION_LOG.EnableHeadersVisualStyles = false;
            this.GV_3D_VISION_LOG.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.GV_3D_VISION_LOG.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.GV_3D_VISION_LOG.Location = new System.Drawing.Point(3, 638);
            this.GV_3D_VISION_LOG.Name = "GV_3D_VISION_LOG";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.GV_3D_VISION_LOG.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.GV_3D_VISION_LOG.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.GV_3D_VISION_LOG.RowTemplate.Height = 23;
            this.GV_3D_VISION_LOG.SelectedIndex = -1;
            this.GV_3D_VISION_LOG.Size = new System.Drawing.Size(338, 244);
            this.GV_3D_VISION_LOG.StripeOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.GV_3D_VISION_LOG.TabIndex = 17;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.UP_Overlay, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.UP_Spec, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.UP_Zoff, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.UP_Zscale, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.UP_Sy, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.UP_Sx, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.uiPanel7, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.uiPanel6, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.uiPanel5, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.uiPanel4, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.uiPanel3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.uiPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.uiPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.UP_Centinal, 6, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 163);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(338, 74);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // UP_Overlay
            // 
            this.UP_Overlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UP_Overlay.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.UP_Overlay.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UP_Overlay.Location = new System.Drawing.Point(241, 38);
            this.UP_Overlay.Margin = new System.Windows.Forms.Padding(1);
            this.UP_Overlay.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_Overlay.Name = "UP_Overlay";
            this.UP_Overlay.Size = new System.Drawing.Size(46, 35);
            this.UP_Overlay.TabIndex = 13;
            this.UP_Overlay.Text = "0.25";
            this.UP_Overlay.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.UP_Overlay.Click += new System.EventHandler(this.UP_Overlay_Click);
            // 
            // UP_Spec
            // 
            this.UP_Spec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UP_Spec.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.UP_Spec.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UP_Spec.Location = new System.Drawing.Point(193, 38);
            this.UP_Spec.Margin = new System.Windows.Forms.Padding(1);
            this.UP_Spec.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_Spec.Name = "UP_Spec";
            this.UP_Spec.Size = new System.Drawing.Size(46, 35);
            this.UP_Spec.TabIndex = 12;
            this.UP_Spec.Text = "0.001";
            this.UP_Spec.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.UP_Spec.Click += new System.EventHandler(this.UP_Spec_Click);
            // 
            // UP_Zoff
            // 
            this.UP_Zoff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UP_Zoff.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.UP_Zoff.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UP_Zoff.Location = new System.Drawing.Point(145, 38);
            this.UP_Zoff.Margin = new System.Windows.Forms.Padding(1);
            this.UP_Zoff.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_Zoff.Name = "UP_Zoff";
            this.UP_Zoff.Size = new System.Drawing.Size(46, 35);
            this.UP_Zoff.TabIndex = 11;
            this.UP_Zoff.Text = "0";
            this.UP_Zoff.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.UP_Zoff.Click += new System.EventHandler(this.UP_Zoff_Click);
            // 
            // UP_Zscale
            // 
            this.UP_Zscale.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UP_Zscale.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.UP_Zscale.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UP_Zscale.Location = new System.Drawing.Point(97, 38);
            this.UP_Zscale.Margin = new System.Windows.Forms.Padding(1);
            this.UP_Zscale.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_Zscale.Name = "UP_Zscale";
            this.UP_Zscale.Size = new System.Drawing.Size(46, 35);
            this.UP_Zscale.TabIndex = 10;
            this.UP_Zscale.Text = "0.0041";
            this.UP_Zscale.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.UP_Zscale.Click += new System.EventHandler(this.UP_Zscale_Click);
            // 
            // UP_Sy
            // 
            this.UP_Sy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UP_Sy.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.UP_Sy.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UP_Sy.Location = new System.Drawing.Point(49, 38);
            this.UP_Sy.Margin = new System.Windows.Forms.Padding(1);
            this.UP_Sy.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_Sy.Name = "UP_Sy";
            this.UP_Sy.Size = new System.Drawing.Size(46, 35);
            this.UP_Sy.TabIndex = 9;
            this.UP_Sy.Text = "0.025";
            this.UP_Sy.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.UP_Sy.Click += new System.EventHandler(this.UP_Sy_Click);
            // 
            // UP_Sx
            // 
            this.UP_Sx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UP_Sx.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.UP_Sx.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UP_Sx.Location = new System.Drawing.Point(1, 38);
            this.UP_Sx.Margin = new System.Windows.Forms.Padding(1);
            this.UP_Sx.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_Sx.Name = "UP_Sx";
            this.UP_Sx.Size = new System.Drawing.Size(46, 35);
            this.UP_Sx.TabIndex = 8;
            this.UP_Sx.Text = "0.025";
            this.UP_Sx.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.UP_Sx.Click += new System.EventHandler(this.UP_Sx_Click);
            // 
            // uiPanel7
            // 
            this.uiPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel7.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel7.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.uiPanel7.Location = new System.Drawing.Point(289, 1);
            this.uiPanel7.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel7.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel7.Name = "uiPanel7";
            this.uiPanel7.Size = new System.Drawing.Size(48, 35);
            this.uiPanel7.TabIndex = 6;
            this.uiPanel7.Text = "Centinal";
            this.uiPanel7.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiPanel6
            // 
            this.uiPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel6.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.uiPanel6.Location = new System.Drawing.Point(241, 1);
            this.uiPanel6.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel6.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel6.Name = "uiPanel6";
            this.uiPanel6.Size = new System.Drawing.Size(46, 35);
            this.uiPanel6.TabIndex = 5;
            this.uiPanel6.Text = "Overlay";
            this.uiPanel6.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiPanel5
            // 
            this.uiPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel5.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.uiPanel5.Location = new System.Drawing.Point(193, 1);
            this.uiPanel5.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel5.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel5.Name = "uiPanel5";
            this.uiPanel5.Size = new System.Drawing.Size(46, 35);
            this.uiPanel5.TabIndex = 4;
            this.uiPanel5.Text = "Spec";
            this.uiPanel5.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiPanel4
            // 
            this.uiPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel4.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiPanel4.Location = new System.Drawing.Point(145, 1);
            this.uiPanel4.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel4.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel4.Name = "uiPanel4";
            this.uiPanel4.Size = new System.Drawing.Size(46, 35);
            this.uiPanel4.TabIndex = 3;
            this.uiPanel4.Text = "Z-Off";
            this.uiPanel4.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiPanel3
            // 
            this.uiPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel3.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.uiPanel3.Location = new System.Drawing.Point(97, 1);
            this.uiPanel3.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel3.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel3.Name = "uiPanel3";
            this.uiPanel3.Size = new System.Drawing.Size(46, 35);
            this.uiPanel3.TabIndex = 2;
            this.uiPanel3.Text = "Z-Scale";
            this.uiPanel3.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiPanel2
            // 
            this.uiPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel2.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiPanel2.Location = new System.Drawing.Point(49, 1);
            this.uiPanel2.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel2.Name = "uiPanel2";
            this.uiPanel2.Size = new System.Drawing.Size(46, 35);
            this.uiPanel2.TabIndex = 1;
            this.uiPanel2.Text = "Sy";
            this.uiPanel2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiPanel1
            // 
            this.uiPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiPanel1.Location = new System.Drawing.Point(1, 1);
            this.uiPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel1.Name = "uiPanel1";
            this.uiPanel1.Size = new System.Drawing.Size(46, 35);
            this.uiPanel1.TabIndex = 0;
            this.uiPanel1.Text = "Sx";
            this.uiPanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UP_Centinal
            // 
            this.UP_Centinal.ActiveText = "";
            this.UP_Centinal.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.UP_Centinal.InActiveText = "";
            this.UP_Centinal.Location = new System.Drawing.Point(291, 40);
            this.UP_Centinal.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_Centinal.Name = "UP_Centinal";
            this.UP_Centinal.Size = new System.Drawing.Size(44, 29);
            this.UP_Centinal.TabIndex = 7;
            // 
            // MainUIPanel
            // 
            this.MainUIPanel.Controls.Add(this.ViewerHost);
            this.MainUIPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainUIPanel.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.MainUIPanel.Location = new System.Drawing.Point(1183, 43);
            this.MainUIPanel.Margin = new System.Windows.Forms.Padding(4, 3, 0, 3);
            this.MainUIPanel.MinimumSize = new System.Drawing.Size(1, 1);
            this.MainUIPanel.Name = "MainUIPanel";
            this.MainUIPanel.Padding = new System.Windows.Forms.Padding(3);
            this.MainUIPanel.Size = new System.Drawing.Size(389, 999);
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
            this.ViewerHost.Size = new System.Drawing.Size(383, 993);
            this.ViewerHost.TabIndex = 0;
            this.ViewerHost.Text = "elementHost1";
            this.ViewerHost.Child = null;
            // 
            // BTN_IMAGE_OPEN
            // 
            this.BTN_IMAGE_OPEN.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BTN_IMAGE_OPEN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_IMAGE_OPEN.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(230)))), ((int)(((byte)(255)))));
            this.BTN_IMAGE_OPEN.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMAGE_OPEN.ForeColor = System.Drawing.Color.Black;
            this.BTN_IMAGE_OPEN.Location = new System.Drawing.Point(1575, 3);
            this.BTN_IMAGE_OPEN.MinimumSize = new System.Drawing.Size(1, 1);
            this.BTN_IMAGE_OPEN.Name = "BTN_IMAGE_OPEN";
            this.BTN_IMAGE_OPEN.Size = new System.Drawing.Size(342, 34);
            this.BTN_IMAGE_OPEN.Symbol = 61893;
            this.BTN_IMAGE_OPEN.SymbolColor = System.Drawing.Color.Black;
            this.BTN_IMAGE_OPEN.TabIndex = 7;
            this.BTN_IMAGE_OPEN.Text = "Image Open";
            this.BTN_IMAGE_OPEN.TipsFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMAGE_OPEN.Click += new System.EventHandler(this.BTN_IMAGE_OPEN_Click);
            // 
            // IntensityPanelUI
            // 
            this.IntensityPanelUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IntensityPanelUI.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.IntensityPanelUI.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.IntensityPanelUI.Location = new System.Drawing.Point(3, 3);
            this.IntensityPanelUI.MinimumSize = new System.Drawing.Size(1, 1);
            this.IntensityPanelUI.Name = "IntensityPanelUI";
            this.IntensityPanelUI.Size = new System.Drawing.Size(387, 34);
            this.IntensityPanelUI.TabIndex = 8;
            this.IntensityPanelUI.Text = "Intensity";
            this.IntensityPanelUI.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // IntensityImageBox
            // 
            this.IntensityImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IntensityImageBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IntensityImageBox.Location = new System.Drawing.Point(3, 43);
            this.IntensityImageBox.Name = "IntensityImageBox";
            this.IntensityImageBox.Size = new System.Drawing.Size(387, 999);
            this.IntensityImageBox.TabIndex = 12;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 7;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.UP_MinpxKernel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.uiPanel8, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.uiPanel20, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.UP_Minpixel, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 243);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(338, 74);
            this.tableLayoutPanel2.TabIndex = 20;
            // 
            // uiPanel20
            // 
            this.uiPanel20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel20.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel20.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiPanel20.Location = new System.Drawing.Point(1, 1);
            this.uiPanel20.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel20.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel20.Name = "uiPanel20";
            this.uiPanel20.Size = new System.Drawing.Size(46, 35);
            this.uiPanel20.TabIndex = 0;
            this.uiPanel20.Text = "MinPx";
            this.uiPanel20.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UP_Minpixel
            // 
            this.UP_Minpixel.Active = true;
            this.UP_Minpixel.ActiveText = "";
            this.UP_Minpixel.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.UP_Minpixel.InActiveText = "";
            this.UP_Minpixel.Location = new System.Drawing.Point(3, 40);
            this.UP_Minpixel.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_Minpixel.Name = "UP_Minpixel";
            this.UP_Minpixel.Size = new System.Drawing.Size(42, 29);
            this.UP_Minpixel.TabIndex = 7;
            // 
            // uiPanel8
            // 
            this.uiPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiPanel8.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.uiPanel8.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.uiPanel8.Location = new System.Drawing.Point(49, 1);
            this.uiPanel8.Margin = new System.Windows.Forms.Padding(1);
            this.uiPanel8.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel8.Name = "uiPanel8";
            this.uiPanel8.Size = new System.Drawing.Size(46, 35);
            this.uiPanel8.TabIndex = 8;
            this.uiPanel8.Text = "PxKernel";
            this.uiPanel8.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UP_MinpxKernel
            // 
            this.UP_MinpxKernel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UP_MinpxKernel.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.UP_MinpxKernel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UP_MinpxKernel.Location = new System.Drawing.Point(49, 38);
            this.UP_MinpxKernel.Margin = new System.Windows.Forms.Padding(1);
            this.UP_MinpxKernel.MinimumSize = new System.Drawing.Size(1, 1);
            this.UP_MinpxKernel.Name = "UP_MinpxKernel";
            this.UP_MinpxKernel.Size = new System.Drawing.Size(46, 35);
            this.UP_MinpxKernel.TabIndex = 14;
            this.UP_MinpxKernel.Text = "3";
            this.UP_MinpxKernel.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.UP_MinpxKernel.Click += new System.EventHandler(this.UP_MinpxKernel_Click);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.Controls.Add(this.MainUITLP);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "MainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 883, 641);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.MainUITLP.ResumeLayout(false);
            this.MainUITLP.PerformLayout();
            this.ButtonUITLP.ResumeLayout(false);
            this.uiTableLayoutPanel1.ResumeLayout(false);
            this.TLP_INTENSITY.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GV_3D_VISION_LOG)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.MainUIPanel.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
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
        private Sunny.UI.UIPanel IntensityPanelUI;
        private Sunny.UI.UIPanel TWODPanelUI;
        private Sunny.UI.UIPanel ZMapPanelUI;
        private Sunny.UI.UIPanel THREEDPanelUI;
        private Cyotek.Windows.Forms.ImageBox TWODImageBox;
        private Cyotek.Windows.Forms.ImageBox ZMapImageBox;
        private Cyotek.Windows.Forms.ImageBox IntensityImageBox;
        private Sunny.UI.UISymbolButton BTN_SET_ROI;
        private Sunny.UI.UISymbolButton BTN_SHOW_ROI;
        private Sunny.UI.UIListBox LB_3D_VISION_LOG;
        private Sunny.UI.UISymbolButton BTN_3D_VISION_LOG_CLEAR;
        private Sunny.UI.UIDataGridView GV_3D_VISION_LOG;
        private Sunny.UI.UISymbolButton BTN_3D_VISION_LOG_DATA_CLEAR;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Sunny.UI.UIPanel uiPanel7;
        private Sunny.UI.UIPanel uiPanel6;
        private Sunny.UI.UIPanel uiPanel5;
        private Sunny.UI.UIPanel uiPanel4;
        private Sunny.UI.UIPanel uiPanel3;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UISwitch UP_Centinal;
        private Sunny.UI.UIPanel UP_Sx;
        private Sunny.UI.UIPanel UP_Overlay;
        private Sunny.UI.UIPanel UP_Spec;
        private Sunny.UI.UIPanel UP_Zoff;
        private Sunny.UI.UIPanel UP_Zscale;
        private Sunny.UI.UIPanel UP_Sy;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Sunny.UI.UIPanel uiPanel20;
        private Sunny.UI.UISwitch UP_Minpixel;
        private Sunny.UI.UIPanel UP_MinpxKernel;
        private Sunny.UI.UIPanel uiPanel8;
    }
}

