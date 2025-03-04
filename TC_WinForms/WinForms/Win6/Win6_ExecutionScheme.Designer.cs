namespace TC_WinForms.WinForms
{
    partial class Win6_ExecutionScheme
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
            pictureBoxExecutionScheme = new PictureBox();
            btnUploadExecutionScheme = new Button();
            btnDeleteES = new Button();
            tblpMain = new TableLayoutPanel();
            pnlImage = new Panel();
            pnlControls = new Panel();
            pnlInfo = new Panel();
            txtNote = new TextBox();
            txtTechInfo = new TextBox();
            txtArticle = new TextBox();
            lblNote = new Label();
            lblTechInfo = new Label();
            lblArticle = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxExecutionScheme).BeginInit();
            tblpMain.SuspendLayout();
            pnlImage.SuspendLayout();
            pnlControls.SuspendLayout();
            pnlInfo.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBoxExecutionScheme
            // 
            pictureBoxExecutionScheme.BackgroundImageLayout = ImageLayout.None;
            pictureBoxExecutionScheme.Dock = DockStyle.Fill;
            pictureBoxExecutionScheme.Location = new Point(0, 0);
            pictureBoxExecutionScheme.Margin = new Padding(5);
            pictureBoxExecutionScheme.Name = "pictureBoxExecutionScheme";
            pictureBoxExecutionScheme.Size = new Size(984, 822);
            pictureBoxExecutionScheme.TabIndex = 0;
            pictureBoxExecutionScheme.TabStop = false;
            // 
            // btnUploadExecutionScheme
            // 
            btnUploadExecutionScheme.Location = new Point(11, 14);
            btnUploadExecutionScheme.Margin = new Padding(5);
            btnUploadExecutionScheme.Name = "btnUploadExecutionScheme";
            btnUploadExecutionScheme.Size = new Size(170, 50);
            btnUploadExecutionScheme.TabIndex = 1;
            btnUploadExecutionScheme.Text = "Загрузить";
            btnUploadExecutionScheme.UseVisualStyleBackColor = true;
            btnUploadExecutionScheme.Click += btnUploadExecutionScheme_Click;
            // 
            // btnDeleteES
            // 
            btnDeleteES.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeleteES.Location = new Point(803, 14);
            btnDeleteES.Margin = new Padding(5);
            btnDeleteES.Name = "btnDeleteES";
            btnDeleteES.Size = new Size(170, 50);
            btnDeleteES.TabIndex = 2;
            btnDeleteES.Text = "Удалить";
            btnDeleteES.UseVisualStyleBackColor = true;
            btnDeleteES.Click += btnDeleteES_Click;
            // 
            // tblpMain
            // 
            tblpMain.ColumnCount = 1;
            tblpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblpMain.Controls.Add(pnlImage, 0, 1);
            tblpMain.Controls.Add(pnlControls, 0, 2);
            tblpMain.Controls.Add(pnlInfo, 0, 0);
            tblpMain.Dock = DockStyle.Fill;
            tblpMain.Location = new Point(0, 0);
            tblpMain.Name = "tblpMain";
            tblpMain.RowCount = 3;
            tblpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 30.7426662F));
            tblpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 69.25733F));
            tblpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tblpMain.Size = new Size(990, 1276);
            tblpMain.TabIndex = 3;
            // 
            // pnlImage
            // 
            pnlImage.Controls.Add(pictureBoxExecutionScheme);
            pnlImage.Dock = DockStyle.Fill;
            pnlImage.Location = new Point(3, 370);
            pnlImage.Name = "pnlImage";
            pnlImage.Size = new Size(984, 822);
            pnlImage.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(btnDeleteES);
            pnlControls.Controls.Add(btnUploadExecutionScheme);
            pnlControls.Dock = DockStyle.Fill;
            pnlControls.Location = new Point(3, 1198);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(984, 75);
            pnlControls.TabIndex = 1;
            // 
            // pnlInfo
            // 
            pnlInfo.Controls.Add(txtNote);
            pnlInfo.Controls.Add(txtTechInfo);
            pnlInfo.Controls.Add(txtArticle);
            pnlInfo.Controls.Add(lblNote);
            pnlInfo.Controls.Add(lblTechInfo);
            pnlInfo.Controls.Add(lblArticle);
            pnlInfo.Dock = DockStyle.Fill;
            pnlInfo.Location = new Point(3, 3);
            pnlInfo.Name = "pnlInfo";
            pnlInfo.Size = new Size(984, 361);
            pnlInfo.TabIndex = 2;
            // 
            // txtNote
            // 
            txtNote.Location = new Point(345, 166);
            txtNote.Multiline = true;
            txtNote.Name = "txtNote";
            txtNote.Size = new Size(613, 179);
            txtNote.TabIndex = 5;
            // 
            // txtTechInfo
            // 
            txtTechInfo.Location = new Point(345, 75);
            txtTechInfo.Multiline = true;
            txtTechInfo.Name = "txtTechInfo";
            txtTechInfo.Size = new Size(613, 71);
            txtTechInfo.TabIndex = 4;
            // 
            // txtArticle
            // 
            txtArticle.Location = new Point(345, 13);
            txtArticle.Name = "txtArticle";
            txtArticle.Size = new Size(613, 39);
            txtArticle.TabIndex = 3;
            // 
            // lblNote
            // 
            lblNote.AutoSize = true;
            lblNote.Location = new Point(20, 169);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(161, 32);
            lblNote.TabIndex = 2;
            lblNote.Text = "Примечание:";
            // 
            // lblTechInfo
            // 
            lblTechInfo.AutoSize = true;
            lblTechInfo.Location = new Point(20, 75);
            lblTechInfo.Name = "lblTechInfo";
            lblTechInfo.Size = new Size(265, 32);
            lblTechInfo.TabIndex = 1;
            lblTechInfo.Text = "Техпроцесс/Параметр:";
            // 
            // lblArticle
            // 
            lblArticle.AutoSize = true;
            lblArticle.Location = new Point(20, 16);
            lblArticle.Name = "lblArticle";
            lblArticle.Size = new Size(109, 32);
            lblArticle.TabIndex = 0;
            lblArticle.Text = "Артикул:";
            // 
            // Win6_ExecutionScheme
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(990, 1276);
            Controls.Add(tblpMain);
            Margin = new Padding(5);
            Name = "Win6_ExecutionScheme";
            Text = "Win6_ExecutionScheme";
            Load += Win6_ExecutionScheme_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxExecutionScheme).EndInit();
            tblpMain.ResumeLayout(false);
            pnlImage.ResumeLayout(false);
            pnlControls.ResumeLayout(false);
            pnlInfo.ResumeLayout(false);
            pnlInfo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBoxExecutionScheme;
        private Button btnUploadExecutionScheme;
        private Button btnDeleteES;
        private TableLayoutPanel tblpMain;
        private Panel pnlImage;
        private Panel pnlControls;
        private Panel pnlInfo;
        private Label lblArticle;
        private Label lblNote;
        private Label lblTechInfo;
        private TextBox txtNote;
        private TextBox txtTechInfo;
        private TextBox txtArticle;
    }
}