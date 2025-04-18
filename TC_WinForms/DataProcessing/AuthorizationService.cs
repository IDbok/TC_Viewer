﻿using Serilog;
using TcModels.Models;


namespace TC_WinForms.DataProcessing;

public class AuthorizationService
{
	private static readonly ILogger _logger;
	private static readonly object locker = new object();

    public static User CurrentUser { get; private set; }

    private static Dictionary<string, User> Users = new Dictionary<string, User>();
    private static Dictionary<string, string> Passwords = new Dictionary<string, string>();

    static AuthorizationService()
    {

		_logger = Log.Logger.ForContext<AuthorizationService>();
		_logger.Information("Инициализация сервиса авторизации");

		InitializeUsers();
    }

    private static void InitializeUsers()
    {
        var users = new List<User>
        {
            new User("user", "f88k44", null, null, User.Role.User),
            new User("manager", "99eUiS", null, null, User.Role.ProjectManager),
            new User("lead", "dXLPdF", null, null, User.Role.Lead),
            new User("implementer", "30yP0e", null, null, User.Role.Implementer),
            new User("admin", "admin1", null, null, User.Role.Lead)

        };

        foreach (var user in users)
        {
            Users.Add(user.Login(), user);
            // Здесь следует хранить хеш пароля, а не сам пароль
            Passwords.Add(user.Login(), user.Password());
        }
    }

    public static Author? AuthorizeUser(string login, string password)
    {
		_logger.Information("Попытка авторизации пользователя {Login}", login);

		lock (locker)
        {
            if (Passwords.TryGetValue(login.ToLower(), out var storedPassword) && storedPassword == password)
            {
                var user = Users[login.ToLower()];
                var userRole = user.UserRole();
                if (user != null)
                {
                    CurrentUser = user;

                    if (userRole == User.Role.Admin)
                    {
                        Program.IsTestMode = true;
						_logger.Debug("Режим тестирования активирован для администратора {Login}", login);
					}
				}

				_logger.Information("Пользователь {UserName} успешно авторизован с ролью {UserRole}", 
                    user.Name(), userRole);

                return new Author
                {
                    Name = user.Name(),
                    Surname = user.Surname(),
                    Email = user.Login(),
                    AccessLevel = user.AccessLevel()
                };
            }
			_logger.Warning("Неудачная попытка авторизации для пользователя {Login}", login);
			throw new UnauthorizedAccessException("Ошибка при авторизации пользователя.");
		}
    }

    public static string UserRoleConverter(User.Role role)
    {
        return role switch
        {
            User.Role.User => "Пользователь",
            User.Role.ProjectManager => "Руководитель комплексных проектов",
            User.Role.Lead => "Технолог руководитель",
            User.Role.Implementer => "Технолог исполнитель",
            User.Role.Admin => "Администратор",

            _ => "Неизвестно"
        };
    }

    public class User
    {
        public enum Role {Lead , Implementer, ProjectManager, User, Admin}

        private string login;
        private string password; // Только для инициализации, не для использования напрямую!
        private string name;
        private string surname;
        private int accessLevel;
        private Role role;

        public User(string login, string password, string name, string surname, Role role)
        {
            this.login = login;
            this.password = password; // Сохранение хешированного пароля
            this.name = name;
            this.surname = surname;
            this.accessLevel = (int)role;
            this.role = role;
        }

        public string Login() => login;
        public string Password() => password;
        public string Name() => name ?? "no information";
        public string Surname() => surname ?? "no information";
        public int AccessLevel() => accessLevel;
        public Role UserRole() => role;
    }
}

