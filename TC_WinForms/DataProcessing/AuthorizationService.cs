using TcModels.Models;
using System;
using System.Collections.Generic;
using System.Threading;


namespace TC_WinForms.DataProcessing;

public class AuthorizationService
{
    private static readonly object locker = new object();

    public static User CurrentUser { get; private set; }

    private static Dictionary<string, User> Users = new Dictionary<string, User>();
    private static Dictionary<string, string> Passwords = new Dictionary<string, string>();

    static AuthorizationService()
    {
        InitializeUsers();
    }

    private static void InitializeUsers()
    {
        var users = new List<User>
        {
            new User("user", "pass", null, null, User.Role.User),
            new User("manager", "pass", null, null, User.Role.ProjectManager),
            new User("lead", "pass", null, null, User.Role.Lead),
            new User("implementer", "pass", null, null, User.Role.Implementer)
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
        lock (locker)
        {
            if (Passwords.TryGetValue(login.ToLower(), out var storedPassword) && storedPassword == password)
            {
                var user = Users[login.ToLower()];
                if (user != null)
                {
                    CurrentUser = user;
                }
                return new Author
                {
                    Name = user.Name(),
                    Surname = user.Surname(),
                    Email = user.Login(),
                    AccessLevel = user.AccessLevel()
                };
            }
            throw new UnauthorizedAccessException("Ошибка при авторизации пользователя.");
        }
    }

    public class User
    {
        public enum Role {Lead , Implementer, ProjectManager, User}

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

