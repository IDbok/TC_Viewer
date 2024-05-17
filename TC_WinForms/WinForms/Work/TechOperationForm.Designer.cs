namespace TC_WinForms.WinForms.Work
{
    partial class TechOperationForm
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
            dgvMain = new DataGridView();
            button2 = new Button();
            pnlControls = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            pnlControls.SuspendLayout();
            SuspendLayout();
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.AllowUserToDeleteRows = false;
            dgvMain.ColumnHeadersHeight = 29;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 60);
            dgvMain.Name = "dgvMain";
            dgvMain.RowHeadersWidth = 51;
            dgvMain.Size = new Size(1077, 570);
            dgvMain.TabIndex = 1;
            // 
            // button2
            // 
            button2.Location = new Point(35, 12);
            button2.Name = "button2";
            button2.Size = new Size(194, 29);
            button2.TabIndex = 0;
            button2.Text = "Редактировать";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(button2);
            pnlControls.Dock = DockStyle.Top;
            pnlControls.Location = new Point(0, 0);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(1077, 60);
            pnlControls.TabIndex = 2;
            // 
            // TechOperationForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1077, 630);
            Controls.Add(dgvMain);
            Controls.Add(pnlControls);
            Name = "TechOperationForm";
            Text = "TechOperationForm";
            FormClosing += TechOperationForm_FormClosing;
            FormClosed += TechOperationForm_FormClosed;
            Load += TechOperationForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            pnlControls.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvMain;
        private Button button2;
        private Panel pnlControls;
    }
}