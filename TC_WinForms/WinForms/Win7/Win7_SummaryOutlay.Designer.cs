namespace TC_WinForms.WinForms.Win7
{
    partial class Win7_SummaryOutlay
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
            tbpnlMain = new TableLayoutPanel();
            pnlControls = new Panel();
            pnlButtons = new Panel();
            btnSettings = new Button();
            btnPrint = new Button();
            cmbxUnit = new ComboBox();
            txtSearch = new TextBox();
            pnlDgv = new Panel();
            dgvMain = new DataGridView();
            tbpnlMain.SuspendLayout();
            pnlControls.SuspendLayout();
            pnlButtons.SuspendLayout();
            pnlDgv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            SuspendLayout();
            // 
            // tbpnlMain
            // 
            tbpnlMain.ColumnCount = 1;
            tbpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbpnlMain.Controls.Add(pnlControls, 0, 0);
            tbpnlMain.Controls.Add(pnlDgv, 0, 1);
            tbpnlMain.Dock = DockStyle.Fill;
            tbpnlMain.Location = new Point(0, 0);
            tbpnlMain.Name = "tbpnlMain";
            tbpnlMain.RowCount = 2;
            tbpnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tbpnlMain.RowStyles.Add(new RowStyle());
            tbpnlMain.Size = new Size(800, 450);
            tbpnlMain.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.Controls.Add(pnlButtons);
            pnlControls.Controls.Add(cmbxUnit);
            pnlControls.Controls.Add(txtSearch);
            pnlControls.Dock = DockStyle.Fill;
            pnlControls.Location = new Point(3, 3);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(794, 54);
            pnlControls.TabIndex = 0;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnSettings);
            pnlButtons.Controls.Add(btnPrint);
            pnlButtons.Dock = DockStyle.Right;
            pnlButtons.Location = new Point(531, 0);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(263, 54);
            pnlButtons.TabIndex = 3;
            // 
            // btnSettings
            // 
            btnSettings.Location = new Point(134, 9);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(122, 39);
            btnSettings.TabIndex = 3;
            btnSettings.Visible = false;
            btnSettings.Text = "Настройки";
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Click += btnSettings_Click;
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(6, 9);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(122, 39);
            btnPrint.TabIndex = 2;
            btnPrint.Text = "Печать";
            btnPrint.UseVisualStyleBackColor = true;
            btnPrint.Click += btnPrint_Click;
            // 
            // cmbxUnit
            // 
            cmbxUnit.FormattingEnabled = true;
            cmbxUnit.Location = new Point(243, 18);
            cmbxUnit.Name = "cmbxUnit";
            cmbxUnit.Size = new Size(121, 23);
            cmbxUnit.TabIndex = 1;
            cmbxUnit.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(9, 18);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(228, 23);
            txtSearch.TabIndex = 0;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // pnlDgv
            // 
            pnlDgv.Controls.Add(dgvMain);
            pnlDgv.Dock = DockStyle.Fill;
            pnlDgv.Location = new Point(3, 63);
            pnlDgv.Name = "pnlDgv";
            pnlDgv.Size = new Size(794, 397);
            pnlDgv.TabIndex = 1;
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.AllowUserToDeleteRows = false;
            dgvMain.AllowUserToResizeRows = false;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 0);
            dgvMain.Margin = new Padding(3, 2, 3, 2);
            dgvMain.Name = "dgvMain";
            dgvMain.ReadOnly = true;
            dgvMain.Size = new Size(794, 397);
            dgvMain.TabIndex = 0;
            // 
            // Win7_SummaryOutlay
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tbpnlMain);
            Name = "Win7_SummaryOutlay";
            Text = "Win7_SummaryOutlay";
            Load += Win7_SummaryOutlay_Load;
            SizeChanged += Win7_SummaryOutlay_SizeChanged;
            tbpnlMain.ResumeLayout(false);
            pnlControls.ResumeLayout(false);
            pnlControls.PerformLayout();
            pnlButtons.ResumeLayout(false);
            pnlDgv.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tbpnlMain;
        private Panel pnlControls;
        private ComboBox comboBox2;
        private ComboBox cmbxUnit;
        private TextBox txtSearch;
        private Panel pnlDgv;
        private DataGridView dgvMain;
        private Button btnPrint;
        private Panel pnlButtons;
        private Button btnSettings;
    }
}