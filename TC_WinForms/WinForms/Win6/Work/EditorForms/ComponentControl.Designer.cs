namespace TC_WinForms.WinForms.Win6.Work.EditorForms
{
    partial class ComponentControl
    {
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
            dgvComponentsLocal = new DataGridView();
            idColumn = new DataGridViewTextBoxColumn();
            deleteBtnColumn = new DataGridViewButtonColumn();
            nameColumn = new DataGridViewTextBoxColumn();
            typeColumn = new DataGridViewTextBoxColumn();
            unitColumn = new DataGridViewTextBoxColumn();
            countColumn = new DataGridViewTextBoxColumn();
            commentColumn = new DataGridViewTextBoxColumn();
            pnlControls = new Panel();
            pnlFilter = new Panel();
            groupBoxFilter = new GroupBox();
            cmbxFilter = new ComboBox();
            pnlSearch = new Panel();
            groupBoxSearch = new GroupBox();
            txtSearch = new TextBox();
            pnlDgvAll = new Panel();
            dgvComponentsGlobal = new DataGridView();
            idAllColumn = new DataGridViewTextBoxColumn();
            addBtnColumn = new DataGridViewButtonColumn();
            nameAllColumn = new DataGridViewTextBoxColumn();
            typeAllColumn = new DataGridViewTextBoxColumn();
            unitAllColumn = new DataGridViewTextBoxColumn();
            countAllColumn = new DataGridViewTextBoxColumn();
            tblpMain.SuspendLayout();
            pnlDgvLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvComponentsLocal).BeginInit();
            pnlControls.SuspendLayout();
            pnlFilter.SuspendLayout();
            groupBoxFilter.SuspendLayout();
            pnlSearch.SuspendLayout();
            groupBoxSearch.SuspendLayout();
            pnlDgvAll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvComponentsGlobal).BeginInit();
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
            pnlDgvLocal.Controls.Add(dgvComponentsLocal);
            pnlDgvLocal.Dock = DockStyle.Fill;
            pnlDgvLocal.Location = new Point(3, 3);
            pnlDgvLocal.Name = "pnlDgvLocal";
            pnlDgvLocal.Size = new Size(1037, 191);
            pnlDgvLocal.TabIndex = 0;
            // 
            // dgvComponentsLocal
            // 
            dgvComponentsLocal.AllowUserToAddRows = false;
            dgvComponentsLocal.AllowUserToDeleteRows = false;
            dgvComponentsLocal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvComponentsLocal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvComponentsLocal.Columns.AddRange(new DataGridViewColumn[] { idColumn, deleteBtnColumn, nameColumn, typeColumn, unitColumn, countColumn, commentColumn });
            dgvComponentsLocal.Dock = DockStyle.Fill;
            dgvComponentsLocal.Location = new Point(0, 0);
            dgvComponentsLocal.Name = "dgvComponentsLocal";
            dgvComponentsLocal.Size = new Size(1037, 191);
            dgvComponentsLocal.TabIndex = 0;
            // 
            // idColumn
            // 
            idColumn.HeaderText = "Id";
            idColumn.Name = "idColumn";
            idColumn.Visible = false;
            // 
            // deleteBtnColumn
            // 
            deleteBtnColumn.FillWeight = 45F;
            deleteBtnColumn.HeaderText = "";
            deleteBtnColumn.MinimumWidth = 40;
            deleteBtnColumn.Name = "deleteBtnColumn";
            // 
            // nameColumn
            // 
            nameColumn.HeaderText = "Наименование";
            nameColumn.Name = "nameColumn";
            nameColumn.Resizable = DataGridViewTriState.True;
            nameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // typeColumn
            // 
            typeColumn.FillWeight = 65F;
            typeColumn.HeaderText = "Тип (исполнение)";
            typeColumn.MinimumWidth = 60;
            typeColumn.Name = "typeColumn";
            // 
            // unitColumn
            // 
            unitColumn.FillWeight = 50F;
            unitColumn.HeaderText = "Ед. Изм.";
            unitColumn.MinimumWidth = 40;
            unitColumn.Name = "unitColumn";
            // 
            // countColumn
            // 
            countColumn.FillWeight = 45F;
            countColumn.HeaderText = "Кол-во";
            countColumn.MinimumWidth = 40;
            countColumn.Name = "countColumn";
            // 
            // commentColumn
            // 
            commentColumn.HeaderText = "Комментарии";
            commentColumn.Name = "commentColumn";
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlFilter);
            pnlControls.Controls.Add(pnlSearch);
            pnlControls.Dock = DockStyle.Fill;
            pnlControls.Location = new Point(3, 200);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(1037, 48);
            pnlControls.TabIndex = 1;
            // 
            // pnlFilter
            // 
            pnlFilter.Controls.Add(groupBoxFilter);
            pnlFilter.Dock = DockStyle.Left;
            pnlFilter.Location = new Point(220, 0);
            pnlFilter.Name = "pnlFilter";
            pnlFilter.Padding = new Padding(5, 2, 5, 2);
            pnlFilter.Size = new Size(300, 48);
            pnlFilter.TabIndex = 3;
            // 
            // groupBoxFilter
            // 
            groupBoxFilter.Controls.Add(cmbxFilter);
            groupBoxFilter.Dock = DockStyle.Fill;
            groupBoxFilter.Location = new Point(5, 2);
            groupBoxFilter.Margin = new Padding(10, 3, 3, 3);
            groupBoxFilter.Name = "groupBoxFilter";
            groupBoxFilter.Padding = new Padding(0);
            groupBoxFilter.Size = new Size(290, 44);
            groupBoxFilter.TabIndex = 1;
            groupBoxFilter.TabStop = false;
            groupBoxFilter.Text = "Фильтр категория";
            // 
            // cmbxFilter
            // 
            cmbxFilter.Dock = DockStyle.Fill;
            cmbxFilter.FormattingEnabled = true;
            cmbxFilter.Location = new Point(0, 16);
            cmbxFilter.Name = "cmbxFilter";
            cmbxFilter.Size = new Size(290, 23);
            cmbxFilter.TabIndex = 0;
            // 
            // pnlSearch
            // 
            pnlSearch.Controls.Add(groupBoxSearch);
            pnlSearch.Dock = DockStyle.Left;
            pnlSearch.Location = new Point(0, 0);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Padding = new Padding(5, 2, 5, 2);
            pnlSearch.Size = new Size(220, 48);
            pnlSearch.TabIndex = 2;
            // 
            // groupBoxSearch
            // 
            groupBoxSearch.Controls.Add(txtSearch);
            groupBoxSearch.Dock = DockStyle.Fill;
            groupBoxSearch.Location = new Point(5, 2);
            groupBoxSearch.Name = "groupBoxSearch";
            groupBoxSearch.Padding = new Padding(0);
            groupBoxSearch.Size = new Size(210, 44);
            groupBoxSearch.TabIndex = 0;
            groupBoxSearch.TabStop = false;
            groupBoxSearch.Text = "Поиск";
            // 
            // txtSearch
            // 
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Location = new Point(0, 16);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(210, 23);
            txtSearch.TabIndex = 0;
            // 
            // pnlDgvAll
            // 
            pnlDgvAll.Controls.Add(dgvComponentsGlobal);
            pnlDgvAll.Dock = DockStyle.Fill;
            pnlDgvAll.Location = new Point(3, 254);
            pnlDgvAll.Name = "pnlDgvAll";
            pnlDgvAll.Size = new Size(1037, 163);
            pnlDgvAll.TabIndex = 2;
            // 
            // dgvComponentsGlobal
            // 
            dgvComponentsGlobal.AllowUserToAddRows = false;
            dgvComponentsGlobal.AllowUserToDeleteRows = false;
            dgvComponentsGlobal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvComponentsGlobal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvComponentsGlobal.Columns.AddRange(new DataGridViewColumn[] { idAllColumn, addBtnColumn, nameAllColumn, typeAllColumn, unitAllColumn, countAllColumn });
            dgvComponentsGlobal.Dock = DockStyle.Fill;
            dgvComponentsGlobal.Location = new Point(0, 0);
            dgvComponentsGlobal.Name = "dgvComponentsGlobal";
            dgvComponentsGlobal.Size = new Size(1037, 163);
            dgvComponentsGlobal.TabIndex = 0;
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
            // ComponentControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tblpMain);
            Name = "ComponentControl";
            Size = new Size(1043, 420);
            tblpMain.ResumeLayout(false);
            pnlDgvLocal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvComponentsLocal).EndInit();
            pnlControls.ResumeLayout(false);
            pnlFilter.ResumeLayout(false);
            groupBoxFilter.ResumeLayout(false);
            pnlSearch.ResumeLayout(false);
            groupBoxSearch.ResumeLayout(false);
            groupBoxSearch.PerformLayout();
            pnlDgvAll.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvComponentsGlobal).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tblpMain;
        private Panel pnlDgvLocal;
        private DataGridView dgvComponentsLocal;
        private DataGridViewTextBoxColumn idColumn;
        private DataGridViewButtonColumn deleteBtnColumn;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewTextBoxColumn typeColumn;
        private DataGridViewTextBoxColumn unitColumn;
        private DataGridViewTextBoxColumn countColumn;
        private DataGridViewTextBoxColumn commentColumn;
        private Panel pnlControls;
        private GroupBox groupBoxFilter;
        private GroupBox groupBoxSearch;
        private TextBox txtSearch;
        private Panel pnlFilter;
        private Panel pnlSearch;
        private ComboBox cmbxFilter;
        private Panel pnlDgvAll;
        private DataGridView dgvComponentsGlobal;
        private DataGridViewTextBoxColumn idAllColumn;
        private DataGridViewButtonColumn addBtnColumn;
        private DataGridViewTextBoxColumn nameAllColumn;
        private DataGridViewTextBoxColumn typeAllColumn;
        private DataGridViewTextBoxColumn unitAllColumn;
        private DataGridViewTextBoxColumn countAllColumn;
    }
}
