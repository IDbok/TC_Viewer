namespace TC_WinForms.WinForms
{
    partial class Win7_TechOperation_Window
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
            panel2 = new Panel();
            groupBox10 = new GroupBox();
            comboBoxTPCategoriya = new ComboBox();
            groupBox5 = new GroupBox();
            textBoxPoiskTP = new TextBox();
            dataGridViewTPLocal = new DataGridView();
            dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
            DateGridLocalTPDetete = new DataGridViewButtonColumn();
            dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
            Time = new DataGridViewTextBoxColumn();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Order1 = new DataGridViewTextBoxColumn();
            panel1 = new Panel();
            button1 = new Button();
            checkBox1 = new CheckBox();
            label2 = new Label();
            textBox1 = new TextBox();
            label1 = new Label();
            dataGridViewTPAll = new DataGridView();
            dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
            dataGridViewCheckBoxColumn2 = new DataGridViewButtonColumn();
            dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
            TimeExecution = new DataGridViewTextBoxColumn();
            cbxIsReleased = new CheckBox();
            tableLayoutPanel1.SuspendLayout();
            panel2.SuspendLayout();
            groupBox10.SuspendLayout();
            groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTPLocal).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTPAll).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel2, 0, 1);
            tableLayoutPanel1.Controls.Add(dataGridViewTPLocal, 0, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(dataGridViewTPAll, 0, 3);
            tableLayoutPanel1.Location = new Point(-1, 4);
            tableLayoutPanel1.Margin = new Padding(4, 4, 4, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(1091, 985);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.Controls.Add(groupBox10);
            panel2.Controls.Add(groupBox5);
            panel2.Location = new Point(4, 521);
            panel2.Margin = new Padding(4, 4, 4, 4);
            panel2.Name = "panel2";
            panel2.Size = new Size(1083, 92);
            panel2.TabIndex = 8;
            // 
            // groupBox10
            // 
            groupBox10.Controls.Add(comboBoxTPCategoriya);
            groupBox10.Location = new Point(398, 0);
            groupBox10.Margin = new Padding(4, 4, 4, 4);
            groupBox10.Name = "groupBox10";
            groupBox10.Padding = new Padding(4, 4, 4, 4);
            groupBox10.Size = new Size(386, 80);
            groupBox10.TabIndex = 7;
            groupBox10.TabStop = false;
            groupBox10.Text = "Категория";
            // 
            // comboBoxTPCategoriya
            // 
            comboBoxTPCategoriya.FormattingEnabled = true;
            comboBoxTPCategoriya.Location = new Point(8, 31);
            comboBoxTPCategoriya.Margin = new Padding(4, 4, 4, 4);
            comboBoxTPCategoriya.Name = "comboBoxTPCategoriya";
            comboBoxTPCategoriya.Size = new Size(370, 33);
            comboBoxTPCategoriya.TabIndex = 0;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(textBoxPoiskTP);
            groupBox5.Location = new Point(4, 0);
            groupBox5.Margin = new Padding(4, 4, 4, 4);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(4, 4, 4, 4);
            groupBox5.Size = new Size(386, 80);
            groupBox5.TabIndex = 6;
            groupBox5.TabStop = false;
            groupBox5.Text = "Поиск";
            // 
            // textBoxPoiskTP
            // 
            textBoxPoiskTP.Location = new Point(20, 32);
            textBoxPoiskTP.Margin = new Padding(4, 4, 4, 4);
            textBoxPoiskTP.Name = "textBoxPoiskTP";
            textBoxPoiskTP.Size = new Size(320, 31);
            textBoxPoiskTP.TabIndex = 0;
            // 
            // dataGridViewTPLocal
            // 
            dataGridViewTPLocal.AllowUserToAddRows = false;
            dataGridViewTPLocal.AllowUserToDeleteRows = false;
            dataGridViewTPLocal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTPLocal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewTPLocal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTPLocal.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn7, DateGridLocalTPDetete, dataGridViewTextBoxColumn8, Time, Column1, Column2, Order1 });
            dataGridViewTPLocal.Location = new Point(4, 154);
            dataGridViewTPLocal.Margin = new Padding(4, 4, 4, 4);
            dataGridViewTPLocal.Name = "dataGridViewTPLocal";
            dataGridViewTPLocal.RowHeadersWidth = 51;
            dataGridViewTPLocal.RowTemplate.Height = 29;
            dataGridViewTPLocal.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTPLocal.Size = new Size(1083, 359);
            dataGridViewTPLocal.TabIndex = 2;
            // 
            // dataGridViewTextBoxColumn7
            // 
            dataGridViewTextBoxColumn7.HeaderText = "Id";
            dataGridViewTextBoxColumn7.MinimumWidth = 6;
            dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            dataGridViewTextBoxColumn7.Visible = false;
            // 
            // DateGridLocalTPDetete
            // 
            DateGridLocalTPDetete.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            DateGridLocalTPDetete.HeaderText = "";
            DateGridLocalTPDetete.MinimumWidth = 6;
            DateGridLocalTPDetete.Name = "DateGridLocalTPDetete";
            DateGridLocalTPDetete.Resizable = DataGridViewTriState.True;
            DateGridLocalTPDetete.SortMode = DataGridViewColumnSortMode.Automatic;
            DateGridLocalTPDetete.Text = "Удалить";
            DateGridLocalTPDetete.Width = 125;
            // 
            // dataGridViewTextBoxColumn8
            // 
            dataGridViewTextBoxColumn8.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn8.FillWeight = 69.7860947F;
            dataGridViewTextBoxColumn8.HeaderText = "Технологические переходы";
            dataGridViewTextBoxColumn8.MinimumWidth = 6;
            dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            dataGridViewTextBoxColumn8.ReadOnly = true;
            // 
            // Time
            // 
            Time.FillWeight = 69.7860947F;
            Time.HeaderText = "Время действ., мин.";
            Time.MinimumWidth = 6;
            Time.Name = "Time";
            // 
            // Column1
            // 
            Column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            Column1.HeaderText = "Этап";
            Column1.MinimumWidth = 6;
            Column1.Name = "Column1";
            Column1.Width = 130;
            // 
            // Column2
            // 
            Column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            Column2.HeaderText = "Последовательно";
            Column2.MinimumWidth = 6;
            Column2.Name = "Column2";
            Column2.Width = 130;
            // 
            // Order1
            // 
            Order1.HeaderText = "Order";
            Order1.MinimumWidth = 6;
            Order1.Name = "Order1";
            Order1.Visible = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(cbxIsReleased);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(checkBox1);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(4, 4);
            panel1.Margin = new Padding(4, 4, 4, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(1083, 142);
            panel1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(850, 98);
            button1.Margin = new Padding(4, 4, 4, 4);
            button1.Name = "button1";
            button1.Size = new Size(225, 40);
            button1.TabIndex = 4;
            button1.Text = "Сохранить";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(142, 85);
            checkBox1.Margin = new Padding(4, 4, 4, 4);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(22, 21);
            checkBox1.TabIndex = 3;
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 85);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(108, 25);
            label2.TabIndex = 2;
            label2.Text = "Типовая ТО";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(142, 15);
            textBox1.Margin = new Padding(4, 4, 4, 4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(559, 31);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 19);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(90, 25);
            label1.TabIndex = 0;
            label1.Text = "Название";
            // 
            // dataGridViewTPAll
            // 
            dataGridViewTPAll.AllowUserToAddRows = false;
            dataGridViewTPAll.AllowUserToDeleteRows = false;
            dataGridViewTPAll.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTPAll.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewTPAll.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTPAll.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn5, dataGridViewCheckBoxColumn2, dataGridViewTextBoxColumn6, TimeExecution });
            dataGridViewTPAll.Location = new Point(4, 621);
            dataGridViewTPAll.Margin = new Padding(4, 4, 4, 4);
            dataGridViewTPAll.Name = "dataGridViewTPAll";
            dataGridViewTPAll.RowHeadersWidth = 51;
            dataGridViewTPAll.RowTemplate.Height = 29;
            dataGridViewTPAll.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTPAll.Size = new Size(1083, 360);
            dataGridViewTPAll.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn5
            // 
            dataGridViewTextBoxColumn5.HeaderText = "Id";
            dataGridViewTextBoxColumn5.MinimumWidth = 6;
            dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            dataGridViewTextBoxColumn5.Visible = false;
            // 
            // dataGridViewCheckBoxColumn2
            // 
            dataGridViewCheckBoxColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCheckBoxColumn2.HeaderText = "";
            dataGridViewCheckBoxColumn2.MinimumWidth = 100;
            dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            dataGridViewCheckBoxColumn2.Resizable = DataGridViewTriState.True;
            dataGridViewCheckBoxColumn2.Text = "Добавить";
            dataGridViewCheckBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn6
            // 
            dataGridViewTextBoxColumn6.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn6.FillWeight = 167.914459F;
            dataGridViewTextBoxColumn6.HeaderText = "Технологические переходы";
            dataGridViewTextBoxColumn6.MinimumWidth = 6;
            dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            dataGridViewTextBoxColumn6.ReadOnly = true;
            dataGridViewTextBoxColumn6.Resizable = DataGridViewTriState.True;
            dataGridViewTextBoxColumn6.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // TimeExecution
            // 
            TimeExecution.HeaderText = "Время действ., мин.";
            TimeExecution.MinimumWidth = 6;
            TimeExecution.Name = "TimeExecution";
            // 
            // cbxIsReleased
            // 
            cbxIsReleased.AutoSize = true;
            cbxIsReleased.Location = new Point(734, 17);
            cbxIsReleased.Name = "cbxIsReleased";
            cbxIsReleased.Size = new Size(149, 29);
            cbxIsReleased.TabIndex = 25;
            cbxIsReleased.Text = "Опубликован";
            cbxIsReleased.UseVisualStyleBackColor = true;
            // 
            // Win7_TechOperation_Window
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1091, 992);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4, 4, 4, 4);
            Name = "Win7_TechOperation_Window";
            Text = "Win7_TechOperation_Window";
            FormClosing += Win7_TechOperation_Window_FormClosing;
            tableLayoutPanel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            groupBox10.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTPLocal).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTPAll).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private TextBox textBox1;
        private Label label1;
        private CheckBox checkBox1;
        private Label label2;
        private DataGridView dataGridViewTPAll;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewButtonColumn dataGridViewCheckBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn TimeExecution;
        private DataGridView dataGridViewTPLocal;
        private Button button1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private DataGridViewButtonColumn DateGridLocalTPDetete;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private DataGridViewTextBoxColumn Time;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Order1;
        private Panel panel2;
        private GroupBox groupBox10;
        private ComboBox comboBoxTPCategoriya;
        private GroupBox groupBox5;
        private TextBox textBoxPoiskTP;
        private CheckBox cbxIsReleased;
    }
}