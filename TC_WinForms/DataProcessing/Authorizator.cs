using TcModels.Models;

namespace TC_WinForms.DataProcessing;

public static class Authorizator
{

    public static Author CurrentUser { get; private set; } = new Author();

    //private static User authUser = null;
    //public static User GetUser() => authUser;
    //public static string AuthUserName() => authUser.Name();
    //public static string AuthUserSurname() => authUser.Surname();
    public static string AuthUserFullName() => CurrentUser.Name + " " + CurrentUser.Surname;
    //public static string AuthUserEmail() => authUser.Login();

    public static int AuthUserAccessLevel() => CurrentUser.AccessLevel;

    private static Dictionary<string, User2> Users = new Dictionary<string, User2>();

    private static Dictionary<string, string> Passwords = new Dictionary<string, string>();

    public static bool IsUserExist(string login, string password)
    {
        try
        {
            Users.Clear();
            Passwords.Clear();
            // creatin new user
            Users = new Dictionary<string, User2>()
            {
                {"user", new User2("user", "password", null, null, User2.Role.User)},
                {"manager",new User2("manager", "password", null, null, User2.Role.ProjectManager) },
                {"lead", new User2("lead", "password", null, null, User2.Role.Lead)},
                {"implementer", new User2("implementer", "password",null, null, User2.Role.Implementer)}
            };

            if (Passwords[login.ToLower()] == password)
            {
                CurrentUser = new Author() {
                    Name = Users[login.ToLower()].Name(),
                    Surname = Users[login.ToLower()].Surname(),
                    Email = Users[login.ToLower()].Login(),
                    AccessLevel = Users[login.ToLower()].AccessLevel()
                };
                //authUser = Users[login.ToLower()].Copy();
                return true;
            }
        }
        catch (Exception e) {
            MessageBox.Show("Error auth"/*e.Message*/); }
        return false;
    }

    public class User2
    {
        public enum Role
        {
            User = 0,
            ProjectManager = 1,
            Implementer = 2,
            Lead = 3
        }


        private string? name;
        private string? surname;

        private int accessLevel = 0;

        private string login;

        public string Login() => login;
        //public string Password() => password;
        public string Name() => name != null ? name : "no information";
        public string Surname() => surname != null ? surname : "no information";

        public int AccessLevel() => accessLevel;
        public User2(string login, string password, string? name, string? surname, Role role = Role.User)
        {
            this.login = login;
            Passwords.Add(login, password);
            this.name = name;
            this.surname = surname;
            this.accessLevel = (int)role;
        }
        public User2(string login, string password): this(login, password, name: null, surname: null, role: Role.User)
        { }
        public User2 Copy()
        {
            return (User2)this.MemberwiseClone();
        }
    }
}
