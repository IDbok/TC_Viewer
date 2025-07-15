namespace TC_WinForms.WinForms.Work
{
    partial class CoefficientForm
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
            groupBox1 = new GroupBox();
            panel1 = new Panel();
            label1 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            tbxCoefficient = new TextBox();
            groupBox2 = new GroupBox();
            panel2 = new Panel();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            textBox2 = new TextBox();
            groupBox3 = new GroupBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label9 = new Label();
            label10 = new Label();
            btnAddCommand = new Button();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            panel2.SuspendLayout();
            groupBox3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(tbxCoefficient);
            groupBox1.Location = new Point(12, 51);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(490, 99);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Время действия, мин.";
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Location = new Point(6, 50);
            panel1.Name = "panel1";
            panel1.Size = new Size(105, 24);
            panel1.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Right;
            label1.Location = new Point(88, 0);
            label1.Name = "label1";
            label1.Size = new Size(17, 20);
            label1.TabIndex = 0;
            label1.Text = "5";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(363, 50);
            label4.Name = "label4";
            label4.Size = new Size(17, 20);
            label4.TabIndex = 4;
            label4.Text = "5";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(348, 50);
            label3.Name = "label3";
            label3.Size = new Size(19, 20);
            label3.TabIndex = 3;
            label3.Text = "=";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(177, 24);
            label2.Name = "label2";
            label2.Size = new Size(71, 20);
            label2.TabIndex = 2;
            label2.Text = "формула";
            // 
            // tbxCoefficient
            // 
            tbxCoefficient.Location = new Point(117, 47);
            tbxCoefficient.Name = "tbxCoefficient";
            tbxCoefficient.Size = new Size(225, 27);
            tbxCoefficient.TabIndex = 1;
            tbxCoefficient.TextChanged += tbxCoefficient_TextChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(panel2);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(textBox2);
            groupBox2.Location = new Point(12, 156);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(490, 99);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "Пример";
            // 
            // panel2
            // 
            panel2.Controls.Add(label5);
            panel2.Location = new Point(6, 50);
            panel2.Name = "panel2";
            panel2.Size = new Size(105, 24);
            panel2.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Right;
            label5.Location = new Point(88, 0);
            label5.Name = "label5";
            label5.Size = new Size(17, 20);
            label5.TabIndex = 0;
            label5.Text = "5";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(363, 50);
            label6.Name = "label6";
            label6.Size = new Size(25, 20);
            label6.TabIndex = 4;
            label6.Text = "25";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(348, 50);
            label7.Name = "label7";
            label7.Size = new Size(19, 20);
            label7.TabIndex = 3;
            label7.Text = "=";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(177, 24);
            label8.Name = "label8";
            label8.Size = new Size(71, 20);
            label8.TabIndex = 2;
            label8.Text = "формула";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(117, 47);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(225, 27);
            textBox2.TabIndex = 1;
            textBox2.Text = "*5";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(flowLayoutPanel1);
            groupBox3.Location = new Point(12, 261);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(490, 235);
            groupBox3.TabIndex = 7;
            groupBox3.TabStop = false;
            groupBox3.Text = "Правила расчета времени тех. перехода";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Controls.Add(label9);
            flowLayoutPanel1.Location = new Point(6, 26);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(484, 203);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(3, 0);
            label9.Name = "label9";
            label9.Size = new Size(50, 20);
            label9.TabIndex = 0;
            label9.Text = "label9";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(18, 19);
            label10.Name = "label10";
            label10.Size = new Size(58, 20);
            label10.TabIndex = 8;
            label10.Text = "label10";
            // 
            // btnAddCommand
            // 
            btnAddCommand.Location = new Point(169, 524);
            btnAddCommand.Name = "btnAddCommand";
            btnAddCommand.Size = new Size(185, 29);
            btnAddCommand.TabIndex = 9;
            btnAddCommand.Text = "Добавить";
            btnAddCommand.UseVisualStyleBackColor = true;
            btnAddCommand.Click += btnAddCommand_Click;
            // 
            // CoefficientForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(530, 587);
            Controls.Add(btnAddCommand);
            Controls.Add(label10);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "CoefficientForm";
            Text = "Коэффициент";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            groupBox3.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private Label label4;
        private Label label3;
        private Label label2;
        private TextBox tbxCoefficient;
        private Panel panel1;
        private Label label1;
        private GroupBox groupBox2;
        private Panel panel2;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private TextBox textBox2;
        private GroupBox groupBox3;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label9;
        private Label label10;
        private Button btnAddCommand;
    }
}