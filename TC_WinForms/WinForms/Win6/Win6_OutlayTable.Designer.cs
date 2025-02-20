namespace TC_WinForms.WinForms.Win6
{
    partial class Win6_OutlayTable
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
            pnlDgv = new Panel();
            dgvMain = new DataGridView();
            pnlDgv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            SuspendLayout();
            // 
            // pnlDgv
            // 
            pnlDgv.Controls.Add(dgvMain);
            pnlDgv.Dock = DockStyle.Fill;
            pnlDgv.Location = new Point(0, 0);
            pnlDgv.Name = "pnlDgv";
            pnlDgv.Padding = new Padding(7, 5, 7, 5);
            pnlDgv.Size = new Size(364, 450);
            pnlDgv.TabIndex = 0;
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.AllowUserToDeleteRows = false;
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(7, 5);
            dgvMain.Name = "dgvMain";
            dgvMain.ReadOnly = true;
            dgvMain.Size = new Size(350, 440);
            dgvMain.TabIndex = 1;
            // 
            // Win6_OutlayTable
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(364, 450);
            Controls.Add(pnlDgv);
            Name = "Win6_OutlayTable";
            Text = "Таблица затрат";
            Load += Win6_OutlayTable_Load;
            pnlDgv.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Panel pnlControls;
        private Button btnCalculateOutlay;
        private Panel pnlDgv;
        private DataGridView dgvMain;
    }
}