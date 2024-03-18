using OfficeOpenXml;
using System.Data.Common;
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
            "Примечание", "Индекс", "Категория ТО", "Инструменты", "Тип", "Этап, формула", "Строка"
        };
        public WorkParser()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
        }
        public List<TechOperation> ParseExcelToObjectsTechOperation(string filePath, string sheetName)
        {
            var objList = new List<TechOperation>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = StartRow; row <= rowCount; row++)
                {
                    var linkValie = Convert.ToString(worksheet.Cells[row, 9].Value);
                    var manufacturer = Convert.ToString(worksheet.Cells[row, 8].Value);


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

                for (int row = StartRow; row <= stepRowCount; row++)
                {
                    ProcessRow(stepSheet, row, stepColumnsNumbers, 
                        ref lastToOrderInTc, ref previousToId, ref previousTcId, objList);
                }
            }

            return objList;
        }

        private void ProcessRow(ExcelWorksheet stepSheet, int row, Dictionary<string, int> stepColumnsNumbers, 
            ref int lastToOrderInTc, ref int previousToId, ref int previousTcId, List<TechOperationWork> objList)
        {
            int currentTcId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["TC_ID"]].Value);
            int currentToId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Id ТО"]].Value);



            ExecutionWork exWork = ParseTechExecutionWork(stepSheet, row, stepColumnsNumbers, currentTcId, out var tool, out var component);


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

            bool repeat = false;

            // check if idTP is null or empty
            if (idTP == 0)
            {
                // check if it "Повторить" item or Tools and Components
                if(name.Contains("Повторить п.") )
                {
                    // idTP = 140;
                    repeat = true;
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
                techTransitionId = idTP,

                Order = order,

                Value = timeExecution,
                Comments = comments,

                Staffs = staffIds,

                Repeat = repeat,

                Etap = stage.ToString(),
                Posled = parallelIndex.ToString(),

            };

            return obj;
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
