namespace TC_WinForms.WinForms
{
    partial class Win7_TechTransitionEditor
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
            txtName = new TextBox();
            lblName = new Label();
            cbxCategory = new ComboBox();
            lblCategory = new Label();
            lblNameComment = new Label();
            lblTimeComment = new Label();
            txtTime = new TextBox();
            lblTime = new Label();
            rtxtNameComment = new RichTextBox();
            rtxtTimeComment = new RichTextBox();
            cbxTimeCheck = new CheckBox();
            lblTimeUnit = new Label();
            btnClose = new Button();
            btnSave = new Button();
            cbxIsReleased = new CheckBox();
            SuspendLayout();
            // 
            // txtName
            // 
            txtName.Location = new Point(220, 30);
            txtName.Name = "txtName";
            txtName.Size = new Size(375, 31);
            txtName.TabIndex = 7;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblName.Location = new Point(30, 30);
            lblName.Name = "lblName";
            lblName.Size = new Size(150, 25);
            lblName.TabIndex = 6;
            lblName.Text = "Наименование:";
            // 
            // cbxCategory
            // 
            cbxCategory.FormattingEnabled = true;
            cbxCategory.Location = new Point(220, 90);
            cbxCategory.Name = "cbxCategory";
            cbxCategory.Size = new Size(375, 33);
            cbxCategory.TabIndex = 8;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblCategory.Location = new Point(30, 90);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(107, 25);
            lblCategory.TabIndex = 9;
            lblCategory.Text = "Категория:";
            // 
            // lblNameComment
            // 
            lblNameComment.AutoSize = true;
            lblNameComment.Location = new Point(30, 210);
            lblNameComment.Name = "lblNameComment";
            lblNameComment.Size = new Size(156, 50);
            lblNameComment.TabIndex = 10;
            lblNameComment.Text = "Комментарий\r\nк наименованию:";
            // 
            // lblTimeComment
            // 
            lblTimeComment.AutoSize = true;
            lblTimeComment.Location = new Point(30, 340);
            lblTimeComment.Name = "lblTimeComment";
            lblTimeComment.Size = new Size(125, 75);
            lblTimeComment.TabIndex = 11;
            lblTimeComment.Text = "Комментарий\r\nк времени\r\nвыполнения:";
            // 
            // txtTime
            // 
            txtTime.Location = new Point(220, 150);
            txtTime.Name = "txtTime";
            txtTime.Size = new Size(144, 31);
            txtTime.TabIndex = 14;
            // 
            // lblTime
            // 
            lblTime.AutoSize = true;
            lblTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblTime.Location = new Point(30, 150);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(190, 25);
            lblTime.TabIndex = 13;
            lblTime.Text = "Время выполнения:";
            // 
            // rtxtNameComment
            // 
            rtxtNameComment.Location = new Point(220, 210);
            rtxtNameComment.Name = "rtxtNameComment";
            rtxtNameComment.Size = new Size(530, 110);
            rtxtNameComment.TabIndex = 15;
            rtxtNameComment.Text = "";
            // 
            // rtxtTimeComment
            // 
            rtxtTimeComment.Location = new Point(220, 340);
            rtxtTimeComment.Name = "rtxtTimeComment";
            rtxtTimeComment.Size = new Size(530, 110);
            rtxtTimeComment.TabIndex = 16;
            rtxtTimeComment.Text = "";
            // 
            // cbxTimeCheck
            // 
            cbxTimeCheck.AutoSize = true;
            cbxTimeCheck.Location = new Point(446, 152);
            cbxTimeCheck.Name = "cbxTimeCheck";
            cbxTimeCheck.Size = new Size(130, 29);
            cbxTimeCheck.TabIndex = 17;
            cbxTimeCheck.Text = "проверено";
            cbxTimeCheck.UseVisualStyleBackColor = true;
            // 
            // lblTimeUnit
            // 
            lblTimeUnit.AutoSize = true;
            lblTimeUnit.Location = new Point(368, 156);
            lblTimeUnit.Name = "lblTimeUnit";
            lblTimeUnit.Size = new Size(54, 25);
            lblTimeUnit.TabIndex = 18;
            lblTimeUnit.Text = ", мин";
            // 
            // btnClose
            // 
            btnClose.Location = new Point(580, 461);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(170, 70);
            btnClose.TabIndex = 19;
            btnClose.Text = "Закрыть";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(30, 461);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(170, 70);
            btnSave.TabIndex = 20;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // cbxIsReleased
            // 
            cbxIsReleased.AutoSize = true;
            cbxIsReleased.Location = new Point(601, 32);
            cbxIsReleased.Name = "cbxIsReleased";
            cbxIsReleased.Size = new Size(149, 29);
            cbxIsReleased.TabIndex = 25;
            cbxIsReleased.Text = "Опубликован";
            cbxIsReleased.UseVisualStyleBackColor = true;
            // 
            // Win7_TechTransitionEditor
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(778, 544);
            Controls.Add(cbxIsReleased);
            Controls.Add(btnSave);
            Controls.Add(btnClose);
            Controls.Add(lblTimeUnit);
            Controls.Add(cbxTimeCheck);
            Controls.Add(rtxtTimeComment);
            Controls.Add(rtxtNameComment);
            Controls.Add(txtTime);
            Controls.Add(lblTime);
            Controls.Add(lblTimeComment);
            Controls.Add(lblNameComment);
            Controls.Add(lblCategory);
            Controls.Add(cbxCategory);
            Controls.Add(txtName);
            Controls.Add(lblName);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximumSize = new Size(800, 600);
            MinimumSize = new Size(800, 600);
            Name = "Win7_TechTransitionEditor";
            Text = "Win7_TechTransitionEditor";
            Load += Win7_TechTransitionEditor_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtName;
        private Label lblName;
        private ComboBox cbxCategory;
        private Label lblCategory;
        private Label lblNameComment;
        private Label lblTimeComment;
        private TextBox txtTime;
        private Label lblTime;
        private RichTextBox rtxtNameComment;
        private RichTextBox rtxtTimeComment;
        private CheckBox cbxTimeCheck;
        private Label lblTimeUnit;
        private Button btnClose;
        private Button btnSave;
        private CheckBox cbxIsReleased;
    }
}