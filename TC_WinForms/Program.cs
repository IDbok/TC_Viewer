using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Nancy.Json;
using OfficeOpenXml;
using TC_WinForms.DataProcessing;
using TC_WinForms.WinForms;
using TcModels.Models;

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

        public static Config configGlobal;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            ApplicationConfiguration.Initialize();

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
                    config.ConnectionString = "server=localhost;database=tavrida_db_v10;user=root;password=root";

                    JavaScriptSerializer javaScriptSerializer1 = new JavaScriptSerializer();
                    string? bbn = javaScriptSerializer1.Serialize(config);
                    System.IO.File.WriteAllText($"appsettings.json", bbn);
                }
            }

            JavaScriptSerializer javaScriptSerializer3 = new JavaScriptSerializer();
            var configGlo = javaScriptSerializer3.Deserialize<Config>(text1);
            configGlobal = configGlo;
            TcDbConnector.StaticClass.ConnectString = configGlobal.ConnectionString;

            MainForm = new Win7_new(3);//new Win6_new(1);//new Win8();//new Win1();//new Win6(new Button { Name = "btnUpdateTC" /*"btnAddNewTC"*/ });// new Win3();//

            Application.Run(MainForm);
        }

    }
}