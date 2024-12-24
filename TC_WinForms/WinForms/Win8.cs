using Serilog;
using TC_WinForms.DataProcessing;

namespace TC_WinForms.WinForms
{
	public partial class Win8 : Form
	{
		private readonly ILogger _logger;
		public Win8()
		{
			_logger = Log.Logger.ForContext<Win8>();
			_logger.Information("Инициализация формы авторизации Win8");

			InitializeComponent();

        }

        private void btnAuthorization_Click(object sender, EventArgs e)
        {
			try
			{
				_logger.Information("Нажата кнопка авторизации. Начало обработки...");

				txtLogin.Text = txtLogin.Text.Trim();
				txtPassword.Text = txtPassword.Text.Trim();

				_logger.Debug("Введенные данные. Логин: {Login}, Пароль - {PasswordLength} символов",
					txtLogin.Text, txtPassword.Text.Length);

				if (txtLogin.Text.Length > 0 && txtPassword.Text.Length > 0)
				{
					if (txtPassword.Text.Length < 4)
					{
						throw new Exception("Пароль должен состоять минимум из 4 символов.");
					}

					_logger.Information("Попытка авторизации пользователя {Login}", txtLogin.Text);
					AuthorizationService.AuthorizeUser(txtLogin.Text, txtPassword.Text);

					if (AuthorizationService.CurrentUser != null)
					{
						_logger.Information("Успешная авторизация. Роль пользователя: {UserRole}",
							AuthorizationService.CurrentUser.UserRole());

						Program.MainForm = new Win7_new(AuthorizationService.CurrentUser.UserRole());
						Program.MainForm.Show();
						this.Dispose();

						_logger.Information("Форма Win8 успешно закрыта.");
					}
					else
					{
						_logger.Warning("Пользователь {Login} не найден.", txtLogin.Text);
						MessageBox.Show("Пользователь не найден!");
					}
				}
				else
				{
					_logger.Warning("Один или оба поля для ввода пусты.");
					MessageBox.Show("Заполните все поля!");
				}
			}
			catch (Exception ex)
			{
				if (txtPassword.Text.Length < 4)
				{
					_logger.Warning("Ошибка авторизации: {Error}", ex.Message);
					MessageBox.Show(ex.Message);
				}
				else
				{
					_logger.Error(ex, "Произошла ошибка при авторизации.");
					MessageBox.Show("Произошла ошибка при авторизации!");
				}
			}
        }

        private void btnQuit_Click(object sender, EventArgs e)
		{
			_logger.Information("Нажата кнопка выхода. Завершение приложения...");
			Application.ExitThread();
        }

    }
}
