using TcModels.Models;

namespace TC_WinForms.DataProcessing
{
    public static class Authorizator
    {
        public static Author author = new(); 

        //private static User authUser = null;
        //public static User GetUser() => authUser;
        //public static string AuthUserName() => authUser.Name();
        //public static string AuthUserSurname() => authUser.Surname();
        public static string AuthUserFullName() => author.Name + " " + author.Surname;
        //public static string AuthUserEmail() => authUser.Login();

        public static int AuthUserAccessLevel() => author.AccessLevel;

        private static Dictionary<string, User> Users = new Dictionary<string, User>();

        private static Dictionary<string, string> Passwords = new Dictionary<string, string>();

        public static bool IsUserExust(string login, string password)
        {
            try
            {

                Users.Clear();
                Passwords.Clear();
                // creatin new user
                Users = new Dictionary<string, User>()
                {
                    {"newuser@mail.com", new User("newuser@mail.com", "password", "Александр", "Кузнецов", 0)},
                    {"bokarev.fic@gmail.com", new User("bokarev.fic@gmail.com", "pass", "Игорь", "Бокарев", 2)},
                    {"anpa@tavrida.com", new User("anpa@tavrida.com", "pass","Павел", "Анохин", 1)}
                };


                if (Passwords[login.ToLower()] == password)
                {
                    author = new Author() {
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

        public class User
        {
            private string? name;
            private string? surname;

            private int accessLevel = 0;

            private string login;

            public string Login() => login;
            //public string Password() => password;
            public string Name() => name != null ? name : "no information";
            public string Surname() => surname != null ? surname : "no information";

            public int AccessLevel() => accessLevel;
            public User(string login, string password, string name, string surname, int accessLevel = 0)
            {
                this.login = login;
                Passwords.Add(login, password);
                this.name = name;
                this.surname = surname;
            }
            public User(string login, string password): this(login, password, name: null, surname: null, accessLevel: 0)
            { }
            public User Copy()
            {
                return (User)this.MemberwiseClone();
            }
        }
    }
}
