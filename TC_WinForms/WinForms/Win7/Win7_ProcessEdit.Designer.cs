namespace TC_WinForms.WinForms
{
    partial class Win7_ProcessEdit
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
            tableLayoutPanel1 = new TableLayoutPanel();
            groupBox2 = new GroupBox();
            textBoxPoisk = new TextBox();
            dataGridViewTPLocal = new DataGridView();
            dataGridViewTextBoxColumn12 = new DataGridViewTextBoxColumn();
            dataGridViewButtonColumn2 = new DataGridViewButtonColumn();
            Column3 = new DataGridViewButtonColumn();
            dataGridViewTextBoxColumn13 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            panel1 = new Panel();
            btnCancel = new Button();
            btnSave = new Button();
            txtDescription = new TextBox();
            label3 = new Label();
            txtType = new TextBox();
            label2 = new Label();
            txtName = new TextBox();
            label1 = new Label();
            dataGridViewAllTP = new DataGridView();
            Id = new DataGridViewTextBoxColumn();
            Add = new DataGridViewButtonColumn();
            NameTO = new DataGridViewTextBoxColumn();
            Column1 = new DataGridViewTextBoxColumn();
            tableLayoutPanel1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTPLocal).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAllTP).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(groupBox2, 0, 2);
            tableLayoutPanel1.Controls.Add(dataGridViewTPLocal, 0, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(dataGridViewAllTP, 0, 3);
            tableLayoutPanel1.Location = new Point(0, 1);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1529, 772);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(textBoxPoisk);
            groupBox2.Location = new Point(3, 419);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(292, 64);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "Поиск";
            // 
            // textBoxPoisk
            // 
            textBoxPoisk.Location = new Point(16, 26);
            textBoxPoisk.Name = "textBoxPoisk";
            textBoxPoisk.Size = new Size(257, 27);
            textBoxPoisk.TabIndex = 0;
            textBoxPoisk.TextChanged += textBoxPoisk_TextChanged;
            // 
            // dataGridViewTPLocal
            // 
            dataGridViewTPLocal.AllowUserToAddRows = false;
            dataGridViewTPLocal.AllowUserToDeleteRows = false;
            dataGridViewTPLocal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTPLocal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewTPLocal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTPLocal.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn12, dataGridViewButtonColumn2, Column3, dataGridViewTextBoxColumn13, Column2 });
            dataGridViewTPLocal.Location = new Point(3, 133);
            dataGridViewTPLocal.Name = "dataGridViewTPLocal";
            dataGridViewTPLocal.RowHeadersWidth = 51;
            dataGridViewTPLocal.RowTemplate.Height = 29;
            dataGridViewTPLocal.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTPLocal.Size = new Size(1523, 280);
            dataGridViewTPLocal.TabIndex = 2;
            // 
            // dataGridViewTextBoxColumn12
            // 
            dataGridViewTextBoxColumn12.HeaderText = "Id";
            dataGridViewTextBoxColumn12.MinimumWidth = 6;
            dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            dataGridViewTextBoxColumn12.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn12.Visible = false;
            // 
            // dataGridViewButtonColumn2
            // 
            dataGridViewButtonColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewButtonColumn2.HeaderText = "";
            dataGridViewButtonColumn2.MinimumWidth = 6;
            dataGridViewButtonColumn2.Name = "dataGridViewButtonColumn2";
            dataGridViewButtonColumn2.Resizable = DataGridViewTriState.True;
            dataGridViewButtonColumn2.Text = "Удалить";
            dataGridViewButtonColumn2.Width = 125;
            // 
            // Column3
            // 
            Column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            Column3.HeaderText = "";
            Column3.MinimumWidth = 6;
            Column3.Name = "Column3";
            Column3.Width = 125;
            // 
            // dataGridViewTextBoxColumn13
            // 
            dataGridViewTextBoxColumn13.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn13.FillWeight = 69.7860947F;
            dataGridViewTextBoxColumn13.HeaderText = "Артикл";
            dataGridViewTextBoxColumn13.MinimumWidth = 6;
            dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            dataGridViewTextBoxColumn13.ReadOnly = true;
            dataGridViewTextBoxColumn13.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // Column2
            // 
            Column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Column2.HeaderText = "Название";
            Column2.MinimumWidth = 6;
            Column2.Name = "Column2";
            Column2.ReadOnly = true;
            Column2.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(btnCancel);
            panel1.Controls.Add(btnSave);
            panel1.Controls.Add(txtDescription);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(txtType);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(txtName);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1523, 124);
            panel1.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(1326, 22);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(154, 61);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Отменить";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += button2_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(1126, 22);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(154, 61);
            btnSave.TabIndex = 6;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += button1_Click;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(92, 78);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(579, 27);
            txtDescription.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(9, 81);
            label3.Name = "label3";
            label3.Size = new Size(79, 20);
            label3.TabIndex = 4;
            label3.Text = "Описание";
            // 
            // txtType
            // 
            txtType.Location = new Point(92, 45);
            txtType.Name = "txtType";
            txtType.Size = new Size(579, 27);
            txtType.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 48);
            label2.Name = "label2";
            label2.Size = new Size(35, 20);
            label2.TabIndex = 2;
            label2.Text = "Тип";
            // 
            // txtName
            // 
            txtName.Location = new Point(92, 12);
            txtName.Name = "txtName";
            txtName.Size = new Size(579, 27);
            txtName.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 15);
            label1.Name = "label1";
            label1.Size = new Size(77, 20);
            label1.TabIndex = 0;
            label1.Text = "Название";
            // 
            // dataGridViewAllTP
            // 
            dataGridViewAllTP.AllowUserToAddRows = false;
            dataGridViewAllTP.AllowUserToDeleteRows = false;
            dataGridViewAllTP.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewAllTP.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewAllTP.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewAllTP.Columns.AddRange(new DataGridViewColumn[] { Id, Add, NameTO, Column1 });
            dataGridViewAllTP.Location = new Point(3, 489);
            dataGridViewAllTP.Name = "dataGridViewAllTP";
            dataGridViewAllTP.RowHeadersWidth = 51;
            dataGridViewAllTP.RowTemplate.Height = 29;
            dataGridViewAllTP.Size = new Size(1523, 280);
            dataGridViewAllTP.TabIndex = 1;
            // 
            // Id
            // 
            Id.HeaderText = "Id";
            Id.MinimumWidth = 6;
            Id.Name = "Id";
            Id.Visible = false;
            // 
            // Add
            // 
            Add.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            Add.FillWeight = 50F;
            Add.HeaderText = "";
            Add.MinimumWidth = 50;
            Add.Name = "Add";
            Add.Resizable = DataGridViewTriState.True;
            Add.Width = 125;
            // 
            // NameTO
            // 
            NameTO.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            NameTO.FillWeight = 167.914459F;
            NameTO.HeaderText = "Артикл";
            NameTO.MinimumWidth = 6;
            NameTO.Name = "NameTO";
            NameTO.ReadOnly = true;
            NameTO.Resizable = DataGridViewTriState.True;
            NameTO.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // Column1
            // 
            Column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Column1.HeaderText = "Название";
            Column1.MinimumWidth = 6;
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            // 
            // Win7_ProcessEdit
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1531, 776);
            Controls.Add(tableLayoutPanel1);
            Name = "Win7_ProcessEdit";
            Text = "Win7_Process";
            tableLayoutPanel1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTPLocal).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAllTP).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private TextBox txtName;
        private Label label1;
        private DataGridView dataGridViewAllTP;
        private TextBox txtType;
        private Label label2;
        private TextBox txtDescription;
        private Label label3;
        private DataGridView dataGridViewTPLocal;
        private Button btnCancel;
        private Button btnSave;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewButtonColumn Add;
        private DataGridViewTextBoxColumn NameTO;
        private DataGridViewTextBoxColumn Column1;
        private GroupBox groupBox2;
        private TextBox textBoxPoisk;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private DataGridViewButtonColumn dataGridViewButtonColumn2;
        private DataGridViewButtonColumn Column3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private DataGridViewTextBoxColumn Column2;
    }
}