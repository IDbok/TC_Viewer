using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ExcelParsing.DataProcessing
{
    public class ExcelExporter
    {
        public static readonly Color _lightGreen = Color.FromArgb(197, 224, 180);
        public static readonly Color _lightYellow = Color.FromArgb(237, 125, 49);
        public static readonly Color _lightGey = Color.FromArgb(242, 242, 242);

        private ExcelPackage _excelPackage;
        public ExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
        }
        public void CreateSheet(string name)
        {
            _excelPackage.Workbook.Worksheets.Add(name);
        }

        public void AddTableHeaders(ExcelWorksheet sheet, string[] headers, int headRow, int[] columnNums)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[headRow, columnNums[i]].Value = headers[i];
                sheet.Cells[headRow, columnNums[i]].Style.WrapText = true;
                sheet.Cells[headRow, columnNums[i]].Style.Font.Bold = true; // Делаем заголовок жирным
            }

            MergeRowCellsByColumns(sheet, headRow, columnNums);

            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.BackgroundColor.SetColor(_lightGreen);
            // Форматирование ячеек
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        }

        public void AddTableHeader(ExcelWorksheet sheet, string header, int headRow, int[] columnNums)
        {
            if (columnNums.Length < 2)
            {
                throw new ArgumentException("Для объединения столбцов должно быть указано не менее двух столбцов");
            }
            if (headRow < 1)
            {
                throw new ArgumentException("Номер строки заголовка должен быть больше 0");
            }

            sheet.Cells[headRow, 1].Value = header;
            var mergeRange = sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1];
            mergeRange.Merge = true;
            mergeRange.Style.Font.Bold = true;
            mergeRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            mergeRange.Style.Fill.BackgroundColor.SetColor(_lightYellow);

        }
        public void ApplyCellFormatting(ExcelRange cells)
        {
            cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            // Здесь можно добавить другие стили...
        }
        public void MergeRowCellsByColumns(ExcelWorksheet sheet, int rowNumber, int[] columnNums)
        {
            // Проходим через каждый столбец в массиве
            for (int i = 0; i < columnNums.Length - 1; i++)
            {
                // Определение следующего столбца из массива
                int startColumn = columnNums[i];
                int endColumn = columnNums[i + 1] - 1;

                // Если начальный и конечный столбцы равны, значит, столбец не должен объединяться с другими
                if (startColumn == endColumn)
                {
                    continue; // Пропускаем итерацию
                }

                // Объединяем ячейки для текущего диапазона
                sheet.Cells[rowNumber, startColumn, rowNumber, endColumn].Merge = true;
            }
        }
        public void ColorizeEditableColumn(ExcelWorksheet sheet, int columnNumber, int startRow, int endRow)
        {
            // Применяем стиль к столбцу
            sheet.Cells[startRow, columnNumber, endRow, columnNumber].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[startRow, columnNumber, endRow, columnNumber].Style.Fill.BackgroundColor.SetColor(_lightGey);
        }

        public void AutoFitRowHeightForMergedCells(ExcelWorksheet sheet, int rowNumber, double defaultRowHeight)
        {
            double maxHeight = defaultRowHeight;
            var row = sheet.Row(rowNumber);

            for (int col = 1; col <= sheet.Dimension.End.Column; col++)
            {
                var cell = sheet.Cells[rowNumber, col];
                if (cell.Merge && cell.Start.Column == col)
                {
                    // Эта ячейка - начало объединённого диапазона
                    double mergedCellWidth = 0;
                    for (int mergedCol = cell.Start.Column; mergedCol <= cell.End.Column; mergedCol++)
                    {
                        mergedCellWidth += sheet.Column(mergedCol).Width;
                    }

                    double requiredHeight = CalculateRequiredHeight(cell.Text, mergedCellWidth);
                    maxHeight = Math.Max(maxHeight, requiredHeight);
                }
                else if (!cell.Merge)
                {
                    // Обычная ячейка, не объединённая
                    double cellWidth = sheet.Column(col).Width;
                    double requiredHeight = CalculateRequiredHeight(cell.Text, cellWidth);
                    maxHeight = Math.Max(maxHeight, requiredHeight);
                }
            }

            row.CustomHeight = true;
            row.Height = maxHeight;
        }

        // Вспомогательный метод для расчёта высоты на основе текста и ширины ячейки
        private double CalculateRequiredHeight(string text, double cellWidth)
        {
            // Приблизительный расчёт, нужно подстроить под ваш конкретный случай
            double approximateCharWidth = 0.1; // Подберите значение экспериментально
            double maxCharsPerRow = cellWidth / approximateCharWidth;
            int numRows = (int)Math.Ceiling(text.Length / maxCharsPerRow);

            int newLineCount = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Length - 1;
            numRows += newLineCount;

            double singleRowHeight = 15; // Подберите значение экспериментально

            return numRows * singleRowHeight; // Возвращает расчётную высоту
        }
        public void SaveAs(string filePath)
        {
            _excelPackage.SaveAs(new FileInfo(filePath));
        }

        public void Close()
        {
            _excelPackage.Dispose();
        }
    }
}
