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
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(38, 71);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(85, 25);
            label1.TabIndex = 0;
            label1.Text = "Артикул";
            // 
            // txtArticle
            // 
            txtArticle.Location = new Point(276, 68);
            txtArticle.Margin = new Padding(4, 4, 4, 4);
            txtArticle.Name = "txtArticle";
            txtArticle.Size = new Size(628, 31);
            txtArticle.TabIndex = 2;
            txtArticle.TextChanged += textBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(38, 125);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(103, 25);
            label2.TabIndex = 2;
            label2.Text = "Тип карты";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(38, 180);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(83, 25);
            label3.TabIndex = 4;
            label3.Text = "Сеть, кВ";
            // 
            // txtTechProcessType
            // 
            txtTechProcessType.Location = new Point(276, 230);
            txtTechProcessType.Margin = new Padding(4, 4, 4, 4);
            txtTechProcessType.Name = "txtTechProcessType";
            txtTechProcessType.Size = new Size(628, 31);
            txtTechProcessType.TabIndex = 5;
            txtTechProcessType.TextChanged += textBox1_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(38, 234);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(156, 25);
            label4.TabIndex = 6;
            label4.Text = "Тип тех. процесса";
            // 
            // txtTechProcess
            // 
            txtTechProcess.Location = new Point(276, 286);
            txtTechProcess.Margin = new Padding(4, 4, 4, 4);
            txtTechProcess.Name = "txtTechProcess";
            txtTechProcess.Size = new Size(628, 31);
            txtTechProcess.TabIndex = 6;
            txtTechProcess.TextChanged += textBox1_TextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(38, 290);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(115, 25);
            label5.TabIndex = 8;
            label5.Text = "Тех. процесс";
            // 
            // txtParametr
            // 
            txtParametr.Location = new Point(276, 342);
            txtParametr.Margin = new Padding(4, 4, 4, 4);
            txtParametr.Name = "txtParametr";
            txtParametr.Size = new Size(628, 31);
            txtParametr.TabIndex = 7;
            txtParametr.TextChanged += textBox1_TextChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(38, 346);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(94, 25);
            label6.TabIndex = 10;
            label6.Text = "Параметр";
            // 
            // txtFinalProduct
            // 
            txtFinalProduct.Location = new Point(276, 400);
            txtFinalProduct.Margin = new Padding(4, 4, 4, 4);
            txtFinalProduct.Name = "txtFinalProduct";
            txtFinalProduct.Size = new Size(628, 31);
            txtFinalProduct.TabIndex = 8;
            txtFinalProduct.TextChanged += textBox1_TextChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(38, 404);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(167, 25);
            label7.TabIndex = 12;
            label7.Text = "Конечный продукт";
            // 
            // txtApplicability
            // 
            txtApplicability.Location = new Point(276, 454);
            txtApplicability.Margin = new Padding(4, 4, 4, 4);
            txtApplicability.Name = "txtApplicability";
            txtApplicability.Size = new Size(628, 31);
            txtApplicability.TabIndex = 9;
            txtApplicability.TextChanged += textBox1_TextChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(38, 458);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(223, 25);
            label8.TabIndex = 14;
            label8.Text = "Применимость тех. карты";
            // 
            // txtNote
            // 
            txtNote.Location = new Point(276, 511);
            txtNote.Margin = new Padding(4, 4, 4, 4);
            txtNote.Name = "txtNote";
            txtNote.Size = new Size(628, 31);
            txtNote.TabIndex = 10;
            txtNote.TextChanged += textBox1_TextChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(38, 515);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(116, 25);
            label9.TabIndex = 16;
            label9.Text = "Примечания";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(771, 578);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(82, 25);
            label10.TabIndex = 18;
            label10.Text = "Наличие";
            label10.Visible = false;
            // 
            // chbxIsCompleted
            // 
            chbxIsCompleted.AutoSize = true;
            chbxIsCompleted.Location = new Point(879, 578);
            chbxIsCompleted.Margin = new Padding(4, 4, 4, 4);
            chbxIsCompleted.Name = "chbxIsCompleted";
            chbxIsCompleted.Size = new Size(22, 21);
            chbxIsCompleted.TabIndex = 19;
            chbxIsCompleted.UseVisualStyleBackColor = true;
            chbxIsCompleted.Visible = false;
            // 
            // cbxType
            // 
            cbxType.FormattingEnabled = true;
            cbxType.Location = new Point(276, 121);
            cbxType.Margin = new Padding(4, 4, 4, 4);
            cbxType.Name = "cbxType";
            cbxType.Size = new Size(319, 33);
            cbxType.TabIndex = 3;
            cbxType.SelectedIndexChanged += comboBoxType_SelectedIndexChanged;
            // 
            // cbxNetworkVoltage
            // 
            cbxNetworkVoltage.FormattingEnabled = true;
            cbxNetworkVoltage.Location = new Point(276, 176);
            cbxNetworkVoltage.Margin = new Padding(4, 4, 4, 4);
            cbxNetworkVoltage.Name = "cbxNetworkVoltage";
            cbxNetworkVoltage.Size = new Size(319, 33);
            cbxNetworkVoltage.TabIndex = 4;
            cbxNetworkVoltage.SelectedIndexChanged += comboBoxType_SelectedIndexChanged;
            // 
            // btnSaveAndOpen
            // 
            btnSaveAndOpen.Location = new Point(38, 629);
            btnSaveAndOpen.Margin = new Padding(4, 4, 4, 4);
            btnSaveAndOpen.Name = "btnSaveAndOpen";
            btnSaveAndOpen.Size = new Size(261, 79);
            btnSaveAndOpen.TabIndex = 22;
            btnSaveAndOpen.Text = "Открыть";
            btnSaveAndOpen.UseVisualStyleBackColor = true;
            btnSaveAndOpen.Click += btnSaveAndOpen_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(319, 629);
            btnSave.Margin = new Padding(4, 4, 4, 4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(276, 79);
            btnSave.TabIndex = 23;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += button2_Click;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Location = new Point(746, 629);
            btnExportExcel.Margin = new Padding(4, 4, 4, 4);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(158, 79);
            btnExportExcel.TabIndex = 24;
            btnExportExcel.Text = "Печать";
            btnExportExcel.UseVisualStyleBackColor = true;
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // txtName
            // 
            txtName.Location = new Point(276, 19);
            txtName.Margin = new Padding(4, 4, 4, 4);
            txtName.Name = "txtName";
            txtName.Size = new Size(628, 31);
            txtName.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblName.Location = new Point(38, 22);
            lblName.Margin = new Padding(4, 0, 4, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(135, 25);
            lblName.TabIndex = 25;
            lblName.Text = "Наименование";
            // 
            // cbxStatus
            // 
            cbxStatus.FormattingEnabled = true;
            cbxStatus.Location = new Point(276, 568);
            cbxStatus.Margin = new Padding(4, 4, 4, 4);
            cbxStatus.Name = "cbxStatus";
            cbxStatus.Size = new Size(319, 33);
            cbxStatus.TabIndex = 26;
            cbxStatus.Visible = false;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblStatus.Location = new Point(38, 571);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(63, 25);
            lblStatus.TabIndex = 27;
            lblStatus.Text = "Статус";
            lblStatus.Visible = false;
            // 
            // Win7_1_TCs_Window
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(952, 782);
            Controls.Add(cbxStatus);
            Controls.Add(lblStatus);
            Controls.Add(txtName);
            Controls.Add(lblName);
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
            Margin = new Padding(4, 4, 4, 4);
            MinimumSize = new Size(970, 636);
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
        private TextBox txtName;
        private Label lblName;
        private ComboBox cbxStatus;
        private Label lblStatus;
    }
}