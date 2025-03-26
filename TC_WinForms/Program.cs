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

				MessageBox.Show("��������� ������ ��� ������� ����������:" +"\n" + ex,
					"������", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

			// ��������� ������� ����� appsettings.json � ������� ��� � ����������� �� ���������, ���� �� �����������
			EnsureAppSettingsExists();

			IConfiguration config = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory) // �����, ��� ����� exe
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
				.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
				.Build();

			var connectionString = config["Config:ConnectionString"];
			var isFirstRun = bool.Parse(config["IsFirstRun"] ?? "false");

            var logPath = config.GetValue<string>("LogsFolder");

            if (logPath == null)
                logPath = "C:/tempLogs";

			// ������ ��������� ���� ��������� ����� � ������������ ���� ��� �����
			var tempLogPath = Path.Combine(logPath, "TC_Viewer", Environment.UserName, "logs", "log-.json");//Path.GetTempPath(), "TC_Viewer", "logs", "log-.json");
			config.GetSection("Serilog:WriteTo:0:Args")["path"] = tempLogPath;

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.Enrich.WithClassName()
				.CreateLogger();

            Log.Information("Application started");

            Log.Information($"���������: {environment}\n" +
                               $"ConnectionString: {config["Config:ConnectionString"]}\n" +
                               $"IsFirstRun: {config["IsFirstRun"]}\n" +
                               $"LogsFolder: {config["LogsFolder"]}\n");

			ApplicationConfiguration.Initialize();

            

            // ������������� ������ ����������� � ��
            //var connectionString = configuration.GetValue<string>("Config:ConnectionString");
            if (connectionString != null)
            {
                TcDbConnector.StaticClass.ConnectString = connectionString;
            }
            else
            {
                throw new Exception("������ ����������� � �� �� ������� � ����� ������������.");
            }

            if (isFirstRun)
            {
                ApplyMigrationsIfNecessary();

                // ��������� ���� � ����� ������������
                try
                {
                    UpdateFirstRunFlag();
                }
                catch (Exception ex)
                {
					Log.Error(ex, "������ ���������� ����� ������� �������.");

					MessageBox.Show("������ ���������� ����� ������� �������: " + ex.Message,
						"������", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
#if DEBUG
			// �������� ������� ���������� � �� ����� �������� � ������ debug
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
                        Log.Information("�������� ������� ���������.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "������ ���������� ��������.");
                        throw;
                    }
                }
                else
                {
                    Log.Information("��� �������� ��� ���� ���������.");
                }
            }
        }

		static void EnsureAppSettingsExists()
		{
			// ���������� ��������� (�� ��������� - Production)
			var environment = Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT") ?? "Production";
			var configFileName = $"appsettings.{environment}.json";

			// ��������� ������� ��������� ����� ������������
			if (!File.Exists("appsettings.json"))
			{
				Log.Information("�������� appsettings.json �����������. ������ ���� � ����������� �� ���������...");
				CreateDefaultAppSettings("appsettings.json");
			}

			// ��������� ������� ��������� ����� (appsettings.Development.json ��� appsettings.Production.json)
			if (!File.Exists(configFileName))
			{
				Log.Information($"���� {configFileName} �����������. ������ ���� � ����������� �� ���������...");
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

			// ������������ � JSON � �������� ���������������
			var options = new JsonSerializerOptions { WriteIndented = true };
			var json = JsonSerializer.Serialize(defaultConfig, options);

			// ������ � ����
			File.WriteAllText(fileName, json);
			Log.Information($"���� {fileName} ������.");
		}

		static void UpdateFirstRunFlag()
		{
			var environment = Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT") ?? "Production";
			var configFileName = $"appsettings.{environment}.json";
			var json = File.ReadAllText(configFileName);
			var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
			if (jsonObj == null)
			{
				throw new Exception("������ ������ ����� ������������.");
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
                throw new Exception("������������ �� ������!");
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
                        LoadTechnologicalCardEditor(451); // ����_1.1.5 (495)
                        //PrintTechnologicalCard(494); // ����_1.1.3 (494)
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