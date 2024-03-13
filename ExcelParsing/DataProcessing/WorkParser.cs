using OfficeOpenXml;
using System.Data.Common;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;



namespace ExcelParsing.DataProcessing
{
    public class WorkParser
    {
        List<TechTransition> _newTransitions;

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

                for (int row = 2; row <= rowCount; row++)
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

        public List<TechTransition> ParseExcelToObjectsTechTransition(string filePath, string sheetName)
        {
            var objList = new List<TechTransition>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                   

                    var obj = new TechTransition
                    {
                        //Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                        Category = Convert.ToString(worksheet.Cells[row, 2].Value),
                        Name = Convert.ToString(worksheet.Cells[row, 3].Value),
                        TimeExecution = Convert.ToDouble(worksheet.Cells[row, 4].Value),

                        CommentName = Convert.ToString(worksheet.Cells[row, 6].Value),
                        CommentTimeExecution = Convert.ToString(worksheet.Cells[row, 7].Value),
                        TimeExecutionChecked = Convert.ToBoolean(worksheet.Cells[row, 8].Value),

                    };

                    objList.Add(obj);
                }
            }

            return objList;
        }

        public List<TechOperationWork> ParseExcelToObjectExecutionWork(string filePath, string stepSheetName, string toolSheetName, string repeatSheetName,out List<TechTransition> newTransitions)
        {
            var objList = new List<TechOperationWork>();
            
            int lastToOrderInTc = 0;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var stepSheet = package.Workbook.Worksheets[stepSheetName];
                int stepRowCount = stepSheet.Dimension.Rows;
                var stepColumns = new List<string>
                {
                    "Id ТО",
                    "Id ТП",
                    "TC_ID",
                    "Артикул",
                    "№",
                    "Технологические операции",
                    "Исполнитель",
                    "Технологические переходы",
                    "Время действ., мин.",
                    "Время выполнения этапа, мин.",
                    "№ СЗ",
                    "Примечание",
                    "Индекс",
                    "Категория ТО",
                };
                GetColumnsNumbers(stepColumns, 1, stepSheet, out Dictionary<string, int> stepColumnsNumbers);

                //var toolSheet = package.Workbook.Worksheets[toolSheetName];
                //int toolRowCount = toolSheet.Dimension.Rows;

                //var repeatSheet = package.Workbook.Worksheets[repeatSheetName];
                //int repeatRowCount = repeatSheet.Dimension.Rows;

                for (int row = 2; row <= stepRowCount; row++)
                {
                    int previousToId = 0;
                    int previousTcId = 0;

                    int currentTcId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["TC_ID"]].Value);
                    int currentToId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Id ТО"]].Value);

                    ExecutionWork exWork = ParseTechExecutionWork(stepSheet, row, stepColumnsNumbers, currentTcId);

                    if (row == 2 || currentToId != previousToId || currentTcId != previousTcId)
                    {
                        if (currentTcId != previousTcId)
                        {
                            previousTcId = currentTcId;
                            lastToOrderInTc = 0;
                        }
                        lastToOrderInTc++;

                        previousToId = currentToId;

                        TechOperationWork techOperationWork =  new TechOperationWork
                        {
                            techOperationId = currentToId,
                            TechnologicalCardId = currentTcId,
                            Order = lastToOrderInTc,
                        };

                        techOperationWork.executionWorks.Add(exWork);

                        objList.Add(techOperationWork);
                    }
                    else
                    {
                        objList[objList.Count - 1].executionWorks.Add(exWork);
                    }
                }
            }
            newTransitions = _newTransitions;
            return objList;
        }

        private ExecutionWork ParseTechExecutionWork(ExcelWorksheet stepSheet, int row, Dictionary<string, int> stepColumnsNumbers, int tcId)
        {

            int order = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["№"]].Value);

            TechTransition techTransition;
            int idTP = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Id ТП"]].Value);
            
            string name = Convert.ToString(stepSheet.Cells[row, stepColumnsNumbers["Технологические переходы"]].Value);
            double timeExecution = Convert.ToDouble(stepSheet.Cells[row, stepColumnsNumbers["Время действ., мин."]].Value);

            string comments = Convert.ToString(stepSheet.Cells[row, stepColumnsNumbers["Примечание"]].Value);

            string listStaff = Convert.ToString(stepSheet.Cells[row, stepColumnsNumbers["Исполнитель"]].Value);
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
                else
                {
                    // create a new TechTransition
                    techTransition = new TechTransition
                    {
                        Name = name,
                        TimeExecution = timeExecution,
                    };
                    _newTransitions.Add(techTransition);
                }
            }


            var obj = new ExecutionWork
            {
                // techOperationWorkId = idTO,
                techTransitionId = idTP,

                Order = order,

                Value = timeExecution,
                Comments = comments,

                Staffs = staffIds,

                Repeat = repeat,
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

        private void GetColumnsNumbers(List<string> columns,int columnRow, ExcelWorksheet worksheet, out Dictionary<string, int> columnsNumbers)
        {
            columnsNumbers = new Dictionary<string, int>();
            for (int i = 1; i <= worksheet.Dimension.Columns; i++)
            {
                string value = Convert.ToString(worksheet.Cells[columnRow, i].Value);
                if (columns.Contains(value))
                {
                    columnsNumbers.Add(value, i);
                }
            }
        }
    }
}
