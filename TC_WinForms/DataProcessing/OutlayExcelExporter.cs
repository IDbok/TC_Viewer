using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent;
using TC_WinForms.Extensions;
using TcModels.Models;
using ExcelParsing.DataProcessing;

namespace TC_WinForms.DataProcessing
{
    public class OutlayExcelExporter
    {
        private ExcelPackage _excelPackage;
        private ExcelExporter _exporter;

        private readonly double _defaultRowHeight = 14.5;

        private Dictionary<int, double> _columnWidths = new Dictionary<int, double>
            {
                { 1, 3.45 },    //Номер записи
                { 2, 11 },      //Наименование
                { 5, 16.27 },   //Ед. измерения
                { 6, 6.27 },    //Итого
            };

        public OutlayExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
            _exporter = new ExcelExporter();
        }

        public OutlayExcelExporter(ExcelPackage excelPackage)
        {
            _excelPackage = excelPackage;
            _exporter = new ExcelExporter();
        }

        public static void ExportOutlatytoFile(ExcelPackage excelPackage, List<Outlay> outlays)
        {
            var outlayExporter = new OutlayExcelExporter();
            string sheetName = "Таблица затрат";
            // todo: add header of the table
            var sheet = excelPackage.Workbook.Worksheets[sheetName] ?? excelPackage.Workbook.Worksheets.Add(sheetName);

            outlayExporter.AddOutlayDataToExel(sheet, outlays);

            // Установка параметров для вывода на печать
            outlayExporter.SetPrinterSettings(sheet);
        }

        public void AddOutlayDataToExel(ExcelWorksheet sheet, List<Outlay> outlays)
        {
            int headRow = 2;
            Dictionary<string, int> headersColumns =
                new Dictionary<string, int>
                {
                    { "№", 1 },
                    { "Наименование", 2 },
                    { "Ед. Изм.", 5 },
                    { "Итого", 6 },
                    { "конец", 7 }
                };
            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();
            _exporter.AddTableHeader(sheet, "7. Таблица затрат", headRow - 1, columnNums);
            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);
            //// Добавление данных
            int currentRow = headRow + 1;
            var order = 1;
            foreach (Outlay outlay in outlays)
            {
                sheet.Cells[currentRow, columnNums[0]].Value = order;
                sheet.Cells[currentRow, columnNums[1]].Value = outlay.Name == null
                                                               ? outlay.Type.GetDescription()
                                                               : $"{outlay.Type.GetDescription()} ({outlay.Name})";
                sheet.Cells[currentRow, columnNums[2]].Value = outlay.OutlayUnitType.GetDescription();
                sheet.Cells[currentRow, columnNums[3]].Value = outlay.OutlayValue;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, columnNums[1]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[2]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);
                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);
                currentRow++;
                order++;
            }
            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = 33;
            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);
        }

        private void SetPrinterSettings(ExcelWorksheet sheet)
        {
            // Настройка параметров печати
            var printerSettings = sheet.PrinterSettings;

            // Установка ориентации страницы: Ландшафтная
            printerSettings.Orientation = eOrientation.Landscape;

            // Установка размера бумаги: A4
            printerSettings.PaperSize = ePaperSize.A4;
            printerSettings.Scale = 85;

            // Установка полей страницы (в сантиметрах, если в дюймах - нужно умножить на 2.54)
            printerSettings.TopMargin = 1.0m / 2.54m;
            printerSettings.BottomMargin = 1.0m / 2.54m;
            printerSettings.LeftMargin = 1.0m / 2.54m;
            printerSettings.RightMargin = 1.0m / 2.54m;

            // Повторение строк заголовков на каждой странице печати
            printerSettings.RepeatRows = sheet.Cells["1:1"];

            //// Повторение столбцов на каждой странице печати
            //printerSettings.RepeatColumns = sheet.Cells["A:A"];
        }
    }
}
