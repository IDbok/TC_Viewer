
using ExcelParsing.DataProcessing;
using Newtonsoft.Json;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace ExcelParsing
{
    public class DbCreatorParser
    {
        public static void ParseDictionaty(string structFilePath,string folderToSaveJson)
        {
            string filepath = structFilePath; //@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка структур.xlsx";

            var parser = new ExcelParser();

            var parsedDataStaff = parser.ParseExcelToObjectsStaff(filepath);
            var parsedDataComponent = parser.ParseExcelToObjectsComponent(filepath);
            var parsedDataMachine = parser.ParseExcelToObjectsMachine(filepath);
            var parsedDataProtection = parser.ParseExcelToObjectsProtection(filepath);
            var parsedDataTool = parser.ParseExcelToObjectsTool(filepath);

            var jsonStaff = JsonConvert.SerializeObject(parsedDataStaff, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Staff.json", jsonStaff);

            var jsonComponent = JsonConvert.SerializeObject(parsedDataComponent, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Component.json", jsonComponent);

            var jsonMachine = JsonConvert.SerializeObject(parsedDataMachine, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Machine.json", jsonMachine);

            var jsonProtection = JsonConvert.SerializeObject(parsedDataProtection, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Protection.json", jsonProtection);

            var jsonTool = JsonConvert.SerializeObject(parsedDataTool, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Tool.json", jsonTool);

        }

        public static void ParseTechnologicalCard(string tcFilePath, string folderToSaveJson)
        {
            string filepath = tcFilePath; //@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка структур.xlsx";

            var parser = new ExcelParser();

            var parsedDataTechnologicalCard = parser.ParseExcelToObjectsTc(filepath);

            var jsonTechnologicalCard = JsonConvert.SerializeObject(parsedDataTechnologicalCard, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "TechnologicalCard.json", jsonTechnologicalCard);
        }

        public static void ParseIntermediateObjects(string intermediateDictionaryFilePath,  string folderToSaveJson)
        {
            string filepath = intermediateDictionaryFilePath; //@""C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка промежуточных сущностей.xlsx"";

            var parser = new ExcelParser();

            var parsedDataStaff = parser.ParseExcelToObjectsStaff_TC(filepath, out _ );
            var parsedDataComponent = parser.ParseExcelToIntermediateStructObjects<Component_TC,Component>(filepath, out _);
            var parsedDataMachine = parser.ParseExcelToIntermediateStructObjects<Machine_TC, Machine>(filepath, out _);
            var parsedDataProtection = parser.ParseExcelToIntermediateStructObjects<Protection_TC, Protection>(filepath, out _);
            var parsedDataTool = parser.ParseExcelToIntermediateStructObjects<Tool_TC, Tool>(filepath, out _);

            var jsonStaff = JsonConvert.SerializeObject(parsedDataStaff, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Staff_TC.json", jsonStaff);

            var jsonComponent = JsonConvert.SerializeObject(parsedDataComponent, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Component_TC.json", jsonComponent);

            var jsonMachine = JsonConvert.SerializeObject(parsedDataMachine, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Machine_TC.json", jsonMachine);

            var jsonProtection = JsonConvert.SerializeObject(parsedDataProtection, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Protection_TC.json", jsonProtection);

            var jsonTool = JsonConvert.SerializeObject(parsedDataTool, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "Tool_TC.json", jsonTool);
        }

        public static void ParseWorkSteps(string workStepsFilePath, string folderToSaveJson)
        {
            string filepath = workStepsFilePath; //@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка структур.xlsx";
            string sheetName = "WorkStep_TC and Tools";

            var parser = new WorkParser();

            var parsedDataWorkSteps = parser.ParseExcelToObjectsTechOperationWork(filepath, sheetName);

            var jsonWorkSteps = JsonConvert.SerializeObject(parsedDataWorkSteps, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "WorkSteps.json", jsonWorkSteps);
        }
    }
}
