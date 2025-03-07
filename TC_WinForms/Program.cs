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
using TC_WinForms.WinForms.Win6.RoadMap;

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

				MessageBox.Show("Произошла ошибка при запуске приложения:" +"\n" + ex,
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

			// Проверяем наличие файла appsettings.json и создаем его с настройками по умолчанию, если он отсутствует
			EnsureAppSettingsExists();

			IConfiguration config = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory) // Папка, где лежит exe
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
				.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
				.Build();

			var connectionString = config["Config:ConnectionString"];
			var isFirstRun = bool.Parse(config["IsFirstRun"] ?? "false");

            var logPath = config.GetValue<string>("LogsFolder");

            if (logPath == null)
                logPath = "C:/tempLogs";

			// Читаем настройки пути временной папки и модифицируем путь для логов
			var tempLogPath = Path.Combine(logPath, "TC_Viewer", Environment.UserName, "logs", "log-.json");//Path.GetTempPath(), "TC_Viewer", "logs", "log-.json");
			config.GetSection("Serilog:WriteTo:0:Args")["path"] = tempLogPath;

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.Enrich.WithClassName()
				.CreateLogger();

            Log.Information("Application started");

            Log.Information($"Окружение: {environment}\n" +
                               $"ConnectionString: {config["Config:ConnectionString"]}\n" +
                               $"IsFirstRun: {config["IsFirstRun"]}\n" +
                               $"LogsFolder: {config["LogsFolder"]}\n");

			ApplicationConfiguration.Initialize();

            

            // Устанавливаем строку подключения к БД
            //var connectionString = configuration.GetValue<string>("Config:ConnectionString");
            if (connectionString != null)
            {
                TcDbConnector.StaticClass.ConnectString = connectionString;
            }
            else
            {
                throw new Exception("Строка подключения к БД не найдена в файле конфигурации.");
            }

            if (isFirstRun)
            {
                ApplyMigrationsIfNecessary();

                // Обновляем флаг в файле конфигурации
                try
                {
                    UpdateFirstRunFlag();
                }
                catch (Exception ex)
                {
					Log.Error(ex, "Ошибка обновления флага первого запуска.");

					MessageBox.Show("Ошибка обновления флага первого запуска: " + ex.Message,
						"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
#if DEBUG
			// Очистить таблицу блокировок в БД перед запуском в режиме debug
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
                        Log.Information("Миграции успешно применены.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Ошибка применения миграций.");
                        throw;
                    }
                }
                else
                {
                    Log.Information("Все миграции уже были применены.");
                }
            }
        }

		static void EnsureAppSettingsExists()
		{
			// Определяем окружение (по умолчанию - Production)
			var environment = Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT") ?? "Production";
			var configFileName = $"appsettings.{environment}.json";

			// Проверяем наличие основного файла конфигурации
			if (!File.Exists("appsettings.json"))
			{
				Log.Information("Основной appsettings.json отсутствует. Создаём файл с настройками по умолчанию...");
				CreateDefaultAppSettings("appsettings.json");
			}

			// Проверяем наличие окружного файла (appsettings.Development.json или appsettings.Production.json)
			if (!File.Exists(configFileName))
			{
				Log.Information($"Файл {configFileName} отсутствует. Создаём файл с настройками по умолчанию...");
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

			// Сериализация в JSON с красивым форматированием
			var options = new JsonSerializerOptions { WriteIndented = true };
			var json = JsonSerializer.Serialize(defaultConfig, options);

			// Запись в файл
			File.WriteAllText(fileName, json);
			Log.Information($"Файл {fileName} создан.");
		}

		static void UpdateFirstRunFlag()
		{
			var environment = Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT") ?? "Production";
			var configFileName = $"appsettings.{environment}.json";
			var json = File.ReadAllText(configFileName);
			var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
			if (jsonObj == null)
			{
				throw new Exception("Ошибка чтения файла конфигурации.");
			}
			jsonObj["IsFirstRun"] = false;
			string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(configFileName, output);
		}

#if DEBUG
        static void RunDebugMode()
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
                throw new Exception("Пользователь не найден!");
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
                        LoadTechnologicalCardEditor(451); // ТКПС_1.1.5 (495)
                        //PrintTechnologicalCard(494); // ТКПС_1.1.3 (494)
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