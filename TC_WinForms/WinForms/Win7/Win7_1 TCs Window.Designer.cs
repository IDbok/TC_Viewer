namespace TC_WinForms.WinForms
{
    partial class Win7_1_TCs_Window
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
            label1 = new Label();
            textBox1 = new TextBox();
            label2 = new Label();
            label3 = new Label();
            textBox4 = new TextBox();
            label4 = new Label();
            textBox5 = new TextBox();
            label5 = new Label();
            textBox6 = new TextBox();
            label6 = new Label();
            textBox7 = new TextBox();
            label7 = new Label();
            textBox8 = new TextBox();
            label8 = new Label();
            textBox9 = new TextBox();
            label9 = new Label();
            label10 = new Label();
            checkBox1 = new CheckBox();
            comboBoxType = new ComboBox();
            comboBoxNetworkVoltage = new ComboBox();
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(30, 27);
            label1.Name = "label1";
            label1.Size = new Size(69, 20);
            label1.TabIndex = 0;
            label1.Text = "Артикул";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(221, 24);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(503, 27);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(30, 70);
            label2.Name = "label2";
            label2.Size = new Size(83, 20);
            label2.TabIndex = 2;
            label2.Text = "Тип карты";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(30, 114);
            label3.Name = "label3";
            label3.Size = new Size(67, 20);
            label3.TabIndex = 4;
            label3.Text = "Сеть, кВ";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(221, 154);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(503, 27);
            textBox4.TabIndex = 7;
            textBox4.TextChanged += textBox1_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(30, 157);
            label4.Name = "label4";
            label4.Size = new Size(133, 20);
            label4.TabIndex = 6;
            label4.Text = "Тип тех. процесса";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(221, 199);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(503, 27);
            textBox5.TabIndex = 9;
            textBox5.TextChanged += textBox1_TextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(30, 202);
            label5.Name = "label5";
            label5.Size = new Size(97, 20);
            label5.TabIndex = 8;
            label5.Text = "Тех. процесс";
            // 
            // textBox6
            // 
            textBox6.Location = new Point(221, 244);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(503, 27);
            textBox6.TabIndex = 11;
            textBox6.TextChanged += textBox1_TextChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(30, 247);
            label6.Name = "label6";
            label6.Size = new Size(79, 20);
            label6.TabIndex = 10;
            label6.Text = "Параметр";
            // 
            // textBox7
            // 
            textBox7.Location = new Point(221, 290);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(503, 27);
            textBox7.TabIndex = 13;
            textBox7.TextChanged += textBox1_TextChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(30, 293);
            label7.Name = "label7";
            label7.Size = new Size(140, 20);
            label7.TabIndex = 12;
            label7.Text = "Конечный продукт";
            // 
            // textBox8
            // 
            textBox8.Location = new Point(221, 333);
            textBox8.Name = "textBox8";
            textBox8.Size = new Size(503, 27);
            textBox8.TabIndex = 15;
            textBox8.TextChanged += textBox1_TextChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(30, 336);
            label8.Name = "label8";
            label8.Size = new Size(189, 20);
            label8.TabIndex = 14;
            label8.Text = "Применимость тех. карты";
            // 
            // textBox9
            // 
            textBox9.Location = new Point(221, 379);
            textBox9.Name = "textBox9";
            textBox9.Size = new Size(503, 27);
            textBox9.TabIndex = 17;
            textBox9.TextChanged += textBox1_TextChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(30, 382);
            label9.Name = "label9";
            label9.Size = new Size(99, 20);
            label9.TabIndex = 16;
            label9.Text = "Примечания";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(30, 428);
            label10.Name = "label10";
            label10.Size = new Size(70, 20);
            label10.TabIndex = 18;
            label10.Text = "Наличие";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(221, 427);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(18, 17);
            checkBox1.TabIndex = 19;
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // comboBoxType
            // 
            comboBoxType.FormattingEnabled = true;
            comboBoxType.Location = new Point(221, 67);
            comboBoxType.Name = "comboBoxType";
            comboBoxType.Size = new Size(256, 28);
            comboBoxType.TabIndex = 20;
            comboBoxType.SelectedIndexChanged += comboBoxType_SelectedIndexChanged;
            // 
            // comboBoxNetworkVoltage
            // 
            comboBoxNetworkVoltage.FormattingEnabled = true;
            comboBoxNetworkVoltage.Location = new Point(221, 111);
            comboBoxNetworkVoltage.Name = "comboBoxNetworkVoltage";
            comboBoxNetworkVoltage.Size = new Size(256, 28);
            comboBoxNetworkVoltage.TabIndex = 21;
            comboBoxNetworkVoltage.SelectedIndexChanged += comboBoxType_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.Location = new Point(30, 503);
            button1.Name = "button1";
            button1.Size = new Size(209, 63);
            button1.TabIndex = 22;
            button1.Text = "Сохранить и открыть";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(290, 503);
            button2.Name = "button2";
            button2.Size = new Size(221, 63);
            button2.TabIndex = 23;
            button2.Text = "Сохранить";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Win7_1_TCs_Window
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(787, 624);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(comboBoxNetworkVoltage);
            Controls.Add(comboBoxType);
            Controls.Add(checkBox1);
            Controls.Add(label10);
            Controls.Add(textBox9);
            Controls.Add(label9);
            Controls.Add(textBox8);
            Controls.Add(label8);
            Controls.Add(textBox7);
            Controls.Add(label7);
            Controls.Add(textBox6);
            Controls.Add(label6);
            Controls.Add(textBox5);
            Controls.Add(label5);
            Controls.Add(textBox4);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Name = "Win7_1_TCs_Window";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Win7_1_TCs_Window";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private Label label2;
        private Label label3;
        private TextBox textBox4;
        private Label label4;
        private TextBox textBox5;
        private Label label5;
        private TextBox textBox6;
        private Label label6;
        private TextBox textBox7;
        private Label label7;
        private TextBox textBox8;
        private Label label8;
        private TextBox textBox9;
        private Label label9;
        private Label label10;
        private CheckBox checkBox1;
        private ComboBox comboBoxType;
        private ComboBox comboBoxNetworkVoltage;
        private Button button1;
        private Button button2;
    }
}