namespace TC_WinForms
{
    partial class Win1
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
            gbxAuthorizationForm = new GroupBox();
            lblAuthSurname = new Label();
            txtPassword = new TextBox();
            lblAuthLogin = new Label();
            txtLogin = new TextBox();
            btnAuthorization = new Button();
            btnTcDesign = new Button();
            btnNext = new Button();
            gbxFunctionalityChoice = new GroupBox();
            btnTcEditng = new Button();
            lbxPreviousActionsTC = new ListBox();
            pnlControlPanel = new Panel();
            pnlToolBar = new Panel();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            gbxAuthorizationForm.SuspendLayout();
            gbxFunctionalityChoice.SuspendLayout();
            pnlControlPanel.SuspendLayout();
            pnlToolBar.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // gbxAuthorizationForm
            // 
            gbxAuthorizationForm.Controls.Add(lblAuthSurname);
            gbxAuthorizationForm.Controls.Add(txtPassword);
            gbxAuthorizationForm.Controls.Add(lblAuthLogin);
            gbxAuthorizationForm.Controls.Add(txtLogin);
            gbxAuthorizationForm.Controls.Add(btnAuthorization);
            gbxAuthorizationForm.Location = new Point(12, 42);
            gbxAuthorizationForm.Name = "gbxAuthorizationForm";
            gbxAuthorizationForm.Size = new Size(360, 195);
            gbxAuthorizationForm.TabIndex = 1;
            gbxAuthorizationForm.TabStop = false;
            gbxAuthorizationForm.Text = "Форма авторизации";
            // 
            // lblAuthSurname
            // 
            lblAuthSurname.AutoSize = true;
            lblAuthSurname.Location = new Point(9, 84);
            lblAuthSurname.Name = "lblAuthSurname";
            lblAuthSurname.Size = new Size(123, 20);
            lblAuthSurname.TabIndex = 4;
            lblAuthSurname.Text = "Введите пароль:";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Gill Sans Ultra Bold", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtPassword.Location = new Point(141, 81);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(200, 26);
            txtPassword.TabIndex = 3;
            // 
            // lblAuthLogin
            // 
            lblAuthLogin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblAuthLogin.AutoSize = true;
            lblAuthLogin.Location = new Point(19, 35);
            lblAuthLogin.Name = "lblAuthLogin";
            lblAuthLogin.Size = new Size(113, 20);
            lblAuthLogin.TabIndex = 2;
            lblAuthLogin.Text = "Введите логин:";
            // 
            // txtLogin
            // 
            txtLogin.Location = new Point(141, 32);
            txtLogin.Name = "txtLogin";
            txtLogin.Size = new Size(200, 27);
            txtLogin.TabIndex = 1;
            // 
            // btnAuthorization
            // 
            btnAuthorization.Location = new Point(181, 130);
            btnAuthorization.Name = "btnAuthorization";
            btnAuthorization.Size = new Size(160, 45);
            btnAuthorization.TabIndex = 0;
            btnAuthorization.Text = "Авторизация";
            btnAuthorization.UseVisualStyleBackColor = true;
            btnAuthorization.Click += btnAuthorization_Click;
            // 
            // btnTcDesign
            // 
            btnTcDesign.Location = new Point(19, 28);
            btnTcDesign.Name = "btnTcDesign";
            btnTcDesign.Size = new Size(140, 90);
            btnTcDesign.TabIndex = 2;
            btnTcDesign.Text = "Создать новой";
            btnTcDesign.UseVisualStyleBackColor = true;
            btnTcDesign.Click += btnTcDesign_Click;
            // 
            // btnNext
            // 
            btnNext.Enabled = false;
            btnNext.Location = new Point(216, 335);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(160, 45);
            btnNext.TabIndex = 4;
            btnNext.Text = "Далее";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // gbxFunctionalityChoice
            // 
            gbxFunctionalityChoice.Controls.Add(btnTcEditng);
            gbxFunctionalityChoice.Controls.Add(btnTcDesign);
            gbxFunctionalityChoice.Enabled = false;
            gbxFunctionalityChoice.Location = new Point(12, 248);
            gbxFunctionalityChoice.Name = "gbxFunctionalityChoice";
            gbxFunctionalityChoice.Size = new Size(360, 127);
            gbxFunctionalityChoice.TabIndex = 5;
            gbxFunctionalityChoice.TabStop = false;
            gbxFunctionalityChoice.Text = "Выбор функционала";
            // 
            // btnTcEditng
            // 
            btnTcEditng.Location = new Point(201, 28);
            btnTcEditng.Name = "btnTcEditng";
            btnTcEditng.Size = new Size(140, 90);
            btnTcEditng.TabIndex = 3;
            btnTcEditng.Text = "Редактировать";
            btnTcEditng.UseVisualStyleBackColor = true;
            btnTcEditng.Click += btnTcEditng_Click;
            // 
            // lbxPreviousActionsTC
            // 
            lbxPreviousActionsTC.FormattingEnabled = true;
            lbxPreviousActionsTC.ItemHeight = 20;
            lbxPreviousActionsTC.Location = new Point(13, 16);
            lbxPreviousActionsTC.Name = "lbxPreviousActionsTC";
            lbxPreviousActionsTC.Size = new Size(363, 304);
            lbxPreviousActionsTC.TabIndex = 6;
            // 
            // pnlControlPanel
            // 
            pnlControlPanel.Controls.Add(btnNext);
            pnlControlPanel.Controls.Add(lbxPreviousActionsTC);
            pnlControlPanel.Dock = DockStyle.Right;
            pnlControlPanel.Enabled = false;
            pnlControlPanel.Location = new Point(382, 25);
            pnlControlPanel.Name = "pnlControlPanel";
            pnlControlPanel.Size = new Size(400, 428);
            pnlControlPanel.TabIndex = 8;
            // 
            // pnlToolBar
            // 
            pnlToolBar.Controls.Add(toolStrip1);
            pnlToolBar.Dock = DockStyle.Top;
            pnlToolBar.Location = new Point(0, 0);
            pnlToolBar.Name = "pnlToolBar";
            pnlToolBar.Size = new Size(782, 25);
            pnlToolBar.TabIndex = 10;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(782, 27);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(49, 24);
            toolStripButton1.Text = "Файл";
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(69, 24);
            toolStripButton2.Text = "Главная";
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(71, 24);
            toolStripButton3.Text = "Справка";
            // 
            // Win1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 453);
            Controls.Add(pnlControlPanel);
            Controls.Add(gbxFunctionalityChoice);
            Controls.Add(pnlToolBar);
            Controls.Add(gbxAuthorizationForm);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Win1";
            RightToLeft = RightToLeft.No;
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Форма авторизации";
            FormClosing += Win1_FormClosing;
            gbxAuthorizationForm.ResumeLayout(false);
            gbxAuthorizationForm.PerformLayout();
            gbxFunctionalityChoice.ResumeLayout(false);
            pnlControlPanel.ResumeLayout(false);
            pnlToolBar.ResumeLayout(false);
            pnlToolBar.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox gbxAuthorizationForm;
        private Button btnTcDesign;
        private Button button2;
        private Button btnNext;
        private GroupBox gbxFunctionalityChoice;
        private Button btnTcEditng;
        private ListBox lbxPreviousActionsTC;
        private Label lblAuthLogin;
        private TextBox txtLogin;
        private Button btnAuthorization;
        private Label lblAuthSurname;
        private TextBox txtPassword;
        private Panel pnlControlPanel;
        private Panel pnlToolBar;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
    }
}