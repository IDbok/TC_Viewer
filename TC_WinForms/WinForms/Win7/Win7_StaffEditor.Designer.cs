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
            ((System.ComponentModel.ISupportInitialize)dgvRelatedStaffs).BeginInit();
            SuspendLayout();
            // 
            // txtType
            // 
            txtType.Location = new Point(170, 65);
            txtType.Margin = new Padding(2);
            txtType.Name = "txtType";
            txtType.Size = new Size(440, 27);
            txtType.TabIndex = 7;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblType.Location = new Point(20, 65);
            lblType.Margin = new Padding(2, 0, 2, 0);
            lblType.Name = "lblType";
            lblType.Size = new Size(141, 20);
            lblType.TabIndex = 6;
            lblType.Text = "Тип (исполнение):";
            // 
            // txtName
            // 
            txtName.Location = new Point(170, 20);
            txtName.Margin = new Padding(2);
            txtName.Name = "txtName";
            txtName.Size = new Size(301, 27);
            txtName.TabIndex = 5;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblName.Location = new Point(20, 20);
            lblName.Margin = new Padding(2, 0, 2, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(122, 20);
            lblName.TabIndex = 4;
            lblName.Text = "Наименование:";
            // 
            // rtxtFunctions
            // 
            rtxtFunctions.Location = new Point(170, 110);
            rtxtFunctions.Margin = new Padding(2);
            rtxtFunctions.Name = "rtxtFunctions";
            rtxtFunctions.Size = new Size(440, 65);
            rtxtFunctions.TabIndex = 8;
            rtxtFunctions.Text = "";
            // 
            // lblFunctions
            // 
            lblFunctions.AutoSize = true;
            lblFunctions.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblFunctions.Location = new Point(20, 110);
            lblFunctions.Margin = new Padding(2, 0, 2, 0);
            lblFunctions.Name = "lblFunctions";
            lblFunctions.Size = new Size(79, 20);
            lblFunctions.TabIndex = 9;
            lblFunctions.Text = "Функции:";
            // 
            // rtxtComment
            // 
            rtxtComment.Location = new Point(170, 445);
            rtxtComment.Margin = new Padding(2);
            rtxtComment.Name = "rtxtComment";
            rtxtComment.Size = new Size(440, 145);
            rtxtComment.TabIndex = 11;
            rtxtComment.Text = "";
            // 
            // rtxtQualification
            // 
            rtxtQualification.Location = new Point(170, 190);
            rtxtQualification.Margin = new Padding(2);
            rtxtQualification.Name = "rtxtQualification";
            rtxtQualification.Size = new Size(440, 90);
            rtxtQualification.TabIndex = 12;
            rtxtQualification.Text = "";
            // 
            // lblCombineResponsibility
            // 
            lblCombineResponsibility.AutoSize = true;
            lblCombineResponsibility.Location = new Point(20, 335);
            lblCombineResponsibility.Margin = new Padding(2, 0, 2, 0);
            lblCombineResponsibility.Name = "lblCombineResponsibility";
            lblCombineResponsibility.Size = new Size(110, 60);
            lblCombineResponsibility.TabIndex = 13;
            lblCombineResponsibility.Text = "Возможность \r\nсовмещения\r\nобязанностей:";
            // 
            // lblComment
            // 
            lblComment.AutoSize = true;
            lblComment.Location = new Point(20, 445);
            lblComment.Margin = new Padding(2, 0, 2, 0);
            lblComment.Name = "lblComment";
            lblComment.Size = new Size(110, 20);
            lblComment.TabIndex = 15;
            lblComment.Text = "Комментарии:";
            // 
            // lblQualification
            // 
            lblQualification.AutoSize = true;
            lblQualification.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblQualification.Location = new Point(20, 190);
            lblQualification.Margin = new Padding(2, 0, 2, 0);
            lblQualification.Name = "lblQualification";
            lblQualification.Size = new Size(123, 20);
            lblQualification.TabIndex = 16;
            lblQualification.Text = "Квалификация:";
            // 
            // btnClose
            // 
            btnClose.Location = new Point(465, 607);
            btnClose.Margin = new Padding(2);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(136, 56);
            btnClose.TabIndex = 18;
            btnClose.Text = "Закрыть";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(25, 607);
            btnSave.Margin = new Padding(2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(136, 56);
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
            dgvRelatedStaffs.Location = new Point(170, 335);
            dgvRelatedStaffs.Margin = new Padding(2);
            dgvRelatedStaffs.Name = "dgvRelatedStaffs";
            dgvRelatedStaffs.ReadOnly = true;
            dgvRelatedStaffs.RowHeadersWidth = 62;
            dgvRelatedStaffs.RowTemplate.Height = 33;
            dgvRelatedStaffs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRelatedStaffs.Size = new Size(334, 94);
            dgvRelatedStaffs.TabIndex = 19;
            // 
            // btnAddRelatedStaff
            // 
            btnAddRelatedStaff.Location = new Point(508, 335);
            btnAddRelatedStaff.Margin = new Padding(2);
            btnAddRelatedStaff.Name = "btnAddRelatedStaff";
            btnAddRelatedStaff.Size = new Size(102, 27);
            btnAddRelatedStaff.TabIndex = 20;
            btnAddRelatedStaff.Text = "Добавить";
            btnAddRelatedStaff.UseVisualStyleBackColor = true;
            btnAddRelatedStaff.Click += btnAddRelatedStaff_Click;
            // 
            // btnDeleteRelatedStaff
            // 
            btnDeleteRelatedStaff.Location = new Point(508, 401);
            btnDeleteRelatedStaff.Margin = new Padding(2);
            btnDeleteRelatedStaff.Name = "btnDeleteRelatedStaff";
            btnDeleteRelatedStaff.Size = new Size(102, 27);
            btnDeleteRelatedStaff.TabIndex = 21;
            btnDeleteRelatedStaff.Text = "Удалить";
            btnDeleteRelatedStaff.UseVisualStyleBackColor = true;
            btnDeleteRelatedStaff.Click += btnDeleteRelatedStaff_Click;
            // 
            // cbxIsReleased
            // 
            cbxIsReleased.AutoSize = true;
            cbxIsReleased.Location = new Point(485, 20);
            cbxIsReleased.Margin = new Padding(2);
            cbxIsReleased.Name = "cbxIsReleased";
            cbxIsReleased.Size = new Size(125, 24);
            cbxIsReleased.TabIndex = 22;
            cbxIsReleased.Text = "Опубликован";
            cbxIsReleased.UseVisualStyleBackColor = true;
            cbxIsReleased.CheckedChanged += cbxIsReleased_CheckedChanged;
            // 
            // txtClassifierCode
            // 
            txtClassifierCode.Location = new Point(170, 290);
            txtClassifierCode.Margin = new Padding(2);
            txtClassifierCode.Name = "txtClassifierCode";
            txtClassifierCode.Size = new Size(440, 27);
            txtClassifierCode.TabIndex = 24;
            // 
            // lblClassifierCode
            // 
            lblClassifierCode.AutoSize = true;
            lblClassifierCode.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblClassifierCode.Location = new Point(20, 290);
            lblClassifierCode.Margin = new Padding(2, 0, 2, 0);
            lblClassifierCode.Name = "lblClassifierCode";
            lblClassifierCode.Size = new Size(111, 20);
            lblClassifierCode.TabIndex = 23;
            lblClassifierCode.Text = "Код в classifier:";
            // 
            // Win7_StaffEditor
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(626, 682);
            Controls.Add(txtClassifierCode);
            Controls.Add(lblClassifierCode);
            Controls.Add(cbxIsReleased);
            Controls.Add(btnDeleteRelatedStaff);
            Controls.Add(btnAddRelatedStaff);
            Controls.Add(dgvRelatedStaffs);
            Controls.Add(btnClose);
            Controls.Add(btnSave);
            Controls.Add(lblQualification);
            Controls.Add(lblComment);
            Controls.Add(lblCombineResponsibility);
            Controls.Add(rtxtQualification);
            Controls.Add(rtxtComment);
            Controls.Add(lblFunctions);
            Controls.Add(rtxtFunctions);
            Controls.Add(txtType);
            Controls.Add(lblType);
            Controls.Add(txtName);
            Controls.Add(lblName);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(2);
            MaximumSize = new Size(644, 729);
            MinimumSize = new Size(644, 729);
            Name = "Win7_StaffEditor";
            Text = "Win_7_StaffEditor";
            Load += Win7_StaffEditor_Load;
            ((System.ComponentModel.ISupportInitialize)dgvRelatedStaffs).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
    }
}