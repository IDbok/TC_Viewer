﻿namespace TC_WinForms.WinForms
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
            txtArticle = new TextBox();
            label2 = new Label();
            label3 = new Label();
            txtTechProcessType = new TextBox();
            label4 = new Label();
            txtTechProcess = new TextBox();
            label5 = new Label();
            txtParametr = new TextBox();
            label6 = new Label();
            txtFinalProduct = new TextBox();
            label7 = new Label();
            txtApplicability = new TextBox();
            label8 = new Label();
            txtNote = new TextBox();
            label9 = new Label();
            label10 = new Label();
            chbxIsCompleted = new CheckBox();
            cbxType = new ComboBox();
            cbxNetworkVoltage = new ComboBox();
            btnSaveAndOpen = new Button();
            btnSave = new Button();
            btnExportExcel = new Button();
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
            // txtArticle
            // 
            txtArticle.Location = new Point(221, 24);
            txtArticle.Name = "txtArticle";
            txtArticle.Size = new Size(503, 27);
            txtArticle.TabIndex = 1;
            txtArticle.TextChanged += textBox1_TextChanged;
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
            // txtTechProcessType
            // 
            txtTechProcessType.Location = new Point(221, 154);
            txtTechProcessType.Name = "txtTechProcessType";
            txtTechProcessType.Size = new Size(503, 27);
            txtTechProcessType.TabIndex = 7;
            txtTechProcessType.TextChanged += textBox1_TextChanged;
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
            // txtTechProcess
            // 
            txtTechProcess.Location = new Point(221, 199);
            txtTechProcess.Name = "txtTechProcess";
            txtTechProcess.Size = new Size(503, 27);
            txtTechProcess.TabIndex = 9;
            txtTechProcess.TextChanged += textBox1_TextChanged;
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
            // txtParametr
            // 
            txtParametr.Location = new Point(221, 244);
            txtParametr.Name = "txtParametr";
            txtParametr.Size = new Size(503, 27);
            txtParametr.TabIndex = 11;
            txtParametr.TextChanged += textBox1_TextChanged;
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
            // txtFinalProduct
            // 
            txtFinalProduct.Location = new Point(221, 290);
            txtFinalProduct.Name = "txtFinalProduct";
            txtFinalProduct.Size = new Size(503, 27);
            txtFinalProduct.TabIndex = 13;
            txtFinalProduct.TextChanged += textBox1_TextChanged;
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
            // txtApplicability
            // 
            txtApplicability.Location = new Point(221, 333);
            txtApplicability.Name = "txtApplicability";
            txtApplicability.Size = new Size(503, 27);
            txtApplicability.TabIndex = 15;
            txtApplicability.TextChanged += textBox1_TextChanged;
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
            // txtNote
            // 
            txtNote.Location = new Point(221, 379);
            txtNote.Name = "txtNote";
            txtNote.Size = new Size(503, 27);
            txtNote.TabIndex = 17;
            txtNote.TextChanged += textBox1_TextChanged;
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
            // chbxIsCompleted
            // 
            chbxIsCompleted.AutoSize = true;
            chbxIsCompleted.Location = new Point(221, 427);
            chbxIsCompleted.Name = "chbxIsCompleted";
            chbxIsCompleted.Size = new Size(18, 17);
            chbxIsCompleted.TabIndex = 19;
            chbxIsCompleted.UseVisualStyleBackColor = true;
            // 
            // cbxType
            // 
            cbxType.FormattingEnabled = true;
            cbxType.Location = new Point(221, 67);
            cbxType.Name = "cbxType";
            cbxType.Size = new Size(256, 28);
            cbxType.TabIndex = 20;
            cbxType.SelectedIndexChanged += comboBoxType_SelectedIndexChanged;
            // 
            // cbxNetworkVoltage
            // 
            cbxNetworkVoltage.FormattingEnabled = true;
            cbxNetworkVoltage.Location = new Point(221, 111);
            cbxNetworkVoltage.Name = "cbxNetworkVoltage";
            cbxNetworkVoltage.Size = new Size(256, 28);
            cbxNetworkVoltage.TabIndex = 21;
            cbxNetworkVoltage.SelectedIndexChanged += comboBoxType_SelectedIndexChanged;
            // 
            // btnSaveAndOpen
            // 
            btnSaveAndOpen.Location = new Point(30, 503);
            btnSaveAndOpen.Name = "btnSaveAndOpen";
            btnSaveAndOpen.Size = new Size(209, 63);
            btnSaveAndOpen.TabIndex = 22;
            btnSaveAndOpen.Text = "Открыть";
            btnSaveAndOpen.UseVisualStyleBackColor = true;
            btnSaveAndOpen.Click += btnSaveAndOpen_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(255, 503);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(221, 63);
            btnSave.TabIndex = 23;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += button2_Click;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Location = new Point(597, 503);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(126, 63);
            btnExportExcel.TabIndex = 24;
            btnExportExcel.Text = "Печать";
            btnExportExcel.UseVisualStyleBackColor = true;
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // Win7_1_TCs_Window
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(762, 626);
            Controls.Add(btnExportExcel);
            Controls.Add(btnSave);
            Controls.Add(btnSaveAndOpen);
            Controls.Add(cbxNetworkVoltage);
            Controls.Add(cbxType);
            Controls.Add(chbxIsCompleted);
            Controls.Add(label10);
            Controls.Add(txtNote);
            Controls.Add(label9);
            Controls.Add(txtApplicability);
            Controls.Add(label8);
            Controls.Add(txtFinalProduct);
            Controls.Add(label7);
            Controls.Add(txtParametr);
            Controls.Add(label6);
            Controls.Add(txtTechProcess);
            Controls.Add(label5);
            Controls.Add(txtTechProcessType);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtArticle);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimumSize = new Size(780, 520);
            Name = "Win7_1_TCs_Window";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Win7_1_TCs_Window";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtArticle;
        private Label label2;
        private Label label3;
        private TextBox txtTechProcessType;
        private Label label4;
        private TextBox txtTechProcess;
        private Label label5;
        private TextBox txtParametr;
        private Label label6;
        private TextBox txtFinalProduct;
        private Label label7;
        private TextBox txtApplicability;
        private Label label8;
        private TextBox txtNote;
        private Label label9;
        private Label label10;
        private CheckBox chbxIsCompleted;
        private ComboBox cbxType;
        private ComboBox cbxNetworkVoltage;
        private Button btnSaveAndOpen;
        private Button button2;
        private Button btnExportExcel;
        private Button btnSave;
    }
}