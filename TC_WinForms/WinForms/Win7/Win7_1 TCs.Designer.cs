﻿namespace TC_WinForms.WinForms
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
			lblStatusFilter = new Label();
			cbxStatusFilter = new ComboBox();
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
			pnlFilterAdditional = new Panel();
			((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
			pnlControls.SuspendLayout();
			pnlFilters.SuspendLayout();
			pnlControlBtns.SuspendLayout();
			pnlDataViewer.SuspendLayout();
			pnlFilterAdditional.SuspendLayout();
			SuspendLayout();
			// 
			// dgvMain
			// 
			dgvMain.AllowUserToAddRows = false;
			dgvMain.AllowUserToDeleteRows = false;
			dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dgvMain.Dock = DockStyle.Fill;
			dgvMain.Location = new Point(0, 0);
			dgvMain.Margin = new Padding(4, 3, 4, 3);
			dgvMain.Name = "dgvMain";
			dgvMain.ReadOnly = true;
			dgvMain.RowHeadersWidth = 51;
			dgvMain.RowTemplate.Height = 29;
			dgvMain.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvMain.Size = new Size(1287, 393);
			dgvMain.TabIndex = 0;
			// 
			// pnlControls
			// 
			pnlControls.Controls.Add(pnlFilters);
			pnlControls.Controls.Add(pnlControlBtns);
			pnlControls.Dock = DockStyle.Top;
			pnlControls.Location = new Point(0, 0);
			pnlControls.Margin = new Padding(4, 3, 4, 3);
			pnlControls.Name = "pnlControls";
			pnlControls.Size = new Size(1287, 125);
			pnlControls.TabIndex = 1;
			// 
			// pnlFilters
			// 
			pnlFilters.Controls.Add(pnlFilterAdditional);
			pnlFilters.Controls.Add(lblType);
			pnlFilters.Controls.Add(cbxTypeFilter);
			pnlFilters.Controls.Add(lblVoltageFilter);
			pnlFilters.Controls.Add(lblSearch);
			pnlFilters.Controls.Add(cbxNetworkVoltageFilter);
			pnlFilters.Controls.Add(txtSearch);
			pnlFilters.Dock = DockStyle.Fill;
			pnlFilters.Location = new Point(0, 0);
			pnlFilters.Margin = new Padding(4, 3, 4, 3);
			pnlFilters.Name = "pnlFilters";
			pnlFilters.Size = new Size(548, 125);
			pnlFilters.TabIndex = 25;
			// 
			// lblStatusFilter
			// 
			lblStatusFilter.AutoSize = true;
			lblStatusFilter.Location = new Point(273, 5);
			lblStatusFilter.Name = "lblStatusFilter";
			lblStatusFilter.Size = new Size(64, 23);
			lblStatusFilter.TabIndex = 29;
			lblStatusFilter.Text = "Статус:";
			// 
			// cbxStatusFilter
			// 
			cbxStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
			cbxStatusFilter.FormattingEnabled = true;
			cbxStatusFilter.Location = new Point(342, 3);
			cbxStatusFilter.Name = "cbxStatusFilter";
			cbxStatusFilter.Size = new Size(144, 31);
			cbxStatusFilter.TabIndex = 28;
			// 
			// lblType
			// 
			lblType.AutoSize = true;
			lblType.Location = new Point(292, 47);
			lblType.Name = "lblType";
			lblType.Size = new Size(43, 23);
			lblType.TabIndex = 27;
			lblType.Text = "Тип:";
			// 
			// cbxTypeFilter
			// 
			cbxTypeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
			cbxTypeFilter.FormattingEnabled = true;
			cbxTypeFilter.Location = new Point(342, 44);
			cbxTypeFilter.Name = "cbxTypeFilter";
			cbxTypeFilter.Size = new Size(144, 31);
			cbxTypeFilter.TabIndex = 26;
			// 
			// lblVoltageFilter
			// 
			lblVoltageFilter.AutoSize = true;
			lblVoltageFilter.Location = new Point(227, 7);
			lblVoltageFilter.Name = "lblVoltageFilter";
			lblVoltageFilter.Size = new Size(115, 23);
			lblVoltageFilter.TabIndex = 25;
			lblVoltageFilter.Text = "Напряжение:";
			// 
			// lblSearch
			// 
			lblSearch.AutoSize = true;
			lblSearch.Location = new Point(11, 23);
			lblSearch.Name = "lblSearch";
			lblSearch.Size = new Size(62, 23);
			lblSearch.TabIndex = 24;
			lblSearch.Text = "Поиск:";
			// 
			// cbxNetworkVoltageFilter
			// 
			cbxNetworkVoltageFilter.DropDownStyle = ComboBoxStyle.DropDownList;
			cbxNetworkVoltageFilter.FormattingEnabled = true;
			cbxNetworkVoltageFilter.Location = new Point(342, 7);
			cbxNetworkVoltageFilter.Name = "cbxNetworkVoltageFilter";
			cbxNetworkVoltageFilter.Size = new Size(144, 31);
			cbxNetworkVoltageFilter.TabIndex = 23;
			// 
			// txtSearch
			// 
			txtSearch.Location = new Point(11, 47);
			txtSearch.Name = "txtSearch";
			txtSearch.Size = new Size(202, 30);
			txtSearch.TabIndex = 22;
			txtSearch.TextChanged += txtSearch_TextChanged;
			// 
			// pnlControlBtns
			// 
			pnlControlBtns.Controls.Add(btnViewMode);
			pnlControlBtns.Controls.Add(btnDeleteTC);
			pnlControlBtns.Controls.Add(btnUpdateTC);
			pnlControlBtns.Controls.Add(btnCreateTC);
			pnlControlBtns.Dock = DockStyle.Right;
			pnlControlBtns.Location = new Point(548, 0);
			pnlControlBtns.Margin = new Padding(4, 3, 4, 3);
			pnlControlBtns.Name = "pnlControlBtns";
			pnlControlBtns.Size = new Size(739, 125);
			pnlControlBtns.TabIndex = 24;
			// 
			// btnViewMode
			// 
			btnViewMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnViewMode.Location = new Point(45, 14);
			btnViewMode.Margin = new Padding(4, 3, 4, 3);
			btnViewMode.Name = "btnViewMode";
			btnViewMode.Size = new Size(157, 60);
			btnViewMode.TabIndex = 26;
			btnViewMode.Text = "Просмотр";
			btnViewMode.UseVisualStyleBackColor = true;
			btnViewMode.Click += btnViewMode_Click;
			// 
			// btnDeleteTC
			// 
			btnDeleteTC.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnDeleteTC.Location = new Point(568, 14);
			btnDeleteTC.Margin = new Padding(4, 3, 4, 3);
			btnDeleteTC.Name = "btnDeleteTC";
			btnDeleteTC.Size = new Size(157, 60);
			btnDeleteTC.TabIndex = 25;
			btnDeleteTC.Text = "Удалить";
			btnDeleteTC.UseVisualStyleBackColor = true;
			btnDeleteTC.Click += btnDeleteTC_Click;
			// 
			// btnUpdateTC
			// 
			btnUpdateTC.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnUpdateTC.Location = new Point(393, 14);
			btnUpdateTC.Margin = new Padding(4, 3, 4, 3);
			btnUpdateTC.Name = "btnUpdateTC";
			btnUpdateTC.Size = new Size(157, 60);
			btnUpdateTC.TabIndex = 24;
			btnUpdateTC.Text = "Редактировать";
			btnUpdateTC.UseVisualStyleBackColor = true;
			btnUpdateTC.Click += btnUpdateTC_Click;
			// 
			// btnCreateTC
			// 
			btnCreateTC.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnCreateTC.Location = new Point(220, 14);
			btnCreateTC.Margin = new Padding(4, 3, 4, 3);
			btnCreateTC.Name = "btnCreateTC";
			btnCreateTC.Size = new Size(157, 60);
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
			pnlDataViewer.Location = new Point(0, 125);
			pnlDataViewer.Margin = new Padding(4, 3, 4, 3);
			pnlDataViewer.Name = "pnlDataViewer";
			pnlDataViewer.Size = new Size(1287, 393);
			pnlDataViewer.TabIndex = 2;
			// 
			// progressBar
			// 
			progressBar.Anchor = AnchorStyles.None;
			progressBar.Location = new Point(397, 60);
			progressBar.MarqueeAnimationSpeed = 30;
			progressBar.Name = "progressBar";
			progressBar.Size = new Size(508, 30);
			progressBar.Style = ProgressBarStyle.Marquee;
			progressBar.TabIndex = 1;
			progressBar.Visible = false;
			// 
			// pnlFilterAdditional
			// 
			pnlFilterAdditional.Controls.Add(cbxStatusFilter);
			pnlFilterAdditional.Controls.Add(lblStatusFilter);
			pnlFilterAdditional.Dock = DockStyle.Bottom;
			pnlFilterAdditional.Location = new Point(0, 81);
			pnlFilterAdditional.Name = "pnlFilterAdditional";
			pnlFilterAdditional.Size = new Size(548, 44);
			pnlFilterAdditional.TabIndex = 30;
			// 
			// Win7_1_TCs
			// 
			AutoScaleDimensions = new SizeF(9F, 23F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1287, 518);
			Controls.Add(pnlDataViewer);
			Controls.Add(pnlControls);
			Margin = new Padding(4, 3, 4, 3);
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
			pnlFilterAdditional.ResumeLayout(false);
			pnlFilterAdditional.PerformLayout();
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
		private Label lblStatusFilter;
		private ComboBox cbxStatusFilter;
		private Panel pnlFilterAdditional;
	}
}