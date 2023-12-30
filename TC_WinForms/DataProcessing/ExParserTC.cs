using OfficeOpenXml;
using System.Text.Json;
using System.Text.RegularExpressions;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.DataProcessing
{
    internal static class ExParserTC
    {

        internal delegate void MessageHandler(string message);
        static MessageHandler? msg;
        internal static void RegisterMessageHandler (MessageHandler? messegeOutMethod)  { msg = messegeOutMethod;}
        internal static void RegisterFilePath(string filepath) { ExParserTC.filepath = filepath; }
        internal static void RegisterJsonCatalog(string jsonCatalog) { ExParserTC.jsonCatalog = jsonCatalog; }
        static List<string> listCardName = new();
        internal static void RegisterCardNameToParse(List<string> listCardName) { ExParserTC.listCardName = listCardName; }

        static string filepath;
        static string jsonCatalog;

        static Dictionary<string, EModelType> keyValuePairs = new()
        {
            { "1. Требования к составу бригады и квалификации", EModelType.Staff },
            { "2. Требования к материалам и комплектующим", EModelType.Component },
            { "3. Требования к механизмам", EModelType.Machine },
            { "4. Требования к средствам защиты", EModelType.Protection },
            { "5. Требования к инструментам и приспособлениям", EModelType.Tool },
            { "6. Выполнение работ", EModelType.WorkStep }
        };

        public static void DoMain()
        {
            msg?.Invoke("Выполняется функционал консольного парсинга");
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            try
            {
                // Создаю объект для работы с Excel
                using (var package = new ExcelPackage(new FileInfo(filepath)))
                {

                    var mapper = new FromExelToObjectMapper();
                    foreach (string sheetName in listCardName)
                    {
                        var worksheet = package.Workbook.Worksheets[sheetName];

                        var parsedData = mapper.mapFrom(keyValuePairs, worksheet);

                        foreach (EModelType modelType in parsedData.Keys)
                        {
                            string tableName = keyValuePairs.FirstOrDefault(x => x.Value == modelType).Key;
                            try
                            {
                                SaveToJSON(parsedData[modelType], modelType.ToString(), sheetName);
                            }
                            catch (Exception ex)
                            {
                                msg?.Invoke($"Ошибка при парсинге таблицы {tableName}:\n{ex.Message}");
                            }
                        }
                        msg?.Invoke($"Парсиг карты на листе {sheetName} законцен!");
                    }
                }
            }
            catch (Exception ex)
            {
                msg?.Invoke($"Произошла ошибка: {ex.Message}");
            }
            Console.ReadLine();
        }
        
        public static void SaveToJSON(List<IModelStructure> ListOfModels, string modelName, string sheetName)
        {
            if (ListOfModels.Count != 0)
            {
                foreach (var item in ListOfModels)
                {
                    // Создание объекта для записи без кодировки в Unicode
                    var options = new JsonSerializerOptions
                    {
                        // set the encoder to UnicodeEncoding
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    string? json = null;
                    if (item is Staff staff)
                    { json = JsonSerializer.Serialize(staff, options); }
                    else if (item is Component component)
                    { json = JsonSerializer.Serialize(component, options); }
                    else if (item is Machine machine)
                    { json = JsonSerializer.Serialize(machine, options); }
                    else if (item is Protection protection)
                    { json = JsonSerializer.Serialize(protection, options); }
                    else if (item is Tool tool)
                    { json = JsonSerializer.Serialize(tool, options); }
                    else if (item is WorkStep workStep)
                    { json = JsonSerializer.Serialize(workStep, options); }
                    else
                    { msg?.Invoke($"Ошибка при сохранении в JSON структуры {item.GetType}:" +
                                $"\nнеизвестный тип структуры данных"); }

                    string pathJson = $@"{jsonCatalog}\{sheetName}\{modelName}\";
                    if (!Directory.Exists(pathJson)) Directory.CreateDirectory(pathJson);
                    //TODO: cleane folder before saving
                    // File.WriteAllText(pathJson + item.Num + ".json", json);
                }
            }
        }
    }
}
