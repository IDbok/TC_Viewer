using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TC_WinForms.DataProcessing
{
    internal static class FileManager
    {
        // TODO - create an delegate to print messages
        internal delegate void MessageHandler(string message);
        static MessageHandler? PrintMessage;
        internal static void RegisterMessageHandler(MessageHandler? messegeOutMethod) { PrintMessage = messegeOutMethod; }
        public static List<string> GetSheetsNamesWithTC(string filepath)
        {
            List<string> sheetsTC = new List<string>();
            if (File.Exists(filepath))
            {
                try
                {
                    sheetsTC = GetSheetsNames(filepath, "ТК_");
                }
                catch (System.IO.IOException ex)
                {
                    PrintMessage("Произошла ошибка. Проверьте закрыт ли обрабатываемый файл.\n(" + ex + ")");
                }
                catch (Exception) { PrintMessage("Произошла ошибка при открытии файла."); }
            }
            else PrintMessage("Файл по указаному пути не найден!");
            return sheetsTC;
        }
        public static List<string> GetSheetsNames(string filepath, string? nameContains = null )
        {
            List<string> sheetsTK = new();

            using (var package = new ExcelPackage(new FileInfo(filepath)))
            {
                foreach (var ws in package.Workbook.Worksheets)
                {
                    // if nameContains is null then return all sheets
                    if (nameContains == null ? true : ws.Name.ToString().Contains(nameContains)) 
                        sheetsTK.Add(ws.Name);
                }
            }
            return sheetsTK;
        }

        public static void SaveToJSON(object obj, string filename, string sheetName, string jsonCatalog)
        {
            string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            string path = $@"{jsonCatalog}\{sheetName}_{filename}.json";
            System.IO.File.WriteAllText(path, json);
        }
        public static bool CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    // Create and delete the directory to check the permissions
                    var testDir = Directory.CreateDirectory(path);
                    path = testDir.FullName;
                    Directory.Delete(path);
                }
                catch (UnauthorizedAccessException)
                {
                    PrintMessage("Нет разрешения на создание директории.");
                    return false;
                }
                catch (Exception ex)
                {
                    PrintMessage($"Произошла ошибка: {ex.Message}");
                    return false;
                }
            }
            return true;
        }
    }
}
