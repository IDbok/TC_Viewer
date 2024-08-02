
using ExcelParsing.DataProcessing;
using Newtonsoft.Json;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace ExcelParsing
{
    public class DbCreatorParser
    {
        //public static void ParseAll(string structFilePath, string tcFilePath, string intermediateDictionaryFilePath, string workStepsFilePath, string folderToSaveJson = null)
        //{
        //    if (string.IsNullOrEmpty(structFilePath) || string.IsNullOrEmpty(tcFilePath) || string.IsNullOrEmpty(intermediateDictionaryFilePath) || string.IsNullOrEmpty(workStepsFilePath))
        //    {
        //        throw new ArgumentException("One or more paths are invalid");
        //    }
        //    if (!File.Exists(structFilePath) || !File.Exists(tcFilePath) || !File.Exists(intermediateDictionaryFilePath) || !File.Exists(workStepsFilePath))
        //    {
        //        throw new ArgumentException("One or more files do not exist");
        //    }

        //    ParseDictionaries(structFilePath, folderToSaveJson);
        //    ParseTechnologicalCard(tcFilePath, folderToSaveJson);
        //    ParseWorkDictionaries(workStepsFilePath, folderToSaveJson);

        //    // ParseIntermediateObjects(intermediateDictionaryFilePath, folderToSaveJson);


        //    // create new DB to parsed data
        //    //CreateDb();

        //    DbCreator.AddDeserializedDataToDb();

        //    // ParseWorkSteps(workStepsFilePath, folderToSaveJson);
        //}

        public static void ParseTCWorkSteps(string tcFilePath, string tcName)
        {
            var parser = new WorkParser();

            var note = string.Empty;

            parser.ParseTcWorkSteps(tcFilePath, tcName, ref note );
        }

        private static void CreateDb()
        {
            //TcDbConnector.StaticClass.ConnectString = "server=localhost;database=tavrida_db_v11;user=root;password=root";

            using (var db = new MyDbContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
        }
        //public static void ParseDictionaries(string structFilePath,string folderToSaveJson = null)
        //{
        //    string filepath = structFilePath; //@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка структур.xlsx";

        //    var parser = new ExcelParser();

        //    var parsedDataStaff = parser.ParseExcelToObjectsStaff(filepath);
        //    var parsedDataComponent = parser.ParseExcelToObjectsComponent(filepath);
        //    var parsedDataMachine = parser.ParseExcelToObjectsMachine(filepath);
        //    var parsedDataProtection = parser.ParseExcelToObjectsProtection(filepath);
        //    var parsedDataTool = parser.ParseExcelToObjectsTool(filepath);

        //    var jsonStaff = JsonConvert.SerializeObject(parsedDataStaff, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Staff.json", jsonStaff);

        //    var jsonComponent = JsonConvert.SerializeObject(parsedDataComponent, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Component.json", jsonComponent);

        //    var jsonMachine = JsonConvert.SerializeObject(parsedDataMachine, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Machine.json", jsonMachine);

        //    var jsonProtection = JsonConvert.SerializeObject(parsedDataProtection, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Protection.json", jsonProtection);

        //    var jsonTool = JsonConvert.SerializeObject(parsedDataTool, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Tool.json", jsonTool);


        //}
        public static void ParseWorkDictionaries(string filePath, string folderToSaveJson = null)
        {
            var workParser = new WorkParser();

            var parsedDataTechOperation = workParser.ParseExcelToObjectsTechOperation(filePath);
            var jsonTechOperation = JsonConvert.SerializeObject(parsedDataTechOperation, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "TechOperation.json", jsonTechOperation);

            var parsedDataTechTransition = workParser.ParseExcelToObjectsTechTransition(filePath);
            var jsonTechTransition = JsonConvert.SerializeObject(parsedDataTechTransition, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "TechTransition.json", jsonTechTransition);
        }

        public static void ParseTechnologicalCard(string tcFilePath, string folderToSaveJson = null)
        {
            string filepath = tcFilePath; //@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка структур.xlsx";

            var parser = new ExcelParser();

            var parsedDataTechnologicalCard = parser.ParseExcelToObjectsTc(filepath);

            var jsonTechnologicalCard = JsonConvert.SerializeObject(parsedDataTechnologicalCard, Formatting.Indented);
            File.WriteAllText(folderToSaveJson + "TechnologicalCard.json", jsonTechnologicalCard);
        }

        //public static void ParseIntermediateObjects(string intermediateDictionaryFilePath,  string folderToSaveJson = null)
        //{
        //    string filepath = intermediateDictionaryFilePath; //@""C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка промежуточных сущностей.xlsx"";

        //    var parser = new ExcelParser();

        //    var parsedDataStaff = parser.ParseExcelToObjectsStaff_TC(filepath, out _ );
        //    var parsedDataComponent = parser.ParseExcelToIntermediateStructObjects<Component_TC,Component>(filepath, out _);
        //    var parsedDataMachine = parser.ParseExcelToIntermediateStructObjects<Machine_TC, Machine>(filepath, out _);
        //    var parsedDataProtection = parser.ParseExcelToIntermediateStructObjects<Protection_TC, Protection>(filepath, out _);
        //    var parsedDataTool = parser.ParseExcelToIntermediateStructObjects<Tool_TC, Tool>(filepath, out _);

        //    var jsonStaff = JsonConvert.SerializeObject(parsedDataStaff, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Staff_TC.json", jsonStaff);

        //    var jsonComponent = JsonConvert.SerializeObject(parsedDataComponent, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Component_TC.json", jsonComponent);

        //    var jsonMachine = JsonConvert.SerializeObject(parsedDataMachine, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Machine_TC.json", jsonMachine);

        //    var jsonProtection = JsonConvert.SerializeObject(parsedDataProtection, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Protection_TC.json", jsonProtection);

        //    var jsonTool = JsonConvert.SerializeObject(parsedDataTool, Formatting.Indented);
        //    File.WriteAllText(folderToSaveJson + "Tool_TC.json", jsonTool);
        //}

        public static void ParseWorkSteps(string workStepsFilePath, string folderToSaveJson = null)
        {
            string filepath = workStepsFilePath; //@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка структур.xlsx";
            string sheetName = "WorkStep_TC and Tools";

            var parser = new WorkParser();

            var parsedDataWorkSteps = parser.ParseExcelToObjectsTechOperationWork(filepath, sheetName);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var jsonWorkSteps = JsonConvert.SerializeObject(parsedDataWorkSteps, Formatting.Indented,settings);
            File.WriteAllText(folderToSaveJson + "WorkSteps.json", jsonWorkSteps);
        }
    }
}
