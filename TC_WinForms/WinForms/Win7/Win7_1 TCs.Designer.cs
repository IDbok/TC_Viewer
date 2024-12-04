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
            Button button1;
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
            btnViewMode = new Button();
            btnDeleteTC = new Button();
            btnUpdateTC = new Button();
            btnCreateTC = new Button();
            pnlDataViewer = new Panel();
            progressBar = new ProgressBar();
            button1 = new Button();
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
            dgvMain.AllowUserToDeleteRows = false;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 0);
            dgvMain.Margin = new Padding(3, 2, 3, 2);
            dgvMain.Name = "dgvMain";
            dgvMain.ReadOnly = true;
            dgvMain.RowHeadersWidth = 51;
            dgvMain.RowTemplate.Height = 29;
            dgvMain.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMain.Size = new Size(1308, 278);
            dgvMain.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlFilters);
            pnlControls.Controls.Add(pnlControlBtns);
            pnlControls.Dock = DockStyle.Top;
            pnlControls.Location = new Point(0, 0);
            pnlControls.Margin = new Padding(3, 2, 3, 2);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(1308, 60);
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
            pnlFilters.Dock = DockStyle.Fill;
            pnlFilters.Location = new Point(0, 0);
            pnlFilters.Margin = new Padding(3, 2, 3, 2);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(487, 60);
            pnlFilters.TabIndex = 25;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(228, 36);
            lblType.Margin = new Padding(2, 0, 2, 0);
            lblType.Name = "lblType";
            lblType.Size = new Size(30, 15);
            lblType.TabIndex = 27;
            lblType.Text = "Тип:";
            // 
            // cbxTypeFilter
            // 
            cbxTypeFilter.FormattingEnabled = true;
            cbxTypeFilter.Location = new Point(267, 34);
            cbxTypeFilter.Margin = new Padding(2);
            cbxTypeFilter.Name = "cbxTypeFilter";
            cbxTypeFilter.Size = new Size(113, 23);
            cbxTypeFilter.TabIndex = 26;
            cbxTypeFilter.SelectedIndexChanged += cbxType_SelectedIndexChanged;
            // 
            // lblVoltageFilter
            // 
            lblVoltageFilter.AutoSize = true;
            lblVoltageFilter.Location = new Point(177, 10);
            lblVoltageFilter.Margin = new Padding(2, 0, 2, 0);
            lblVoltageFilter.Name = "lblVoltageFilter";
            lblVoltageFilter.Size = new Size(80, 15);
            lblVoltageFilter.TabIndex = 25;
            lblVoltageFilter.Text = "Напряжение:";
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(9, 16);
            lblSearch.Margin = new Padding(2, 0, 2, 0);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(45, 15);
            lblSearch.TabIndex = 24;
            lblSearch.Text = "Поиск:";
            // 
            // cbxNetworkVoltageFilter
            // 
            cbxNetworkVoltageFilter.FormattingEnabled = true;
            cbxNetworkVoltageFilter.Location = new Point(267, 10);
            cbxNetworkVoltageFilter.Margin = new Padding(2);
            cbxNetworkVoltageFilter.Name = "cbxNetworkVoltageFilter";
            cbxNetworkVoltageFilter.Size = new Size(113, 23);
            cbxNetworkVoltageFilter.TabIndex = 23;
            cbxNetworkVoltageFilter.SelectedIndexChanged += cbxNetworkVoltageFilter_SelectedIndexChanged;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(9, 32);
            txtSearch.Margin = new Padding(2);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(158, 23);
            txtSearch.TabIndex = 22;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // pnlControlBtns
            // 
            pnlControlBtns.Controls.Add(button1);
            pnlControlBtns.Controls.Add(btnViewMode);
            pnlControlBtns.Controls.Add(btnDeleteTC);
            pnlControlBtns.Controls.Add(btnUpdateTC);
            pnlControlBtns.Controls.Add(btnCreateTC);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(487, 0);
            pnlControlBtns.Margin = new Padding(3, 2, 3, 2);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(821, 60);
            pnlControlBtns.TabIndex = 24;
            // 
            // btnViewMode
            // 
            btnViewMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnViewMode.Location = new Point(281, 9);
            btnViewMode.Margin = new Padding(3, 2, 3, 2);
            btnViewMode.Name = "btnViewMode";
            btnViewMode.Size = new Size(122, 45);
            btnViewMode.TabIndex = 26;
            btnViewMode.Text = "Просмотр";
            btnViewMode.UseVisualStyleBackColor = true;
            btnViewMode.Click += btnViewMode_Click;
            // 
            // btnDeleteTC
            // 
            btnDeleteTC.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeleteTC.Location = new Point(688, 9);
            btnDeleteTC.Margin = new Padding(3, 2, 3, 2);
            btnDeleteTC.Name = "btnDeleteTC";
            btnDeleteTC.Size = new Size(122, 45);
            btnDeleteTC.TabIndex = 25;
            btnDeleteTC.Text = "Удалить";
            btnDeleteTC.UseVisualStyleBackColor = true;
            btnDeleteTC.Click += btnDeleteTC_Click;
            // 
            // btnUpdateTC
            // 
            btnUpdateTC.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUpdateTC.Location = new Point(552, 9);
            btnUpdateTC.Margin = new Padding(3, 2, 3, 2);
            btnUpdateTC.Name = "btnUpdateTC";
            btnUpdateTC.Size = new Size(122, 45);
            btnUpdateTC.TabIndex = 24;
            btnUpdateTC.Text = "Редактировать";
            btnUpdateTC.UseVisualStyleBackColor = true;
            btnUpdateTC.Click += btnUpdateTC_Click;
            // 
            // btnCreateTC
            // 
            btnCreateTC.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCreateTC.Location = new Point(417, 9);
            btnCreateTC.Margin = new Padding(3, 2, 3, 2);
            btnCreateTC.Name = "btnCreateTC";
            btnCreateTC.Size = new Size(122, 45);
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
            pnlDataViewer.Location = new Point(0, 60);
            pnlDataViewer.Margin = new Padding(3, 2, 3, 2);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1308, 278);
            pnlDataViewer.TabIndex = 2;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.None;
            progressBar.Location = new Point(463, 50);
            progressBar.Margin = new Padding(2);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(395, 20);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 1;
            progressBar.Visible = false;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = SystemColors.Control;
            button1.Location = new Point(153, 9);
            button1.Margin = new Padding(3, 2, 3, 2);
            button1.Name = "button1";
            button1.Size = new Size(122, 45);
            button1.TabIndex = 27;
            button1.Text = "Копировать";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // Win7_1_TCs
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1308, 338);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Win7_1_TCs";
            Text = "Win7_1_TCs";
            Load += Win7_1_TCs_Load;
            SizeChanged += Win7_1_TCs_SizeChanged;
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
        private Button btnViewMode;
    }
}