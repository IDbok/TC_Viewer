namespace TC_WinForms.WinForms
{
    partial class Win7_1_TCs
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvMain = new DataGridView();
            pnlControls = new Panel();
            pnlFilters = new Panel();
            lblType = new Label();
            cbxTypeFilter = new ComboBox();
            lblVoltageFilter = new Label();
            lblSearch = new Label();
            cbxNetworkVoltageFilter = new ComboBox();
            txtSearch = new TextBox();
            pnlControlBtns = new Panel();
            btnDeleteTC = new Button();
            btnUpdateTC = new Button();
            btnCreateTC = new Button();
            pnlDataViewer = new Panel();
            progressBar = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            pnlControls.SuspendLayout();
            pnlFilters.SuspendLayout();
            pnlControlBtns.SuspendLayout();
            pnlDataViewer.SuspendLayout();
            SuspendLayout();
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 0);
            dgvMain.Margin = new Padding(4);
            dgvMain.Name = "dgvMain";
            dgvMain.RowHeadersWidth = 51;
            dgvMain.RowTemplate.Height = 29;
            dgvMain.Size = new Size(1430, 462);
            dgvMain.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlFilters);
            pnlControls.Controls.Add(pnlControlBtns);
            pnlControls.Dock = DockStyle.Top;
            pnlControls.Location = new Point(0, 0);
            pnlControls.Margin = new Padding(4);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(1430, 100);
            pnlControls.TabIndex = 1;
            // 
            // pnlFilters
            // 
            pnlFilters.Controls.Add(lblType);
            pnlFilters.Controls.Add(cbxTypeFilter);
            pnlFilters.Controls.Add(lblVoltageFilter);
            pnlFilters.Controls.Add(lblSearch);
            pnlFilters.Controls.Add(cbxNetworkVoltageFilter);
            pnlFilters.Controls.Add(txtSearch);
            pnlFilters.Dock = DockStyle.Left;
            pnlFilters.Location = new Point(0, 0);
            pnlFilters.Margin = new Padding(4);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(813, 100);
            pnlFilters.TabIndex = 25;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(326, 60);
            lblType.Name = "lblType";
            lblType.Size = new Size(45, 25);
            lblType.TabIndex = 27;
            lblType.Text = "Тип:";
            // 
            // cbxType
            // 
            cbxTypeFilter.FormattingEnabled = true;
            cbxTypeFilter.Location = new Point(369, 57);
            cbxTypeFilter.Name = "cbxType";
            cbxTypeFilter.Size = new Size(160, 33);
            cbxTypeFilter.TabIndex = 26;
            cbxTypeFilter.SelectedIndexChanged += cbxType_SelectedIndexChanged;
            // 
            // lblVoltageFilter
            // 
            lblVoltageFilter.AutoSize = true;
            lblVoltageFilter.Location = new Point(252, 18);
            lblVoltageFilter.Name = "lblVoltageFilter";
            lblVoltageFilter.Size = new Size(119, 25);
            lblVoltageFilter.TabIndex = 25;
            lblVoltageFilter.Text = "Напряжение:";
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(12, 26);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(67, 25);
            lblSearch.TabIndex = 24;
            lblSearch.Text = "Поиск:";
            // 
            // cbxNetworkVoltageFilter
            // 
            cbxNetworkVoltageFilter.FormattingEnabled = true;
            cbxNetworkVoltageFilter.Location = new Point(369, 18);
            cbxNetworkVoltageFilter.Name = "cbxNetworkVoltageFilter";
            cbxNetworkVoltageFilter.Size = new Size(160, 33);
            cbxNetworkVoltageFilter.TabIndex = 23;
            cbxNetworkVoltageFilter.SelectedIndexChanged += cbxNetworkVoltageFilter_SelectedIndexChanged;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(12, 54);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(224, 31);
            txtSearch.TabIndex = 22;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // pnlControlBtns
            // 
            pnlControlBtns.Controls.Add(btnDeleteTC);
            pnlControlBtns.Controls.Add(btnUpdateTC);
            pnlControlBtns.Controls.Add(btnCreateTC);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(840, 0);
            pnlControlBtns.Margin = new Padding(4);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(590, 100);
            pnlControlBtns.TabIndex = 24;
            // 
            // btnDeleteTC
            // 
            btnDeleteTC.Location = new Point(400, 15);
            btnDeleteTC.Margin = new Padding(4);
            btnDeleteTC.Name = "btnDeleteTC";
            btnDeleteTC.Size = new Size(174, 75);
            btnDeleteTC.TabIndex = 25;
            btnDeleteTC.Text = "Удалить";
            btnDeleteTC.UseVisualStyleBackColor = true;
            btnDeleteTC.Click += btnDeleteTC_Click;
            // 
            // btnUpdateTC
            // 
            btnUpdateTC.Location = new Point(206, 15);
            btnUpdateTC.Margin = new Padding(4);
            btnUpdateTC.Name = "btnUpdateTC";
            btnUpdateTC.Size = new Size(174, 75);
            btnUpdateTC.TabIndex = 24;
            btnUpdateTC.Text = "Редактировать";
            btnUpdateTC.UseVisualStyleBackColor = true;
            btnUpdateTC.Click += btnUpdateTC_Click;
            // 
            // btnCreateTC
            // 
            btnCreateTC.Location = new Point(10, 15);
            btnCreateTC.Margin = new Padding(4);
            btnCreateTC.Name = "btnCreateTC";
            btnCreateTC.Size = new Size(174, 75);
            btnCreateTC.TabIndex = 23;
            btnCreateTC.Text = "Добавить";
            btnCreateTC.UseVisualStyleBackColor = true;
            btnCreateTC.Click += btnCreateTC_Click;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Controls.Add(progressBar);
            pnlDataViewer.Controls.Add(dgvMain);
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(0, 100);
            pnlDataViewer.Margin = new Padding(4);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1430, 462);
            pnlDataViewer.TabIndex = 2;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.None;
            progressBar.Location = new Point(441, 84);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(564, 34);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 1;
            progressBar.Visible = false;
            // 
            // Win7_1_TCs
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1430, 562);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Margin = new Padding(4);
            Name = "Win7_1_TCs";
            Text = "Win7_1_TCs";
            Load += Win7_1_TCs_Load;
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            pnlControls.ResumeLayout(false);
            pnlFilters.ResumeLayout(false);
            pnlFilters.PerformLayout();
            pnlControlBtns.ResumeLayout(false);
            pnlDataViewer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvMain;
        private Panel pnlControls;
        private Panel pnlControlBtns;
        private Button btnDeleteTC;
        private Button btnUpdateTC;
        private Button btnCreateTC;
        private Panel pnlFilters;
        private ComboBox cmbProjectName;
        private Panel pnlDataViewer;
        private ProgressBar progressBar;
        private TextBox txtSearch;
        private ComboBox cbxNetworkVoltageFilter;
        private Label lblSearch;
        private Label lblVoltageFilter;
        private Label lblType;
        private ComboBox cbxTypeFilter;
    }
}