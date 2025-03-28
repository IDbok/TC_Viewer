using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;
using TC_WinForms.DataProcessing;
using TC_WinForms.WinForms;
using TcDbConnector;
using TcModels.Models;
using static TC_WinForms.DataProcessing.AuthorizationService.User;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace TC_WinForms
{
    internal static class Program
    {
#if DEBUG
        public static bool IsTestMode = true;
#else
        public static bool IsTestMode = false;
#endif
        //public static ILogger Logger { get; set; } = new Logger();
        public static Form MainForm { get; set; } = null!;
        public static List<Form> FormsBack { get; set; } = new List<Form>();
        public static List<Form> FormsForward { get; set; } = new List<Form>();
        public static List<TechnologicalCard> ExistingCatds { get; set; } = new List<TechnologicalCard>();
        public static List<TechnologicalProcess> ExistingProcces { get; set; } = new List<TechnologicalProcess>();

        public static TechnologicalCard currentTc = new TechnologicalCard();

        //public static TechnologicalCard currentTc { get; set; } = new TechnologicalCard();
        public static TechnologicalCard? NewTc { get; set; }
        public static TechnologicalProcess CurrentTp { get; set; } = new TechnologicalProcess();
        //public static Config configGlobal = new Config();
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                ApplicationStart();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application crashed");

				MessageBox.Show("Ïðîèçîøëà îøèáêà ïðè çàïóñêå ïðèëîæåíèÿ:" +"\n" + ex,
					"Îøèáêà", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
            finally
            {
                Log.Information("Application stopped");
                Log.CloseAndFlush();
            }

        }

        static void ApplicationStart()
        {
			var environment = Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT") ?? "Production";

			// Ïðîâåðÿåì íàëè÷èå ôàéëà appsettings.json è ñîçäàåì åãî ñ íàñòðîéêàìè ïî óìîë÷àíèþ, åñëè îí îòñóòñòâóåò
			EnsureAppSettingsExists();

			IConfiguration config = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory) // Ïàïêà, ãäå ëåæèò exe
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
				.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
				.Build();

			var connectionString = config["Config:ConnectionString"];
			var isFirstRun = bool.Parse(config["IsFirstRun"] ?? "false");

            var logPath = config.GetValue<string>("LogsFolder");

            if (logPath == null)
                logPath = "C:/tempLogs";

			// ×èòàåì íàñòðîéêè ïóòè âðåìåííîé ïàïêè è ìîäèôèöèðóåì ïóòü äëÿ ëîãîâ
			var tempLogPath = Path.Combine(logPath, "TC_Viewer", Environment.UserName, "logs", "log-.json");//Path.GetTempPath(), "TC_Viewer", "logs", "log-.json");
			config.GetSection("Serilog:WriteTo:0:Args")["path"] = tempLogPath;

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.Enrich.WithClassName()
				.CreateLogger();

            Log.Information("Application started");

            Log.Information($"Îêðóæåíèå: {environment}\n" +
                               $"ConnectionString: {config["Config:ConnectionString"]}\n" +
                               $"IsFirstRun: {config["IsFirstRun"]}\n" +
                               $"LogsFolder: {config["LogsFolder"]}\n");

			ApplicationConfiguration.Initialize();

            

            // Óñòàíàâëèâàåì ñòðîêó ïîäêëþ÷åíèÿ ê ÁÄ
            //var connectionString = configuration.GetValue<string>("Config:ConnectionString");
            if (connectionString != null)
            {
                TcDbConnector.StaticClass.ConnectString = connectionString;
            }
            else
            {
                throw new Exception("Ñòðîêà ïîäêëþ÷åíèÿ ê ÁÄ íå íàéäåíà â ôàéëå êîíôèãóðàöèè.");
            }

            if (isFirstRun)
            {
                ApplyMigrationsIfNecessary();

                // Îáíîâëÿåì ôëàã â ôàéëå êîíôèãóðàöèè
                try
                {
                    UpdateFirstRunFlag();
                }
                catch (Exception ex)
                {
					Log.Error(ex, "Îøèáêà îáíîâëåíèÿ ôëàãà ïåðâîãî çàïóñêà.");

					MessageBox.Show("Îøèáêà îáíîâëåíèÿ ôëàãà ïåðâîãî çàïóñêà: " + ex.Message,
						"Îøèáêà", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
#if DEBUG
			// Î÷èñòèòü òàáëèöó áëîêèðîâîê â ÁÄ ïåðåä çàïóñêîì â ðåæèìå debug
			using (var context = new MyDbContext())
			{
				var blockedConcurrencyObjects = context.BlockedConcurrencyObjects.ToList();
				context.BlockedConcurrencyObjects.RemoveRange(blockedConcurrencyObjects);
				context.SaveChanges();
			}
#endif

#if DEBUG
			RunDebugMode();
#else
            RunReleaseMode();
#endif
        }


        static void ApplyMigrationsIfNecessary()
        {
            using (var context = new MyDbContext())
            {
                var pendingMigrations = context.Database.GetPendingMigrations();

                if (pendingMigrations.Any())
                {
                    try
                    {
                        context.Database.Migrate();
                        Log.Information("Ìèãðàöèè óñïåøíî ïðèìåíåíû.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Îøèáêà ïðèìåíåíèÿ ìèãðàöèé.");
                        throw;
                    }
                }
                else
                {
                    Log.Information("Âñå ìèãðàöèè óæå áûëè ïðèìåíåíû.");
                }
            }
        }

		static void EnsureAppSettingsExists()
		{
			// Îïðåäåëÿåì îêðóæåíèå (ïî óìîë÷àíèþ - Production)
			var environment = Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT") ?? "Production";
			var configFileName = $"appsettings.{environment}.json";

			// Ïðîâåðÿåì íàëè÷èå îñíîâíîãî ôàéëà êîíôèãóðàöèè
			if (!File.Exists("appsettings.json"))
			{
				Log.Information("Îñíîâíîé appsettings.json îòñóòñòâóåò. Ñîçäà¸ì ôàéë ñ íàñòðîéêàìè ïî óìîë÷àíèþ...");
				CreateDefaultAppSettings("appsettings.json");
			}

			// Ïðîâåðÿåì íàëè÷èå îêðóæíîãî ôàéëà (appsettings.Development.json èëè appsettings.Production.json)
			if (!File.Exists(configFileName))
			{
				Log.Information($"Ôàéë {configFileName} îòñóòñòâóåò. Ñîçäà¸ì ôàéë ñ íàñòðîéêàìè ïî óìîë÷àíèþ...");
				CreateDefaultAppSettings(configFileName);
			}
		}

		static void CreateDefaultAppSettings(string fileName)
		{
			var defaultConfig = new
			{
				Config = new
				{
					ConnectionString = "server=10.1.100.142;database=tcvdb_main;user=tavrida;password=tavrida$555"
				},
				IsFirstRun = true,
				LogsFolder = "C:/tempLogs",
				Serilog = new
				{
					Using = Array.Empty<string>(),
					MinimumLevel = new
					{
						Default = "Debug",
						Override = new
						{
							Microsoft = "Warning",
							System = "Warning"
						}
					},
					Enrich = new[] { "FromLogContext", "WithMachineName", "WhithProcessId", "WhithThreadId" },
					WriteTo = new[]
					{
				new
				{
					Name = "File",
					Args = new
					{
						path = "logs/log-.json",
						formatter = "Serilog.Formatting.Json.JsonFormatter, Serilog",
						rollingInterval = "Day",
						retainedFileCountLimit = 7
					}
				}
			}
				}
			};

			// Ñåðèàëèçàöèÿ â JSON ñ êðàñèâûì ôîðìàòèðîâàíèåì
			var options = new JsonSerializerOptions { WriteIndented = true };
			var json = JsonSerializer.Serialize(defaultConfig, options);

			// Çàïèñü â ôàéë
			File.WriteAllText(fileName, json);
			Log.Information($"Ôàéë {fileName} ñîçäàí.");
		}

		static void UpdateFirstRunFlag()
		{
			var environment = Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT") ?? "Production";
			var configFileName = $"appsettings.{environment}.json";
			var json = File.ReadAllText(configFileName);
			var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
			if (jsonObj == null)
			{
				throw new Exception("Îøèáêà ÷òåíèÿ ôàéëà êîíôèãóðàöèè.");
			}
			jsonObj["IsFirstRun"] = false;
			string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(configFileName, output);
		}

#if DEBUG
        static void RunDebugMode()
        {
            string login, password;

            Role userRole = Role.Admin;


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
                case Role.Admin:
                    login = "admin"; password = "admin1"; break;
                default:
                    login = "lead"; password = "dXLPdF"; break;
            }

            AuthorizationService.AuthorizeUser(login, password);

            if (AuthorizationService.CurrentUser != null)
            {
                Program.MainForm = new Win7_new(AuthorizationService.CurrentUser.UserRole());
                Application.Run(MainForm);
            }
            else
            {
                throw new Exception("Ïîëüçîâàòåëü íå íàéäåí!");
            }
        }

        static void Test()
        {
            var appIndex = 0;

            switch (appIndex)
            {
                case 0:
                    Application.Run(MainForm); break;

                case 1:
                    {
                        LoadTechnologicalCardEditor(451); // ÒÊÏÑ_1.1.5 (495)
                        //PrintTechnologicalCard(494); // ÒÊÏÑ_1.1.3 (494)
                    }
                    break;

                default:
                    Application.Run(MainForm); break;
            }


            //void PrintTechnologicalCard(int tcId)
            //{
            //    var tcExporter = new ExExportTC();

            //    tcExporter.SaveTCtoExcelFile("TestTC", tcId).Wait();
            //}

            void LoadTechnologicalCardEditor(int tcId, bool isViewMode = false)
            {
                var form = new Win6_new(tcId, viewMode: isViewMode);// 486);
                form.ShowDialog();
            }
        }
#else
        static void RunReleaseMode()
        {
            var authForm = new Win8();
            authForm.ShowDialog();

            Application.Run(MainForm);
        }
#endif

    }




}