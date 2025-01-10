
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
			txtName = new TextBox();
			lblName = new Label();
			cbxStatus = new ComboBox();
			lblStatus = new Label();
			btnClone = new Button();
			panel1 = new Panel();
			panel1.SuspendLayout();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.Location = new Point(26, 62);
			label1.Name = "label1";
			label1.Size = new Size(80, 23);
			label1.TabIndex = 0;
			label1.Text = "Артикул";
			// 
			// txtArticle
			// 
			txtArticle.Location = new Point(241, 59);
			txtArticle.Name = "txtArticle";
			txtArticle.Size = new Size(565, 30);
			txtArticle.TabIndex = 2;
			txtArticle.TextChanged += textBox1_TextChanged;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label2.Location = new Point(26, 112);
			label2.Name = "label2";
			label2.Size = new Size(97, 23);
			label2.TabIndex = 2;
			label2.Text = "Тип карты";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label3.Location = new Point(26, 162);
			label3.Name = "label3";
			label3.Size = new Size(79, 23);
			label3.TabIndex = 4;
			label3.Text = "Сеть, кВ";
			// 
			// txtTechProcessType
			// 
			txtTechProcessType.Location = new Point(241, 208);
			txtTechProcessType.Name = "txtTechProcessType";
			txtTechProcessType.Size = new Size(565, 30);
			txtTechProcessType.TabIndex = 5;
			txtTechProcessType.TextChanged += textBox1_TextChanged;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new Point(26, 212);
			label4.Name = "label4";
			label4.Size = new Size(151, 23);
			label4.TabIndex = 6;
			label4.Text = "Тип тех. процесса";
			// 
			// txtTechProcess
			// 
			txtTechProcess.Location = new Point(241, 260);
			txtTechProcess.Name = "txtTechProcess";
			txtTechProcess.Size = new Size(565, 30);
			txtTechProcess.TabIndex = 6;
			txtTechProcess.TextChanged += textBox1_TextChanged;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new Point(26, 263);
			label5.Name = "label5";
			label5.Size = new Size(110, 23);
			label5.TabIndex = 8;
			label5.Text = "Тех. процесс";
			// 
			// txtParametr
			// 
			txtParametr.Location = new Point(241, 312);
			txtParametr.Name = "txtParametr";
			txtParametr.Size = new Size(565, 30);
			txtParametr.TabIndex = 7;
			txtParametr.TextChanged += textBox1_TextChanged;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Location = new Point(26, 315);
			label6.Name = "label6";
			label6.Size = new Size(88, 23);
			label6.TabIndex = 10;
			label6.Text = "Параметр";
			// 
			// txtFinalProduct
			// 
			txtFinalProduct.Location = new Point(241, 365);
			txtFinalProduct.Name = "txtFinalProduct";
			txtFinalProduct.Size = new Size(565, 30);
			txtFinalProduct.TabIndex = 8;
			txtFinalProduct.TextChanged += textBox1_TextChanged;
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Location = new Point(26, 368);
			label7.Name = "label7";
			label7.Size = new Size(158, 23);
			label7.TabIndex = 12;
			label7.Text = "Конечный продукт";
			// 
			// txtApplicability
			// 
			txtApplicability.Location = new Point(241, 414);
			txtApplicability.Name = "txtApplicability";
			txtApplicability.Size = new Size(565, 30);
			txtApplicability.TabIndex = 9;
			txtApplicability.TextChanged += textBox1_TextChanged;
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Location = new Point(26, 417);
			label8.Name = "label8";
			label8.Size = new Size(213, 23);
			label8.TabIndex = 14;
			label8.Text = "Применимость тех. карты";
			// 
			// txtNote
			// 
			txtNote.Location = new Point(241, 467);
			txtNote.Name = "txtNote";
			txtNote.Size = new Size(565, 30);
			txtNote.TabIndex = 10;
			txtNote.TextChanged += textBox1_TextChanged;
			// 
			// label9
			// 
			label9.AutoSize = true;
			label9.Location = new Point(26, 470);
			label9.Name = "label9";
			label9.Size = new Size(111, 23);
			label9.TabIndex = 16;
			label9.Text = "Примечания";
			// 
			// label10
			// 
			label10.AutoSize = true;
			label10.Location = new Point(686, 528);
			label10.Name = "label10";
			label10.Size = new Size(79, 23);
			label10.TabIndex = 18;
			label10.Text = "Наличие";
			label10.Visible = false;
			// 
			// chbxIsCompleted
			// 
			chbxIsCompleted.AutoSize = true;
			chbxIsCompleted.Location = new Point(783, 528);
			chbxIsCompleted.Name = "chbxIsCompleted";
			chbxIsCompleted.Size = new Size(18, 17);
			chbxIsCompleted.TabIndex = 12;
			chbxIsCompleted.UseVisualStyleBackColor = true;
			chbxIsCompleted.Visible = false;
			// 
			// cbxType
			// 
			cbxType.FormattingEnabled = true;
			cbxType.Location = new Point(241, 108);
			cbxType.Name = "cbxType";
			cbxType.Size = new Size(288, 31);
			cbxType.TabIndex = 3;
			cbxType.SelectedIndexChanged += comboBoxType_SelectedIndexChanged;
			// 
			// cbxNetworkVoltage
			// 
			cbxNetworkVoltage.FormattingEnabled = true;
			cbxNetworkVoltage.Location = new Point(241, 159);
			cbxNetworkVoltage.Name = "cbxNetworkVoltage";
			cbxNetworkVoltage.Size = new Size(288, 31);
			cbxNetworkVoltage.TabIndex = 4;
			cbxNetworkVoltage.SelectedIndexChanged += comboBoxType_SelectedIndexChanged;
			// 
			// btnSaveAndOpen
			// 
			btnSaveAndOpen.Location = new Point(26, 575);
			btnSaveAndOpen.Name = "btnSaveAndOpen";
			btnSaveAndOpen.Size = new Size(191, 72);
			btnSaveAndOpen.TabIndex = 0;
			btnSaveAndOpen.Text = "Открыть";
			btnSaveAndOpen.UseVisualStyleBackColor = true;
			btnSaveAndOpen.Click += btnSaveAndOpen_Click;
			// 
			// btnSave
			// 
			btnSave.Location = new Point(225, 575);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(191, 72);
			btnSave.TabIndex = 13;
			btnSave.Text = "Сохранить";
			btnSave.UseVisualStyleBackColor = true;
			btnSave.Click += btnSave_Click;
			// 
			// btnExportExcel
			// 
			btnExportExcel.Location = new Point(664, 575);
			btnExportExcel.Name = "btnExportExcel";
			btnExportExcel.Size = new Size(142, 72);
			btnExportExcel.TabIndex = 15;
			btnExportExcel.Text = "Печать";
			btnExportExcel.UseVisualStyleBackColor = true;
			btnExportExcel.Click += btnExportExcel_Click;
			// 
			// txtName
			// 
			txtName.Location = new Point(241, 14);
			txtName.Name = "txtName";
			txtName.Size = new Size(565, 30);
			txtName.TabIndex = 1;
			// 
			// lblName
			// 
			lblName.AutoSize = true;
			lblName.Font = new Font("Segoe UI", 9F);
			lblName.Location = new Point(26, 17);
			lblName.Name = "lblName";
			lblName.Size = new Size(129, 23);
			lblName.TabIndex = 25;
			lblName.Text = "Наименование";
			// 
			// cbxStatus
			// 
			cbxStatus.FormattingEnabled = true;
			cbxStatus.Location = new Point(241, 519);
			cbxStatus.Name = "cbxStatus";
			cbxStatus.Size = new Size(288, 31);
			cbxStatus.TabIndex = 11;
			cbxStatus.Visible = false;
			// 
			// lblStatus
			// 
			lblStatus.AutoSize = true;
			lblStatus.Font = new Font("Segoe UI", 9F);
			lblStatus.Location = new Point(26, 522);
			lblStatus.Name = "lblStatus";
			lblStatus.Size = new Size(60, 23);
			lblStatus.TabIndex = 27;
			lblStatus.Text = "Статус";
			lblStatus.Visible = false;
			// 
			// btnClone
			// 
			btnClone.Location = new Point(461, 575);
			btnClone.Name = "btnClone";
			btnClone.Size = new Size(191, 72);
			btnClone.TabIndex = 14;
			btnClone.Text = "Копировать карту";
			btnClone.UseVisualStyleBackColor = true;
			btnClone.Visible = false;
			btnClone.Click += btnClone_Click;
			// 
			// panel1
			// 
			panel1.Controls.Add(lblName);
			panel1.Controls.Add(cbxStatus);
			panel1.Controls.Add(label1);
			panel1.Controls.Add(lblStatus);
			panel1.Controls.Add(txtArticle);
			panel1.Controls.Add(txtName);
			panel1.Controls.Add(label2);
			panel1.Controls.Add(label3);
			panel1.Controls.Add(btnExportExcel);
			panel1.Controls.Add(label4);
			panel1.Controls.Add(btnClone);
			panel1.Controls.Add(btnSave);
			panel1.Controls.Add(txtTechProcessType);
			panel1.Controls.Add(btnSaveAndOpen);
			panel1.Controls.Add(label5);
			panel1.Controls.Add(cbxNetworkVoltage);
			panel1.Controls.Add(txtTechProcess);
			panel1.Controls.Add(cbxType);
			panel1.Controls.Add(label6);
			panel1.Controls.Add(chbxIsCompleted);
			panel1.Controls.Add(txtParametr);
			panel1.Controls.Add(label10);
			panel1.Controls.Add(label7);
			panel1.Controls.Add(txtNote);
			panel1.Controls.Add(txtFinalProduct);
			panel1.Controls.Add(label9);
			panel1.Controls.Add(label8);
			panel1.Controls.Add(txtApplicability);
			panel1.Dock = DockStyle.Fill;
			panel1.Location = new Point(0, 0);
			panel1.Name = "panel1";
			panel1.Size = new Size(857, 727);
			panel1.TabIndex = 28;
			// 
			// Win7_1_TCs_Window
			// 
			AutoScaleDimensions = new SizeF(9F, 23F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(857, 727);
			Controls.Add(panel1);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MinimumSize = new Size(875, 588);
			Name = "Win7_1_TCs_Window";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Win7_1_TCs_Window";
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			ResumeLayout(false);
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
        private TextBox txtName;
        private Label lblName;
        private ComboBox cbxStatus;
        private Label lblStatus;
        private Button btnClone;
        private Panel panel1;
    }
}