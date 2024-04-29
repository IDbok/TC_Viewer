﻿namespace TC_WinForms.WinForms
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
            ((System.ComponentModel.ISupportInitialize)dgvRelatedStaffs).BeginInit();
            SuspendLayout();
            // 
            // txtType
            // 
            txtType.Location = new Point(220, 90);
            txtType.Name = "txtType";
            txtType.Size = new Size(535, 31);
            txtType.TabIndex = 7;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(30, 90);
            lblType.Name = "lblType";
            lblType.Size = new Size(156, 25);
            lblType.TabIndex = 6;
            lblType.Text = "Тип (исполнение):";
            // 
            // txtName
            // 
            txtName.Location = new Point(220, 30);
            txtName.Name = "txtName";
            txtName.Size = new Size(390, 31);
            txtName.TabIndex = 5;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(30, 30);
            lblName.Name = "lblName";
            lblName.Size = new Size(139, 25);
            lblName.TabIndex = 4;
            lblName.Text = "Наименование:";
            // 
            // rtxtFunctions
            // 
            rtxtFunctions.Location = new Point(220, 150);
            rtxtFunctions.Name = "rtxtFunctions";
            rtxtFunctions.Size = new Size(535, 90);
            rtxtFunctions.TabIndex = 8;
            rtxtFunctions.Text = "";
            // 
            // lblFunctions
            // 
            lblFunctions.AutoSize = true;
            lblFunctions.Location = new Point(30, 150);
            lblFunctions.Name = "lblFunctions";
            lblFunctions.Size = new Size(88, 25);
            lblFunctions.TabIndex = 9;
            lblFunctions.Text = "Функции:";
            // 
            // rtxtComment
            // 
            rtxtComment.Location = new Point(220, 560);
            rtxtComment.Name = "rtxtComment";
            rtxtComment.Size = new Size(535, 180);
            rtxtComment.TabIndex = 11;
            rtxtComment.Text = "";
            // 
            // rtxtQualification
            // 
            rtxtQualification.Location = new Point(220, 400);
            rtxtQualification.Name = "rtxtQualification";
            rtxtQualification.Size = new Size(535, 140);
            rtxtQualification.TabIndex = 12;
            rtxtQualification.Text = "";
            // 
            // lblCombineResponsibility
            // 
            lblCombineResponsibility.AutoSize = true;
            lblCombineResponsibility.Location = new Point(30, 270);
            lblCombineResponsibility.Name = "lblCombineResponsibility";
            lblCombineResponsibility.Size = new Size(128, 75);
            lblCombineResponsibility.TabIndex = 13;
            lblCombineResponsibility.Text = "Возможность \r\nсовмещения\r\nобязанностей:";
            // 
            // lblComment
            // 
            lblComment.AutoSize = true;
            lblComment.Location = new Point(30, 560);
            lblComment.Name = "lblComment";
            lblComment.Size = new Size(129, 25);
            lblComment.TabIndex = 15;
            lblComment.Text = "Комментарии:";
            // 
            // lblQualification
            // 
            lblQualification.AutoSize = true;
            lblQualification.Location = new Point(30, 400);
            lblQualification.Name = "lblQualification";
            lblQualification.Size = new Size(134, 25);
            lblQualification.TabIndex = 16;
            lblQualification.Text = "Квалификация:";
            // 
            // btnClose
            // 
            btnClose.Location = new Point(583, 759);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(170, 70);
            btnClose.TabIndex = 18;
            btnClose.Text = "Закрыть";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(30, 759);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(170, 70);
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
            dgvRelatedStaffs.Location = new Point(220, 267);
            dgvRelatedStaffs.Name = "dgvRelatedStaffs";
            dgvRelatedStaffs.ReadOnly = true;
            dgvRelatedStaffs.RowHeadersWidth = 62;
            dgvRelatedStaffs.RowTemplate.Height = 33;
            dgvRelatedStaffs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRelatedStaffs.Size = new Size(417, 117);
            dgvRelatedStaffs.TabIndex = 19;
            // 
            // btnAddRelatedStaff
            // 
            btnAddRelatedStaff.Location = new Point(643, 267);
            btnAddRelatedStaff.Name = "btnAddRelatedStaff";
            btnAddRelatedStaff.Size = new Size(112, 34);
            btnAddRelatedStaff.TabIndex = 20;
            btnAddRelatedStaff.Text = "Добавить";
            btnAddRelatedStaff.UseVisualStyleBackColor = true;
            btnAddRelatedStaff.Click += btnAddRelatedStaff_Click;
            // 
            // btnDeleteRelatedStaff
            // 
            btnDeleteRelatedStaff.Location = new Point(643, 350);
            btnDeleteRelatedStaff.Name = "btnDeleteRelatedStaff";
            btnDeleteRelatedStaff.Size = new Size(112, 34);
            btnDeleteRelatedStaff.TabIndex = 21;
            btnDeleteRelatedStaff.Text = "Удалить";
            btnDeleteRelatedStaff.UseVisualStyleBackColor = true;
            btnDeleteRelatedStaff.Click += btnDeleteRelatedStaff_Click;
            // 
            // cbxIsReleased
            // 
            cbxIsReleased.AutoSize = true;
            cbxIsReleased.Location = new Point(643, 32);
            cbxIsReleased.Name = "cbxIsReleased";
            cbxIsReleased.Size = new Size(115, 29);
            cbxIsReleased.TabIndex = 22;
            cbxIsReleased.Text = "Вырущен";
            cbxIsReleased.UseVisualStyleBackColor = true;
            cbxIsReleased.CheckedChanged += cbxIsReleased_CheckedChanged;
            // 
            // Win7_StaffEditor
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(778, 844);
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
            MaximumSize = new Size(800, 900);
            MinimumSize = new Size(800, 900);
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
    }
}