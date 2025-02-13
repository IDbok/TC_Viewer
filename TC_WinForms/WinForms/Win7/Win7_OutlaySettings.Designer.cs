namespace TC_WinForms.WinForms.Win7
{
    partial class Win7_OutlaySettings
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
            button1 = new Button();
            txtRegular = new TextBox();
            txtLead = new TextBox();
            lblRegular = new Label();
            lblLead = new Label();
            lblDescription = new Label();
            pnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(button1);
            pnlMain.Controls.Add(txtRegular);
            pnlMain.Controls.Add(txtLead);
            pnlMain.Controls.Add(lblRegular);
            pnlMain.Controls.Add(lblLead);
            pnlMain.Controls.Add(lblDescription);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(453, 185);
            pnlMain.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(12, 143);
            button1.Name = "button1";
            button1.Size = new Size(134, 30);
            button1.TabIndex = 1;
            button1.Text = "Перезаписать";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // txtRegular
            // 
            txtRegular.Location = new Point(207, 93);
            txtRegular.Name = "txtRegular";
            txtRegular.Size = new Size(210, 23);
            txtRegular.TabIndex = 3;
            // 
            // txtLead
            // 
            txtLead.Location = new Point(207, 53);
            txtLead.Name = "txtLead";
            txtLead.Size = new Size(210, 23);
            txtLead.TabIndex = 1;
            // 
            // lblRegular
            // 
            lblRegular.AutoSize = true;
            lblRegular.Location = new Point(12, 96);
            lblRegular.Name = "lblRegular";
            lblRegular.Size = new Size(177, 15);
            lblRegular.TabIndex = 2;
            lblRegular.Text = "Ставка для прочего пресонала";
            // 
            // lblLead
            // 
            lblLead.AutoSize = true;
            lblLead.Location = new Point(12, 56);
            lblLead.Name = "lblLead";
            lblLead.Size = new Size(127, 15);
            lblLead.TabIndex = 1;
            lblLead.Text = "Ставка для бригадира";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDescription.Location = new Point(12, 20);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(165, 15);
            lblDescription.TabIndex = 0;
            lblDescription.Text = "Настройки сводных затрат:";
            // 
            // Win7_OutlaySettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(453, 185);
            Controls.Add(pnlMain);
            Name = "Win7_OutlaySettings";
            Text = "Win7_OutlaySettings";
            Load += Win7_OutlaySettings_Load;
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlMain;
        private Label lblRegular;
        private Label lblLead;
        private Label lblDescription;
        private Button button1;
        private TextBox txtRegular;
        private TextBox txtLead;
    }
}