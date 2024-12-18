﻿using TC_WinForms.DataProcessing;
using static TC_WinForms.DataProcessing.Authorizator;

namespace TC_WinForms.WinForms
{
    public partial class Win8 : Form
    {
        public Win8()
        {
            InitializeComponent();
        }

        private void btnAuthorization_Click(object sender, EventArgs e)
        {
            try
            {
                txtLogin.Text = txtLogin.Text.Trim();
                txtPassword.Text = txtPassword.Text.Trim();
                if (txtLogin.Text.Length > 0 && txtPassword.Text.Length > 0)
                {
                    // throw an exception if password less than 4 symbols
                    if (txtPassword.Text.Length < 4) throw new Exception("Пароль должен состоять минимум из 4 символов.");

                    AuthorizationService.AuthorizeUser(txtLogin.Text, txtPassword.Text);

                    if (AuthorizationService.CurrentUser != null)
                    {
                        Program.MainForm = new Win7_new(AuthorizationService.CurrentUser.UserRole());
                        Program.MainForm.Show();
                        this.Dispose();
                    }
                    else MessageBox.Show("Пользователь не найден!");
                }
                else MessageBox.Show("Заполните все поля!");
            }
            catch (Exception ex)
            {
                if (txtPassword.Text.Length < 4) MessageBox.Show(ex.Message);
                else MessageBox.Show("Произошла ошибка при авторизации!");
            }
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private async void Win8_Load(object sender, EventArgs e)
        {
            // данный функционал неактуален
            //// найти переменную среды TEST_MODE
            //// если она есть и равна true, то включить тестовый режим
            
            //string variableName = "TEST_MODE";
            //// Считывание значения переменной среды
            //string? variableValue = Environment.GetEnvironmentVariable(variableName);
            //if (variableValue != null && variableValue.ToLower() == "true")
            //{
            //    /////////////////////////////////
            //    txtLogin.Text = "implementer"; //"lead";// "manager"; // "user"; // "implementer"; //
            //    txtPassword.Text = "pass";

            //    await Task.Delay(1000);
            //    btnAuthorization_Click(null, null);
            //    /////////////////////////////////
            //}



        }
    }
}
