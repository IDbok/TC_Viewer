using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using TcModels.Models;

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
        /// <summary>
        /// Объединяет ячейки в строке с 1 по последний минус 1 номер столбца
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowNumber"></param>
        /// <param name="columnNums"></param>
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
                var cells = sheet.Cells[rowNumber, startColumn, rowNumber, endColumn];
                cells.Merge = true;
            }
        }
        public void MergeColumnCellsByRows(ExcelWorksheet sheet, int startRow, int endRow, int columnNumber)
        {
            // Объединяем ячейки для текущего диапазона
            sheet.Cells[startRow, columnNumber, endRow, columnNumber].Merge = true;
        }
        public void ColorizeEditableColumn(ExcelWorksheet sheet, int columnNumber, int startRow, int endRow)
        {
            if (startRow > endRow) return;
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

        public void AutoFitRowHeightForMergedCells(ExcelWorksheet sheet, int rowNumber, double defaultRowHeight, Dictionary<int, double> columnWidths, int[] columnNums)
        {
            double maxHeight = defaultRowHeight;
            var row = sheet.Row(rowNumber);

            for (int i = 0; i < columnNums.Length-1; i++)
            {
                int startCol = columnNums[i];
                int endCol = (i <= columnNums.Length - 1) ? columnNums[i + 1] - 1 : sheet.Dimension.End.Column;

                double mergedCellWidth = 0;
                for (int col = startCol; col <= endCol; col++)
                {
                    if (columnWidths.TryGetValue(col, out double width))
                    {
                        mergedCellWidth += width;
                    }
                }

                var cell = sheet.Cells[rowNumber, startCol];
                double requiredHeight = CalculateRequiredHeight(cell.Text, mergedCellWidth);
                maxHeight = Math.Max(maxHeight, requiredHeight);
            }

            row.CustomHeight = true;
            row.Height = maxHeight;
        }


        // Вспомогательный метод для расчёта высоты на основе текста и ширины ячейки
        private double CalculateRequiredHeight(string text, double cellWidth)
        {
            // Приблизительный расчёт, нужно подстроить под ваш конкретный случай
            double approximateCharWidth = 1.1; // Подберите значение экспериментально
            double maxCharsPerRow = cellWidth / approximateCharWidth;
            int numRows = (int)Math.Ceiling(text.Length / maxCharsPerRow);

            var splitText = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            int newLineCount = splitText.Length - 1;
            foreach (var line in splitText)
            {
                newLineCount += (int)Math.Ceiling(line.Length / maxCharsPerRow) -1 ;
            }

            numRows = Math.Max(newLineCount, numRows);

            double singleRowHeight = 14.5; // Подберите значение экспериментально

            return numRows * singleRowHeight; // Возвращает расчётную высоту
        }

        public void AddImageToExcel(ImageStorage? imageStorage, ExcelWorksheet sheet, int startRow, int startColumn, int endRow, int endColumn)
        {
            if (imageStorage == null || imageStorage.ImageBase64 == null)
            {
                return;
            }

            // Преобразуем строку Base64 в массив байтов
            byte[] imageBytes = Convert.FromBase64String(imageStorage.ImageBase64);

            // Добавляем рабочий лист
            var worksheet = sheet;

            // Создаем MemoryStream из массива байтов изображения
            using (var stream = new MemoryStream(imageBytes))
            {
                // Загружаем изображение с помощью System.Drawing, чтобы получить его размеры
                using (var image = Image.FromStream(stream))
                {
                    double imageWidth = image.Width;
                    double imageHeight = image.Height;

                    // Добавляем изображение на рабочий лист
                    var picture = sheet.Drawings.AddPicture("ExecutionScheme", new MemoryStream(imageBytes));

                    // Вычисляем размеры ячеек, чтобы уместить изображение
                    var pointToPixel = 1.5; //96d / 72d;

                    double totalWidth = 0;
                    double totalHeight = 0;

                    // Расчет ширины изображения в пикселях, учитывая только видимые столбцы
                    for (int col = startColumn; col <= endColumn; col++)
                    {
                        var column = worksheet.Column(col);
                        if (!column.Hidden) // Проверяем, что столбец не скрыт
                        {
                            totalWidth += column.Width * 7; // 7 - приблизительное количество пикселей на единицу ширины столбца
                        }
                    }

                    for (int row = startRow; row <= endRow; row++)
                    {
                        var rowHeight = sheet.Row(row).Height;
                        totalHeight += sheet.Row(row).Height * pointToPixel; // высота строки в пикселях
                    }

                    // Вычисление масштаба по ширине и высоте
                    double scaleWidth = totalWidth / imageWidth;
                    double scaleHeight = totalHeight / imageHeight;

                    // Определение наибольшего ограничения и установка масштабов
                    double scale = Math.Min(scaleWidth, scaleHeight);
                    double finalWidth = imageWidth * scale;
                    double finalHeight = imageHeight * scale;

                    // Устанавливаем положение изображения в указанной ячейке
                    picture.SetPosition(startRow - 1, 0, startColumn - 1, 0); // Минус 1, потому что строки и столбцы начинаются с 1
                    picture.SetSize((int)finalWidth, (int)finalHeight);
                }

                
                //// Устанавливаем положение изображения в ячейке (например, A1)
                //picture.SetPosition(startRow, 0, startColumn, 0); // row, rowOffsetPixels, col, colOffsetPixels
                //picture.SetSize(200, 200); // Ширина и высота изображения
            }

            Console.WriteLine("Изображение добавлено и файл сохранен.");
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
