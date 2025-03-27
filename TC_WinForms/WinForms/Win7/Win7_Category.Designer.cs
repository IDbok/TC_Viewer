namespace TC_WinForms.WinForms.Win7
{
    partial class Win7_Category
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
            tbplControls = new TableLayoutPanel();
            dgvCategory = new DataGridView();
            Id = new DataGridViewTextBoxColumn();
            ClassName = new DataGridViewTextBoxColumn();
            Type = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            dgvValue = new DataGridView();
            IdDGVValue = new DataGridViewTextBoxColumn();
            ValueDGVValue = new DataGridViewTextBoxColumn();
            pnlValueControls = new Panel();
            btnAdd = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            tbplControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCategory).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvValue).BeginInit();
            pnlValueControls.SuspendLayout();
            SuspendLayout();
            // 
            // tbplControls
            // 
            tbplControls.ColumnCount = 3;
            tbplControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tbplControls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tbplControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tbplControls.Controls.Add(dgvCategory, 0, 1);
            tbplControls.Controls.Add(dgvValue, 2, 1);
            tbplControls.Controls.Add(pnlValueControls, 2, 0);
            tbplControls.Dock = DockStyle.Fill;
            tbplControls.Location = new Point(0, 0);
            tbplControls.Name = "tbplControls";
            tbplControls.RowCount = 2;
            tbplControls.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tbplControls.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbplControls.Size = new Size(853, 450);
            tbplControls.TabIndex = 0;
            // 
            // dgvCategory
            // 
            dgvCategory.AllowUserToAddRows = false;
            dgvCategory.AllowUserToDeleteRows = false;
            dgvCategory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCategory.Columns.AddRange(new DataGridViewColumn[] { Id, ClassName, Type, Value });
            dgvCategory.Dock = DockStyle.Fill;
            dgvCategory.Location = new Point(3, 63);
            dgvCategory.Name = "dgvCategory";
            dgvCategory.ReadOnly = true;
            dgvCategory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCategory.Size = new Size(410, 384);
            dgvCategory.TabIndex = 0;
            // 
            // Id
            // 
            Id.DataPropertyName = "Id";
            Id.HeaderText = "Id";
            Id.Name = "Id";
            Id.ReadOnly = true;
            Id.Visible = false;
            // 
            // ClassName
            // 
            ClassName.DataPropertyName = "ClassName";
            ClassName.HeaderText = "Класс";
            ClassName.Name = "ClassName";
            ClassName.ReadOnly = true;
            // 
            // Type
            // 
            Type.DataPropertyName = "Type";
            Type.HeaderText = "Формат значения";
            Type.Name = "Type";
            Type.ReadOnly = true;
            // 
            // Value
            // 
            Value.DataPropertyName = "Value";
            Value.HeaderText = "Value";
            Value.Name = "Value";
            Value.ReadOnly = true;
            Value.Visible = false;
            // 
            // dgvValue
            // 
            dgvValue.AllowUserToAddRows = false;
            dgvValue.AllowUserToDeleteRows = false;
            dgvValue.BorderStyle = BorderStyle.None;
            dgvValue.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvValue.Columns.AddRange(new DataGridViewColumn[] { IdDGVValue, ValueDGVValue });
            dgvValue.Dock = DockStyle.Fill;
            dgvValue.Location = new Point(439, 63);
            dgvValue.Name = "dgvValue";
            dgvValue.ReadOnly = true;
            dgvValue.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvValue.Size = new Size(411, 384);
            dgvValue.TabIndex = 5;
            // 
            // IdDGVValue
            // 
            IdDGVValue.DataPropertyName = "Id";
            IdDGVValue.HeaderText = "Id";
            IdDGVValue.Name = "IdDGVValue";
            IdDGVValue.ReadOnly = true;
            IdDGVValue.Visible = false;
            // 
            // ValueDGVValue
            // 
            ValueDGVValue.DataPropertyName = "Value";
            ValueDGVValue.HeaderText = "Значение";
            ValueDGVValue.Name = "ValueDGVValue";
            ValueDGVValue.ReadOnly = true;
            // 
            // pnlValueControls
            // 
            pnlValueControls.Controls.Add(btnDelete);
            pnlValueControls.Controls.Add(btnAdd);
            pnlValueControls.Controls.Add(btnEdit);
            pnlValueControls.Dock = DockStyle.Right;
            pnlValueControls.Location = new Point(439, 3);
            pnlValueControls.Name = "pnlValueControls";
            pnlValueControls.Size = new Size(411, 54);
            pnlValueControls.TabIndex = 3;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(152, 9);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(122, 39);
            btnAdd.TabIndex = 7;
            btnAdd.Text = "Добавить значение";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(280, 9);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(122, 39);
            btnEdit.TabIndex = 6;
            btnEdit.Text = "Редактировать значение";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(24, 9);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(122, 39);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "Удалить значение";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // Win7_Category
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(853, 450);
            Controls.Add(tbplControls);
            Name = "Win7_Category";
            Text = "Win7_Category";
            tbplControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCategory).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvValue).EndInit();
            pnlValueControls.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tbplControls;
        private DataGridView dgvCategory;
        private Panel pnlValueControls;
        private DataGridView dgvValue;
        private Button btnAdd;
        private Button btnEdit;
        private DataGridViewTextBoxColumn IdDGVValue;
        private DataGridViewTextBoxColumn ValueDGVValue;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewTextBoxColumn ClassName;
        private DataGridViewTextBoxColumn Key;
        private DataGridViewTextBoxColumn Type;
        private DataGridViewTextBoxColumn Value;
        private Button btnDelete;
    }
}