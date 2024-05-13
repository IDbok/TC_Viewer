using TC_WinForms.DataProcessing;
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
                        this.Hide();
                    }
                    else MessageBox.Show("Пользователь не найден!");

                    //if (IsUserExist(txtLogin.Text, txtPassword.Text))
                    //{
                    //    Program.MainForm = new Win7(AuthUserAccessLevel());
                    //    Program.MainForm.Show();
                    //    this.Hide();
                    //}
                    //else MessageBox.Show("Пользователь не найден!");
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
    }
}
