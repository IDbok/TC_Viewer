namespace TC_WinForms.WinForms
{
    partial class Win7_TechTransition
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
            cbxShowUnReleased = new CheckBox();
            lblCategoryFilter = new Label();
            cbxCategoryFilter = new ComboBox();
            lblSearch = new Label();
            txtSearch = new TextBox();
            pnlControlBtns = new Panel();
            btnUpdate = new Button();
            btnDeleteObj = new Button();
            btnAddNewObj = new Button();
            pnlDataViewer = new Panel();
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
            dgvMain.Margin = new Padding(4);
            dgvMain.Name = "dgvMain";
            dgvMain.ReadOnly = true;
            dgvMain.RowHeadersWidth = 51;
            dgvMain.RowTemplate.Height = 29;
            dgvMain.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMain.Size = new Size(1291, 462);
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
            pnlControls.Size = new Size(1291, 100);
            pnlControls.TabIndex = 1;
            // 
            // pnlFilters
            // 
            pnlFilters.Controls.Add(cbxShowUnReleased);
            pnlFilters.Controls.Add(lblCategoryFilter);
            pnlFilters.Controls.Add(cbxCategoryFilter);
            pnlFilters.Controls.Add(lblSearch);
            pnlFilters.Controls.Add(txtSearch);
            pnlFilters.Dock = DockStyle.Left;
            pnlFilters.Location = new Point(0, 0);
            pnlFilters.Margin = new Padding(4);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(652, 100);
            pnlFilters.TabIndex = 25;
            // 
            // cbxShowUnReleased
            // 
            cbxShowUnReleased.AutoSize = true;
            cbxShowUnReleased.Location = new Point(12, 68);
            cbxShowUnReleased.Name = "cbxShowUnReleased";
            cbxShowUnReleased.Size = new Size(277, 29);
            cbxShowUnReleased.TabIndex = 31;
            cbxShowUnReleased.Text = "Показать неопубликованные";
            cbxShowUnReleased.UseVisualStyleBackColor = true;
            cbxShowUnReleased.CheckedChanged += cbxShowUnReleased_CheckedChanged;
            // 
            // lblCategoryFilter
            // 
            lblCategoryFilter.AutoSize = true;
            lblCategoryFilter.Location = new Point(280, 3);
            lblCategoryFilter.Name = "lblCategoryFilter";
            lblCategoryFilter.Size = new Size(99, 25);
            lblCategoryFilter.TabIndex = 30;
            lblCategoryFilter.Text = "Категория:";
            // 
            // cbxCategoryFilter
            // 
            cbxCategoryFilter.FormattingEnabled = true;
            cbxCategoryFilter.Location = new Point(280, 31);
            cbxCategoryFilter.Name = "cbxCategoryFilter";
            cbxCategoryFilter.Size = new Size(167, 33);
            cbxCategoryFilter.TabIndex = 29;
            cbxCategoryFilter.SelectedIndexChanged += cbxCategoryFilter_SelectedIndexChanged;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(12, 5);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(67, 25);
            lblSearch.TabIndex = 28;
            lblSearch.Text = "Поиск:";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(12, 33);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(224, 31);
            txtSearch.TabIndex = 27;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // pnlControlBtns
            // 
            pnlControlBtns.Controls.Add(btnUpdate);
            pnlControlBtns.Controls.Add(btnDeleteObj);
            pnlControlBtns.Controls.Add(btnAddNewObj);
            pnlControlBtns.Dock = DockStyle.Right;
            pnlControlBtns.Location = new Point(701, 0);
            pnlControlBtns.Margin = new Padding(4);
            pnlControlBtns.Name = "pnlControlBtns";
            pnlControlBtns.Size = new Size(590, 100);
            pnlControlBtns.TabIndex = 24;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(220, 15);
            btnUpdate.Margin = new Padding(4);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(170, 75);
            btnUpdate.TabIndex = 26;
            btnUpdate.Text = "Редактировать";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDeleteObj
            // 
            btnDeleteObj.Location = new Point(400, 15);
            btnDeleteObj.Margin = new Padding(4);
            btnDeleteObj.Name = "btnDeleteObj";
            btnDeleteObj.Size = new Size(170, 75);
            btnDeleteObj.TabIndex = 25;
            btnDeleteObj.Text = "Удалить";
            btnDeleteObj.UseVisualStyleBackColor = true;
            btnDeleteObj.Click += btnDeleteObj_Click;
            // 
            // btnAddNewObj
            // 
            btnAddNewObj.Location = new Point(40, 15);
            btnAddNewObj.Margin = new Padding(4);
            btnAddNewObj.Name = "btnAddNewObj";
            btnAddNewObj.Size = new Size(170, 75);
            btnAddNewObj.TabIndex = 23;
            btnAddNewObj.Text = "Добавить";
            btnAddNewObj.UseVisualStyleBackColor = true;
            btnAddNewObj.Click += btnAddNewObj_Click;
            // 
            // pnlDataViewer
            // 
            pnlDataViewer.Controls.Add(dgvMain);
            pnlDataViewer.Dock = DockStyle.Fill;
            pnlDataViewer.Location = new Point(0, 100);
            pnlDataViewer.Margin = new Padding(4);
            pnlDataViewer.Name = "pnlDataViewer";
            pnlDataViewer.Size = new Size(1291, 462);
            pnlDataViewer.TabIndex = 2;
            // 
            // Win7_TechTransition
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1291, 562);
            Controls.Add(pnlDataViewer);
            Controls.Add(pnlControls);
            Margin = new Padding(4);
            Name = "Win7_TechTransition";
            Text = "Win8_TechTransition";
            FormClosing += Win7_TechTransition_FormClosing;
            Load += Win7_TechTransition_Load;
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
        private Button btnDeleteObj;
        private Button btnAddNewObj;
        private Panel pnlFilters;
        private Panel pnlDataViewer;
        private Label lblSearch;
        private TextBox txtSearch;
        private Label lblCategoryFilter;
        private ComboBox cbxCategoryFilter;
        private Button btnUpdate;
        private CheckBox cbxShowUnReleased;
    }
}