using Microsoft.EntityFrameworkCore;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models;
using Newtonsoft.Json;

namespace TcDbConnector
{
    public class DbCreator
    {

        private static string _path;
        public DbCreator()
        {
            SetPath();
        }
        public static void SetPath()
        {
            string cD = Directory.GetCurrentDirectory();
            string RealPath = Path.GetFullPath(Path.Combine(cD, @"..\..\..\..\"));
            _path = RealPath;
        }

        //public static void SerializeAllObjects(string filePath, string tcFilePath, string intObjFilePath)
        //{
        //    //string filePath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка структур.xlsx";
        //    //string tcFilePath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Список ТК.xlsx";
        //    //string intObjFilePath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\0. Обработка промежуточных сущностей.xlsx";

        //    ExcelParser excelParser = new ExcelParser();

        //    List<Staff> staffList = excelParser.ParseExcelToStaffObjects(filePath);
        //    List<Component> componentList = excelParser.ParseExcelToObjectsComponent(filePath);
        //    List<Machine> machineList = excelParser.ParseExcelToObjectsMachine(filePath);
        //    List<Protection> protectionList = excelParser.ParseExcelToObjectsProtection(filePath);
        //    List<Tool> toolList = excelParser.ParseExcelToObjectsTool(filePath);

        //    File.WriteAllText($@"{_path}\Serialised data\staffList.json", JsonConvert.SerializeObject(staffList, Formatting.Indented));
        //    File.WriteAllText($@"{_path}\Serialised data\componentList.json", JsonConvert.SerializeObject(componentList, Formatting.Indented));
        //    File.WriteAllText($@"{_path}\Serialised data\machineList.json", JsonConvert.SerializeObject(machineList, Formatting.Indented));
        //    File.WriteAllText($@"{_path}\Serialised data\protectionList.json", JsonConvert.SerializeObject(protectionList, Formatting.Indented));
        //    File.WriteAllText($@"{_path}\Serialised data\toolList.json", JsonConvert.SerializeObject(toolList, Formatting.Indented));

        //    List<TechnologicalCard> tcList = excelParser.ParseExcelToTcObjects(tcFilePath);

        //    File.WriteAllText($@"{_path}\Serialised data\tcList.json", JsonConvert.SerializeObject(tcList, Formatting.Indented));

        //    SerializeStaffTc(intObjFilePath);
        //    SerializeComponentTc(intObjFilePath);
        //    SerializeMachineTc(intObjFilePath);
        //    SerializeProtectionTc(intObjFilePath);
        //    SerializeToolTc(intObjFilePath);

        //}

        public static void AddDeserializedDataToDb(string pathToJsonFolder = null)
        {
            //deserialize all objects from json

            string jsonData = File.ReadAllText($@"{pathToJsonFolder}Staff.json");
            List<Staff> staffList = JsonConvert.DeserializeObject<List<Staff>>(jsonData);
            jsonData = File.ReadAllText($@"{pathToJsonFolder}Component.json");
            List<Component> componentList = JsonConvert.DeserializeObject<List<Component>>(jsonData);
            jsonData = File.ReadAllText($@"{pathToJsonFolder}Machine.json");
            List<Machine> machineList = JsonConvert.DeserializeObject<List<Machine>>(jsonData);
            jsonData = File.ReadAllText($@"{pathToJsonFolder}Protection.json");
            List<Protection> protectionList = JsonConvert.DeserializeObject<List<Protection>>(jsonData);
            jsonData = File.ReadAllText($@"{pathToJsonFolder}Tool.json");
            List<Tool> toolList = JsonConvert.DeserializeObject<List<Tool>>(jsonData);

            jsonData = File.ReadAllText($@"{pathToJsonFolder}TechOperation.json");
            List<TechOperation> TOList = JsonConvert.DeserializeObject<List<TechOperation>>(jsonData);

            jsonData = File.ReadAllText($@"{pathToJsonFolder}TechTransition.json");
            List<TechTransition> TPList = JsonConvert.DeserializeObject<List<TechTransition>>(jsonData);

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

                db.TechOperations.AddRange(TOList);
                db.TechTransitions.AddRange(TPList);

                db.SaveChanges();
            }

            DeserializeTcToDb();

            DeserializeStaffTcToDb();
            DeserializeComponentTcToDb();
            DeserializeMachineTcToDb();
            DeserializeProtectionTcToDb();
            DeserializeToolTcToDb();

            //DeserializeWorkStepsToDb();

        }

        private static void DeserializeTcToDb(string pathToJsonFolder = null)
        {
            string jsonData = File.ReadAllText($@"{pathToJsonFolder}TechnologicalCard.json");
            var tcList = JsonConvert.DeserializeObject<List<TechnologicalCard>>(jsonData);

            using (var db = new MyDbContext())
            {

                db.TechnologicalCards.AddRange(tcList);
                db.SaveChanges();
            }
        }

        private static void DeserializeStaffTcToDb(string pathToJsonFolder = null)
        {
            string jsonData = File.ReadAllText($@"{pathToJsonFolder}Staff_TC.json");
            var objTcList = JsonConvert.DeserializeObject<List<Staff_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                Console.WriteLine("----------------- Staff_TC -----------------");
                foreach (var objTc in objTcList)
                {
                    // Console.WriteLine($"entety: Parentid={objTc.ParentId}, ChieldId={objTc.ChildId}, Order={objTc.Order}");
                    var objs = db.Staffs.Find(objTc.ChildId);
                    var tc = db.TechnologicalCards.Find(objTc.ParentId);
                    if (objs != null && tc != null)
                    {
                        objTc.Child = objs;
                        objTc.Parent = tc;
                        db.Staff_TCs.Add(objTc);
                    }
                }
                db.Staff_TCs.AddRange(objTcList);
                db.SaveChanges();
            }
        }
        private static void DeserializeComponentTcToDb(string pathToJsonFolder = null)
        {
            string jsonData = File.ReadAllText($@"{pathToJsonFolder}Component_TC.json");
            var objTcList = JsonConvert.DeserializeObject<List<Component_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                Console.WriteLine("----------------- Component_TC -----------------");
                List<Component_TC> itemsToRemove = new List<Component_TC>();
                foreach (var objTc in objTcList)
                {

                    var objs = db.Components.Find(objTc.ChildId);
                    var tc = db.TechnologicalCards.Find(objTc.ParentId);
                    if (objs != null && tc != null)
                    {
                        objTc.Child = objs;
                        objTc.Parent = tc;
                        try { db.Component_TCs.Add(objTc);/* Console.WriteLine($"entety: Parentid={objTc.ParentId}, ChieldId={objTc.ChildId}, Order={objTc.Order}");*/ }
                        catch { itemsToRemove.Add(objTc); }
                        // db.Component_TCs.Add(objTc); // todo - something wrond with adding to db
                    }
                }
                foreach (var item in itemsToRemove)
                {

                    objTcList.Remove(item);
                    db.Entry(item).State = EntityState.Detached;
                    //Console.WriteLine($"entety Deleted: Parentid={item.ParentId}, ChieldId={item.ChildId}, Order={item.Order}");
                }
                db.Component_TCs.AddRange(objTcList);
                db.SaveChanges();
            }
        }
        private static void DeserializeMachineTcToDb(string pathToJsonFolder = null)
        {
            string jsonData = File.ReadAllText($@"{pathToJsonFolder}Machine_TC.json");
            var objTcList = JsonConvert.DeserializeObject<List<Machine_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                Console.WriteLine("----------------- Machine_TC -----------------");
                foreach (var objTc in objTcList)
                {
                    //Console.WriteLine($"entety: Parentid={objTc.ParentId}, ChieldId={objTc.ChildId}, Order={objTc.Order}");
                    var objs = db.Machines.Find(objTc.ChildId);
                    var tc = db.TechnologicalCards.Find(objTc.ParentId);
                    if (objs != null && tc != null)
                    {
                        objTc.Child = objs;
                        objTc.Parent = tc;
                        db.Machine_TCs.Add(objTc);
                    }
                }
                db.Machine_TCs.AddRange(objTcList);
                db.SaveChanges();
            }
        }

        private static void DeserializeProtectionTcToDb(string pathToJsonFolder = null)
        {
            string jsonData = File.ReadAllText($@"{pathToJsonFolder}Protection_TC.json");
            var objTcList = JsonConvert.DeserializeObject<List<Protection_TC>>(jsonData);

            using (var db = new MyDbContext())
            {
                Console.WriteLine("----------------- Protection_TC -----------------");
                foreach (var objTc in objTcList)
                {
                    // Console.WriteLine($"entety: Parentid={objTc.ParentId}, ChieldId={objTc.ChildId}, Order={objTc.Order}");
                    var objs = db.Protections.Find(objTc.ChildId);
                    var tc = db.TechnologicalCards.Find(objTc.ParentId);

                    if (objs != null && tc != null)
                    {
                        objTc.Child = objs;
                        objTc.Parent = tc;


                        db.Protection_TCs.Add(objTc);
                    }
                }
                db.Protection_TCs.AddRange(objTcList);
                db.SaveChanges();
            }
        }

        private static void DeserializeToolTcToDb(string pathToJsonFolder = null)
        {
            string jsonData = File.ReadAllText($@"{pathToJsonFolder}Tool_TC.json");
            var toolTcList = JsonConvert.DeserializeObject<List<Tool_TC>>(jsonData);

            List<Tool_TC> itemsToRemove = new List<Tool_TC>();
            using (var db = new MyDbContext())
            {

                Console.WriteLine("----------------- Tool_TC -----------------");

                foreach (var toolTc in toolTcList)
                {
                    var tool = db.Tools.Find(toolTc.ChildId);
                    var tc = db.TechnologicalCards.Find(toolTc.ParentId);
                    if (tool != null && tc != null)
                    {
                        toolTc.Child = tool;
                        toolTc.Parent = tc;
                        try
                        {
                            db.Tool_TCs.Add(toolTc);
                            //Console.WriteLine($"entety: Parentid={toolTc.ParentId}, ChieldId={toolTc.ChildId}, Order={toolTc.Order}");
                        }
                        catch (Exception e)
                        { itemsToRemove.Add(toolTc); }
                    }
                }
                foreach (var item in itemsToRemove)
                {
                    toolTcList.Remove(item);
                    db.Entry(item).State = EntityState.Detached;
                    //Console.WriteLine($"entety Deleted: Parentid={item.ParentId}, ChieldId={item.ChildId}, Order={item.Order}");
                }
                db.Tool_TCs.AddRange(toolTcList);
                db.SaveChanges();
            }
        }

        public static void DeserializeWorkStepsToDb(string pathToJsonFolder = null)
        {
            string jsonData = File.ReadAllText($@"{pathToJsonFolder}WorkSteps.json");
            var TOWList = JsonConvert.DeserializeObject<List<TechOperationWork>>(jsonData);

            List<TechOperationWork> itemsToRemove = new List<TechOperationWork>();
            using (var db = new MyDbContext())
            {

                Console.WriteLine("----------------- TechOperationWork -----------------");

                //find all intermediate objects from db
                var staffTcList = db.Staff_TCs.ToList();
                var componentTcList = db.Component_TCs.ToList();
                var machineTcList = db.Machine_TCs.ToList();

                foreach (var TOW in TOWList)
                {
                    var TO = db.TechOperations.Find(TOW.techOperationId);
                    var TC = db.TechnologicalCards.Find(TOW.TechnologicalCardId);

                    if (TO != null && TC != null)
                    {
                        TOW.techOperation = TO;
                        TOW.technologicalCard = TC;

                        foreach(var exW in TOW.executionWorks)
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

                            exW.Staffs = staff_TCList;

                            //var staff_TCList = new List<Staff_TC>();
                            //var machine_TCList = new List<Machine_TC>();
                            //var protection_TCList = new List<Protection_TC>();
                            //foreach (var staff in exW.Staffs)
                            //{
                            //    var s = db.Staff_TCs.Find(staff.IdAuto);
                            //    if (s != null)
                            //    {
                            //        staff_TCList.Add(s);
                            //    }
                            //}
                            //foreach (var machine in exW.Machines)
                            //{
                            //    var m = db.Machine_TCs.Where(x=> x.ChildId == machine.ChildId && x.ParentId == machine.ParentId).FirstOrDefault();
                            //    if (m != null)
                            //    {
                            //        machine_TCList.Add(m);
                            //    }
                            //}
                            //foreach (var protection in exW.Protections)
                            //{
                            //    var p = db.Protection_TCs.Where(x => x.ChildId == protection.ChildId && x.ParentId == protection.ParentId).FirstOrDefault();
                            //    if (p != null)
                            //    {
                            //        protection_TCList.Add(p);
                            //    }
                            //}
                            //exW.Staffs.Clear();
                            //exW.Machines.Clear();
                            //exW.Protections.Clear();

                            //exW.Staffs = staff_TCList;
                            //exW.Machines = machine_TCList;
                            //exW.Protections = protection_TCList;

                            var tT = db.TechTransitions.Find(exW.techTransitionId);
                            if (tT != null)
                            {
                                exW.techTransition = tT;
                            }
                            
                            exW.techOperationWork = TOW;
                        }

                        foreach (var cw in TOW.ComponentWorks)
                        {
                            //var combponentWorkList = new List<ComponentWork>();
                            //foreach (var staff in exW.Staffs)
                            //{
                            //    var existingStaff = db.Staff_TCs.Find(staff.IdAuto);
                            //    if (existingStaff == null)
                            //    {
                            //        // Если сотрудник не найден, добавляем его в контекст
                            //        db.Staff_TCs.Add(staff);
                            //        staff_TCList.Add(staff);
                            //    }
                            //    else
                            //    {
                            //        // Если сотрудник уже существует, используем существующий экземпляр
                            //        staff_TCList.Add(existingStaff);
                            //    }
                            //}

                            var existingComponentId = db.Components.Any(c => c.Id == cw.componentId);

                            if (!existingComponentId)
                            {
                                // Логика обработки случая, когда componentId не найден
                                Console.WriteLine($"Component с ID {cw.componentId} не найден в таблице Components. TO:{TOW.Id} TC:{TOW.TechnologicalCardId}");
                            }
                        }


                        db.TechOperationWorks.Update(TOW);
                        //try
                        //{
                        //    db.TechOperationWorks.Add(TOW);
                        //    //Console.WriteLine($"entety: Parentid={toolTc.ParentId}, ChieldId={toolTc.ChildId}, Order={toolTc.Order}");
                        //}
                        //catch (Exception e)
                        //{ itemsToRemove.Add(TOW); }
                    }
                }

                //foreach (var item in itemsToRemove)
                //{
                //    TOWList.Remove(item);
                //    db.Entry(item).State = EntityState.Detached;
                //    //Console.WriteLine($"entety Deleted: Parentid={item.ParentId}, ChieldId={item.ChildId}, Order={item.Order}");
                //}
                //db.TechOperationWorks.AddRange(TOWList);
                //db.SaveChanges();
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
        public static void AddComponentWorkWithComponentCheck(MyDbContext db, ComponentWork componentWork)
        {
            // Проверяем, существует ли компонент в базе данных
            var component = db.Components.FirstOrDefault(c => c.Id == componentWork.componentId);

            if (component == null)
            {
                // Если компонент не найден, создаем и добавляем новый компонент
                component = new Component
                {
                    Id = componentWork.componentId,
                    // Задаем необходимые свойства нового компонента
                };
                db.Components.Add(component);
                db.SaveChanges(); // Важно сохранить изменения, чтобы новый компонент был доступен в базе данных
            }

            // Теперь безопасно добавляем ComponentWork, поскольку уверены в наличии соответствующего Component
            db.ComponentWorks.Add(componentWork);
            db.SaveChanges();
        }

        //private static void SerializeComponent(string filePath)
        //{
        //    ExcelParser excelParser = new ExcelParser();
        //    var componentList = excelParser.ParseExcelToObjectsComponent(filePath);
        //    File.WriteAllText($@"{_path}\Serialised data\componentList.json", JsonConvert.SerializeObject(componentList, Formatting.Indented));
        //}

        //private static void SerializeStaffTc(string filePath)
        //{
        //    ExcelParser excelParser = new ExcelParser();
        //    var objTcList = excelParser.ParseExcelToStaffTcObjects(filePath, out _);

        //    File.WriteAllText($@"{_path}\Serialised data\staffTcList.json", JsonConvert.SerializeObject(objTcList, Formatting.Indented));
        //}
        //private static void SerializeComponentTc(string filePath)
        //{
        //    ExcelParser excelParser = new ExcelParser();
        //    var componentTcList = excelParser.ParseExcelToIntermediateStructObjects<Component_TC, Component>(filePath, out _);

        //    File.WriteAllText($@"{_path}\Serialised data\componentTcList.json", JsonConvert.SerializeObject(componentTcList, Formatting.Indented));
        //}
        //private static void SerializeMachineTc(string filePath)
        //{
        //    ExcelParser excelParser = new ExcelParser();
        //    var objTcList = excelParser.ParseExcelToIntermediateStructObjects<Machine_TC, Machine>(filePath, out _);

        //    File.WriteAllText($@"{_path}\Serialised data\machineTcList.json", JsonConvert.SerializeObject(objTcList, Formatting.Indented));
        //}
        //private static void SerializeProtectionTc(string filePath)
        //{
        //    ExcelParser excelParser = new ExcelParser();
        //    var objTcList = excelParser.ParseExcelToIntermediateStructObjects<Protection_TC, Protection>(filePath, out _);

        //    File.WriteAllText($@"{_path}\Serialised data\protectionTcList.json", JsonConvert.SerializeObject(objTcList, Formatting.Indented));
        //}
        //private static void SerializeToolTc(string filePath)
        //{
        //    ExcelParser excelParser = new ExcelParser();
        //    var objTcList = excelParser.ParseExcelToIntermediateStructObjects<Tool_TC, Tool>(filePath, out _);

        //    File.WriteAllText($@"{_path}\Serialised data\toolTcList.json", JsonConvert.SerializeObject(objTcList, Formatting.Indented));
        //}
    }
}
