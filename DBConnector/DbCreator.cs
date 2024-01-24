using Microsoft.EntityFrameworkCore;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models;
using ExcelParsing.DataProcessing;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace TcDbConnector
{
    public class DbCreator
    {
        public static void SerializeAllObjects()
        {
            string filePath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка структур.xlsx";
            string tcFilePath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Список ТК.xlsx";
            string intObjFilePath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка промежуточных сущностей.xlsx";

            ExcelParser excelParser = new ExcelParser();

            List<Staff> staffList = excelParser.ParseExcelToStaffObjects(filePath);
            List<Component> componentList = excelParser.ParseExcelToObjectsComponent(filePath);
            List<Machine> machineList = excelParser.ParseExcelToObjectsMachine(filePath);
            List<Protection> protectionList = excelParser.ParseExcelToObjectsProtection(filePath);
            List<Tool> toolList = excelParser.ParseExcelToObjectsTool(filePath);

            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\staffList.json", JsonConvert.SerializeObject(staffList, Formatting.Indented));
            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\componentList.json", JsonConvert.SerializeObject(componentList, Formatting.Indented));
            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\machineList.json", JsonConvert.SerializeObject(machineList, Formatting.Indented));
            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\protectionList.json", JsonConvert.SerializeObject(protectionList, Formatting.Indented));
            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\toolList.json", JsonConvert.SerializeObject(toolList, Formatting.Indented));

            List<TechnologicalCard> tcList = excelParser.ParseExcelToTcObjects(tcFilePath);

            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\tcList.json", JsonConvert.SerializeObject(tcList, Formatting.Indented));

            SerializeStaffTc(intObjFilePath);
            SerializeComponentTc(intObjFilePath);
            SerializeMachineTc(intObjFilePath);
            SerializeProtectionTc(intObjFilePath);
            SerializeToolTc(intObjFilePath);

        }

        public static void AddDeserializedDataToDb(string path= @"C:\Users\bokar\source\TC_Viewer")
        {
            //deserialize all objects from json
            string jsonData = File.ReadAllText($@"{path}\Serialised data\staffList.json");
            List<Staff> staffList = JsonConvert.DeserializeObject<List<Staff>>(jsonData);
            jsonData = File.ReadAllText($@"{path}\Serialised data\ComponentList.json");
            List<Component> componentList = JsonConvert.DeserializeObject<List<Component>>(jsonData);
            jsonData = File.ReadAllText($@"{path}\Serialised data\MachineList.json");
            List<Machine> machineList = JsonConvert.DeserializeObject<List<Machine>>(jsonData);
            jsonData = File.ReadAllText($@"{path}\Serialised data\ProtectionList.json");
            List<Protection> protectionList = JsonConvert.DeserializeObject<List<Protection>>(jsonData);
            jsonData = File.ReadAllText($@"{path}\Serialised data\ToolList.json");
            List<Tool> toolList = JsonConvert.DeserializeObject<List<Tool>>(jsonData);

            //add all objects to new db
            using (var db = new MyDbContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.Staffs.AddRange(staffList);
                db.Components.AddRange(componentList);
                db.Machines.AddRange(machineList);
                db.Protections.AddRange(protectionList);
                db.Tools.AddRange(toolList);

                db.SaveChanges();
            }

            DeserializeTcToDb();

            DeserializeStaffTcToDb();
            DeserializeComponentTcToDb();
            DeserializeMachineTcToDb();
            DeserializeProtectionTcToDb();
            DeserializeToolTcToDb();

        }

        private static void DeserializeTcToDb()
        {
            string jsonData = File.ReadAllText($@"C:\Users\bokar\source\TC_Viewer\Serialised data\tcList.json");
            var tcList = JsonConvert.DeserializeObject<List<TechnologicalCard>>(jsonData);

            using (var db = new MyDbContext())
            {

                db.TechnologicalCards.AddRange(tcList);
                db.SaveChanges();
            }
        }

        private static void DeserializeStaffTcToDb()
        {
            string jsonData = File.ReadAllText($@"C:\Users\bokar\source\TC_Viewer\Serialised data\staffTcList.json");
            var objTcList = JsonConvert.DeserializeObject<List<Staff_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                foreach (var objTc in objTcList)
                {
                    Console.WriteLine($"entety: Parentid={objTc.ParentId}, ChieldId={objTc.ChildId}, Order={objTc.Order}");
                    var objs = db.Staffs.Find(objTc.ChildId);
                    var tc = db.TechnologicalCards.Find(objTc.ParentId);
                    if (objs != null && tc != null)
                    {
                        objTc.Child = objs;
                        objTc.Parent = tc;
                        db.Staff_TCs.Add(objTc); // todo - something wrond with adding to db
                    }
                }
                db.Staff_TCs.AddRange(objTcList);
                db.SaveChanges();
            }
        }
        private static void DeserializeComponentTcToDb()
        {
            string jsonData = File.ReadAllText($@"C:\Users\bokar\source\TC_Viewer\Serialised data\toolTcList.json");
            var objTcList = JsonConvert.DeserializeObject<List<Component_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                foreach (var objTc in objTcList)
                {
                    Console.WriteLine($"entety: Parentid={objTc.ParentId}, ChieldId={objTc.ChildId}, Order={objTc.Order}");
                    var objs = db.Components.Find(objTc.ChildId);
                    var tc = db.TechnologicalCards.Find(objTc.ParentId);
                    if (objs != null && tc != null)
                    {
                        objTc.Child = objs;
                        objTc.Parent = tc;
                        db.Component_TCs.Add(objTc); // todo - something wrond with adding to db
                    }
                }
                db.Component_TCs.AddRange(objTcList);
                db.SaveChanges();
            }
        }
        private static void DeserializeMachineTcToDb()
        {
            string jsonData = File.ReadAllText($@"C:\Users\bokar\source\TC_Viewer\Serialised data\machineTcList.json");
            var objTcList = JsonConvert.DeserializeObject<List<Machine_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                foreach (var objTc in objTcList)
                {
                    Console.WriteLine($"entety: Parentid={objTc.ParentId}, ChieldId={objTc.ChildId}, Order={objTc.Order}");
                    var objs = db.Machines.Find(objTc.ChildId);
                    var tc = db.TechnologicalCards.Find(objTc.ParentId);
                    if (objs != null && tc != null)
                    {
                        objTc.Child = objs;
                        objTc.Parent = tc;
                        db.Machine_TCs.Add(objTc); // todo - something wrond with adding to db
                    }
                }
                db.Machine_TCs.AddRange(objTcList);
                db.SaveChanges();
            }
        }
        private static void DeserializeProtectionTcToDb()
        {
            string jsonData = File.ReadAllText($@"C:\Users\bokar\source\TC_Viewer\Serialised data\protectionTcList.json");
            var objTcList = JsonConvert.DeserializeObject<List<Protection_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                foreach (var objTc in objTcList)
                {
                    Console.WriteLine($"entety: Parentid={objTc.ParentId}, ChieldId={objTc.ChildId}, Order={objTc.Order}");
                    var objs = db.Protections.Find(objTc.ChildId);
                    var tc = db.TechnologicalCards.Find(objTc.ParentId);
                    if (objs != null && tc != null)
                    {
                        objTc.Child = objs;
                        objTc.Parent = tc;
                        db.Protection_TCs.Add(objTc); // todo - something wrond with adding to db
                    }
                }
                db.Protection_TCs.AddRange(objTcList);
                db.SaveChanges();
            }
        }
        private static void DeserializeToolTcToDb()
        {
            string jsonData = File.ReadAllText($@"C:\Users\bokar\source\TC_Viewer\Serialised data\toolTcList.json");
            var toolTcList = JsonConvert.DeserializeObject<List<Tool_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                foreach (var toolTc in toolTcList)
                {
                    Console.WriteLine($"entety: Parentid={toolTc.ParentId}, ChieldId={toolTc.ChildId}, Order={toolTc.Order}");
                    var tool = db.Tools.Find(toolTc.ChildId);
                    var tc = db.TechnologicalCards.Find(toolTc.ParentId);
                    if (tool != null && tc != null)
                    {
                        toolTc.Child = tool;
                        toolTc.Parent = tc;
                        db.Tool_TCs.Add(toolTc);
                    }
                }
                db.Tool_TCs.AddRange(toolTcList);
                db.SaveChanges();
            }
        }

        private static void SerializeComponent(string filePath)
        {
            ExcelParser excelParser = new ExcelParser();
            var componentList = excelParser.ParseExcelToObjectsComponent(filePath);
            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\componentList.json", JsonConvert.SerializeObject(componentList, Formatting.Indented));
        }

        private static void SerializeStaffTc(string filePath)
        {
            ExcelParser excelParser = new ExcelParser();
            var objTcList = excelParser.ParseExcelToStaffTcObjects(filePath, out _);

            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\staffTcList.json", JsonConvert.SerializeObject(objTcList, Formatting.Indented));
        }
        private static void SerializeComponentTc(string filePath)
        {
            ExcelParser excelParser = new ExcelParser();
            var componentTcList = excelParser.ParseExcelToIntermediateStructObjects<Component_TC, Component>(filePath, out _);

            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\componentTcList.json", JsonConvert.SerializeObject(componentTcList, Formatting.Indented));
        }
        private static void SerializeMachineTc(string filePath)
        {
            ExcelParser excelParser = new ExcelParser();
            var objTcList = excelParser.ParseExcelToIntermediateStructObjects<Machine_TC, Machine>(filePath, out _);

            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\machineTcList.json", JsonConvert.SerializeObject(objTcList, Formatting.Indented));
        }
        private static void SerializeProtectionTc(string filePath)
        {
            ExcelParser excelParser = new ExcelParser();
            var objTcList = excelParser.ParseExcelToIntermediateStructObjects<Protection_TC, Protection>(filePath, out _);

            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\protectionTcList.json", JsonConvert.SerializeObject(objTcList, Formatting.Indented));
        }
        private static void SerializeToolTc(string filePath)
        {
            ExcelParser excelParser = new ExcelParser();
            var objTcList = excelParser.ParseExcelToIntermediateStructObjects<Tool_TC, Tool>(filePath, out _);

            File.WriteAllText(@"C:\Users\bokar\source\TC_Viewer\Serialised data\toolTcList.json", JsonConvert.SerializeObject(objTcList, Formatting.Indented));
        }
    }
}
