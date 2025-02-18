namespace TC_WinForms.WinForms
{
    partial class Win7_StaffEditor
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
            txtType = new TextBox();
            lblType = new Label();
            txtName = new TextBox();
            lblName = new Label();
            rtxtFunctions = new RichTextBox();
            lblFunctions = new Label();
            rtxtComment = new RichTextBox();
            rtxtQualification = new RichTextBox();
            lblCombineResponsibility = new Label();
            lblComment = new Label();
            lblQualification = new Label();
            btnClose = new Button();
            btnSave = new Button();
            dgvRelatedStaffs = new DataGridView();
            btnAddRelatedStaff = new Button();
            btnDeleteRelatedStaff = new Button();
            cbxIsReleased = new CheckBox();
            txtClassifierCode = new TextBox();
            lblClassifierCode = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel1 = new Panel();
            panel2 = new Panel();
            txtPrice = new TextBox();
            lblPrice = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvRelatedStaffs).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // txtType
            // 
            txtType.Location = new Point(144, 55);
            txtType.Margin = new Padding(2);
            txtType.Name = "txtType";
            txtType.Size = new Size(386, 23);
            txtType.TabIndex = 7;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblType.Location = new Point(12, 55);
            lblType.Margin = new Padding(2, 0, 2, 0);
            lblType.Name = "lblType";
            lblType.Size = new Size(113, 15);
            lblType.TabIndex = 6;
            lblType.Text = "Тип (исполнение):";
            // 
            // txtName
            // 
            txtName.Location = new Point(144, 21);
            txtName.Margin = new Padding(2);
            txtName.Name = "txtName";
            txtName.Size = new Size(264, 23);
            txtName.TabIndex = 5;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblName.Location = new Point(12, 21);
            lblName.Margin = new Padding(2, 0, 2, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(98, 15);
            lblName.TabIndex = 4;
            lblName.Text = "Наименование:";
            // 
            // rtxtFunctions
            // 
            rtxtFunctions.Location = new Point(144, 88);
            rtxtFunctions.Margin = new Padding(2);
            rtxtFunctions.Name = "rtxtFunctions";
            rtxtFunctions.Size = new Size(386, 50);
            rtxtFunctions.TabIndex = 8;
            rtxtFunctions.Text = "";
            // 
            // lblFunctions
            // 
            lblFunctions.AutoSize = true;
            lblFunctions.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblFunctions.Location = new Point(12, 88);
            lblFunctions.Margin = new Padding(2, 0, 2, 0);
            lblFunctions.Name = "lblFunctions";
            lblFunctions.Size = new Size(64, 15);
            lblFunctions.TabIndex = 9;
            lblFunctions.Text = "Функции:";
            // 
            // rtxtComment
            // 
            rtxtComment.Location = new Point(144, 374);
            rtxtComment.Margin = new Padding(2);
            rtxtComment.Name = "rtxtComment";
            rtxtComment.Size = new Size(386, 110);
            rtxtComment.TabIndex = 14;
            rtxtComment.Text = "";
            // 
            // rtxtQualification
            // 
            rtxtQualification.Location = new Point(144, 148);
            rtxtQualification.Margin = new Padding(2);
            rtxtQualification.Name = "rtxtQualification";
            rtxtQualification.Size = new Size(386, 68);
            rtxtQualification.TabIndex = 9;
            rtxtQualification.Text = "";
            // 
            // lblCombineResponsibility
            // 
            lblCombineResponsibility.AutoSize = true;
            lblCombineResponsibility.Location = new Point(12, 291);
            lblCombineResponsibility.Margin = new Padding(2, 0, 2, 0);
            lblCombineResponsibility.Name = "lblCombineResponsibility";
            lblCombineResponsibility.Size = new Size(86, 45);
            lblCombineResponsibility.TabIndex = 13;
            lblCombineResponsibility.Text = "Возможность \r\nсовмещения\r\nобязанностей:";
            // 
            // lblComment
            // 
            lblComment.AutoSize = true;
            lblComment.Location = new Point(12, 374);
            lblComment.Margin = new Padding(2, 0, 2, 0);
            lblComment.Name = "lblComment";
            lblComment.Size = new Size(87, 15);
            lblComment.TabIndex = 15;
            lblComment.Text = "Комментарии:";
            // 
            // lblQualification
            // 
            lblQualification.AutoSize = true;
            lblQualification.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblQualification.Location = new Point(12, 148);
            lblQualification.Margin = new Padding(2, 0, 2, 0);
            lblQualification.Name = "lblQualification";
            lblQualification.Size = new Size(99, 15);
            lblQualification.TabIndex = 16;
            lblQualification.Text = "Квалификация:";
            // 
            // btnClose
            // 
            btnClose.Location = new Point(410, 0);
            btnClose.Margin = new Padding(2);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(119, 42);
            btnClose.TabIndex = 18;
            btnClose.Text = "Закрыть";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(17, 0);
            btnSave.Margin = new Padding(2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(119, 42);
            btnSave.TabIndex = 17;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // dgvRelatedStaffs
            // 
            dgvRelatedStaffs.AllowUserToAddRows = false;
            dgvRelatedStaffs.AllowUserToDeleteRows = false;
            dgvRelatedStaffs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRelatedStaffs.Location = new Point(144, 291);
            dgvRelatedStaffs.Margin = new Padding(2);
            dgvRelatedStaffs.Name = "dgvRelatedStaffs";
            dgvRelatedStaffs.ReadOnly = true;
            dgvRelatedStaffs.RowHeadersWidth = 62;
            dgvRelatedStaffs.RowTemplate.Height = 33;
            dgvRelatedStaffs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRelatedStaffs.Size = new Size(292, 70);
            dgvRelatedStaffs.TabIndex = 11;
            // 
            // btnAddRelatedStaff
            // 
            btnAddRelatedStaff.Location = new Point(439, 291);
            btnAddRelatedStaff.Margin = new Padding(2);
            btnAddRelatedStaff.Name = "btnAddRelatedStaff";
            btnAddRelatedStaff.Size = new Size(89, 20);
            btnAddRelatedStaff.TabIndex = 12;
            btnAddRelatedStaff.Text = "Добавить";
            btnAddRelatedStaff.UseVisualStyleBackColor = true;
            btnAddRelatedStaff.Click += btnAddRelatedStaff_Click;
            // 
            // btnDeleteRelatedStaff
            // 
            btnDeleteRelatedStaff.Location = new Point(439, 341);
            btnDeleteRelatedStaff.Margin = new Padding(2);
            btnDeleteRelatedStaff.Name = "btnDeleteRelatedStaff";
            btnDeleteRelatedStaff.Size = new Size(89, 20);
            btnDeleteRelatedStaff.TabIndex = 13;
            btnDeleteRelatedStaff.Text = "Удалить";
            btnDeleteRelatedStaff.UseVisualStyleBackColor = true;
            btnDeleteRelatedStaff.Click += btnDeleteRelatedStaff_Click;
            // 
            // cbxIsReleased
            // 
            cbxIsReleased.AutoSize = true;
            cbxIsReleased.Location = new Point(419, 21);
            cbxIsReleased.Margin = new Padding(2);
            cbxIsReleased.Name = "cbxIsReleased";
            cbxIsReleased.Size = new Size(101, 19);
            cbxIsReleased.TabIndex = 6;
            cbxIsReleased.Text = "Опубликован";
            cbxIsReleased.UseVisualStyleBackColor = true;
            cbxIsReleased.CheckedChanged += cbxIsReleased_CheckedChanged;
            // 
            // txtClassifierCode
            // 
            txtClassifierCode.Location = new Point(144, 224);
            txtClassifierCode.Margin = new Padding(2);
            txtClassifierCode.Name = "txtClassifierCode";
            txtClassifierCode.Size = new Size(386, 23);
            txtClassifierCode.TabIndex = 10;
            // 
            // lblClassifierCode
            // 
            lblClassifierCode.AutoSize = true;
            lblClassifierCode.Font = new Font("Segoe UI", 9F);
            lblClassifierCode.Location = new Point(12, 224);
            lblClassifierCode.Margin = new Padding(2, 0, 2, 0);
            lblClassifierCode.Name = "lblClassifierCode";
            lblClassifierCode.Size = new Size(87, 15);
            lblClassifierCode.TabIndex = 23;
            lblClassifierCode.Text = "Код в classifier:";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(panel1);
            flowLayoutPanel1.Controls.Add(panel2);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Margin = new Padding(3, 2, 3, 2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(550, 541);
            flowLayoutPanel1.TabIndex = 25;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblPrice);
            panel1.Controls.Add(txtPrice);
            panel1.Controls.Add(txtName);
            panel1.Controls.Add(txtClassifierCode);
            panel1.Controls.Add(lblName);
            panel1.Controls.Add(lblClassifierCode);
            panel1.Controls.Add(lblType);
            panel1.Controls.Add(cbxIsReleased);
            panel1.Controls.Add(txtType);
            panel1.Controls.Add(btnDeleteRelatedStaff);
            panel1.Controls.Add(rtxtFunctions);
            panel1.Controls.Add(btnAddRelatedStaff);
            panel1.Controls.Add(lblFunctions);
            panel1.Controls.Add(dgvRelatedStaffs);
            panel1.Controls.Add(rtxtComment);
            panel1.Controls.Add(lblQualification);
            panel1.Controls.Add(rtxtQualification);
            panel1.Controls.Add(lblComment);
            panel1.Controls.Add(lblCombineResponsibility);
            panel1.Location = new Point(3, 2);
            panel1.Margin = new Padding(3, 2, 3, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(545, 489);
            panel1.TabIndex = 19;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnSave);
            panel2.Controls.Add(btnClose);
            panel2.Location = new Point(3, 495);
            panel2.Margin = new Padding(3, 2, 3, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(545, 46);
            panel2.TabIndex = 20;
            // 
            // txtPrice
            // 
            txtPrice.Location = new Point(144, 257);
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new Size(193, 23);
            txtPrice.TabIndex = 24;
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(12, 260);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(70, 15);
            lblPrice.TabIndex = 25;
            lblPrice.Text = "Стоимость:";
            // 
            // Win7_StaffEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(550, 541);
            ControlBox = false;
            Controls.Add(flowLayoutPanel1);
            Margin = new Padding(2);
            MaximumSize = new Size(566, 760);
            MinimumSize = new Size(566, 557);
            Name = "Win7_StaffEditor";
            Text = "Win_7_StaffEditor";
            Load += Win7_StaffEditor_Load;
            ((System.ComponentModel.ISupportInitialize)dgvRelatedStaffs).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtType;
        private Label lblType;
        private TextBox txtName;
        private Label lblName;
        private RichTextBox rtxtFunctions;
        private Label lblFunctions;
        private RichTextBox rtxtComment;
        private RichTextBox rtxtQualification;
        private Label lblCombineResponsibility;
        private Label lblComment;
        private Label lblQualification;
        private Button btnClose;
        private Button btnSave;
        private DataGridView dgvRelatedStaffs;
        private Button btnAddRelatedStaff;
        private Button btnDeleteRelatedStaff;
        private CheckBox cbxIsReleased;
        private TextBox txtClassifierCode;
        private Label lblClassifierCode;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel1;
        private Panel panel2;
        private Label lblPrice;
        private TextBox txtPrice;
    }
}