namespace TC_WinForms.WinForms
{
    partial class Win7_BLockService
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
            tlpnlBlockService = new TableLayoutPanel();
            pnlControls = new Panel();
            cmbType = new ComboBox();
            btnUpdate = new Button();
            btnDelete = new Button();
            txtSearch = new TextBox();
            pnlDgv = new Panel();
            dgvMain = new DataGridView();
            idColumn = new DataGridViewTextBoxColumn();
            typeColumn = new DataGridViewTextBoxColumn();
            objectIdColumn = new DataGridViewTextBoxColumn();
            nameColumn = new DataGridViewTextBoxColumn();
            timeStampColumn = new DataGridViewTextBoxColumn();
            tlpnlBlockService.SuspendLayout();
            pnlControls.SuspendLayout();
            pnlDgv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            SuspendLayout();
            // 
            // tlpnlBlockService
            // 
            tlpnlBlockService.ColumnCount = 1;
            tlpnlBlockService.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlBlockService.Controls.Add(pnlControls, 0, 0);
            tlpnlBlockService.Controls.Add(pnlDgv, 0, 1);
            tlpnlBlockService.Dock = DockStyle.Fill;
            tlpnlBlockService.Location = new Point(0, 0);
            tlpnlBlockService.Name = "tlpnlBlockService";
            tlpnlBlockService.RowCount = 2;
            tlpnlBlockService.RowStyles.Add(new RowStyle(SizeType.Percent, 12.8904686F));
            tlpnlBlockService.RowStyles.Add(new RowStyle(SizeType.Percent, 87.1095352F));
            tlpnlBlockService.Size = new Size(707, 295);
            tlpnlBlockService.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(cmbType);
            pnlControls.Controls.Add(btnUpdate);
            pnlControls.Controls.Add(btnDelete);
            pnlControls.Controls.Add(txtSearch);
            pnlControls.Dock = DockStyle.Fill;
            pnlControls.Location = new Point(3, 3);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(701, 32);
            pnlControls.TabIndex = 0;
            // 
            // cmbType
            // 
            cmbType.FormattingEnabled = true;
            cmbType.Location = new Point(308, 4);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(121, 23);
            cmbType.TabIndex = 3;
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(500, 4);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(99, 23);
            btnUpdate.TabIndex = 2;
            btnUpdate.Text = "Обновить";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(605, 4);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(85, 23);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "Удалить";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(299, 23);
            txtSearch.TabIndex = 0;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // pnlDgv
            // 
            pnlDgv.Controls.Add(dgvMain);
            pnlDgv.Dock = DockStyle.Fill;
            pnlDgv.Location = new Point(3, 41);
            pnlDgv.Name = "pnlDgv";
            pnlDgv.Size = new Size(701, 251);
            pnlDgv.TabIndex = 1;
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.AllowUserToDeleteRows = false;
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Columns.AddRange(new DataGridViewColumn[] { idColumn, typeColumn, objectIdColumn, nameColumn, timeStampColumn });
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 0);
            dgvMain.Name = "dgvMain";
            dgvMain.ReadOnly = true;
            dgvMain.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMain.Size = new Size(701, 251);
            dgvMain.TabIndex = 0;
            // 
            // idColumn
            // 
            idColumn.HeaderText = "ID";
            idColumn.Name = "idColumn";
            idColumn.ReadOnly = true;
            idColumn.Visible = false;
            idColumn.Width = 43;
            // 
            // typeColumn
            // 
            typeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            typeColumn.HeaderText = "Вид объекта";
            typeColumn.Name = "typeColumn";
            typeColumn.ReadOnly = true;
            // 
            // objectIdColumn
            // 
            objectIdColumn.HeaderText = "ID Объекта";
            objectIdColumn.Name = "objectIdColumn";
            objectIdColumn.ReadOnly = true;
            objectIdColumn.Visible = false;
            objectIdColumn.Width = 90;
            // 
            // nameColumn
            // 
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            nameColumn.HeaderText = "Наименование";
            nameColumn.Name = "nameColumn";
            nameColumn.ReadOnly = true;
            // 
            // timeStampColumn
            // 
            timeStampColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            timeStampColumn.HeaderText = "Время блокировки";
            timeStampColumn.Name = "timeStampColumn";
            timeStampColumn.ReadOnly = true;
            // 
            // Win7_BLockService
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(707, 295);
            Controls.Add(tlpnlBlockService);
            Name = "Win7_BLockService";
            Text = "Сервис заблокированных объектов";
            Load += Win7_BLockService_Load;
            tlpnlBlockService.ResumeLayout(false);
            pnlControls.ResumeLayout(false);
            pnlControls.PerformLayout();
            pnlDgv.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tlpnlBlockService;
        private Panel pnlControls;
        private TextBox txtSearch;
        private Panel pnlDgv;
        private Button btnUpdate;
        private Button btnDelete;
        private DataGridView dgvMain;
        private DataGridViewTextBoxColumn typeColumn;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn timeStampColumn;
        private ComboBox cmbType;
        private DataGridViewTextBoxColumn idColumn;
        private DataGridViewTextBoxColumn objectIdColumn;
    }
}