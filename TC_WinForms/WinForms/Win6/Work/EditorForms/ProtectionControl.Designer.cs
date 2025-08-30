namespace TC_WinForms.WinForms.Win6.Work.EditorForms
{
    partial class ProtectionControl
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            tblpMain = new TableLayoutPanel();
            pnlDgvLocal = new Panel();
            dgvProtectionsLocal = new DataGridView();
            idColumn = new DataGridViewTextBoxColumn();
            deleteBtnColumn = new DataGridViewButtonColumn();
            checkBoxColumn = new DataGridViewCheckBoxColumn();
            nameColumn = new DataGridViewTextBoxColumn();
            typeColumn = new DataGridViewTextBoxColumn();
            pnlControls = new Panel();
            pnlSearch = new Panel();
            groupBoxSearch = new GroupBox();
            txtSearch = new TextBox();
            pnlDgvAll = new Panel();
            dgvProtectionsGlobal = new DataGridView();
            idAllColumn = new DataGridViewTextBoxColumn();
            addBtnColumn = new DataGridViewButtonColumn();
            nameAllColumn = new DataGridViewTextBoxColumn();
            typeAllColumn = new DataGridViewTextBoxColumn();
            unitAllColumn = new DataGridViewTextBoxColumn();
            countAllColumn = new DataGridViewTextBoxColumn();
            tblpMain.SuspendLayout();
            pnlDgvLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProtectionsLocal).BeginInit();
            pnlControls.SuspendLayout();
            pnlSearch.SuspendLayout();
            groupBoxSearch.SuspendLayout();
            pnlDgvAll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProtectionsGlobal).BeginInit();
            SuspendLayout();
            // 
            // tblpMain
            // 
            tblpMain.ColumnCount = 1;
            tblpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblpMain.Controls.Add(pnlDgvLocal, 0, 0);
            tblpMain.Controls.Add(pnlControls, 0, 1);
            tblpMain.Controls.Add(pnlDgvAll, 0, 2);
            tblpMain.Dock = DockStyle.Fill;
            tblpMain.Location = new Point(0, 0);
            tblpMain.Name = "tblpMain";
            tblpMain.RowCount = 3;
            tblpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 47F));
            tblpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 13F));
            tblpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tblpMain.Size = new Size(1043, 420);
            tblpMain.TabIndex = 0;
            // 
            // pnlDgvLocal
            // 
            pnlDgvLocal.Controls.Add(dgvProtectionsLocal);
            pnlDgvLocal.Dock = DockStyle.Fill;
            pnlDgvLocal.Location = new Point(3, 3);
            pnlDgvLocal.Name = "pnlDgvLocal";
            pnlDgvLocal.Size = new Size(1037, 191);
            pnlDgvLocal.TabIndex = 0;
            // 
            // dgvProtectionsLocal
            // 
            dgvProtectionsLocal.AllowUserToAddRows = false;
            dgvProtectionsLocal.AllowUserToDeleteRows = false;
            dgvProtectionsLocal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProtectionsLocal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProtectionsLocal.Columns.AddRange(new DataGridViewColumn[] { idColumn, deleteBtnColumn, checkBoxColumn, nameColumn, typeColumn });
            dgvProtectionsLocal.Dock = DockStyle.Fill;
            dgvProtectionsLocal.Location = new Point(0, 0);
            dgvProtectionsLocal.Name = "dgvProtectionsLocal";
            dgvProtectionsLocal.Size = new Size(1037, 191);
            dgvProtectionsLocal.TabIndex = 0;
            // 
            // idColumn
            // 
            idColumn.HeaderText = "Id";
            idColumn.Name = "idColumn";
            idColumn.Visible = false;
            // 
            // deleteBtnColumn
            // 
            deleteBtnColumn.FillWeight = 40F;
            deleteBtnColumn.HeaderText = "";
            deleteBtnColumn.MinimumWidth = 50;
            deleteBtnColumn.Name = "deleteBtnColumn";
            // 
            // checkBoxColumn
            // 
            checkBoxColumn.FillWeight = 20F;
            checkBoxColumn.HeaderText = "";
            checkBoxColumn.MinimumWidth = 15;
            checkBoxColumn.Name = "checkBoxColumn";
            // 
            // nameColumn
            // 
            nameColumn.HeaderText = "Наименование";
            nameColumn.MinimumWidth = 90;
            nameColumn.Name = "nameColumn";
            nameColumn.Resizable = DataGridViewTriState.True;
            nameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // typeColumn
            // 
            typeColumn.FillWeight = 105F;
            typeColumn.HeaderText = "Тип (исполнение)";
            typeColumn.MinimumWidth = 100;
            typeColumn.Name = "typeColumn";
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlSearch);
            pnlControls.Dock = DockStyle.Fill;
            pnlControls.Location = new Point(3, 200);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(1037, 48);
            pnlControls.TabIndex = 1;
            // 
            // pnlSearch
            // 
            pnlSearch.Controls.Add(groupBoxSearch);
            pnlSearch.Dock = DockStyle.Left;
            pnlSearch.Location = new Point(0, 0);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Padding = new Padding(5, 2, 5, 2);
            pnlSearch.Size = new Size(260, 48);
            pnlSearch.TabIndex = 2;
            // 
            // groupBoxSearch
            // 
            groupBoxSearch.Controls.Add(txtSearch);
            groupBoxSearch.Dock = DockStyle.Fill;
            groupBoxSearch.Location = new Point(5, 2);
            groupBoxSearch.Name = "groupBoxSearch";
            groupBoxSearch.Padding = new Padding(0);
            groupBoxSearch.Size = new Size(250, 44);
            groupBoxSearch.TabIndex = 0;
            groupBoxSearch.TabStop = false;
            groupBoxSearch.Text = "Поиск";
            // 
            // txtSearch
            // 
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Location = new Point(0, 16);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(250, 23);
            txtSearch.TabIndex = 0;
            // 
            // pnlDgvAll
            // 
            pnlDgvAll.Controls.Add(dgvProtectionsGlobal);
            pnlDgvAll.Dock = DockStyle.Fill;
            pnlDgvAll.Location = new Point(3, 254);
            pnlDgvAll.Name = "pnlDgvAll";
            pnlDgvAll.Size = new Size(1037, 163);
            pnlDgvAll.TabIndex = 2;
            // 
            // dgvProtectionsGlobal
            // 
            dgvProtectionsGlobal.AllowUserToAddRows = false;
            dgvProtectionsGlobal.AllowUserToDeleteRows = false;
            dgvProtectionsGlobal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProtectionsGlobal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProtectionsGlobal.Columns.AddRange(new DataGridViewColumn[] { idAllColumn, addBtnColumn, nameAllColumn, typeAllColumn, unitAllColumn, countAllColumn });
            dgvProtectionsGlobal.Dock = DockStyle.Fill;
            dgvProtectionsGlobal.Location = new Point(0, 0);
            dgvProtectionsGlobal.Name = "dgvProtectionsGlobal";
            dgvProtectionsGlobal.Size = new Size(1037, 163);
            dgvProtectionsGlobal.TabIndex = 0;
            // 
            // idAllColumn
            // 
            idAllColumn.HeaderText = "Id";
            idAllColumn.Name = "idAllColumn";
            idAllColumn.Visible = false;
            // 
            // addBtnColumn
            // 
            addBtnColumn.FillWeight = 50F;
            addBtnColumn.HeaderText = "";
            addBtnColumn.MinimumWidth = 50;
            addBtnColumn.Name = "addBtnColumn";
            // 
            // nameAllColumn
            // 
            nameAllColumn.HeaderText = "Наименование";
            nameAllColumn.Name = "nameAllColumn";
            nameAllColumn.ReadOnly = true;
            // 
            // typeAllColumn
            // 
            typeAllColumn.HeaderText = "Тип (исполнение)";
            typeAllColumn.Name = "typeAllColumn";
            typeAllColumn.ReadOnly = true;
            // 
            // unitAllColumn
            // 
            unitAllColumn.HeaderText = "Ед. Изм.";
            unitAllColumn.Name = "unitAllColumn";
            unitAllColumn.ReadOnly = true;
            // 
            // countAllColumn
            // 
            countAllColumn.HeaderText = "Кол-во";
            countAllColumn.Name = "countAllColumn";
            countAllColumn.ReadOnly = true;
            // 
            // ProtectionControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tblpMain);
            Name = "ProtectionControl";
            Size = new Size(1043, 420);
            tblpMain.ResumeLayout(false);
            pnlDgvLocal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvProtectionsLocal).EndInit();
            pnlControls.ResumeLayout(false);
            pnlSearch.ResumeLayout(false);
            groupBoxSearch.ResumeLayout(false);
            groupBoxSearch.PerformLayout();
            pnlDgvAll.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvProtectionsGlobal).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tblpMain;
        private Panel pnlDgvLocal;
        private DataGridView dgvProtectionsLocal;
        private Panel pnlControls;
        private GroupBox groupBoxSearch;
        private TextBox txtSearch;
        private Panel pnlSearch;
        private Panel pnlDgvAll;
        private DataGridView dgvProtectionsGlobal;
        private DataGridViewTextBoxColumn idColumn;
        private DataGridViewButtonColumn deleteBtnColumn;
        private DataGridViewCheckBoxColumn checkBoxColumn;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn typeColumn;
        private DataGridViewTextBoxColumn idAllColumn;
        private DataGridViewButtonColumn addBtnColumn;
        private DataGridViewTextBoxColumn nameAllColumn;
        private DataGridViewTextBoxColumn typeAllColumn;
        private DataGridViewTextBoxColumn unitAllColumn;
        private DataGridViewTextBoxColumn countAllColumn;
    }
}
