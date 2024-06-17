using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;



namespace ExcelParsing.DataProcessing
{
    public class WorkParser
    {
        List<TechTransition> _newTransitions;

        private const int StartRow = 2;
        private readonly string[] _stepColumns = {
            "Id ТО", "Id ТП", "TC_ID", "Артикул", "№",
            "Технологические операции", "Исполнитель", "Технологические переходы",
            "Время действ., мин.", "Время выполнения этапа, мин.", "№ СЗ",
            "Примечание", "Индекс", "Категория ТО", "Инструменты", "Тип", "Этап, формула", "Строка", "Machinery"
        };
        public WorkParser()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
        }

        public void CacheDbData(out List<TechOperation> techOperations, out List<TechTransition> techTransitions, out List<TechnologicalCard> tcs)
        {
            using (var context = new MyDbContext())
            {
                techOperations = context.TechOperations.ToList();
                techTransitions = context.TechTransitions.ToList();
                tcs = context.TechnologicalCards.ToList();
            }
        }
        public void ParseTcWorkSteps(string tcFilePath, string tcArticle)
        {

            Console.WriteLine($"Парсинг ТК {tcArticle}");


            var objList = new List<TechOperationWork>();

            var fileInfo = new FileInfo(tcFilePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"Файл {tcFilePath} не найден.");
            }

            using (var package = new ExcelPackage(fileInfo))
            {
                var stepSheet = package.Workbook.Worksheets[tcArticle];
                if (stepSheet == null)
                {
                    throw new Exception($"Лист {tcArticle} не найден в файле.");
                }

                // получение необходимых данных из ДБ и их кэширование
                CacheDbData(out var techOperations, out var techTransitions, out var tcs);

                TechnologicalCard? currentTc = tcs.FirstOrDefault(tc => tc.Article == tcArticle);
                if (currentTc == null)
                {
                    throw new Exception($"Технологическая карта {tcArticle} не найдена в кэше.");
                }

                int stepRowCount = stepSheet.Dimension.Rows;

                Console.WriteLine($"Всего строк: {stepRowCount}");

                // определить начало таблицы "6.Выполнение работ"
                int startRow = 0;
                for (int row = 1; row <= stepRowCount; row++)
                {
                    if (stepSheet.Cells[row, 1].Text.Contains("Выполнение работ"))
                    {
                        startRow = row + 1;
                        break;
                    }
                }

                Console.WriteLine("Таблица 6 начинается на строке: " + startRow);

                // Получить все не пустые заголовки и их номера столбцов
                var stepColumnsNumbers = GetColumnsNumbers(startRow, stepSheet);

                // Выделение номера столбцов, название которых соотноситься с названиями в stepColumns
                // отдельно выделить те, которые не соотносятся (время работы механизмов)
                string[] stepColumns = {
                                            "№",
                                            "Технологические операции", "Исполнитель", "Технологические переходы",
                                            "Время выполнения действия, мин.", "Время действ., мин.",
                                            "Время выполнения этапа, мин.", "Время этапа, мин.",
                                            "№ СЗ", "Примечание"
                                        };
                var newStepColumnsNumbers = new Dictionary<string, int>();
                var machinaryColumnsNumbers = new Dictionary<string, int>();

                foreach (var column in stepColumns)
                {
                    foreach (var key in stepColumnsNumbers.Keys)
                    {
                        if (CompareStrings(key, column, maxDistance: 1))
                        {
                            newStepColumnsNumbers[column] = stepColumnsNumbers[key];
                        }
                    }
                }

                foreach (var key in stepColumnsNumbers.Keys)
                {
                    if (!newStepColumnsNumbers.ContainsValue(stepColumnsNumbers[key]))
                    {
                        if (key.Contains("Время"))
                            machinaryColumnsNumbers[key] = stepColumnsNumbers[key];
                    }
                }

                foreach (var key in newStepColumnsNumbers.Keys)
                {
                    Console.WriteLine($"{key} - {newStepColumnsNumbers[key]}");
                }
                foreach(var key in machinaryColumnsNumbers.Keys)
                {
                    Console.WriteLine($"{key} - {machinaryColumnsNumbers[key]}");
                }

                // Парсинг строк
                var currentToId = 0;
                var previousToId = 0;

                var lastToOrderInTc = 1;
                var lastEwOrderInTc = 1;

                for (int row = startRow + 1; row <= stepRowCount; row++)
                {
                    //ExecutionWork executionWork = new ExecutionWork();

                    TechOperation? techOperation = null;
                    TechTransition? techTransition = null;


                    // названия тех операции
                    string techOperationName = stepSheet.Cells[row, newStepColumnsNumbers["Технологические операции"]].Text;
                    // поиск тех операции в кэше
                    if (!string.IsNullOrEmpty(techOperationName))
                    {
                        techOperation = techOperations.FirstOrDefault(to => to.Name == techOperationName);
                        if (techOperation == null)
                        {
                            throw new Exception($"Технологическая операция {techOperationName} (строка {row}) не найдена в кэше.");
                        }
                        currentToId = techOperation.Id;
                        Console.WriteLine($"------------------ ТО: {techOperation.Name}");
                    }

                    //симовлы персонала
                    string listStaff = stepSheet.Cells[row, newStepColumnsNumbers["Исполнитель"]].Text;
                    
                    if(!string.IsNullOrEmpty(listStaff))
                    {
                        // название тех перехода
                        string techTransitionName = stepSheet.Cells[row, newStepColumnsNumbers["Технологические переходы"]].Text.Trim();
                        // поиск тех перехода в кэше
                        if (!string.IsNullOrEmpty(techTransitionName))
                        {
                            techTransition = techTransitions.FirstOrDefault(tt => tt.Name == techTransitionName);
                            if (techTransition == null)
                            {
                                throw new Exception($"Технологический переход {techTransitionName} (строка {row}) не найден в кэше.");
                            }
                            Console.WriteLine($"    ТП: {lastEwOrderInTc}. {techTransition.Name}");
                        }

                        // получаю формулу из ячейки
                        //определение названия столбца с временем выполнения действия
                        string timeExecutionStepColumn = "Время выполнения действия, мин.";
                        string timeExecutionEtapColumn = "Время выполнения этапа, мин.";
                        if (!newStepColumnsNumbers.ContainsKey(timeExecutionStepColumn))
                        {
                            timeExecutionStepColumn = "Время действ., мин.";
                        }
                        if (!newStepColumnsNumbers.ContainsKey(timeExecutionEtapColumn))
                        {
                            timeExecutionEtapColumn = "Время этапа, мин.";
                        }

                        // формула шага
                        string formulaStep = stepSheet.Cells[row, newStepColumnsNumbers[timeExecutionStepColumn]].Formula;
                        // формула этапа
                        string formulaEtap = stepSheet.Cells[row, newStepColumnsNumbers[timeExecutionEtapColumn]].Formula;
                        //формула механизмов
                        Dictionary<string, bool> machineIsParticipates = new Dictionary<string, bool>();
                        foreach (var key in machinaryColumnsNumbers.Keys)
                        {
                            machineIsParticipates[key] = stepSheet.Cells[row, machinaryColumnsNumbers[key]].Text.Length >= 1;
                        }

                        Console.WriteLine($"        Исполнитель: {listStaff}");


                        // намера СЗ
                        string protectionRange = stepSheet.Cells[row, newStepColumnsNumbers["№ СЗ"]].Text;
                        Console.WriteLine($"        СЗ: {protectionRange}");

                        // коэффициент времени тех перехода
                        string coefficient = GetStringBeforeAtSymbol(formulaStep);
                        string coeffText = string.IsNullOrEmpty(coefficient) ? "" : $"Коэффициент: {coefficient}";

                        

                        string timeExecutionStep = string.IsNullOrEmpty(coefficient) ? techTransition!.TimeExecution.ToString() : EvaluateExpression(techTransition!.TimeExecution.ToString() + " *" + coefficient).ToString(); //techTransition.TimeExecution* coefficient;
                        Console.WriteLine($"        Время ТП: {timeExecutionStep}; {coeffText}");

                        // Если номер ТО изменился или это первая строка, то создаем новый объект TechOperationWork
                        if (row == startRow + 1 || currentToId != previousToId)
                        {
                            previousToId = currentToId;

                            var techOperationWork = new TechOperationWork
                            {
                                techOperationId = currentToId,
                                TechnologicalCardId = currentTc.Id,
                                Order = lastToOrderInTc++,
                            };

                            objList.Add(techOperationWork);
                        }

                        var exWork = new ExecutionWork
                        {
                            //techOperationWorkId = currentToId,
                            techTransitionId = techTransition?.Id,
                            Order = lastEwOrderInTc++,
                            //Value = ,
                            Comments = "",
                            Staffs = new List<Staff_TC>(),
                            Protections = new List<Protection_TC>(),
                            Machines = new List<Machine_TC>(),
                            Repeat = false,
                            Etap = "",
                            Posled = "",

                            Coefficient = string.IsNullOrEmpty(coefficient) ? "" : coefficient,
                        };

                        if (exWork != null)
                        {
                            objList[objList.Count - 1].executionWorks.Add(exWork);
                            exWork.techOperationWork = objList[objList.Count - 1];
                        }
                    }
                    


                    //else if (component != null)
                    //{
                    //    objList[objList.Count - 1].ComponentWorks.Add(component);
                    //}
                    //else if (tool != null)
                    //{
                    //    objList[objList.Count - 1].ToolWorks.Add(tool);
                    //}


                }

                Console.WriteLine();



                //var stepColumnsNumbers = GetColumnsNumbers(_stepColumns, 1, stepSheet);

                //int lastToOrderInTc = 0;
                //int previousToId = 0;
                //int previousTcId = 0;

                //var executionWorks = new List<ExecutionWork>();

                //for (int row = StartRow; row <= stepRowCount; row++)
                //{
                //    ProcessRow(stepSheet, row, stepColumnsNumbers,
                //        ref executionWorks,
                //        ref lastToOrderInTc, ref previousToId, ref previousTcId, objList);
                //}
            }
        }
        public List<TechOperation> ParseExcelToObjectsTechOperation(string filePath, string sheetName = "Перечень ТО")
        {
            var objList = new List<TechOperation>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = StartRow; row <= rowCount; row++)
                {

                    var obj = new TechOperation
                    {
                        Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                        Name = Convert.ToString(worksheet.Cells[row, 2].Value),
                        Category = Convert.ToString(worksheet.Cells[row, 3].Value),

                        IsReleased = true,
                    };

                    objList.Add(obj);
                }
            }

            return objList;
        }

        public List<TechTransition> ParseExcelToObjectsTechTransition(string filePath, string sheetName = "Типовые переходы")
        {
            var objList = new List<TechTransition>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = StartRow; row <= rowCount; row++)
                {
                    var obj = new TechTransition
                    {
                        Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                        Name = Convert.ToString(worksheet.Cells[row, 3].Value),
                        Category = Convert.ToString(worksheet.Cells[row, 2].Value),
                        TimeExecution = Convert.ToDouble(worksheet.Cells[row, 4].Value),
                        TimeExecutionChecked = Convert.ToBoolean(worksheet.Cells[row, 8].Value),
                        CommentName = Convert.ToString(worksheet.Cells[row, 6].Value),
                        CommentTimeExecution = Convert.ToString(worksheet.Cells[row, 7].Value),

                        IsReleased = true,
                    };

                    objList.Add(obj);
                }
            }

            return objList;
        }
        public List<TechOperationWork> ParseExcelToObjectsTechOperationWork(string filePath, string stepSheetName)
        {
            var objList = new List<TechOperationWork>();
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"Файл {filePath} не найден.");
            }

            using (var package = new ExcelPackage(fileInfo))
            {
                var stepSheet = package.Workbook.Worksheets[stepSheetName];
                if (stepSheet == null)
                {
                    throw new Exception($"Лист {stepSheetName} не найден в файле.");
                }

                int stepRowCount = stepSheet.Dimension.Rows;
                var stepColumnsNumbers = GetColumnsNumbers(_stepColumns, 1, stepSheet);

                int lastToOrderInTc = 0;
                int previousToId = 0;
                int previousTcId = 0;

                var executionWorks = new List<ExecutionWork>();

                for (int row = StartRow; row <= stepRowCount; row++)
                {
                    ProcessRow(stepSheet, row, stepColumnsNumbers,
                        ref executionWorks,
                        ref lastToOrderInTc, ref previousToId, ref previousTcId, objList);
                }
            }

            return objList;
        }

        private void ProcessRow(ExcelWorksheet stepSheet, int row, Dictionary<string, int> stepColumnsNumbers, 
            ref List<ExecutionWork> executionWorks,
            ref int lastToOrderInTc, ref int previousToId, ref int previousTcId, List<TechOperationWork> objList)
        {
            int currentTcId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["TC_ID"]].Value);
            int currentToId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Id ТО"]].Value);

            ExecutionWork exWork = ParseTechExecutionWork(stepSheet, row, stepColumnsNumbers, currentTcId, 
                ref executionWorks,
                out var tool, out var component);


            if (row == StartRow || currentToId != previousToId || currentTcId != previousTcId)
            {
                if (currentTcId != previousTcId)
                {
                    previousTcId = currentTcId;
                    lastToOrderInTc = 0;
                }

                lastToOrderInTc++;
                previousToId = currentToId;

                var techOperationWork = new TechOperationWork
                {
                    techOperationId = currentToId,
                    TechnologicalCardId = currentTcId,
                    Order = lastToOrderInTc,
                };

                objList.Add(techOperationWork);
            }

            if (exWork != null)
            {
                objList[objList.Count - 1].executionWorks.Add(exWork);
                exWork.techOperationWork = objList[objList.Count - 1];
                executionWorks.Add(exWork);
            }
            else if (component != null)
            {
                objList[objList.Count - 1].ComponentWorks.Add(component);
            }
            else if (tool != null)
            {
                objList[objList.Count - 1].ToolWorks.Add(tool);
            }
        }
        public ExecutionWork? ParseTechExecutionWork(ExcelWorksheet stepSheet, int row, Dictionary<string, int> stepColumnsNumbers, int tcId, 
            ref List<ExecutionWork> executionWorks,
            out ToolWork? tool,
            out ComponentWork? component)
        {
            tool = null;
            component = null;

            int order = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["№"]].Value);

            TechTransition techTransition;
            int idTP = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Id ТП"]].Value);
            
            string name = stepSheet.Cells[row, stepColumnsNumbers["Технологические переходы"]].Text;

            string timeExecutionString = stepSheet.Cells[row, stepColumnsNumbers["Время действ., мин."]].Text;
            bool timeExecutionChecked = double.TryParse(timeExecutionString, out double timeExecution);

            string comments = stepSheet.Cells[row, stepColumnsNumbers["Примечание"]].Text;

            string listStaff = stepSheet.Cells[row, stepColumnsNumbers["Исполнитель"]].Text;
            string[] staffSymbols = listStaff.Split("\n");
            List<Staff_TC> staffIds = FindStaff_TCIds(staffSymbols, tcId);

            string protectionRange = stepSheet.Cells[row, stepColumnsNumbers["№ СЗ"]].Text;
            List<Protection_TC> protections = FindProtection_TCByOrder(protectionRange, tcId);

            string machineRange = stepSheet.Cells[row, stepColumnsNumbers["Machinery"]].Text;
            List<Machine_TC> machines = FindMachine_TCByOrder(machineRange, tcId);

            bool repeat = false;
            var repeatObjs = new List<ExecutionWork>();

            // check if idTP is null or empty
            if (idTP == 0)
            {
                // check if it "Повторить" item or Tools and Components
                if(name.Contains("Повторить п.") )
                {
                    repeat = true;
                    var repeatObjsNumbers = GetExecutinWorksNumbers(name);
                    foreach (var n in repeatObjsNumbers)
                    {
                        var repeatingObj = executionWorks.Find(e => e.techOperationWork.TechnologicalCardId == tcId && e.Order == n && n != order);
                        if (repeatingObj != null)
                        {
                            repeatObjs.Add(repeatingObj);
                        }
                    }
                }
                else if (stepSheet.Cells[row, stepColumnsNumbers["Тип"]].Text == "Component")
                {
                    component = new ComponentWork
                    {
                        componentId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Инструменты"]].Value),
                        Quantity = timeExecution,
                    };

                    return null;
                }
                else if (stepSheet.Cells[row, stepColumnsNumbers["Тип"]].Text == "Tool")
                {
                    tool = new ToolWork
                    {
                        toolId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Инструменты"]].Value),
                        Quantity = timeExecution,
                    };

                    return null;
                }
            }

            string formula = stepSheet.Cells[row, stepColumnsNumbers["Этап, формула"]].Text;
            int rowNum = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Строка"]].Value);

            (string stage, string parallelIndex) = ParseStageFormula2(formula, rowNum);

            var obj = new ExecutionWork
            {
                techTransitionId = idTP == 0? null : idTP,

                Order = order,

                Value = timeExecution,
                Comments = comments,

                Staffs = staffIds,
                Protections = protections,
                Machines = machines,

                Repeat = repeat,

                Etap = stage.ToString(),
                Posled = parallelIndex.ToString(),

            };


            if (obj.Repeat)
            {
                obj.ListexecutionWorkRepeat2 = repeatObjs;
            }

            return obj;
        }

        public List<int> GetExecutinWorksNumbers(string repeatName)
        {
            var numbers = new List<int>();
            var split = repeatName.Split(" ");
            if (split.Length > 1)
            {
                foreach (var s in split)
                {
                    ExtractRange(s, numbers);
                }
            }
            return numbers;
        }
        private void ExtractRange(string textRange, List<int> numbers)
        {
            try
            {
                if (textRange.Contains("-") || textRange.Contains(","))
                {
                    if (textRange.Contains(","))
                    {
                        var range = textRange.Split(",");
                        foreach (var r in range)
                        {
                            SplitRange(r).ForEach(n => numbers.Add(n));
                        }
                    }
                    SplitRange(textRange).ForEach(n => numbers.Add(n));
                }
                else if (int.TryParse(textRange, out int n))
                {
                    numbers.Add(n);
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private List<int> SplitRange(string range)
        {
            var numbers = new List<int>();

            var split = range.Split("-");
            if (split.Length > 1)
            {
                if (int.TryParse(split[0], out int start) && int.TryParse(split[1], out int end))
                {
                    for (int i = start; i <= end; i++)
                    {
                        numbers.Add(i);
                    }
                }
                
            }
            else if (int.TryParse(range, out int n))
            {
                //int.TryParse(range, out int n);
                numbers.Add(n);
            }
            return numbers;
        }
        public List<Staff_TC> FindStaff_TCIds(string[] staff_TcsSymbols, int tcId)
        {
            List<Staff_TC> staff_TCs = new List<Staff_TC>();

            using (var context = new TcDbConnector.MyDbContext())
            {
                foreach (var symbol in staff_TcsSymbols)
                {
                    var staff_tc = context.Staff_TCs.Where(st => st.Symbol == symbol && st.ParentId == tcId).FirstOrDefault();
                    if (staff_tc != null)
                    {
                        staff_TCs.Add(staff_tc);
                    }
                }
            }

            return staff_TCs;
        }
        public List<Protection_TC> FindProtection_TCByOrder(string protectionRange, int tcId)
        {
            List<Protection_TC> protections = new List<Protection_TC>();

            var  protectionIdsList = new List<int>();
            ExtractRange(protectionRange, protectionIdsList);

            using (var context = new TcDbConnector.MyDbContext())
            {
                foreach (var order in protectionIdsList)
                {
                    var protection = context.Protection_TCs.Where(pr => pr.Order == order && pr.ParentId == tcId).FirstOrDefault();
                    if (protection != null)
                    {
                        protections.Add(protection);
                    }
                }
            }

            return protections;
        }

        public List<Machine_TC> FindMachine_TCByOrder(string machineNames, int tcId)
        {
            string[] names = GetMachinesNames(machineNames);

            List<Machine_TC> machinesExist = GetMachinesFromDB(tcId);

            return GetStageMachines(machinesExist, names);
        }

        public static string[] GetMachinesNames(string machineNames)
        {
            string[] textToDelete = new string[] { "Время работы", "мин.", "," };
            string separator = " | ";
            string[] machines = machineNames.Split(separator);

            for (int i = 0; i < machines.Length; i++)
            {
                foreach (var text in textToDelete)
                {
                    machines[i] = machines[i].Replace(text, "").Trim();
                }
            }
            return machines;
        }

        public List<Machine_TC> GetMachinesFromDB(int tcId)
        {
            List<Machine_TC> machines = new List<Machine_TC>();
            using (var db = new MyDbContext())
            {
                machines = db.Machine_TCs.Where(m => m.ParentId == tcId)
                    .Include(m => m.Child)
                    .ToList();
            }
            return machines;
        }

        public static List<Machine_TC> GetStageMachines(List<Machine_TC> existMacines, string[] machinesNames)
        {
            var stageMachines = new List<Machine_TC>();
            int maxDistance = 2;
            foreach (var machineName in machinesNames)
            {
                var machine = existMacines.FirstOrDefault(m => CompareStrings(m.Child.Name, machineName, maxDistance));
                if (machine != null)
                {
                    stageMachines.Add(machine);
                }
            }
            return stageMachines;
        }
        static bool CompareStrings(string source, string target, int maxDistance)
        {
            source = source.ToLowerInvariant().Replace(" ", "");
            target = target.ToLowerInvariant().Replace(" ", "");

            int distance = ComputeLevenshteinDistance(source, target);
            return distance <= maxDistance;
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

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[n, m];
        }

        public Dictionary<string, int> GetColumnsNumbers(string[] columns, int columnRow, ExcelWorksheet worksheet)
        {
            var columnsNumbers = new Dictionary<string, int>();
            for (int i = 1; i <= worksheet.Dimension.Columns; i++)
            {
                string value = worksheet.Cells[columnRow, i].Text;
                if (Array.IndexOf(columns, value) >= 0)
                {
                    columnsNumbers[value] = i;
                }

            }
            return columnsNumbers;
        }
        public Dictionary<string, int> GetColumnsNumbers(int columnRow, ExcelWorksheet worksheet)
        {
            var columnsNumbers = new Dictionary<string, int>();
            for (int i = 1; i <= worksheet.Dimension.Columns; i++)
            {
                string value = worksheet.Cells[columnRow, i].Text;
                if (value.Length >= 1)
                {
                    columnsNumbers[value] = i;
                }

            }
            return columnsNumbers;
        }
        public (string stage, string parallelIndex) ParseStageFormula(string formula, int rowNum)
        {
            if (!formula.ToLower().Contains("sum") || !formula.ToLower().Contains("max"))
            {
                return ("0", "0");
            }

            int b = formula.IndexOf('G');
            var perv = formula.Substring(b, formula.IndexOf(')', b)-b);

            var solit = perv.Split(':');
            if (solit.Length > 0)
            {
                perv = solit[0];
            }


            int v = formula.LastIndexOf('G');
            var Last = formula.Substring(v).Replace(")", "");


            var bn = formula.Replace("MAX", "").Replace("SUM", "").Replace("(", "").Replace(")", "");
            var df = bn.Split(',');


            string finalPosled = "0";
            foreach (string s in df)
            {
                var cc = s.Replace("G", "").Split(':');
                if (cc.Length > 1)
                {
                    if (rowNum >= int.Parse(cc[0])   && rowNum <= int.Parse(cc[1]) )
                    {
                        finalPosled = s;
                        break;
                    }
                }
                else
                {
                    var nm = cc[0];
                    if (rowNum == int.Parse(nm))
                    {
                        finalPosled = s;
                        break;
                    }
                }
            }
            return (perv+ Last, finalPosled);
        }
        public (string stage, string parallelIndex) ParseStageFormula2(string formula, int rowNum)
        {
            // Инициализируем переменные для хранения результатов
            string stage = "0";
            string parallelIndex = "0";

            // Ищем максимальноt и минимально число в формуле и присваиваем их в stage
            (int minValue, int maxValue) = GetMinAndMaxValue(formula);
            stage = $"{minValue}{maxValue}";

            // Убираем из строки все, кроме цифр и диапазонов
            string cleanFormula = new string(formula.Where(c => char.IsDigit(c) || c == ';' || c == '+' || c == ':' || c == ',').ToArray());

            // Разделяем на диапазоны
            string[] ranges = cleanFormula.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            int posledIndex = 0;
            foreach (string range in ranges)
            {
                posledIndex++; // Увеличиваем индекс последовательности для каждого диапазона

                if (range.Contains('+') || range.Contains(';'))
                {
                    // Получаем начало и конец диапазона
                    string[] ranges2 = range.Split(new char[] { '+', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string range2 in ranges2)
                    {
                        // Проверяем, является ли текущий элемент диапазоном
                        if (range2.Contains(':'))
                        {
                            // Получаем начало и конец диапазона
                            int start = int.Parse(range2.Split(':')[0]);
                            int end = int.Parse(range2.Split(':')[1]);

                            // Проверяем, попадает ли rowNum в диапазон
                            if (rowNum >= start && rowNum <= end)
                            {
                                parallelIndex = posledIndex.ToString();
                                break;
                            }
                        }
                        else
                        {
                            // Обработка случая, когда указан один номер строки
                            int line = int.Parse(range2);

                            if (rowNum == line)
                            {
                                parallelIndex = posledIndex.ToString();
                                break;
                            }
                        }
                    }
                }
                else if (!formula.Contains("SUM"))
                {
                    // Проверяем, является ли текущий элемент диапазоном
                    if (range.Contains(':'))
                    {
                        // Получаем начало и конец диапазона
                        int start = int.Parse(range.Split(':')[0]);
                        int end = int.Parse(range.Split(':')[1]);

                        // Проверяем, попадает ли rowNum в диапазон
                        if (rowNum >= start && rowNum <= end)
                        {
                            parallelIndex = "0";
                            break;
                        }
                    }
                    else
                    {
                        // Обработка случая, когда указан один номер строки
                        int line = int.Parse(range);

                        if (rowNum == line)
                        {
                            parallelIndex = "0"; // Для одиночных строк индекс последовательности равен 0
                            break;
                        }
                    }
                }
                else 
                {
                    // Проверяем, является ли текущий элемент диапазоном
                    if (range.Contains(':'))
                    {
                        // Получаем начало и конец диапазона
                        int start = int.Parse(range.Split(':')[0]);
                        int end = int.Parse(range.Split(':')[1]);

                        // Проверяем, попадает ли rowNum в диапазон
                        if (rowNum >= start && rowNum <= end)
                        {
                            parallelIndex = posledIndex.ToString();
                            break;
                        }
                    }
                    else
                    {
                        // Обработка случая, когда указан один номер строки
                        int line = int.Parse(range);

                        if (rowNum == line)
                        {
                            parallelIndex = "0"; // Для одиночных строк индекс последовательности равен 0
                            break;
                        }
                    }
                }

            }

            // Возвращаем индекс параллельности и последовательности
            return (stage, parallelIndex);
        }
        public (int min, int max) GetMinAndMaxValue(string formula)
        {
            string text = formula;

            // Использование регулярного выражения для извлечения всех чисел из текста
            var matches = Regex.Matches(text, @"\d+");

            // Преобразование найденных чисел из строки в int
            var numbers = matches.Cast<Match>().Select(m => int.Parse(m.Value)).ToList();

            // Поиск минимального и максимального значения
            int min = numbers.Min();
            int max = numbers.Max();

            return (min, max);
        }

        static string GetStringBeforeAtSymbol(string input, string symbol = "*INDEX")
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(input))
            {
                return result;
            }

            int atIndex = input.IndexOf(symbol);
            if (atIndex == -1)
            {
                return result; // If '@' not found, return the entire string
            }

            return input.Substring(0, atIndex); // Return substring before '@' and after '='
        }
        static double EvaluateExpression(string expression)
        {
            var table = new DataTable();
            var value = table.Compute(expression.Replace(",","."), string.Empty);
            return Convert.ToDouble(value);
        }
    }
}
