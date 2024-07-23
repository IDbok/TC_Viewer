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
                configGlobal.ConnectionString = "server=localhost;database=tavrida_db_test;user=root;password=root";
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

                        config.ConnectionString = "server=10.1.100.142;database=tcvdb_15;user=tavrida;password=tavrida$555";
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
                string login = "implementer"; //"lead";// "manager"; // "user"; // "implementer"; //
                string password = "pass";
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
            }
            else
            {
                var authForm = new Win8();
                authForm.ShowDialog();
            }
            Test();

            //Application.Run(MainForm);
        }
        static void Test()
        {
            //var form = new DiagramForm(493, false);
            var form = new Win6_new(493);
            form.ShowDialog();
        }
    }

    

    
}