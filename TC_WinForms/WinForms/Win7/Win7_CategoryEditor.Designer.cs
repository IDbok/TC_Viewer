namespace TC_WinForms.WinForms.Win7
{
    partial class Win7_CategoryEditor
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
            pnlMain = new Panel();
            btnAddEdit = new Button();
            txtValue = new TextBox();
            cbxKey = new ComboBox();
            cbxClass = new ComboBox();
            lblValue = new Label();
            lblKey = new Label();
            lblClass = new Label();
            pnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(btnAddEdit);
            pnlMain.Controls.Add(txtValue);
            pnlMain.Controls.Add(cbxKey);
            pnlMain.Controls.Add(cbxClass);
            pnlMain.Controls.Add(lblValue);
            pnlMain.Controls.Add(lblKey);
            pnlMain.Controls.Add(lblClass);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(324, 227);
            pnlMain.TabIndex = 0;
            // 
            // btnAddEdit
            // 
            btnAddEdit.Location = new Point(110, 173);
            btnAddEdit.Name = "btnAddEdit";
            btnAddEdit.Size = new Size(101, 42);
            btnAddEdit.TabIndex = 6;
            btnAddEdit.Click += btnAddEdit_Click;
            btnAddEdit.Text = "Сохранить";
            btnAddEdit.UseVisualStyleBackColor = true;
            // 
            // txtValue
            // 
            txtValue.Location = new Point(86, 99);
            txtValue.Name = "txtValue";
            txtValue.Size = new Size(212, 23);
            txtValue.TabIndex = 5;
            // 
            // cbxKey
            // 
            cbxKey.FormattingEnabled = true;
            cbxKey.Location = new Point(86, 55);
            cbxKey.Name = "cbxKey";
            cbxKey.Size = new Size(212, 23);
            cbxKey.TabIndex = 4;
            // 
            // cbxClass
            // 
            cbxClass.FormattingEnabled = true;
            cbxClass.Location = new Point(86, 12);
            cbxClass.Name = "cbxClass";
            cbxClass.Size = new Size(212, 23);
            cbxClass.TabIndex = 3;
            // 
            // lblValue
            // 
            lblValue.AutoSize = true;
            lblValue.Location = new Point(14, 107);
            lblValue.Name = "lblValue";
            lblValue.Size = new Size(60, 15);
            lblValue.TabIndex = 2;
            lblValue.Text = "Значение";
            // 
            // lblKey
            // 
            lblKey.AutoSize = true;
            lblKey.Location = new Point(14, 63);
            lblKey.Name = "lblKey";
            lblKey.Size = new Size(63, 15);
            lblKey.TabIndex = 1;
            lblKey.Text = "Категория";
            // 
            // lblClass
            // 
            lblClass.AutoSize = true;
            lblClass.Location = new Point(14, 20);
            lblClass.Name = "lblClass";
            lblClass.Size = new Size(39, 15);
            lblClass.TabIndex = 0;
            lblClass.Text = "Класс";
            // 
            // Win7_CategoryEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(324, 227);
            Controls.Add(pnlMain);
            Name = "Win7_CategoryEditor";
            Text = "Win7_CategoryEditor";
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlMain;
        private Label lblKey;
        private Label lblClass;
        private TextBox txtValue;
        private ComboBox cbxKey;
        private ComboBox cbxClass;
        private Label lblValue;
        private Button btnAddEdit;
    }
}