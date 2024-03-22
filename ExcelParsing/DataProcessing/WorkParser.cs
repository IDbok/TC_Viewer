using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data.Common;
using TcDbConnector;
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
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
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
                        var repeatingObj = executionWorks.Find(e => e.techOperationWork.TechnologicalCardId == tcId && e.Order == n);
                        repeatObjs.Add(repeatingObj);
                        //continue;
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

            (string stage, string parallelIndex) = ParseStageFormula(formula, rowNum);

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

                    //if (s.Contains("-") || s.Contains(","))
                    //{
                    //    if(s.Contains(","))
                    //    {
                    //        var range = s.Split(",");
                    //        foreach (var r in range)
                    //        {
                    //            SplitRange(r).ForEach(n => numbers.Add(n));
                    //        }
                    //    }
                    //    SplitRange(s).ForEach(n => numbers.Add(n));
                    //}
                    //else if (int.TryParse(s, out int n))
                    //{
                    //    numbers.Add(n);
                    //}
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

        private (string stage, string parallelIndex) ParseStageFormula(string formula, int rowNum)
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
    }
}
