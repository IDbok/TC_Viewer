namespace TC_WinForms.WinForms
{
    partial class Win8
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
            lblServerAdress = new Label();
            textBox1 = new TextBox();
            btnQuit = new Button();
            lblAuthSurname = new Label();
            txtPassword = new TextBox();
            lblAuthLogin = new Label();
            txtLogin = new TextBox();
            btnAuthorization = new Button();
            gbxAuthorizationForm.SuspendLayout();
            SuspendLayout();
            // 
            // gbxAuthorizationForm
            // 
            gbxAuthorizationForm.Controls.Add(lblServerAdress);
            gbxAuthorizationForm.Controls.Add(textBox1);
            gbxAuthorizationForm.Controls.Add(btnQuit);
            gbxAuthorizationForm.Controls.Add(lblAuthSurname);
            gbxAuthorizationForm.Controls.Add(txtPassword);
            gbxAuthorizationForm.Controls.Add(lblAuthLogin);
            gbxAuthorizationForm.Controls.Add(txtLogin);
            gbxAuthorizationForm.Controls.Add(btnAuthorization);
            gbxAuthorizationForm.Dock = DockStyle.Fill;
            gbxAuthorizationForm.ImeMode = ImeMode.NoControl;
            gbxAuthorizationForm.Location = new Point(0, 0);
            gbxAuthorizationForm.Name = "gbxAuthorizationForm";
            gbxAuthorizationForm.Size = new Size(415, 312);
            gbxAuthorizationForm.TabIndex = 2;
            gbxAuthorizationForm.TabStop = false;
            gbxAuthorizationForm.Text = "Форма авторизации";
            // 
            // lblServerAdress
            // 
            lblServerAdress.AutoSize = true;
            lblServerAdress.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            lblServerAdress.Location = new Point(26, 251);
            lblServerAdress.Name = "lblServerAdress";
            lblServerAdress.Size = new Size(144, 25);
            lblServerAdress.TabIndex = 7;
            lblServerAdress.Text = "Адрес сервера:";
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Gill Sans Ultra Bold", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBox1.Location = new Point(185, 253);
            textBox1.Name = "textBox1";
            textBox1.PasswordChar = '*';
            textBox1.Size = new Size(200, 26);
            textBox1.TabIndex = 6;
            // 
            // btnQuit
            // 
            btnQuit.BackColor = Color.Brown;
            btnQuit.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            btnQuit.ForeColor = SystemColors.ButtonHighlight;
            btnQuit.Location = new Point(375, 0);
            btnQuit.Name = "btnQuit";
            btnQuit.Size = new Size(40, 34);
            btnQuit.TabIndex = 5;
            btnQuit.Text = "X";
            btnQuit.UseVisualStyleBackColor = false;
            btnQuit.Click += btnQuit_Click;
            // 
            // lblAuthSurname
            // 
            lblAuthSurname.AutoSize = true;
            lblAuthSurname.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            lblAuthSurname.Location = new Point(26, 111);
            lblAuthSurname.Name = "lblAuthSurname";
            lblAuthSurname.Size = new Size(154, 25);
            lblAuthSurname.TabIndex = 4;
            lblAuthSurname.Text = "Введите пароль:";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Gill Sans Ultra Bold", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtPassword.Location = new Point(185, 113);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(200, 26);
            txtPassword.TabIndex = 3;
            // 
            // lblAuthLogin
            // 
            lblAuthLogin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblAuthLogin.AutoSize = true;
            lblAuthLogin.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            lblAuthLogin.Location = new Point(26, 59);
            lblAuthLogin.Name = "lblAuthLogin";
            lblAuthLogin.Size = new Size(141, 25);
            lblAuthLogin.TabIndex = 2;
            lblAuthLogin.Text = "Введите логин:";
            // 
            // txtLogin
            // 
            txtLogin.Location = new Point(185, 60);
            txtLogin.Name = "txtLogin";
            txtLogin.Size = new Size(200, 27);
            txtLogin.TabIndex = 1;
            // 
            // btnAuthorization
            // 
            btnAuthorization.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            btnAuthorization.Location = new Point(225, 157);
            btnAuthorization.Name = "btnAuthorization";
            btnAuthorization.Size = new Size(160, 45);
            btnAuthorization.TabIndex = 0;
            btnAuthorization.Text = "Вход";
            btnAuthorization.UseVisualStyleBackColor = true;
            btnAuthorization.Click += btnAuthorization_Click;
            // 
            // Win8
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            ClientSize = new Size(415, 312);
            ControlBox = false;
            Controls.Add(gbxAuthorizationForm);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Win8";
            ShowIcon = false;
            Text = "Win8";
            gbxAuthorizationForm.ResumeLayout(false);
            gbxAuthorizationForm.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox gbxAuthorizationForm;
        private Label lblAuthSurname;
        private TextBox txtPassword;
        private Label lblAuthLogin;
        private TextBox txtLogin;
        private Button btnAuthorization;
        private Button btnQuit;
        private Label lblServerAdress;
        private TextBox textBox1;
    }
}