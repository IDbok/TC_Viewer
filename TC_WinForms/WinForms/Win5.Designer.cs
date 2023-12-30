namespace TC_WinForms
{
    partial class Win5
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
            cmbChoice = new ComboBox();
            panel1 = new Panel();
            btnCancel = new Button();
            btnAccept = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // cmbChoice
            // 
            cmbChoice.Anchor = AnchorStyles.None;
            cmbChoice.FormattingEnabled = true;
            cmbChoice.Location = new Point(19, 27);
            cmbChoice.Name = "cmbChoice";
            cmbChoice.Size = new Size(229, 28);
            cmbChoice.TabIndex = 0;
            cmbChoice.SelectedIndexChanged += cmbChoice_SelectedIndexChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnCancel);
            panel1.Controls.Add(btnAccept);
            panel1.Controls.Add(cmbChoice);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(272, 138);
            panel1.TabIndex = 1;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(154, 87);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(94, 29);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Отменить";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnAccept
            // 
            btnAccept.Enabled = false;
            btnAccept.Location = new Point(19, 87);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(94, 29);
            btnAccept.TabIndex = 1;
            btnAccept.Text = "Применить";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // Win5
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(272, 138);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "Win5";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Выбор [название категории]";
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ComboBox cmbChoice;
        private Panel panel1;
        private Button btnCancel;
        private Button btnAccept;
    }
}