using ExcelParsing.DataProcessing;
using Microsoft.EntityFrameworkCore;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.TcContent;
using TcModels.Models.IntermediateTables;
using Newtonsoft.Json;
using TcModels.Models.Interfaces;
using System.Dynamic;
using ExcelParsing;
using TcModels.Models.TcContent.Work;
using System.Xml.Linq;
using TcDbConnector.Repositories;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Serilog;
using Serilog.Events;
using Newtonsoft.Json.Linq;
using Serilog.Parsing;

namespace TestConsoleAppTC;

internal class Program
{
    public static List<TechnologicalCard> ExistingCatds { get; set; } = new List<TechnologicalCard>();
    public static List<TechnologicalProcess> ExistingProcces { get; set; } = new List<TechnologicalProcess>();
    public static TechnologicalCard? CurrentTc { get; set; }
    public static TechnologicalProcess CurrentTp { get; set; }
    // create dictionarys with start and end rows for EModelType from keyValuePairs

    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        // получить ТК по id
        //TcDbConnector.StaticClass.ConnectString = "server=localhost;database=tavrida_db_main;user=root;password=root";

        //MapLogsToExcel();
        LoadLogsToSeqNew();
        //LoadLogsToSeqOldVersion();

        // вывести в консоль имя пользователя windows
        Console.WriteLine(Environment.UserName);

		Console.ReadLine();
    }

	static void MapLogsToExcel()
	{
		// Путь к файлу с логами
		string logFilePath = "C:/Users/bokar/Downloads/log-20241219 (1).json"; // Замените на путь к вашему JSON-файлу
		string excelFilePath = "C:/Users/bokar/Downloads/logs.xlsx";

		// Чтение логов из файла
		List<LogEntry> logEntries;
		using (var reader = new StreamReader(logFilePath))
		{
			logEntries = reader
				.ReadToEnd()
				.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(line => JsonConvert.DeserializeObject<LogEntry>(line))
				.ToList();
		}

		// установить лицензию для EPPlus
		ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

		// Создание Excel-файла
		using (var package = new ExcelPackage())
		{
			var worksheet = package.Workbook.Worksheets.Add("Logs");

			// Заголовки столбцов
			var headers = new[] { "Timestamp", "Level", "Message", "MachineName", "ClassName", "TcId", "Properties" };
			for (int i = 0; i < headers.Length; i++)
			{
				worksheet.Cells[1, i + 1].Value = headers[i];
				worksheet.Cells[1, i + 1].Style.Font.Bold = true;
				worksheet.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
			}

			// Заполнение данными
			for (int row = 0; row < logEntries.Count; row++)
			{
				var entry = logEntries[row];

				// Подстановка значений в MessageTemplate
				string populatedMessage = PopulateMessageTemplate(entry.MessageTemplate, entry.Properties);

				// Получение дополнительных полей из Properties
				string machineName = entry.Properties != null && entry.Properties.ContainsKey("MachineName") ? entry.Properties["MachineName"].ToString() : "null";
				string className = entry.Properties != null && entry.Properties.ContainsKey("ClassName") ? entry.Properties["ClassName"].ToString() : "null";
				string tcId = entry.Properties != null && entry.Properties.ContainsKey("TcId") ? entry.Properties["TcId"].ToString() : "null";

				worksheet.Cells[row + 2, 1].Value = entry.Timestamp;
				worksheet.Cells[row + 2, 2].Value = entry.Level;
				worksheet.Cells[row + 2, 3].Value = populatedMessage;
				worksheet.Cells[row + 2, 4].Value = machineName;
				worksheet.Cells[row + 2, 5].Value = className;
				worksheet.Cells[row + 2, 6].Value = tcId;
				worksheet.Cells[row + 2, 7].Value = JsonConvert.SerializeObject(entry.Properties, Formatting.Indented);

				// Границы для ячеек
				for (int col = 1; col <= headers.Length; col++)
				{
					worksheet.Cells[row + 2, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
				}
			}

			// Автоматическое изменение ширины столбцов
			worksheet.Cells.AutoFitColumns();

			// Сохранение файла
			package.SaveAs(new FileInfo(excelFilePath));
		}

		Console.WriteLine($"Логи успешно экспортированы в файл: {excelFilePath}");
	}

	// Метод для подстановки значений в MessageTemplate
	private static string PopulateMessageTemplate(string template, Dictionary<string, object> properties)
	{
		if (properties == null || string.IsNullOrEmpty(template))
			return template;

		foreach (var property in properties)
		{
			string placeholder = "{" + property.Key + "}";
			template = template.Replace(placeholder, property.Value?.ToString() ?? "null");
		}

		return template;
	}

	// Модель данных для лога
	public class LogEntry
	{
		public string Timestamp { get; set; }
		public string Level { get; set; }
		public string MessageTemplate { get; set; }
		public Dictionary<string, object> Properties { get; set; }
	}

	static void LoadLogsToSeqOldVersion()
	{
		var seqLogger = new LoggerConfiguration()
			.WriteTo.Seq("http://localhost:8081") // URL Вашего Seq
			.CreateLogger();

		// Путь к папке, где лежат логи
		var pathToLogs = @"C:\Users\bokar\Desktop\Логи для анализа";
		// Имя файла лога
		var fileName = "log-20250123.json";

		using (var reader = new StreamReader(Path.Combine(pathToLogs, fileName)))
		{
			string? line;
			while ((line = reader.ReadLine()) != null)
			{
				JObject log;
				try
				{
					log = JsonConvert.DeserializeObject<JObject>(line);
					if (log == null)
					{
						Console.WriteLine($"Неверный формат (пустой JSON): {line}");
						continue;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка при десериализации: {ex.Message}");
					continue;
				}

				// Проверяем обязательные поля
				if (!log.ContainsKey("Timestamp") ||
					!log.ContainsKey("Level") ||
					!log.ContainsKey("MessageTemplate"))
				{
					Console.WriteLine($"Неверный формат лога (нет нужных полей): {line}");
					continue;
				}

				// Считываем Timestamp, Level, MessageTemplate
				var timestamp = log["Timestamp"]?.ToObject<DateTimeOffset?>();
				var levelStr = log["Level"]?.ToString();
				var messageTemplateStr = log["MessageTemplate"]?.ToString();

				if (timestamp == null || levelStr == null || messageTemplateStr == null)
				{
					Console.WriteLine($"Неверный формат полей лога: {line}");
					continue;
				}

				// Преобразуем строку уровня в Enum (LogEventLevel)
				if (!Enum.TryParse(typeof(LogEventLevel), levelStr, true, out var parsedLevelObj))
				{
					Console.WriteLine($"Не удалось интерпретировать Level='{levelStr}'");
					continue;
				}
				var parsedLevel = (LogEventLevel)parsedLevelObj;

				// Собираем дополнительные свойства (TcId, ClassName, ...)
				var propertiesObject = log["Properties"] as JObject;
				var logEventProperties = new List<LogEventProperty>();

				if (propertiesObject != null)
				{
					foreach (var prop in propertiesObject)
					{
						// Создаём LogEventProperty
						// Простейший путь: всё превращаем в строку, 
						// или, если уверены в типе, используем ScalarValue(<конкретный тип>)
						// (можно и умнее - проверять JTokenType, как вы делали ранее)
						var propValue = prop.Value.ToString();
						var logEventProp = new LogEventProperty(
							prop.Key,
							new ScalarValue(propValue)
						);
						logEventProperties.Add(logEventProp);
					}
				}

				// Парсим сам шаблон сообщения
				// (чтобы Serilog мог корректно понимать {placeholders} и связывать их с properties)
				var parser = new MessageTemplateParser();
				var parsedTemplate = parser.Parse(messageTemplateStr);

				// Теперь создаём свой LogEvent с нужным временем:
				var logEvent = new LogEvent(
					timestamp: timestamp.Value,    // <-- тут указываем именно ваше историческое время
					level: parsedLevel,
					exception: null,              // Если нужно, можно доставать из JSON
					messageTemplate: parsedTemplate,
					properties: logEventProperties
				);

				// Отправляем его в Seq
				seqLogger.Write(logEvent);
			}
		}

		seqLogger.Dispose();
	}

	static void LoadLogsToSeqNew()
	{
		var seqLogger = new LoggerConfiguration()
			.WriteTo.Seq("http://localhost:8081") // Укажите URL Вашего Seq
			.CreateLogger();

		// Путь к папке, где лежат логи
		var pathToLogs = @"C:\Users\bokar\Desktop\Логи для анализа";
		// Имя файла лога
		var fileName = "log-20250123.json";

		using (var reader = new StreamReader(Path.Combine(pathToLogs, fileName)))
		{
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				// Преобразуем каждую строку в JObject для удобной работы
				JObject log;
				try
				{
					log = JsonConvert.DeserializeObject<JObject>(line);
					if (log == null)
					{
						Console.WriteLine($"Неверный формат (пустой JSON): {line}");
						continue;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка при десериализации: {ex.Message}");
					continue;
				}

				// Проверяем обязательные поля
				if (!log.ContainsKey("Timestamp") ||
					!log.ContainsKey("Level") ||
					!log.ContainsKey("MessageTemplate"))
				{
					Console.WriteLine($"Неверный формат лога (нет нужных полей): {line}");
					continue;
				}

				// Считываем Timestamp (DateTimeOffset), Level (строка), MessageTemplate (строка)
				var timestamp = log["Timestamp"]?.ToObject<DateTimeOffset>();
				var level = log["Level"]?.ToString();
				var messageTemplate = log["MessageTemplate"]?.ToString();

				if (timestamp == null || level == null || messageTemplate == null)
				{
					Console.WriteLine($"Неверный формат полей лога: {line}");
					continue;
				}

				// Получаем объект Properties (если есть)
				var properties = log["Properties"] as JObject;

				// Создаём «ветвь» логгера, в которую по очереди добавим все свойства
				var contextLogger = seqLogger;
				if (properties != null)
				{
					foreach (var prop in properties)
					{
						// prop.Key – название свойства (напр. "TcId", "ClassName")
						// prop.Value – значение свойства (JToken)
						// Можно привести к нужному типу, например int, string и т.д.
						// Если не уверены в типе – можно просто передавать как строку или оставить as JToken
						var token = prop.Value;
						if (token.Type == JTokenType.Integer)
						{
							contextLogger = (Serilog.Core.Logger)contextLogger.ForContext(prop.Key, token.Value<int>());
						}
						else if (token.Type == JTokenType.Float)
						{
							contextLogger = (Serilog.Core.Logger)contextLogger.ForContext(prop.Key, token.Value<double>());
						}
						else if (token.Type == JTokenType.Boolean)
						{
							contextLogger = (Serilog.Core.Logger)contextLogger.ForContext(prop.Key, token.Value<bool>());
						}
						else
						{
							// Для всех остальных случаев (строка, объект и т.д.) передаём как есть
							contextLogger = (Serilog.Core.Logger)contextLogger.ForContext(prop.Key, token.ToString());
						}
					}
				}

				// Теперь пишем лог в Seq, указывая оригинальный Timestamp
				//contextLogger.Write(
				//    (LogEventLevel)Enum.Parse(typeof(LogEventLevel), level, true),
				//    timestamp.Value.ToString("o"), // Convert DateTimeOffset to string
				//    messageTemplate
				//);

				contextLogger.Write(
	                (LogEventLevel)Enum.Parse(typeof(LogEventLevel), level, true),
	                //timestamp.Value, // <-- Важный момент: передаём DateTimeOffset как второй параметр
	                messageTemplate
                );
			}
		}

		seqLogger.Dispose();
	}

	static void LoadLogsToSeq() // добавляет в seq логи из файла, но пока без параметров и указывает время добавление, а не из лога
    {
        var seqLogger = new LoggerConfiguration()
            .WriteTo.Seq("http://localhost:8081") // Укажите URL Seq
            .CreateLogger();

        var pathToLogs = @"C:/Users/bokar/source/TC_Viewer/TC_WinForms/bin/Debug/net6.0-windows/logs";
        using (var reader = new StreamReader(pathToLogs + "/log-20241122.json"))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var log = JsonConvert.DeserializeObject<dynamic>(line);

                // Проверка обязательных полей для Seq
                if (log.ContainsKey("Timestamp") && log.ContainsKey("Level") && log.ContainsKey("MessageTemplate"))
                {
                    var timestamp = log["Timestamp"];
                    var level = log["Level"];
                    var messageTemplate = log["MessageTemplate"];
                    var properties = log.ContainsKey("Properties") ? log["Properties"] : null;

                    // Отправляем лог в Seq
                    seqLogger
                        .ForContext("Properties", properties) // Добавляем свойства, если они есть
                        .Write((LogEventLevel)Enum.Parse(typeof(LogEventLevel), level.ToString(), true),
                            messageTemplate.ToString());
                }
                else
                {
                    Console.WriteLine($"Неверный формат лога: {line}");
                }
            }
        }

        seqLogger.Dispose();
    }

    static void LoadImage()
    {
        // Пример пути к файлу Excel и изображению
        var excelPath = @"C:\Users\bokar\Desktop\testPicture.xlsx";
        var imagePath = @"C:\Users\bokar\Desktop\photo_2024-06-26_01-05-21.jpg";
        //"C:\Users\bokar\Desktop\photo_2023-07-31_23-00-57.jpg";// 

        // Создание файла Excel
        using (var package = new ExcelPackage(new FileInfo(excelPath)))
        {
            var worksheet = package.Workbook.Worksheets.First(); //.Add("Sheet1");

            // Получение начальной и конечной позиции ячеек по столбцам и строкам
            var startCell = worksheet.Cells["C1"];
            var endCell = worksheet.Cells["Z50"];

            // Получение координат столбцов
            var startColumn = startCell.Start.Column;
            var endColumn = endCell.End.Column;

            // Получение координат строк
            var startRow = startCell.Start.Row;
            var endRow = endCell.End.Row;

            // Расчет ширины изображения в пикселях, учитывая только видимые столбцы
            double widthInPixels = 0;
            for (int col = startColumn; col <= endColumn; col++)
            {
                var column = worksheet.Column(col);
                if (!column.Hidden) // Проверяем, что столбец не скрыт
                {
                    widthInPixels += column.Width * 7; // 7 - приблизительное количество пикселей на единицу ширины столбца
                }
            }

            // Расчет высоты изображения в пикселях, учитывая только видимые строки
            double heightInPixels = 0;
            for (int row = startRow; row <= endRow; row++)
            {
                var rowHeight = worksheet.Row(row).Height;
                heightInPixels += rowHeight * 0.75; // 0.75 - коэффициент перевода высоты строки в пиксели
            }

            // Загрузка изображения для получения его исходных размеров
            using (var img = Image.FromFile(imagePath))
            {
                // Вычисление масштаба по ширине и высоте
                double scaleWidth = widthInPixels / img.Width;
                double scaleHeight = heightInPixels / img.Height;

                // Определение наибольшего ограничения и установка масштабов
                double scale = Math.Min(scaleWidth, scaleHeight);
                double finalWidth = img.Width * scale;
                double finalHeight = img.Height * scale;

                // Добавление изображения на лист
                var picture = worksheet.Drawings.AddPicture("MyImage111", new FileInfo(imagePath));

                // Установка позиции и размеров изображения
                picture.SetPosition(startRow - 1, 0, startColumn - 1, 0); // Позиция начальной ячейки (строка, смещение, столбец, смещение)
                picture.SetSize((int)finalWidth, (int)finalHeight); // Установка итоговых размеров изображения
            }

            // Сохранение файла
            package.Save();
        }
    }
    static void LoadImageInRangeExcel2()
    {
        var filePath = @"C:\Users\bokar\Desktop\testPicture.xlsx";
        var imagePath = @"C:\Users\bokar\Desktop\photo_2024-06-26_01-05-21.jpg";
        // @"C:\Users\bokar\Desktop\photo_2023-09-04_09-57-04.jpg";
        // @"C:\Users\bokar\Desktop\IMG_2563.JPEG";

        var image = new FileInfo(imagePath);
        var file = new FileInfo(filePath);

        using var excelPackage = new ExcelPackage(file);
        if (excelPackage.Workbook.Worksheets.Count == 0)
        {
            excelPackage.Workbook.Worksheets.Add("Sheet1");
        }
        var ws = excelPackage.Workbook.Worksheets.First();

        double picWidth, picHeight;

        using (var image2 = Image.FromFile(imagePath))
        {
            picWidth = image2.Width;
            picHeight = image2.Height;
        }

        var leftTop = ws.Cells["A1"].Start;
        var rightBottom = ws.Cells["D10"].End;

        int totalWidth = 0;
        int totalHeight = 0;

        // Конвертация ширины столбцов в пиксели
        for (int col = leftTop.Column; col <= rightBottom.Column; col++)
        {
            var colWidth = GetColumnWidthInPixels(ws.Column(col).Width);
            totalWidth += colWidth;
            Console.WriteLine($"столбец {col} с шириной {colWidth}");
        }

        // Конвертация высоты строк в пиксели
        for (int row = leftTop.Row; row <= rightBottom.Row; row++)
        {
            var rowHeight = GetRowHeightInPixels(ws.Row(row).Height);
            totalHeight += rowHeight;
            Console.WriteLine($"строка {row} с высотой {rowHeight}");
        }

        Console.WriteLine($"Итого: высота - {totalHeight}, ширина - {totalWidth}");
        Console.WriteLine($"Картинка имеет габариты: {picHeight}/{picWidth} (высота/ширина)");

        double scaleWidth = (double)totalWidth / picWidth;
        double scaleHeight = (double)totalHeight / picHeight;

        // Выбираем наименьший коэффициент для сохранения пропорций
        double scale = Math.Min(scaleWidth, scaleHeight);
        int scaleInPercent = (int)(scale * 100);

        Console.WriteLine($"Картинка изменится на {scaleInPercent}%");

        var picture = ws.Drawings.AddPicture("pic7", image);
        picture.SetPosition(0, 0, 0, 0);

        var pixelWidth = (int)( 400 / 1.5 );
        var pixelHeight = (int)( 267 / 1.5 );

        picture.SetSize(pixelWidth, pixelHeight);

        excelPackage.Save();
    }

    static int GetColumnWidthInPixels(double columnWidth)
    {
        // Примерная конвертация ширины столбца в пиксели
        if (columnWidth < 1) columnWidth = 1;
        return (int)Math.Truncate((columnWidth - 0.72) * 256 / 7 + 5);
    }

    static int GetRowHeightInPixels(double rowHeight)
    {
        double dpi = 96; // DPI экрана
        return (int)(rowHeight * dpi / 72); // Конвертация пунктов в пиксели
    }

    static void LoadImageInRangeExcel()
    {
        var exporter = new ExcelExporter();

        var filePath = @"C:\Users\bokar\Desktop\testPicture2.xlsx";

        var imagePath = @"C:\Users\bokar\Desktop\IMG_2563.JPEG";// @"C:\Users\bokar\Desktop\photo_2024-06-26_01-05-21.jpg"; //@"C:\Users\bokar\Desktop\photo_2023-07-31_23-00-57.jpg"; // @"C:\Users\bokar\Desktop\photo_2023-09-04_09-57-04.jpg";// 

        var image = new FileInfo(imagePath);

        var file = new FileInfo(filePath);

        var excelPackage = new ExcelPackage(file);

        // добавить в ячейку А2 слово хуй
        if (excelPackage.Workbook.Worksheets.Count == 0)
        {
            excelPackage.Workbook.Worksheets.Add("Sheet1");
        }
        var ws = excelPackage.Workbook.Worksheets.First();

        //ws.Cells["A2"].Value = "хуй";
        //Console.WriteLine("Слово добавлено");



        // установить позицию и размер картинки
        double picWidth;
        double picHeight;

        // Загружаем изображение с помощью System.Drawing, чтобы получить его размеры
        using (var image2 = Image.FromFile(imagePath))
        {
            picWidth = image2.Width;
            picHeight = image2.Height;
        }

        // установить размеры картинки исходя из области между ячейками
        var leftTop = ws.Cells["A1"].Start;
        var rightBottom = ws.Cells["D10"].End;

        //var widthToPixels =  -1 ;//7.5; // 7.5 пикселей на единицу ширины
        //var heightToPixels = 15; // 15 пикселей на единицу высоты

        var totalWidth = 0;// ws.Column(1).Width;
        var totalHeight = 0;// ws.Row(3).Height;

        for (int col = leftTop.Column; col <= rightBottom.Column; col++)
        {
            var colWidth = GetWidthInPix(ws.Column(col).Width);
            totalWidth += colWidth; // ширина столбца в пикселях
            Console.WriteLine($"столбец {col} с шириной {colWidth}");
        }


        for (int row = leftTop.Row; row <= rightBottom.Row; row++)
        {
            var rowHeight = GetHeightInPix(ws.Row(row).Height);
            totalHeight += rowHeight; // высота строки в пикселях

            Console.WriteLine($"строка {row} с высотой {rowHeight}");
        }

        Console.WriteLine($"Итого: высота - {totalHeight}, ширина - {totalWidth}");
        Console.WriteLine($"Картинка имеет габариты: {picHeight}/{picWidth} (высота/ширина)");
        // Рассчитываем коэффициенты масштабирования по ширине и высоте
        double scaleWidth = totalWidth / picWidth;
        double scaleHeight = totalHeight / picHeight;

        // Выбираем наименьший коэффициент, чтобы сохранить пропорции
        double scale = Math.Min(scaleWidth, scaleHeight);
        // Округляем до целого процента вниз
        var scaleInPercentDouble = Math.Floor(scale * 0.675 * 100);
        int scaleInPercent = (int)(scaleInPercentDouble);

        Console.WriteLine($"Картинка изменится на {scaleInPercent}% ({scaleInPercentDouble})");


        // добавить картинку в ячейку А3
        var picture = ws.Drawings.AddPicture("pic3", image);

        picture.SetPosition(0, 0, 0, 0);
        picture.SetSize(scaleInPercent);
        Console.WriteLine("Картинка добавлена");

        // сохранить файл
        excelPackage.Save();


        int GetWidthInPix(double width)
        {
            var coef = 1.0564;
            return (int)(width * 114 / 9.73 / coef); //(int)(width -1)/7 -7; //(int)(width * 7.5);
        }
        int GetHeightInPix(double height)
        {
            var coef = 1.03125;
            return (int)(height * 2 / coef);
        }
    }

    static void UpdateDictionary()
    {
        TcDbConnector.StaticClass.ConnectString = "server=localhost;database=tavrida_db_main;user=root;password=root";

        var context = new MyDbContext();

        var parser = new DictionaryParser(context);

        parser.ParseDictionaries(@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\ТК\Исходные\Справочник_ТК (6).xlsx");
    }
    static void MakeAllStatusesDraft(TechnologicalCard.TechnologicalCardStatus status)
    {
        using (var db = new MyDbContext())
        {
            var allTC = db.TechnologicalCards.ToList();
            foreach (var tc in allTC)
            {
                tc.Status = status;
            }
            db.SaveChanges();
        }
    }
    static void CoefficientTests()
    {
        using (var context = new MyDbContext())
        {
            var techTransition = context.TechTransitions.FirstOrDefault();
            var techOperation = context.TechOperations.FirstOrDefault();
            var tc = context.TechnologicalCards.FirstOrDefault();

            var TechOperationWork = new TechOperationWork
            {
                Order = 0,
                techOperation = techOperation,
                technologicalCard = tc
            };
            // Создаем новый объект ExecutionWork
            var parentExecutionWork = WorkParser.CreateExecutionWork(techTransition, 1,"0","0","");
            //var parentExecutionWork = new ExecutionWork
            //{
            //    techOperationWorkId = 1,
            //    techTransitionId = 1,
            //    Comments = "test comment",
            //    // Заполните нужные поля
            //};

            var childExecutionWork = WorkParser.CreateExecutionWork(techTransition, 21, "0", "0", "");
            //var childExecutionWork = new ExecutionWork
            //{
            //    techOperationWorkId = 1,
            //    techTransitionId = 1,
            //    // Заполните нужные поля
            //    Comments = "test comment",
            //};

            TechOperationWork.executionWorks.Add(parentExecutionWork);
            TechOperationWork.executionWorks.Add(childExecutionWork);
            //context.ExecutionWorks.Add(parentExecutionWork);
            //context.ExecutionWorks.Add(childExecutionWork);

            var executionWorkLink = new ExecutionWorkRepeat
            {
                ParentExecutionWork = parentExecutionWork,
                ChildExecutionWork = childExecutionWork,
                NewCoefficient = "*1",
                NewEtap = "Etap1",
                NewPosled = "Posled1"
            };



            context.TechOperationWorks.Add(TechOperationWork);

            // Сохраняем изменения, чтобы получить Id для новых объектов
            //context.SaveChanges();

            // Создаем связь
            

            context.ExecutionWorkRepeats.Add(executionWorkLink);

            // Сохраняем изменения
            context.SaveChanges();
        }

        //using (var db = new MyDbContext())
        //{
        //    db.Database.EnsureDeleted();
        //    db.Database.EnsureCreated();

        //    var allTC = db.ExecutionWorks
        //        .ToList();
        //}

    }


    static void TCExecitionsPicture()
    {
        var dictionary = new Dictionary<string, List<string>>()
        {
            { "ТКР_v4.0_ред.xlsx", new List<string>
                {
                    "ТКР10_1.1.8",
                    "ТКР10_1.1.10",
                    "ТКР10_1.2.1",
                    "ТКР10_1.2.1.2",
                    "ТКР10_1.4.8",
                    "ТКР10_1.4.8_3",
                    "ТКР10_1.4.8_5",
                    "ТКР10_1.4.8_6",
                    "ТКР10_1.4.9",
                    "ТКР10_1.4.14",
                    "ТКР10_1.4.14.2",
                    "ТКР10_1.4.14.3",
                    "ТКР10_1.4.20",
                    "ТКР10_1.4.21",
                    "ТКР10_1.5.2.3",
                    "ТКР10_1.6.1",
                    "ТКР10_2.2.1",
                    "ТКР10_2.2.1(1.1)",
                    "ТКР10_2.2.2",
                    "ТКР10_2.2.2 (1.1)",
                    "ТКР10_2.2.2 (2)",
                    "ТКР10_2.2.2 (2.1)",
                    "ТКР10_2.2.3",
                    "ТКР10_2.2.3 (1.1)",
                    "ТКР10_2.2.3 (2)",
                    "ТКР10_2.2.3 (2.1)",
                    "ТКР10_4.2.1",
                    "ТКР10_4.3.1",
                    "ТКР10_6.2.1",
                    "ТКР10_8.5.1",
                    "ТКР10_8.7.1",
                    "ТКР10_8.9.1"
                }
            },
            { "ТКМ_v16.2.xlsx", new List<string>
                {
                    "ТКМ04_1.1.1",
                    "ТКМ04_1.1.2",
                    "ТКМ04_1.1.3",
                    "ТКМ04_1.1.4",
                    "ТКМ04_1.2.1",
                    "ТКМ04_1.2.2",
                    "ТКМ04_1.2.3",
                    "ТКМ04_1.2.4",
                    "ТКМ04_1.4.14",
                    "ТКМ04_1.4.17",
                    "ТКМ04_1.6.1",
                    "ТКМ04_1.18.1",
                    "ТКМ35_6",
                    "ТКМ10_1.1.1",
                    "ТКМ10_1.3.8",
                    "ТКМ10_1.8.6",
                    "ТКМ10_1.13.2"
                }
            },
            { "ТК_ТТ_v4.3.xlsx", new List<string>
                {
                    "ТК_1.17.1",
                    "ТК_1.17.2"
                }
            }
        };

        var listDraftTC = dictionary.Values.SelectMany(list => list).ToList();  

        var tcRepo = new TechnologicalCardRepository(new MyDbContext());

        var allTC = tcRepo.GetAll();

        var draftT = allTC.Where(tc => listDraftTC.Contains(tc.Article)).ToList();
        var wsParser = new WorkParser();
        foreach (var fileName in dictionary.Keys) 
        {
            wsParser.ParseExecutionPictures($@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\ТК\{fileName}", dictionary[fileName], tcRepo);
        }
    }

    static void ParseAllTC()
    {
        var exParser = new ExcelParser();
        var folder = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\ТК";
        var fileNames = new List<string>
        {
            //"ТКР_v4.0_ред.xlsx",
            "ТКМ_v16.2_ред.xlsx",
            //"ТК_ТТ_v4.3_ред.xlsx"
        };
        var parsingFolder = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\ТК\";
        var parsingFileName = "Отчёт парсинга.xlsx";
        exParser.ParseAllTCs(folder, fileNames, parsingFolder, parsingFileName);//);//
    }
    static void CheckOldForm()
    {
        var exParser = new ExcelParser();
        var folder = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\ТК";
        var fileNames = new List<string>
        {
            "ТКР_v4.0_ред.xlsx",
            "ТКМ_v16.2_ред.xlsx",
            "ТК_ТТ_v4.3_ред.xlsx"
        };
        var parsingFolder = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\ТК\";
        var parsingFileName = "Cтарая форма.xlsx";
        exParser.FindOldFormTC(folder, fileNames, parsingFolder, parsingFileName);//);//
    }

    static void DeleteAllFromTC(string article)
    {
        using(var db = new MyDbContext())
        {
            var tc = db.TechnologicalCards.Where(tc => tc.Article == article)
                .Include(tc => tc.Staff_TCs )
                .Include(tc => tc.Component_TCs)
                .Include(tc => tc.Tool_TCs)
                .Include(tc => tc.Machine_TCs)
                .Include(tc => tc.Protection_TCs)
                .Include(tc => tc.TechOperationWorks)
                .FirstOrDefault();

            if (tc != null)
            {
                tc.Staff_TCs.Clear();
                tc.Component_TCs.Clear();
                tc.Tool_TCs.Clear();
                tc.Machine_TCs.Clear();
                tc.Protection_TCs.Clear();

                tc.TechOperationWorks.Clear();

                //tc.ExecutionSchemeBase64 = null;

                tc.Status = TechnologicalCard.TechnologicalCardStatus.Created;

                db.SaveChanges();
            }
            else
            {
                throw new Exception($"ТК {article} не найдена");
            }
        }
    }

    static void ParseTEST()
    {
        var parser2 = new WorkParser();
        parser2.Test();
    }

    

    static void GetStaffs()
    {
        //using (var context = new MyDbContext())
        //{
        //    // Получаем последние выпущенные версии
        //    var lastReleased = context.Staffs
        //        //.Where(s => s.IsReleased)
        //        .OrderByDescending(s => s.Version)
        //        .GroupBy(s => s.OriginalId)
        //        .Select(g => g.FirstOrDefault())
        //        .ToList();

        //    // Получаем последние версии вне зависимости от того, выпущены они или нет
        //    var lastVersions = context.Staffs
        //        .GroupBy(s => s.OriginalId)
        //        .Select(g => g.OrderByDescending(s => s.Version).FirstOrDefault()).ToList();

        //    // Объединяем результаты на стороне клиента
        //    var combinedResults = lastReleased.Concat(lastVersions).Distinct().ToList();

        //}

        using (var context = new MyDbContext())
        {
            // Загружаем все объекты Staff
            var staffs = context.Staffs.ToList();

            // Группируем по OriginalId и выбираем последнюю выпущенную и последнюю версию
            var staffVersions = staffs
                .GroupBy(s => s.OriginalId)
                .Select(g => new
                {
                    LastReleased = g.Where(s => s.IsReleased).OrderByDescending(s => s.Version).FirstOrDefault(),
                    LastVersion = g.OrderByDescending(s => s.Version).FirstOrDefault()
                })
                .ToList();

            // Выбираем уникальные объекты для каждой группы
            var result = new List<Staff>();
            foreach (var group in staffVersions)
            {
                if (group.LastReleased != null && group.LastVersion != null && group.LastReleased.Id != group.LastVersion.Id)
                {
                    result.Add(group.LastReleased);
                    result.Add(group.LastVersion);
                }
                else if (group.LastReleased != null)
                {
                    result.Add(group.LastReleased);
                }
                else if (group.LastVersion != null)
                {
                    result.Add(group.LastVersion);
                }
            }

            result = result.Distinct().ToList(); // Убираем дубликаты, если они есть
        }


    }
    static void CreateNewVersionStaff() 
    {
        var staff = new Staff()
        {
            Id = 37,
            Name = "TestVersion",
            Type = "Test",
            Functions = "Test",
            CombineResponsibility = "Test",
            Qualification = "Test",
            Comment = "Test",
            OriginalId = 37
        };

        CreateOrUpdateStaff(staff);

    }
    static void CreateOrUpdateStaff(Staff staff)
    {
        using (var dbContext = new MyDbContext())

            if (staff.Id == 0) // Новый объект
            {
                staff.UpdateDate = DateTime.Now;
                staff.Version = 1;

                dbContext.Staffs.Add(staff);
                dbContext.SaveChanges();

                staff.OriginalId = staff.Id;
                dbContext.SaveChanges();
            }
            // Обновление существующего объекта
            else if (staff.IsReleased == false) // если объект не выпущен его промежуточные версии не сохраняются
            {
                var updatingStaff = dbContext.Staffs.Where(x => x.Id == staff.Id).FirstOrDefault();

                if(updatingStaff == null)
                {
                    throw new Exception("Объект не найден");
                }

                updatingStaff.ApplyUpdates(staff);
                updatingStaff.UpdateDate = DateTime.Now;

                dbContext.Staffs.Update(updatingStaff);
                dbContext.SaveChanges();
            }
            else // Обновление существующего объекта
            {
                var newVersion = new Staff();
                newVersion.ApplyUpdates(staff);

                newVersion.IsReleased = false;
                newVersion.OriginalId = staff.OriginalId;
                var lastVersion = dbContext.Staffs.Where(x => x.OriginalId == staff.OriginalId).OrderByDescending(x => x.Version).Select(x => x.Version).FirstOrDefault();
                newVersion.Version = lastVersion + 1;
                newVersion.UpdateDate = DateTime.Now;

                dbContext.Staffs.Add(newVersion);
                dbContext.SaveChanges();
            }
        
    }

    static void SetOriginId()
    {
        using (var db = new MyDbContext())
        {
            var allTO = db.Staffs.ToList();
            foreach (var to in allTO)
            {
                to.OriginalId = to.Id;
                to.UpdateDate = DateTime.Now.Date;
            }
            db.SaveChanges();
        }
    }

    static void GetAllTPCategories()
    {
        using (var db = new MyDbContext())
        {
            var allTP = db.TechTransitions.ToList();
            var unique = allTP.GroupBy(tp => tp.Category)
                      .OrderByDescending(group => group.Count())
                      .Select(group => group.Key)
                      .ToList();

            ShowCategories(unique, "Components");
            CombineCategories(unique);

            Console.WriteLine();


        }
        void CombineCategories(List<string> cat)
        {
            var combinedUnits = string.Join(",", cat);
            Console.WriteLine(combinedUnits);
        }
        void ShowCategories(List<string> units, string listName)
        {
            Console.WriteLine(listName);
            foreach (var unit in units)
            {
                Console.WriteLine(unit);
            }
        }
    }
    static void SetTypicalTO()
    {
        using (var db = new MyDbContext())
        {
            var allTO = db.TechOperations
                .Include(to => to.techTransitionTypicals)
                .ToList();

            var allTP = db.TechTransitions
                .ToList();

            Dictionary<string, List<(string, string, string)>> keyValuePairs = new Dictionary<string, List<(string, string, string)>>
            {
                { "Выгрузка стоек", new List<(string, string, string)>
                    {
                        { ("Определить место установки техники", "0","0") },
                        { ("Дать команду на установку техники", "0", "0") },
                        { ("Установить автокран","1000", "0") },
                        { ("Установить грузовик", "1000", "0") },
                        { ("Закрепить грузоподъёмные стропы", "0", "0") },
                        { ("Дать команду на перемещение груза", "0", "0") },
                        { ("Поднять груз", "0", "0") },
                        { ("Переместить груз", "0", "0") },
                        { ("Опустить груз", "0", "0") },
                        { ("Отсоединить грузоподъёмные стропы", "0", "0") },
                        { ("Убрать грузовик", "0", "0") },
                    }
                },
                { "Установка вертикальной стойки", new List<(string, string, string)>
                    {
                        { ("Перевести БКМ в режим автокрана", "0", "0") },
                        { ("Закрепить грузоподъёмный строп", "0", "0") },
                        { ("Дать команду на перемещение груза", "0", "0") },
                        { ("Поднять груз", "0", "0") },
                        { ("Переместить груз на место", "0", "0") },
                        { ("Произвести засыпку грунтом с послойным трамбованием", "0", "0") },
                        { ("Проверить правильность установки стойки", "0", "0") },
                        { ("Устранить отклонение стойки", "0", "0") },
                        { ("Отсоединить грузоподъёмный строп", "0", "0") },

                    }
                },
                { "Бурение котлована БКМ", new List<(string, string, string)>
                    {
                        { ("Определить место установки техники", "0", "0") },
                        { ("Дать команду на установку техники", "0", "0") },
                        { ("Установить БКМ", "1000", "0") },
                        { ("Перевести БКМ в режим бура", "1000", "0") },
                        { ("Измерить и нанести метки", "0", "0") },
                        { ("Установить бур для бурения", "0", "0") },
                        { ("Проверить правильность установки бура", "0", "0") },
                        { ("Дать команду на бурение", "0", "0") },
                        { ("Пробурить отверстие", "0", "0") },
                        { ("Измерить и нанести метки", "0", "0") },

                    }
                },
                { "Установка подкоса на новой опоре", new List<(string, string, string)>
                    {
                        {("Перевести БКМ в режим автокрана", "0", "0")},
                        {("Закрепить грузоподъёмный строп", "0", "0")},
                        {("Дать команду на перемещение груза", "0", "0")},
                        {("Поднять груз", "0", "0")},
                        {("Переместить груз на место", "0", "0")},
                        {("Произвести засыпку грунтом с послойным трамбованием", "0", "0")},
                        {("Положить в сумку инструменты и материалы", "0", "0")},
                        {("Надеть лазы монтерские", "0", "0")},
                        {("Надеть страховочную привязь", "0", "0")},
                        {("Подняться", "0","0")},
                        {("Надеть на шпильки",  "0","0")},
                        {("Наживить и затянуть гайку(и)/болт(ы)", "0", "0")},
                        {("Загнуть заземляющий проводник", "0", "0")},
                        {("Зачистить поверхность", "0", "0")},
                        {("Раскрутить гайку(и)/болт(ы)", "0", "0")},
                        {("Установить плашку(и)", "0", "0")},
                        {("Наживить и затянуть гайку(и)/болт(ы)", "0", "0")},
                        {("Спуститься", "0", "0")},
                        {("Снять лазы монтерские",  "1000","1")},
                        {("Снять страховочную привязь",  "1000","1")},
                        {("Отсоединить грузоподъёмный строп",  "1000","0")},
                    }
                },
                { "Демонтаж вертикальной стойки", new List<(string, string, string)>
                    {
                        {("Перевести БКМ в режим автокрана", "0", "0")},
                        {("Закрепить грузоподъёмный строп", "0", "0")},
                        {("Дать команду на перемещение груза", "0", "0")},
                        {("Поднять груз", "0", "0")},
                        {("Переместить груз на место", "0", "0")},
                        {("Отсоединить грузоподъёмный строп", "0", "0")},

                    }
                },
                { "Демонтаж подкоса", new List<(string, string, string)>
                    {
                        {("Надеть лазы монтерские",  "1000","1")},
                        {("Надеть страховочную привязь", "1000", "1")},
                        {("Положить в сумку инструменты и материалы", "1000", "1")},
                        {("Подняться", "1000", "1")},
                        {("Перевести БКМ в режим автокрана", "1000", "2")},
                        {("Закрепить грузоподъёмный строп", "1000", "2")},
                        {("Дать команду на перемещение груза", "1000", "2")},
                        {("Поднять груз", "1000", "2")},
                        {("Демонтировать крепление металлоконструкции", "0", "0")},
                        {("Демонтировать заземляющий проводник", "0", "0")},
                        {("Спуститься", "0", "0")},
                        {("Снять лазы монтерские", "2000", "1")},
                        {("Снять страховочную привязь", "2000", "1")},
                        {("Дать команду на перемещение груза", "2000", "2")},
                        {("Поднять груз", "2000", "2")},
                        {("Переместить груз на место", "2000", "2")},
                        {("Отсоединить грузоподъёмный строп", "2000", "2")},

                    }
                },

            };
            foreach (var nameTO in keyValuePairs)
            {
                var TO = allTO.FirstOrDefault(x => x.Name == nameTO.Key);

                if (TO != null)
                {
                    TO?.techTransitionTypicals.Clear();

                    foreach (var nameTP in nameTO.Value)
                    {
                        FindAndAddTPtoTO(TO, allTP, nameTP.Item1, nameTP.Item2, nameTP.Item3);
                    }

                }
            }

            db.SaveChanges();
            //FindAndAddTPtoTO(TO, allTP, "Определить место установки техники");

        }

        void FindAndAddTPtoTO(TechOperation TO, List<TechTransition> allTP, string nameTP,
            string? etap = "", string? posled = "")
        {
            var TP = allTP.FirstOrDefault(x => x.Name == nameTP);

            TO.techTransitionTypicals.Add(new TechTransitionTypical
            {
                TechTransition = TP,
                TechOperation = TO,
                Etap = etap,
                Posled = posled
            });
        }
    }
    static void GetAllTypes()
    {
        using (var db = new MyDbContext())
        {
            List<string> allTypes = new List<string>();
            // get all units from db
            var allComp = db.Components.ToList();
            var allTypesComp = SelectUniqueTypes(allComp);
            ShowTypes(allTypesComp, "Components");
            CombineTypes(allTypesComp);

            var allTools = db.Tools.ToList();
            var allTypesTools = SelectUniqueTypes(allTools);//.Select(tool => tool.Unit).Distinct().ToList();
            ShowTypes(allTypesTools, "Tools");
            CombineTypes(allTypesTools);

            var allMachines = db.Machines.ToList();
            var allTypesMachines = SelectUniqueTypes(allMachines);
            ShowTypes(allTypesMachines, "Machines");
            CombineTypes(allTypesMachines);

            var allProtections = db.Protections.ToList();
            var allTypesProtections = SelectUniqueTypes(allProtections);
            ShowTypes(allTypesProtections, "Protections");
            CombineTypes(allTypesProtections);

            // get unique units
            var uniqueTypes = allTypesComp.Union(allTypesTools).Union(allTypesMachines).Union(allTypesProtections).Distinct().ToList();

            ShowTypes(uniqueTypes, "Unique types");
            CombineTypes(uniqueTypes);
        }

        void CombineTypes(List<string> units)
        {
            var combinedUnits = string.Join(",", units);
            Console.WriteLine(combinedUnits);
        }
        void ShowTypes(List<string> types, string listName)
        {
            Console.WriteLine(listName);
            foreach (var type in types)
            {
                Console.WriteLine(type);
            }
            Console.WriteLine();
        }
        List<string> SelectUniqueTypes<T>(List<T> objList) where T : class, IModelStructure
        {
            return objList.GroupBy(comp => comp.Type)
                      .OrderByDescending(group => group.Count())
                      .Select(group => group.Key)
                      .ToList();
        }
    }
    static void GetAllCategories()
    {
        using(var db = new MyDbContext())
        {
            var allComp = db.Components.ToList();
            var uniqueComp = allComp.GroupBy(comp => comp.Categoty)
                      .OrderByDescending(group => group.Count())
                      .Select(group => group.Key)
                      .ToList();
            ShowCategories(uniqueComp, "Components");
            CombineCategories(uniqueComp);

            Console.WriteLine();

            var allTools = db.Tools.ToList();
            var uniqueTools = allTools.GroupBy(comp => comp.Categoty)
                      .OrderByDescending(group => group.Count())
                      .Select(group => group.Key)
                      .ToList();
            ShowCategories(uniqueTools, "Tools");
            CombineCategories(uniqueTools);
            Console.WriteLine();

        }
        void CombineCategories(List<string> cat)
        {
            var combinedUnits = string.Join(",", cat);
            Console.WriteLine(combinedUnits);
        }
        void ShowCategories(List<string> units, string listName)
        {
            Console.WriteLine(listName);
            foreach (var unit in units)
            {
                Console.WriteLine(unit);
            }
        }
    }
    static void GetAllUnits()
    {
        using (var db = new MyDbContext())
        {
            List<string> allUnits = new List<string>();
            // get all units from db
            var allComp = db.Components.ToList();
            var allUnitsComp = SelectUniqueUnits(allComp);
            ShowUnits(allUnitsComp, "Components");
            CombineUnits(allUnitsComp);

            var allTools = db.Tools.ToList();
            var allUnitsTools = SelectUniqueUnits(allTools);//.Select(tool => tool.Unit).Distinct().ToList();
            ShowUnits(allUnitsTools, "Tools");
            CombineUnits(allUnitsTools);
            
            var allMachines = db.Machines.ToList();
            var allUnitsMachines = SelectUniqueUnits(allMachines);
            ShowUnits(allUnitsMachines, "Machines");
            CombineUnits(allUnitsMachines);

            var allProtections = db.Protections.ToList();
            var allUnitsProtections = SelectUniqueUnits(allProtections);
            ShowUnits(allUnitsProtections, "Protections");
            CombineUnits(allUnitsProtections);

            // get unique units
            var uniqueUnits = allUnitsComp.Union(allUnitsTools).Union(allUnitsMachines).Union(allUnitsProtections).Distinct().ToList();

            ShowUnits(uniqueUnits, "Unique units");
            CombineUnits(uniqueUnits);
        }
        
        void CombineUnits(List<string> units)
        {
            var combinedUnits = string.Join(",", units);
            Console.WriteLine(combinedUnits);
        }
        void ShowUnits(List<string> units, string listName)
        {
            Console.WriteLine(listName);
            foreach (var unit in units)
            {
                Console.WriteLine(unit);
            }
            Console.WriteLine();
        }
        List<string> SelectUniqueUnits<T>(List<T> objList) where T : class, IModelStructure
        {
            return  objList.GroupBy(comp => comp.Unit)
                      .OrderByDescending(group => group.Count())
                      .Select(group => group.Key)
                      .ToList();
        }
    }
    
    static void GetEmptyStringUnits()
    {
        using (var db = new MyDbContext())
        {

            var allComp = db.Tools.ToList();
            var allUnitsComp = allComp.Where(comp => comp.Unit == "").ToList();
            foreach (var comp in allUnitsComp)
            {
                Console.WriteLine(comp.Name);
            }
        }


    }
    static async Task<TechnologicalCard?> GetTechnologicalCardToExportAsync(int id)
    {
        using (var db = new MyDbContext())
        {
            var tc = db.TechnologicalCards.Where(tc => tc.Id == id)

                .Include(tc => tc.Staff_TCs).ThenInclude(tc => tc.Child)
                .Include(tc => tc.Component_TCs).ThenInclude(tc => tc.Child)
                .Include(tc => tc.Tool_TCs).ThenInclude(tc => tc.Child)
                .Include(tc => tc.Machine_TCs).ThenInclude(tc => tc.Child)
                .Include(tc => tc.Protection_TCs).ThenInclude(tc => tc.Child)

                .Include(tc => tc.TechOperationWorks).ThenInclude(tc => tc.techOperation)

                .FirstOrDefault();
            var towIds = tc.TechOperationWorks.Select(tow => tow.Id).ToList();

            var ew = await db.ExecutionWorks.Where(ew => towIds.Contains(ew.techOperationWorkId))
                .Include(ew => ew.Staffs)
                .Include(ew => ew.Machines)
                .Include(ew => ew.Protections)
                .Include(ew => ew.techTransition)
                .Include(ew => ew.ListexecutionWorkRepeat2)
                .ToListAsync();
            var tw = await db.ToolWorks.Where(tw => towIds.Contains(tw.techOperationWorkId))
                .Include(tw => tw.tool)
                .ToListAsync();
            var cw = await db.ComponentWorks.Where(cw => towIds.Contains(cw.techOperationWorkId))
                .Include(cw => cw.component)
                .ToListAsync();

            foreach (var tow in tc.TechOperationWorks)
            {
                var ewList = ew.Where(ew => ew.techOperationWorkId == tow.Id).ToList();
                var twList = tw.Where(tw => tw.techOperationWorkId == tow.Id).ToList();
                var cwList = cw.Where(cw => cw.techOperationWorkId == tow.Id).ToList();

                tow.executionWorks = ewList;
                tow.ToolWorks = twList;
                tow.ComponentWorks = cwList;
            }

            return tc;
        }
    }
    async void ParseTcToExcel()
    {
        var export = new TCExcelExporter();

        using (var db = new MyDbContext())
        {
            var tc = db.TechnologicalCards.Where(tc => tc.Id == 2)

                .Include(tc => tc.Staff_TCs).ThenInclude(tc => tc.Child)
                .Include(tc => tc.Component_TCs).ThenInclude(tc => tc.Child)
                .Include(tc => tc.Tool_TCs).ThenInclude(tc => tc.Child)
                .Include(tc => tc.Machine_TCs).ThenInclude(tc => tc.Child)
                .Include(tc => tc.Protection_TCs).ThenInclude(tc => tc.Child)

                .Include(tc => tc.TechOperationWorks).ThenInclude(tc => tc.techOperation)

                .FirstOrDefault();
            var towIds = tc.TechOperationWorks.Select(tow => tow.Id).ToList();

            var ew = await db.ExecutionWorks.Where(ew => towIds.Contains(ew.techOperationWorkId))
                .Include(ew => ew.Staffs)
                .Include(ew => ew.Machines)
                .Include(ew => ew.Protections)
                .Include(ew => ew.techTransition)
                .Include(ew => ew.ListexecutionWorkRepeat2)
                .ToListAsync();
            var tw = await db.ToolWorks.Where(tw => towIds.Contains(tw.techOperationWorkId))
                .Include(tw => tw.tool)
                .ToListAsync();
            var cw = await db.ComponentWorks.Where(cw => towIds.Contains(cw.techOperationWorkId))
                .Include(cw => cw.component)
                .ToListAsync();

            foreach (var tow in tc.TechOperationWorks)
            {
                var ewList = ew.Where(ew => ew.techOperationWorkId == tow.Id).ToList();
                var twList = tw.Where(tw => tw.techOperationWorkId == tow.Id).ToList();
                var cwList = cw.Where(cw => cw.techOperationWorkId == tow.Id).ToList();

                tow.executionWorks = ewList;
                tow.ToolWorks = twList;
                tow.ComponentWorks = cwList;
            }

            if (tc != null)
            {
                export.ExportTCtoFile(@"C:\Tests\Тест2.xlsx", tc);
            }
        }
    }

    static int ComputeLevenshteinDistance(string source, string target)
    {
        var n = source.Length;
        var m = target.Length;
        var matrix = new int[n + 1, m + 1];

        if (n == 0)
            return m;

        if (m == 0)
            return n;

        for (int i = 0; i <= n; matrix[i, 0] = i++) { }
        for (int j = 0; j <= m; matrix[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                int left = matrix[i - 1, j] + 1;
                int right = matrix[i, j - 1] + 1;
                int corner = matrix[i - 1, j - 1] + cost;

                int result = Math.Min(
                    Math.Min(left, right),
                    corner);
                matrix[i, j] = result;
            }
        }

        return matrix[n, m];
    }
    

    static void DeleteAllTO()
    {
        using (var db = new MyDbContext())
        {
            var allTO = db.TechOperationWorks
                .Include(db => db.ComponentWorks)
                .Include(db => db.ToolWorks)
                .Include(db => db.executionWorks)
                .ThenInclude(db => db.Staffs)
                .Include(db => db.executionWorks)
                .ThenInclude(db => db.Machines)
                .Include(db => db.executionWorks)
                .ThenInclude(db => db.Protections)

                .ToList();
            db.TechOperationWorks.RemoveRange(allTO);
            db.SaveChanges();
        }
    }
    static void ParseWS()
    {

        string filepath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка промежуточной сущности Ход работ.xlsx";
        string sheetName = "WorkStep_TC and Tools";

        var parser = new WorkParser();

        var parsedDataWorkSteps = parser.ParseExcelToObjectsTechOperationWork(filepath, sheetName);

        int i = 0;
        var TOWList = parsedDataWorkSteps;
        using (var db = new MyDbContext())
        {

            Console.WriteLine("----------------- TechOperationWork -----------------");

            //find all intermediate objects from db
            var staffTcList = db.Staff_TCs.ToList();
            var componentTcList = db.Component_TCs.ToList();
            var machineTcList = db.Machine_TCs.ToList();


            foreach (var TOW in TOWList)
            {
                // find inxef of TOW in TOWList
                int index = TOWList.IndexOf(TOW);

                var TO = db.TechOperations.Find(TOW.techOperationId);
                var TC = db.TechnologicalCards.Find(TOW.TechnologicalCardId);

                if (TO != null && TC != null)
                {
                    TOW.techOperation = TO;
                    TOW.technologicalCard = TC;

                    foreach (var exW in TOW.executionWorks)
                    {
                        var staff_TCList = new List<Staff_TC>();
                        foreach (var staff in exW.Staffs)
                        {
                            var existingStaff = db.Staff_TCs.Find(staff.IdAuto);
                            if (existingStaff == null)
                            {
                                // Если сотрудник не найден, добавляем его в контекст
                                db.Staff_TCs.Add(staff);
                                staff_TCList.Add(staff);
                            }
                            else
                            {
                                // Если сотрудник уже существует, используем существующий экземпляр
                                staff_TCList.Add(existingStaff);
                            }
                        }

                        var protection_TCList = new List<Protection_TC>();
                        foreach (var protection in exW.Protections)
                        {
                            var existingobj = db.Protection_TCs.Where(x => x.ParentId == protection.ParentId && x.ChildId == protection.ChildId).FirstOrDefault();
                            if (existingobj == null)
                            {
                                // Если сотрудник не найден, добавляем его в контекст
                                db.Protection_TCs.Add(protection);
                                protection_TCList.Add(protection);
                            }
                            else
                            {
                                // Если сотрудник уже существует, используем существующий экземпляр
                                protection_TCList.Add(existingobj);
                            }
                        }

                        var machine_TCList = new List<Machine_TC>();
                        foreach (var machine in exW.Machines)
                        {
                            var existingobj = db.Machine_TCs.Where(x => x.ParentId == machine.ParentId && x.ChildId == machine.ChildId).FirstOrDefault();
                            if (existingobj == null)
                            {
                                // Если сотрудник не найден, добавляем его в контекст
                                db.Machine_TCs.Add(machine);
                                machine_TCList.Add(machine);
                            }
                            else
                            {
                                // Если сотрудник уже существует, используем существующий экземпляр
                                machine_TCList.Add(existingobj);
                            }
                        }
                        exW.Staffs.Clear();
                        exW.Machines.Clear();
                        exW.Protections.Clear();

                        exW.Staffs = staff_TCList;
                        exW.Machines = machine_TCList;
                        exW.Protections = protection_TCList;

                        var tT = db.TechTransitions.Find(exW.techTransitionId);
                        if (tT != null)
                        {
                            exW.techTransition = tT;
                        }

                        exW.techOperationWork = TOW;
                    }

                    foreach (var cw in TOW.ComponentWorks)
                    {

                        var existingComponentId = db.Components.Any(c => c.Id == cw.componentId);

                        if (!existingComponentId)
                        {
                            // Логика обработки случая, когда componentId не найден
                            Console.WriteLine($"Component с ID {cw.componentId} не найден в таблице Components. TO:{TOW.Id} TC:{TOW.TechnologicalCardId}");
                        }
                    }

                    db.TechOperationWorks.Update(TOW);
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка при сохранении изменений: {e.Message}");
            }
        }
    }
    static void TechOperationWork()
    {
        string jsonData = File.ReadAllText($@"WorkSteps.json");
        var TOWList = JsonConvert.DeserializeObject<List<TechOperationWork>>(jsonData);

        using (var db = new MyDbContext())
        {
            foreach (var techOperationWork in TOWList)
            {
                foreach(var ex in techOperationWork.executionWorks)
                {
                    var tT = db.TechTransitions.Find(ex.techTransitionId);
                    if (tT == null)
                    {
                        Console.WriteLine($"TechTransition {ex.techTransitionId} in TC:{techOperationWork.TechnologicalCardId} TO order {techOperationWork.Order} not found");
                    }
                }
            }
        }
    }

    static void AddTOandTPtoDB()
    {
        var jsonData = File.ReadAllText($@"TechOperation.json");
        List<TechOperation> TOList = JsonConvert.DeserializeObject<List<TechOperation>>(jsonData);

        jsonData = File.ReadAllText($@"TechTransition.json");
        List<TechTransition> TPList = JsonConvert.DeserializeObject<List<TechTransition>>(jsonData);

        //add all objects to new db
        using (var db = new MyDbContext())
        {

            db.TechOperations.AddRange(TOList);
            db.TechTransitions.AddRange(TPList);

            db.SaveChanges();
        }
    }
    public static void  ParseTOandTP()
    {
        string filepath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка промежуточной сущности Ход работ.xlsx";

        string sheetNameTO = "Перечень ТО";
        string sheetNameTP = "Типовые переходы";
        var parser = new WorkParser();

        var parsedData = parser.ParseExcelToObjectsTechOperation(filepath, sheetNameTO);
        var parsedDataTP = parser.ParseExcelToObjectsTechTransition(filepath, sheetNameTP); 
        //save result to json
        var json = JsonConvert.SerializeObject(parsedData, Formatting.Indented);
        File.WriteAllText("TechOperation.json", json);

        var jsonTP = JsonConvert.SerializeObject(parsedDataTP, Formatting.Indented);
        File.WriteAllText("TechTransition.json", jsonTP);
    }
    public static void CheckTechOperationWorkParser()
    {
        string filepath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка промежуточной сущности Ход работ.xlsx";

        string sheetName = "WorkStep_TC and Tools";
        var parser = new WorkParser();

        var parsedData = parser.ParseExcelToObjectsTechOperationWork(filepath, sheetName);

        //save result to json
        var json = JsonConvert.SerializeObject(parsedData, Formatting.Indented);
        File.WriteAllText("parsedData.json", json);
    }

    public static void SaveParsedDataToDb()
    {
        var parsedData = JsonConvert.DeserializeObject<List<TechOperationWork>>(File.ReadAllText("parsedData.json"));

        using (var db = new MyDbContext())
        {
            var staffTcInDb = db.Staff_TCs.ToDictionary(st => st.IdAuto, st => st);

            foreach (var techOperationWork in parsedData)
            {
                foreach (var executionWork in techOperationWork.executionWorks)
                {
                    var staffIds = executionWork.Staffs.Select(st => st.IdAuto).ToList();
                    executionWork.Staffs.Clear(); 

                    foreach (var staffId in staffIds)
                    {
                        if (staffTcInDb.TryGetValue(staffId, out var staffTc))
                        {
                            executionWork.Staffs.Add(staffTc);
                        }
                        else
                        {
                            Console.WriteLine($"нет объекта Staff_TC с AutoId {staffId}");
                        }
                    }
                }

                if (techOperationWork.Id == 0)
                {
                    db.TechOperationWorks.Add(techOperationWork);
                }
                else
                {
                    db.TechOperationWorks.Update(techOperationWork);
                }
            }

            //db.SaveChanges();
        }
    }

    public static void ChangeSymbol()
    {
        using (var db = new MyDbContext())
        {
            var staff_tc = db.Set<Staff_TC>().Include(tc => tc.Child).FirstOrDefault();
            Console.WriteLine(staff_tc);
            staff_tc.Symbol = "test";

            Console.WriteLine(staff_tc);

            db.SaveChanges();
        }
    }

    public static void DeleteIntermediate()
    {
        
        using(var db = new MyDbContext())
        {
            var staff_tc = db.Set<Staff_TC>().Include(tc => tc.Child).FirstOrDefault();
            Console.WriteLine(staff_tc);

            db.Remove(staff_tc);
            db.SaveChanges();
        }

    }

    public static void ReferenseType()
    {
        var staff = new Staff { Id = 1, Name = "Геодезист", Type = "OSU" };
        var staff2 = new Staff { Id = 2, Name = "Водитель", Type = "Бригадный автомобиль OSU" };

        var staff_tc = new Staff_TC { ParentId = 1, Child = staff, Symbol = "Г1", Order = 1 };
        var staff_tc2 = new Staff_TC { ParentId = 1, Child = staff, Symbol = "Г2", Order = 2 };
        var staff_tc3 = new Staff_TC { ParentId = 1, Child = staff2, Symbol = "Г2", Order = 3 };

        var list1 = new List<Staff>() { staff, staff2 };
        var staff_tcList = new List<Staff_TC>() { staff_tc, staff_tc2, staff_tc3 };

        var combinedList = from obj1 in list1
                           join obj2 in staff_tcList on obj1.Id equals obj2.ChildId
                           select new { obj1, obj2 };


        var displayList = staff_tcList.Select(i => new ExpandoObject() as IDictionary<string, Object>);

        //var printList = staff_tcList.Select(i => new Intermed {ParentId =i.ParentId, ChildId= i.Child.Id, ChildName = i.Child.Name, ChildType = i.Child.Type, Symbol = i.Symbol, Order = i.Order }).ToList();

        foreach (var item in displayList.ToList())
        {
            //Console.WriteLine(item.Keys + " - " + item.Values.Id);
        }
        
        //foreach(var item in printList)
        //{
        //    Console.WriteLine(item.ParentId + " - " + item.ChildId + " | " + item.ChildName + " " + item.ChildType + " " + item.Symbol + " " + item.Order);
        //}

        Console.WriteLine();

        foreach (var item in staff_tcList)
        {
            Console.WriteLine(item.ParentId + " - " + item.Child.Id + " | " + item.Child.Name + " " + item.Child.Type + " " + item.Symbol + " " + item.Order);
        }

        

    }
    public class Intermed
    {
        public int ParentId { get; set; }
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public string ChildType { get; set; }
        public string Symbol { get; set; }
        public int Order { get; set; }
    }
    public static void TestGenerics<T,C>()
    where T : class, IIntermediateTable<TechnologicalCard, C>
    where C : class, IDGViewable, INameable
    {

        var objList = GetIntermediateObjectList<T, C>(1);
        foreach(var obj in objList)
        {
            Console.WriteLine(obj.Child.Id + " " +obj.Child.Name);
        }

    }
    public static List<T> GetIntermediateObjectList<T, C>(int parentId) where T : class, IIntermediateTable<TechnologicalCard, C>
    {
        try
        {
            // todo - Db connection error holder 
            using (var context = new MyDbContext())
            {

                return context.Set<T>().Where(obj => obj.ParentId == 1)
                                .Include(tc => tc.Child)
                                .Cast<T>()
                                .ToList();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static List<T> GetList<T>() where T : class, IIdentifiable
    {
        try
        {
            // todo - Db connection error holder 
            using (var context = new MyDbContext())
            {
                if (typeof(T) == typeof(TechnologicalProcess))
                    return context.Set<TechnologicalProcess>()
                                    .Include(tp => tp.TechnologicalCards)
                                    .Cast<T>()
                                    .ToList();

                else if (typeof(T) == typeof(TechnologicalCard))
                    return context.Set<TechnologicalCard>()
                                    .Include(tc => tc.Staffs)
                                    .Include(tc => tc.Components)
                                    .Include(tc => tc.Tools)
                                    .Include(tc => tc.Machines)
                                    .Include(tc => tc.Protections)
                                    //.Include(tc => tc.WorkSteps)
                                    .Cast<T>()
                                    .ToList();
                else return context.Set<T>().ToList();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static void DbTest2() 
    {
        Author author = new Author { Name = "Tom", Surname = "Smith", Email = "someemail@mail.com" };
        Author author2 = new Author { Name = "Tom2", Surname = "Smith", Email = "someemail@mail.com" };

        TechnologicalCard tc1 = new TechnologicalCard
        {
            Article = "ТК_1.1",
            Name = "ТК_Вынос осей",
            Authors = new List<Author> { author, author2 }
        };

        List<Staff> staffList = new List<Staff>()
            {
                new(){
                    Id = 1,
                    Name = "Геодезист",
                    Type = "OSU",
                    CombineResponsibility = "Водитель: Бригадный автомобиль OSU",
                    Qualification = "1. Профильная квалификация."
                },

                new(){
                    Id = 2,
                    Name = "Водитель",
                    Type = "Бригадный автомобиль OSU",
                    CombineResponsibility = "-",
                    Qualification = "1. Водительское удостоверение категории B."
                },
                new(){
                    Id = 3,
                    Name = "Руководитель работ",
                    Type = "Бригадный автомобиль OSU",
                    CombineResponsibility = "Водитель: Бригадный автомобиль OSU",
                    Qualification = "1. Аттестация по промышленной безопасности в категориях Б.9.3; Б.9.4; Г.1.1; Г.2.2.\r\n2. Ответственный за производство работ кранами.\r\n3. Удостоверение по пожарной безопасности.\r\n4. При совмещении используются требования к совмещаемой роли."
                },
                new(){
                    Id = 4,
                    Name = "Монтажник по монтажу стальных и железобетонных конструкций",
                    Type = "Бригадный автомобиль OSU",
                    CombineResponsibility = "Водитель: Бригадный автомобиль OSU\r\nРуководитель работ OSU\r\nЭлектросварщик ручной сварки ОРБ/OSU\r\n Машинист: Автовышка ОРБ/OSU",
                    Qualification = "1. Разряд - 3.\r\n2. Удостоверение стропальщика.\r\n3. Удостоверение по пожарной безопасности.\r\n4. При совмещении используются требования к совмещаемой роли."
                },
                new(){
                    Id = 5,
                    Name = "Машинист",
                    Type = "Экскаватор-погрузчик OSU",
                    CombineResponsibility = "-",
                    Qualification = "1. Удостоверение тракториста-машиниста с отметкой «Машинист экскаватора», «Водитель погрузчика» соответствующей категории («А», «В», «С», «D»)."
                },

            };

        List<Component> componentList = new List<Component>()
            {
                new()
                {
                    Id = 1,
                    Name = "Стальная полоса",
                    Type = "5.0х40 ГЦ",
                    Unit = "м"
                },
                new()
                {
                    Id = 2,
                    Name = "Арматура",
                    Type = "Круг В-II-18",
                    Unit = "м"
                },
                new()
                {
                    Id = 3,
                    Name = "Электрод",
                    Type = "ОК-46 3мм",
                    Unit = "шт."
                }
            };

        List<Machine> machineList = new List<Machine>()
            {
                new()
                {
                    Id = 1,
                    Name = "Бригадный автомобиль",
                    Type = "OSU",
                    Unit = "шт.",
                },
                new()
                {
                    Id = 2,
                    Name = "Экскаватор-погрузчик",
                    Type = "OSU",
                    Unit = "шт.",
                },
                new()
                {
                    Id = 3,
                    Name = "Автобетоносмеситель СБ-159А",
                    Type = "OSU",
                    Unit = "шт.",
                },
                new()
                {
                    Id = 4,
                    Name = "Автобетононасос на базе КАМАЗ 65115\t\r\n",
                    Type = "OSU",
                    Unit = "шт.",
                },
                new()
                {
                    Id = 5,
                    Name = "Автомобиль КАМАЗ 65117 с КМУ KAN GLIM KS2056\t\r\n",
                    Type = "OSU",
                    Unit = "шт.",
                }
            };

        List<Protection> protectionList = new List<Protection>()
            {
                new()
                {
                    Id = 1,
                    Name = "Защитная каска",
                    Type = "Термо Босс",
                    Unit = "шт.",
                },
                new()
                {
                    Id = 2,
                    Name = "Защитные очки",
                    Type = "TBD",
                    Unit = "шт.",
                },
                new()
                {
                    Id = 3,
                    Name = "Спецодежда",
                    Type = "по сезону",
                    Unit = "шт.",
                },
                new()
                {
                    Id = 4,
                    Name = "Сигнальный жилет",
                    Type = "2-й класс защиты",
                    Unit = "шт.",
                }
            };

        List<Tool> toolList = new List<Tool>()
            {
                new(){
                    Id = 1,
                    Name = "Тахеометр",
                    Type = "TCR 705",
                    Unit = "шт.",},
                new(){
                    Id = 2,
                    Name = "Штатив",
                    Type = "GST05",
                    Unit = "шт.",},
                new(){
                    Id = 3,
                    Name = "Телескопическая веха",
                    Type = "RGK СLS25-DL",
                    Unit = "шт.",},
                new(){
                    Id = 4,
                    Name = "Отражатель",
                    Type = "Mini (D25, К-17.5) SM",
                    Unit = "шт.",},
                new(){
                    Id = 5,
                    Name = "Молоток",
                    Type = "Слесарный",
                    Unit = "шт.",},
                new(){
                    Id = 6,
                    Name = "Стеклопластиковый колышек",
                    Type = "12x1500",
                    Unit = "шт.",},
                new(){
                    Id = 7,
                    Name = "Перманентный маркер",
                    Type = "Черный (3 мм)",
                    Unit = "шт.",},
            };

        //List<Operation> operationList = new List<Operation>()
        //{
        //    new()
        //    {
        //        Id = 1,
        //        Name = "Вынос углов ограждения",
        //        Order = 1,
        //    },
        //};

        tc1.Staff_TCs.Add(new Staff_TC { Child = staffList[0], Symbol = "Г1", Order = 1 });
        tc1.Staff_TCs.Add(new Staff_TC { Child = staffList[0], Symbol = "Г2", Order = 2 });
        tc1.Staff_TCs.Add(new Staff_TC { Child = staffList[1], Symbol = "Г2", Order = 3 }); // todo - ??? if symbol is the same does that mean that it is the same person?

        //tc1.Component_TCs.Add(new Component_TC { Component = componentList[0], Quantity = 1, Order = 1 });

        tc1.Machine_TCs.Add(new Machine_TC { Child = machineList[0], Quantity = 1, Order = 1 });

        tc1.Protection_TCs.Add(new Protection_TC { Child = protectionList[0], Quantity = 2, Order = 1 });
        tc1.Protection_TCs.Add(new Protection_TC { Child = protectionList[1], Quantity = 2, Order = 2 });
        tc1.Protection_TCs.Add(new Protection_TC { Child = protectionList[2], Quantity = 2, Order = 3 });
        tc1.Protection_TCs.Add(new Protection_TC { Child = protectionList[3], Quantity = 2, Order = 4 });

        tc1.Tool_TCs.Add(new Tool_TC { Child = toolList[0], Quantity = 1, Order = 1 });
        tc1.Tool_TCs.Add(new Tool_TC { Child = toolList[1], Quantity = 1, Order = 2 });
        tc1.Tool_TCs.Add(new Tool_TC { Child = toolList[2], Quantity = 1, Order = 3 });
        tc1.Tool_TCs.Add(new Tool_TC { Child = toolList[3], Quantity = 1, Order = 4 });
        tc1.Tool_TCs.Add(new Tool_TC { Child = toolList[4], Quantity = 1, Order = 5 });
        tc1.Tool_TCs.Add(new Tool_TC { Child = toolList[5], Quantity = 4, Order = 6 });
        tc1.Tool_TCs.Add(new Tool_TC { Child = toolList[6], Quantity = 1, Order = 7 });


        List<WorkStep> workSteps = new List<WorkStep>()
            {
                new WorkStep
                {
                    Id = 1,
                    Order = 1,

                    // todo - if we want to use staff symbol in workstep we need to add it before description
                    Description = "- Г1 занести координаты точек улов ограждения в тахеометр (после того как все координаты успешно внесены, на дисплее высветится, сколько метров до данной точки и в градусах, минутах, секундах будет указываться угол поворота до направления на точку);" +
                    "\r\n- Г2 с раздвижной вехой с отражателем переместится на потенциальную точку «1», и поставить веху в приблизительном месте, повернуть отражатель в сторону Г1; " +
                    "\r\n- Г1 навести на отражатель перекрестие зрительной трубы тахеометра и нажать клавишу «РАССТ», измерить расстояние до отражателя, на тахеометре высветится, сколько осталось передвинуть рейку на Г2 или от него, также на дисплее виден угол поворота до проектируемой точки;" +
                    "\r\n- Г1 сообщает о полученных ему данных Г2;" +
                    "\r\n- Г2  удлинить или сократить расстояние до находимой точки, как по расстоянию, так и по углу. Как только достигается точность 0,002 м. по расстоянию и 0°0ґ00Ѕ по углу на месте стояния вехи с отражателем;" +
                    "\r\n- Г2 молотком забить стеклопластиковый колышек, используя молоток; " +
                    "\r\n- Г1 проверяет точку тахеометром, если отклонение есть, то следует подбить колышек так, чтобы устранить отклонение.",

                    StepExecutionTime = 5,
                    Stage = 1,
                    StageExecutionTime = 0.5f,
                    MachineExecutionTime = 0,
                    Protections = "Защитная каска\r\nЗащитные очки\r\nСпецодежда\r\nСигнальный жилет",
                    Comments = "При необходимости установить ограждение"
                },
            };

        workSteps[0].Staff_TCs.Add(tc1.Staff_TCs[0]);
        workSteps[0].Staff_TCs.Add(tc1.Staff_TCs[1]);

        //operationList[0].WorkSteps.AddRange(new List<WorkStep>() { workSteps[0], workSteps[1], workSteps[2], workSteps[3] });
    }

    public static void ReadDB()
    {
        using (var db = new MyDbContext())
        {
            // get all information about technological card with id = 1
            var tc = db.TechnologicalCards
                .Include(tc=>tc.Staff_TCs)
                .Include(tc => tc.Component_TCs)
                .Include(tc => tc.Machine_TCs)
                .Include(tc => tc.Protection_TCs)
                .Include(tc => tc.Tool_TCs)
                .Include(tc => tc.Staffs)
                .Include(tc => tc.Components)
                .Include(tc => tc.Machines)
                .Include(tc => tc.Protections)
                .Include(tc => tc.Tools)
                .FirstOrDefault(tc => tc.Id == 1);
            // print in console information about staff and his Symbol in TC
            Console.WriteLine($"Staff_TCs contains in TC: {tc.Staff_TCs.Count}");

            foreach (var staff in tc.Staff_TCs)
            {
                Console.WriteLine($"{staff.Order}.{staff.Child.Name} - {staff.Child.Type} - {staff.Child.CombineResponsibility} - {staff.Child.Qualification} - {staff.Symbol}");
            }

            Console.WriteLine($"Component_TCs contains in TC: {tc.Component_TCs.Count}");

            foreach (var st in tc.Component_TCs)
            {
                Console.WriteLine($"{st.Order}.{st.Child.Name} - {st.Child.Type}  - {st.Quantity}{st.Child.Unit}");
            }
            Console.WriteLine($"Machine_TCs contains in TC: {tc.Machine_TCs.Count}");

            foreach (var st in tc.Machine_TCs)
            {
                Console.WriteLine($"{st.Order}.{st.Child.Name} - {st.Child.Type}  - {st.Quantity}{st.Child.Unit}");
            }
            Console.WriteLine($"Protection_TCs contains in TC: {tc.Protection_TCs.Count}");

            foreach (var st in tc.Protection_TCs)
            {
                Console.WriteLine($"{st.Order}.{st.Child.Name} - {st.Child.Type}   -  {st.Quantity} {st.Child.Unit}");
            }
            Console.WriteLine($"Tool_TCs contains in TC: {tc.Tool_TCs.Count}");

            foreach (var st in tc.Tool_TCs)
            {
                Console.WriteLine($"{st.Order}.{st.Child.Name} - {st.Child.Type}  - {st.Quantity}{st.Child.Unit}");
            }
        }
    }
    public static void ParserTest()
    {
        Dictionary<EModelType?, int> modelStartRows = new();
        Dictionary<EModelType?, int> modelEndRows = new();

        Dictionary<EModelType, List<List<string>>> datastore = new();
        Dictionary<string, EModelType> keyValuePairs = new()
        {
            { "1. Требования к составу бригады и квалификации", EModelType.Staff },
            { "2. Требования к материалам и комплектующим", EModelType.Component },
            { "3. Требования к механизмам", EModelType.Machine },
            { "4. Требования к средствам защиты", EModelType.Protection },
            { "5. Требования к инструментам и приспособлениям", EModelType.Tool },
            //{ "6. Выполнение работ", EModelType.WorkStep }
        };
        Dictionary<EModelType, List<string>> columnNames = new()
            {
            { EModelType.Staff, new List<string>
                { "№", "Наименование", "Тип (исполнение)", "Возможность совмещения обязанностей",
                /*"Группа ЭБ, не ниже", "Разряд,\r\nне ниже",*/ "Квалификация","Обозначение в ТК"} },
            { EModelType.Component, new List<string>
                { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во", "Стоимость, руб. без НДС" } },
            { EModelType.Machine, new List<string>
                { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во"/*, "Стоимость, руб. без НДС"*/ } },
            { EModelType.Protection, new List<string>
                { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во"/*, "Стоимость, руб. без НДС"*/ } },
            { EModelType.Tool, new List<string>
                { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во"/*, "Стоимость, руб. без НДС"*/ } },
            //{ EModelType.WorkStep, new List<string> {"Наименование", "Версия", "Описание" } }
        };
        string filepath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\Пример\ТК_ТТ_v4.0_Уфа.xlsx";
        var parser = new ExcelParser();
        parser.FindTableBorderRows(keyValuePairs, filepath, out modelStartRows, out modelEndRows, sheetName: "ТК_1.1");

        // create a loop for each model type in enum EModelType
        foreach (EModelType model in Enum.GetValues(typeof(EModelType)))
        {
            // create a list of lists of strings for each model type
            if (model == EModelType.WorkStep) continue;
            Console.WriteLine($"{model.ToString()} - {modelStartRows[model]} - {modelEndRows[model]}");
            datastore.Add(model, parser.ParseRowsToStrings(columnNames[model], filepath,
                sheetName: "ТК_1.1", startRow: modelStartRows[model], endRow: modelEndRows[model]));
            foreach (var row in datastore[model])
            {
                foreach (var cell in row)
                {
                    // todo - ERROR last element in table exept
                    Console.Write($"{cell} ");
                }
                Console.WriteLine();
            }
        }
    }
}