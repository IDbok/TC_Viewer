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
            dataGridViewTPLocal = new DataGridView();
            panel1 = new Panel();
            button2 = new Button();
            button1 = new Button();
            textBox3 = new TextBox();
            label3 = new Label();
            textBox2 = new TextBox();
            label2 = new Label();
            textBox1 = new TextBox();
            label1 = new Label();
            dataGridViewAllTP = new DataGridView();
            dataGridViewTextBoxColumn12 = new DataGridViewTextBoxColumn();
            dataGridViewButtonColumn2 = new DataGridViewButtonColumn();
            dataGridViewTextBoxColumn13 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Id = new DataGridViewTextBoxColumn();
            Add = new DataGridViewButtonColumn();
            NameTO = new DataGridViewTextBoxColumn();
            Column1 = new DataGridViewTextBoxColumn();
            tableLayoutPanel1.SuspendLayout();
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
            tableLayoutPanel1.Controls.Add(dataGridViewTPLocal, 0, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(dataGridViewAllTP, 0, 3);
            tableLayoutPanel1.Location = new Point(0, 1);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1529, 772);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // dataGridViewTPLocal
            // 
            dataGridViewTPLocal.AllowUserToAddRows = false;
            dataGridViewTPLocal.AllowUserToDeleteRows = false;
            dataGridViewTPLocal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTPLocal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewTPLocal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTPLocal.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn12, dataGridViewButtonColumn2, dataGridViewTextBoxColumn13, Column2 });
            dataGridViewTPLocal.Location = new Point(3, 133);
            dataGridViewTPLocal.Name = "dataGridViewTPLocal";
            dataGridViewTPLocal.RowHeadersWidth = 51;
            dataGridViewTPLocal.RowTemplate.Height = 29;
            dataGridViewTPLocal.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTPLocal.Size = new Size(1523, 290);
            dataGridViewTPLocal.TabIndex = 2;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(textBox3);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(textBox2);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1523, 124);
            panel1.TabIndex = 0;
            // 
            // button2
            // 
            button2.Location = new Point(1326, 22);
            button2.Name = "button2";
            button2.Size = new Size(154, 61);
            button2.TabIndex = 7;
            button2.Text = "Отменить";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(1126, 22);
            button1.Name = "button1";
            button1.Size = new Size(154, 61);
            button1.TabIndex = 6;
            button1.Text = "Сохранить";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(92, 78);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(579, 27);
            textBox3.TabIndex = 5;
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
            // textBox2
            // 
            textBox2.Location = new Point(92, 45);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(579, 27);
            textBox2.TabIndex = 3;
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
            // textBox1
            // 
            textBox1.Location = new Point(92, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(579, 27);
            textBox1.TabIndex = 1;
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
            dataGridViewAllTP.Location = new Point(3, 479);
            dataGridViewAllTP.Name = "dataGridViewAllTP";
            dataGridViewAllTP.RowHeadersWidth = 51;
            dataGridViewAllTP.RowTemplate.Height = 29;
            dataGridViewAllTP.Size = new Size(1523, 290);
            dataGridViewAllTP.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn12
            // 
            dataGridViewTextBoxColumn12.HeaderText = "Id";
            dataGridViewTextBoxColumn12.MinimumWidth = 6;
            dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            dataGridViewTextBoxColumn12.Visible = false;
            // 
            // dataGridViewButtonColumn2
            // 
            dataGridViewButtonColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewButtonColumn2.HeaderText = "";
            dataGridViewButtonColumn2.MinimumWidth = 6;
            dataGridViewButtonColumn2.Name = "dataGridViewButtonColumn2";
            dataGridViewButtonColumn2.Resizable = DataGridViewTriState.True;
            dataGridViewButtonColumn2.SortMode = DataGridViewColumnSortMode.Automatic;
            dataGridViewButtonColumn2.Text = "Удалить";
            dataGridViewButtonColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn13
            // 
            dataGridViewTextBoxColumn13.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn13.FillWeight = 69.7860947F;
            dataGridViewTextBoxColumn13.HeaderText = "Артикл";
            dataGridViewTextBoxColumn13.MinimumWidth = 6;
            dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            dataGridViewTextBoxColumn13.ReadOnly = true;
            // 
            // Column2
            // 
            Column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Column2.HeaderText = "Название";
            Column2.MinimumWidth = 6;
            Column2.Name = "Column2";
            Column2.ReadOnly = true;
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
            ((System.ComponentModel.ISupportInitialize)dataGridViewTPLocal).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAllTP).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private TextBox textBox1;
        private Label label1;
        private DataGridView dataGridViewAllTP;
        private TextBox textBox2;
        private Label label2;
        private TextBox textBox3;
        private Label label3;
        private DataGridView dataGridViewTPLocal;
        private Button button2;
        private Button button1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private DataGridViewButtonColumn dataGridViewButtonColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewButtonColumn Add;
        private DataGridViewTextBoxColumn NameTO;
        private DataGridViewTextBoxColumn Column1;
    }
}