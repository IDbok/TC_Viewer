using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using TcModels.Models;
using TcModels.Models.IntermediateTables;

namespace ExcelParsing.DataProcessing
{
    public class TCExcelExporter
    {
        private ExcelPackage _excelPackage;
        private ExcelExporter _exporter;

        private Color _lightGreen = Color.FromArgb(197, 224, 180);
        private Color _lightYellow = Color.FromArgb(237, 125, 49);
        private Color _lightGey = Color.FromArgb(242, 242, 242);

        public TCExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
            _exporter = new ExcelExporter();
        }

        public void CreateTC(string fileFolderPath, TechnologicalCard tc)
        {
            string article = tc.Article;
            string filePath = fileFolderPath + article + ".xlsx";

            CreateNewFile(filePath);

            // todo: add header of the table
            var sheet = _excelPackage.Workbook.Worksheets[article] ?? _excelPackage.Workbook.Worksheets.Add(article);

            SetColumnWigth(sheet);

            var lastRow = AddStaffDataToExcel(tc.Staff_TCs.OrderBy(x => x.Order).ToList(), sheet, 3);

            lastRow = AddComponentDataToExcel(tc.Component_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            lastRow = AddMachineDataToExcel(tc.Machine_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            lastRow = AddProtectionDataToExcel(tc.Protection_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            lastRow = AddToolDataToExcel(tc.Tool_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            Save();
            Close();
        }

        public void CreateNewFile(string filePath)
        {
            // Создание нового файла Excel (если файл уже существует, он будет перезаписан)
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            _excelPackage = new ExcelPackage(fileInfo);
        }
        private void SetColumnWigth(ExcelWorksheet sheet)
        {
            // Ширина колонок
            sheet.Column(1).Width = 3.91;
            sheet.Column(2).Width = 16.92;
            sheet.Column(3).Width = 44;
            sheet.Column(4).Width = 19.27;
            sheet.Column(5).Width = 13.27;
            sheet.Column(6).Width = 11.91;
            sheet.Column(7).Width = 11.36;
            sheet.Column(8).Width = 11.36;
            sheet.Column(9).Width = 11;
            sheet.Column(10).Width = 2.73;
            sheet.Column(11).Width = 8;
            sheet.Column(12).Width = 8.8;
            sheet.Column(13).Width = 8.8;
            sheet.Column(14).Width = 14.4;
            sheet.Column(15).Width = 12.4;
        }
        public int AddStaffDataToExcel(List<Staff_TC> staff_tcList, ExcelWorksheet sheet, int headRow)
        {
            // Добавление заголовков
            string[] headers = new string[] { "№", "Наименование", "Тип (исполнение)", "Возможность совмещения обязанностей", "Квалификация", "Обозначение в ТК" };
            int[] columnNums = new int[] { 1, 2, 4, 5, 7, 14 ,15}; // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "1. Требования к составу бригады и квалификации", headRow - 1, columnNums);
            
            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var staff_tc in staff_tcList)
            {
                var staff = staff_tc.Child;
                // Номер и наименование
                sheet.Cells[currentRow, columnNums[0]].Value = staff_tc.Order;
                sheet.Cells[currentRow, columnNums[1]].Value = staff?.Name;
                sheet.Cells[currentRow, columnNums[2]].Value = staff?.Type;

                // Возможность совмещения обязанностей и квалификация
                sheet.Cells[currentRow, columnNums[3]].Value = staff?.CombineResponsibility;
                sheet.Cells[currentRow, columnNums[4]].Value = staff?.Qualification;

                // Обозначение в ТК
                sheet.Cells[currentRow, columnNums[5]].Value = staff_tc.Symbol;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, columnNums[3]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[4]].Style.WrapText = true;

                sheet.Cells[currentRow, columnNums[4]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = 33;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow-1, columnNums[0], currentRow-1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[5], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddComponentDataToExcel(List<Component_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            // Добавление заголовков
            string[] headers = new string[] { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во"};
            int[] columnNums = new int[] { 1, 2, 4, 5, 6, 7 }; // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "2. Требования к материалам и комплектующим", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var obj_tc in object_tcList)
            {
                var child = obj_tc.Child;

                sheet.Cells[currentRow, columnNums[0]].Value = obj_tc.Order;

                sheet.Cells[currentRow, columnNums[1]].Value = child?.Name;
                sheet.Cells[currentRow, columnNums[2]].Value = child?.Type;
                sheet.Cells[currentRow, columnNums[3]].Value = child?.Unit;

                sheet.Cells[currentRow, columnNums[4]].Value = obj_tc.Quantity;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, columnNums[1]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[2]].Style.WrapText = true;

                sheet.Cells[currentRow, columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = 33;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddMachineDataToExcel(List<Machine_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            // Добавление заголовков
            string[] headers = new string[] { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во" };
            int[] columnNums = new int[] { 1, 2, 4, 5, 6, 7 }; // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "3. Требования к механизмам", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var obj_tc in object_tcList)
            {
                var child = obj_tc.Child;

                sheet.Cells[currentRow, columnNums[0]].Value = obj_tc.Order;

                sheet.Cells[currentRow, columnNums[1]].Value = child?.Name;
                sheet.Cells[currentRow, columnNums[2]].Value = child?.Type;
                sheet.Cells[currentRow, columnNums[3]].Value = child?.Unit;

                sheet.Cells[currentRow, columnNums[4]].Value = obj_tc.Quantity;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, columnNums[1]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[2]].Style.WrapText = true;

                sheet.Cells[currentRow, columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = 33;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddProtectionDataToExcel(List<Protection_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            // Добавление заголовков
            string[] headers = new string[] { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во" };
            int[] columnNums = new int[] { 1, 2, 4, 5, 6, 7 }; // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "4. Требования к средствам защиты", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var obj_tc in object_tcList)
            {
                var child = obj_tc.Child;

                sheet.Cells[currentRow, columnNums[0]].Value = obj_tc.Order;

                sheet.Cells[currentRow, columnNums[1]].Value = child?.Name;
                sheet.Cells[currentRow, columnNums[2]].Value = child?.Type;
                sheet.Cells[currentRow, columnNums[3]].Value = child?.Unit;

                sheet.Cells[currentRow, columnNums[4]].Value = obj_tc.Quantity;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, columnNums[1]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[2]].Style.WrapText = true;

                sheet.Cells[currentRow, columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = 33;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }
        public int AddToolDataToExcel(List<Tool_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            // Добавление заголовков
            string[] headers = new string[] { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во" };
            int[] columnNums = new int[] { 1, 2, 4, 5, 6, 7 }; // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "5. Требования к инструментам и приспособлениям", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var obj_tc in object_tcList)
            {
                var child = obj_tc.Child;

                sheet.Cells[currentRow, columnNums[0]].Value = obj_tc.Order;

                sheet.Cells[currentRow, columnNums[1]].Value = child?.Name;
                sheet.Cells[currentRow, columnNums[2]].Value = child?.Type;
                sheet.Cells[currentRow, columnNums[3]].Value = child?.Unit;

                sheet.Cells[currentRow, columnNums[4]].Value = obj_tc.Quantity;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, columnNums[1]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[2]].Style.WrapText = true;

                sheet.Cells[currentRow, columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = 33;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }
        public void Save()
        {
            // Сохраняет изменения в пакете Excel
            _excelPackage.Save();
        }
        public void Close()
        {
            // Закрывает пакет и освобождает все связанные ресурсы
            _excelPackage.Dispose();
        }
    }
}
