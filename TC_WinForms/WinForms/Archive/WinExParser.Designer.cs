namespace TC_WinForms
{
    partial class WinExParser
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
            clbxTCSheets = new CheckedListBox();
            lblTCSheets = new Label();
            txtFilePath = new TextBox();
            lblFilePath = new Label();
            btnParseFile = new Button();
            gbxFilePath = new GroupBox();
            btnFilePathCheck = new Button();
            gbxTCSheets = new GroupBox();
            btnTCSheetsSelectAll = new Button();
            gbxFilePath.SuspendLayout();
            gbxTCSheets.SuspendLayout();
            SuspendLayout();
            // 
            // clbxTCSheets
            // 
            clbxTCSheets.FormattingEnabled = true;
            clbxTCSheets.Location = new Point(219, 26);
            clbxTCSheets.MultiColumn = true;
            clbxTCSheets.Name = "clbxTCSheets";
            clbxTCSheets.Size = new Size(352, 158);
            clbxTCSheets.TabIndex = 9;
            // 
            // lblTCSheets
            // 
            lblTCSheets.AutoSize = true;
            lblTCSheets.Location = new Point(5, 26);
            lblTCSheets.Name = "lblTCSheets";
            lblTCSheets.Size = new Size(182, 20);
            lblTCSheets.TabIndex = 8;
            lblTCSheets.Text = "Название листа с картой";
            // 
            // txtFilePath
            // 
            txtFilePath.Location = new Point(219, 15);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(384, 27);
            txtFilePath.TabIndex = 7;
            txtFilePath.Text = "Путь к файлу из сохранённых настроек";
            // 
            // lblFilePath
            // 
            lblFilePath.AutoSize = true;
            lblFilePath.Location = new Point(6, 18);
            lblFilePath.Name = "lblFilePath";
            lblFilePath.Size = new Size(161, 20);
            lblFilePath.TabIndex = 6;
            lblFilePath.Text = "Путь к файлу с картой";
            // 
            // btnParseFile
            // 
            btnParseFile.Location = new Point(450, 287);
            btnParseFile.Name = "btnParseFile";
            btnParseFile.Size = new Size(165, 30);
            btnParseFile.TabIndex = 5;
            btnParseFile.Text = "Продолжить";
            btnParseFile.UseVisualStyleBackColor = true;
            btnParseFile.Click += btnParseFile_Click;
            // 
            // gbxFilePath
            // 
            gbxFilePath.Controls.Add(btnFilePathCheck);
            gbxFilePath.Controls.Add(lblFilePath);
            gbxFilePath.Controls.Add(txtFilePath);
            gbxFilePath.Location = new Point(13, 12);
            gbxFilePath.Name = "gbxFilePath";
            gbxFilePath.Size = new Size(607, 82);
            gbxFilePath.TabIndex = 13;
            gbxFilePath.TabStop = false;
            // 
            // btnFilePathCheck
            // 
            btnFilePathCheck.Location = new Point(219, 48);
            btnFilePathCheck.Name = "btnFilePathCheck";
            btnFilePathCheck.Size = new Size(271, 30);
            btnFilePathCheck.TabIndex = 15;
            btnFilePathCheck.Text = "Получить список карт в файле";
            btnFilePathCheck.UseVisualStyleBackColor = true;
            btnFilePathCheck.Click += btnFilePathCheck_Click;
            // 
            // gbxTCSheets
            // 
            gbxTCSheets.Controls.Add(btnTCSheetsSelectAll);
            gbxTCSheets.Controls.Add(lblTCSheets);
            gbxTCSheets.Controls.Add(clbxTCSheets);
            gbxTCSheets.Location = new Point(13, 89);
            gbxTCSheets.Name = "gbxTCSheets";
            gbxTCSheets.Size = new Size(607, 192);
            gbxTCSheets.TabIndex = 14;
            gbxTCSheets.TabStop = false;
            gbxTCSheets.Visible = false;
            // 
            // btnTCSheetsSelectAll
            // 
            btnTCSheetsSelectAll.Location = new Point(67, 49);
            btnTCSheetsSelectAll.Name = "btnTCSheetsSelectAll";
            btnTCSheetsSelectAll.Size = new Size(120, 52);
            btnTCSheetsSelectAll.TabIndex = 10;
            btnTCSheetsSelectAll.Text = "Выделить всё";
            btnTCSheetsSelectAll.UseVisualStyleBackColor = true;
            btnTCSheetsSelectAll.Click += btnTCSheetsSelectAll_Click;
            // 
            // ExParserMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(631, 329);
            Controls.Add(gbxTCSheets);
            Controls.Add(gbxFilePath);
            Controls.Add(btnParseFile);
            Name = "ExParserMain";
            Text = "Парсинг ТК из excel файла";
            gbxFilePath.ResumeLayout(false);
            gbxFilePath.PerformLayout();
            gbxTCSheets.ResumeLayout(false);
            gbxTCSheets.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private CheckedListBox clbxTCSheets;
        private Label lblTCSheets;
        private TextBox txtFilePath;
        private Label lblFilePath;
        private Button btnParseFile;
        private GroupBox gbxFilePath;
        private GroupBox gbxTCSheets;
        private Button btnFilePathCheck;
        private Button btnTCSheetsSelectAll;
    }
}