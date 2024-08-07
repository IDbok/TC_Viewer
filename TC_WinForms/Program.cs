using Nancy.Json;
using System.Drawing;
using System.IO;
using TC_WinForms.DataProcessing;
using TC_WinForms.WinForms;
using TC_WinForms.WinForms.BlockScheme;
using TC_WinForms.WinForms.Diagram;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.AuthorizationService.User;

namespace TC_WinForms
{
    internal static class Program
    {
        public static bool testMode = true; //false;//
        public static Form MainForm { get; set; }
        public static List<Form> FormsBack { get; set; } = new List<Form>();
        public static List<Form> FormsForward { get; set; } = new List<Form>();
        public static List<TechnologicalCard> ExistingCatds { get; set; } = new List<TechnologicalCard>();
        public static List<TechnologicalProcess> ExistingProcces { get; set; } = new List<TechnologicalProcess>();
        
        public static TechnologicalCard currentTc = new TechnologicalCard();

        //public static TechnologicalCard currentTc { get; set; } = new TechnologicalCard();
        public static TechnologicalCard? NewTc { get; set; }
        public static TechnologicalProcess CurrentTp { get; set; } = new TechnologicalProcess();

        public static Config configGlobal = new Config();
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            ApplicationConfiguration.Initialize();

            string variableName = "TEST_MODE";
            // Считывание значения переменной среды
            string? variableValue = Environment.GetEnvironmentVariable(variableName);
            bool isTestMode = variableValue != null && variableValue.ToLower() == "true";

            if (isTestMode)
            {
                configGlobal.ConnectionString = "server=localhost;database=tavrida_db_main;user=root;password=root";
            }
            else
            {
                string text1 = "";
                while (true)
                {
                    try
                    {
                        text1 = System.IO.File.ReadAllText($"appsettings.json");
                        break;
                    }
                    catch (Exception e)
                    {
                        Config config = new Config();

                        config.ConnectionString = "server=10.1.100.142;database=tcvdb_main;user=tavrida;password=tavrida$555";
                        // "server=127.0.0.1;port=3306;database=tavrida_db_main;user=root;password=lsSB1UaiX5"
                        JavaScriptSerializer javaScriptSerializer1 = new JavaScriptSerializer();
                        string? bbn = javaScriptSerializer1.Serialize(config);
                        System.IO.File.WriteAllText($"appsettings.json", bbn);
                    }
                }

                JavaScriptSerializer javaScriptSerializer3 = new JavaScriptSerializer();
                var configGlo = javaScriptSerializer3.Deserialize<Config>(text1);
                configGlobal = configGlo;
            }

            TcDbConnector.StaticClass.ConnectString = configGlobal.ConnectionString;

            if (isTestMode)
            {
                string login, password;
                Role userRole = Role.Lead;

                switch (userRole)
                {
                    case Role.User:
                        login = "user"; password = "f88k44"; break;
                    case Role.ProjectManager:
                        login = "manager"; password = "99eUiS"; break;
                    case Role.Lead:
                        login = "lead"; password = "dXLPdF"; break;
                    case Role.Implementer:
                        login = "implementer"; password = "30yP0e"; break;
                    default:
                        login = "lead"; password = "dXLPdF"; break;
                }
                AuthorizationService.AuthorizeUser(login, password);

                if (AuthorizationService.CurrentUser != null)
                {
                    Program.MainForm = new Win7_new(AuthorizationService.CurrentUser.UserRole());
                    //Program.MainForm.Show();
                }
                else
                {
                    throw new Exception("Пользователь не найден!");
                }

                Test();
            }
            else
            {
                var authForm = new Win8();
                authForm.ShowDialog();

                Application.Run(MainForm);
            }
        }

        static void Test()
        {
            var appIndex = 1;

            switch (appIndex)
            {
                case 0:
                    Application.Run(MainForm); break;

                case 1:
                    {
                        //var form = new DiagramForm(493, false);
                        var form = new Win6_new(493, viewMode: true);// 486);
                        form.ShowDialog();
                    }
                    break;

                default:
                    Application.Run(MainForm); break;
            }    
            
        }
    }

    

    
}