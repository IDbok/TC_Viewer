
using OfficeOpenXml;
using TcModels.Models.TcContent;

namespace ExcelParsing.DataProcessing
{
    public class WorkParser
    {
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
    }
}
