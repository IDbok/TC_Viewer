namespace TC_WinForms.WinForms.Win6.Work.EditorForms
{
    partial class StaffControl
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
            dgvStaffsLocal = new DataGridView();
            idColumn = new DataGridViewTextBoxColumn();
            deleteBtnColumn = new DataGridViewButtonColumn();
            checkBoxColumn = new DataGridViewCheckBoxColumn();
            symbolColumn = new DataGridViewTextBoxColumn();
            nameColumn = new DataGridViewTextBoxColumn();
            typeColumn = new DataGridViewTextBoxColumn();
            functionsColumn = new DataGridViewTextBoxColumn();
            combResponseColumn = new DataGridViewTextBoxColumn();
            qualificationColumn = new DataGridViewTextBoxColumn();
            commentColumn = new DataGridViewTextBoxColumn();
            pnlControls = new Panel();
            pnlSearch = new Panel();
            groupBoxSearch = new GroupBox();
            txtSearch = new TextBox();
            pnlDgvAll = new Panel();
            dgvStaffsGlobal = new DataGridView();
            idAllColumn = new DataGridViewTextBoxColumn();
            addBtnColumn = new DataGridViewButtonColumn();
            nameAllColumn = new DataGridViewTextBoxColumn();
            typeAllColumn = new DataGridViewTextBoxColumn();
            functionColumn = new DataGridViewTextBoxColumn();
            combResponceAllColumn = new DataGridViewTextBoxColumn();
            qualificationAllColumn = new DataGridViewTextBoxColumn();
            commentAllColumn = new DataGridViewTextBoxColumn();
            tblpMain.SuspendLayout();
            pnlDgvLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStaffsLocal).BeginInit();
            pnlControls.SuspendLayout();
            pnlSearch.SuspendLayout();
            groupBoxSearch.SuspendLayout();
            pnlDgvAll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStaffsGlobal).BeginInit();
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
            pnlDgvLocal.Controls.Add(dgvStaffsLocal);
            pnlDgvLocal.Dock = DockStyle.Fill;
            pnlDgvLocal.Location = new Point(3, 3);
            pnlDgvLocal.Name = "pnlDgvLocal";
            pnlDgvLocal.Size = new Size(1037, 191);
            pnlDgvLocal.TabIndex = 0;
            // 
            // dgvStaffsLocal
            // 
            dgvStaffsLocal.AllowUserToAddRows = false;
            dgvStaffsLocal.AllowUserToDeleteRows = false;
            dgvStaffsLocal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStaffsLocal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStaffsLocal.Columns.AddRange(new DataGridViewColumn[] { idColumn, deleteBtnColumn, checkBoxColumn, symbolColumn, nameColumn, typeColumn, functionsColumn, combResponseColumn, qualificationColumn, commentColumn });
            dgvStaffsLocal.Dock = DockStyle.Fill;
            dgvStaffsLocal.Location = new Point(0, 0);
            dgvStaffsLocal.Name = "dgvStaffsLocal";
            dgvStaffsLocal.Size = new Size(1037, 191);
            dgvStaffsLocal.TabIndex = 0;
            // 
            // idColumn
            // 
            idColumn.HeaderText = "Id";
            idColumn.Name = "idColumn";
            idColumn.Visible = false;
            // 
            // deleteBtnColumn
            // 
            deleteBtnColumn.FillWeight = 85F;
            deleteBtnColumn.HeaderText = "";
            deleteBtnColumn.MinimumWidth = 80;
            deleteBtnColumn.Name = "deleteBtnColumn";
            // 
            // checkBoxColumn
            // 
            checkBoxColumn.FillWeight = 20F;
            checkBoxColumn.HeaderText = "";
            checkBoxColumn.MinimumWidth = 15;
            checkBoxColumn.Name = "checkBoxColumn";
            // 
            // symbolColumn
            // 
            symbolColumn.FillWeight = 75F;
            symbolColumn.HeaderText = "Обозначение";
            symbolColumn.MinimumWidth = 70;
            symbolColumn.Name = "symbolColumn";
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
            typeColumn.HeaderText = "Тип";
            typeColumn.MinimumWidth = 100;
            typeColumn.Name = "typeColumn";
            // 
            // functionsColumn
            // 
            functionsColumn.FillWeight = 105F;
            functionsColumn.HeaderText = "Функции";
            functionsColumn.MinimumWidth = 100;
            functionsColumn.Name = "functionsColumn";
            // 
            // combResponseColumn
            // 
            combResponseColumn.FillWeight = 105F;
            combResponseColumn.HeaderText = "Возможность совмещения обязанностей";
            combResponseColumn.MinimumWidth = 100;
            combResponseColumn.Name = "combResponseColumn";
            combResponseColumn.ReadOnly = true;
            // 
            // qualificationColumn
            // 
            qualificationColumn.FillWeight = 105F;
            qualificationColumn.HeaderText = "Квалификация";
            qualificationColumn.MinimumWidth = 100;
            qualificationColumn.Name = "qualificationColumn";
            qualificationColumn.ReadOnly = true;
            // 
            // commentColumn
            // 
            commentColumn.FillWeight = 105F;
            commentColumn.HeaderText = "Комментарии";
            commentColumn.MinimumWidth = 100;
            commentColumn.Name = "commentColumn";
            commentColumn.ReadOnly = true;
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
            pnlDgvAll.Controls.Add(dgvStaffsGlobal);
            pnlDgvAll.Dock = DockStyle.Fill;
            pnlDgvAll.Location = new Point(3, 254);
            pnlDgvAll.Name = "pnlDgvAll";
            pnlDgvAll.Size = new Size(1037, 163);
            pnlDgvAll.TabIndex = 2;
            // 
            // dgvStaffsGlobal
            // 
            dgvStaffsGlobal.AllowUserToAddRows = false;
            dgvStaffsGlobal.AllowUserToDeleteRows = false;
            dgvStaffsGlobal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStaffsGlobal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStaffsGlobal.Columns.AddRange(new DataGridViewColumn[] { idAllColumn, addBtnColumn, nameAllColumn, typeAllColumn, functionColumn, combResponceAllColumn, qualificationAllColumn, commentAllColumn });
            dgvStaffsGlobal.Dock = DockStyle.Fill;
            dgvStaffsGlobal.Location = new Point(0, 0);
            dgvStaffsGlobal.Name = "dgvStaffsGlobal";
            dgvStaffsGlobal.Size = new Size(1037, 163);
            dgvStaffsGlobal.TabIndex = 0;
            // 
            // idAllColumn
            // 
            idAllColumn.HeaderText = "Id";
            idAllColumn.Name = "idAllColumn";
            idAllColumn.Visible = false;
            // 
            // addBtnColumn
            // 
            addBtnColumn.HeaderText = "";
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
            typeAllColumn.HeaderText = "Тип";
            typeAllColumn.Name = "typeAllColumn";
            typeAllColumn.ReadOnly = true;
            // 
            // functionColumn
            // 
            functionColumn.HeaderText = "Функции";
            functionColumn.Name = "functionColumn";
            functionColumn.ReadOnly = true;
            // 
            // combResponceAllColumn
            // 
            combResponceAllColumn.HeaderText = "Возможность совмещения обязанностей";
            combResponceAllColumn.Name = "combResponceAllColumn";
            combResponceAllColumn.ReadOnly = true;
            // 
            // qualificationAllColumn
            // 
            qualificationAllColumn.HeaderText = "Квалификация";
            qualificationAllColumn.Name = "qualificationAllColumn";
            qualificationAllColumn.ReadOnly = true;
            // 
            // commentAllColumn
            // 
            commentAllColumn.HeaderText = "Комментарии";
            commentAllColumn.Name = "commentAllColumn";
            commentAllColumn.ReadOnly = true;
            // 
            // StaffControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tblpMain);
            Name = "StaffControl";
            Size = new Size(1043, 420);
            tblpMain.ResumeLayout(false);
            pnlDgvLocal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvStaffsLocal).EndInit();
            pnlControls.ResumeLayout(false);
            pnlSearch.ResumeLayout(false);
            groupBoxSearch.ResumeLayout(false);
            groupBoxSearch.PerformLayout();
            pnlDgvAll.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvStaffsGlobal).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tblpMain;
        private Panel pnlDgvLocal;
        private DataGridView dgvStaffsLocal;
        private Panel pnlControls;
        private GroupBox groupBoxSearch;
        private TextBox txtSearch;
        private Panel pnlSearch;
        private Panel pnlDgvAll;
        private DataGridView dgvStaffsGlobal;
        private DataGridViewTextBoxColumn idAllColumn;
        private DataGridViewButtonColumn addBtnColumn;
        private DataGridViewTextBoxColumn nameAllColumn;
        private DataGridViewTextBoxColumn typeAllColumn;
        private DataGridViewTextBoxColumn functionColumn;
        private DataGridViewTextBoxColumn combResponceAllColumn;
        private DataGridViewTextBoxColumn qualificationAllColumn;
        private DataGridViewTextBoxColumn commentAllColumn;
        private DataGridViewTextBoxColumn idColumn;
        private DataGridViewButtonColumn deleteBtnColumn;
        private DataGridViewCheckBoxColumn checkBoxColumn;
        private DataGridViewTextBoxColumn symbolColumn;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn typeColumn;
        private DataGridViewTextBoxColumn functionsColumn;
        private DataGridViewTextBoxColumn combResponseColumn;
        private DataGridViewTextBoxColumn qualificationColumn;
        private DataGridViewTextBoxColumn commentColumn;
    }
}
