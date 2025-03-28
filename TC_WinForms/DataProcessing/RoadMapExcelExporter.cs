using ExcelParsing.DataProcessing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.RoadMap;

namespace TC_WinForms.DataProcessing
{
    class RoadMapExcelExporter
    {
        private ExcelPackage _excelPackage;
        private ExcelExporter _exporter;

        private readonly double _defaultRowHeight = 35;
        private readonly double _defaultColumnWidth = 6.27;

        private Dictionary<int, double> _columnWidths = new Dictionary<int, double>
            {
                { 1, 3.45 },    //Номер записи
                { 2, 30 },      //Технологическая операция
                { 3, 11 },   //Персонал
                { 4, 35 },    //Примечание
            };

        public RoadMapExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
            _exporter = new ExcelExporter();
        }

        public void ExportRoadMaptoFile(ExcelPackage excelPackage, List<RoadMapItem> roadMapItems)
        {
            string sheetName = "Дорожная карта";
            // todo: add header of the table
            var sheet = excelPackage.Workbook.Worksheets[sheetName] ?? excelPackage.Workbook.Worksheets.Add(sheetName);

            CompliteColumnsWidth(roadMapItems.Select(s => s.SequenceData.Length).FirstOrDefault());
            SetColumnWigth(sheet);

            AddRoadMapDataToExel(sheet, roadMapItems);

            // Установка параметров для вывода на печать
            SetPrinterSettings(sheet);
        }

        private void AddRoadMapDataToExel(ExcelWorksheet sheet, List<RoadMapItem> roadMapItems)
        {
            int headRow = 2;
            var colimnCountTime = roadMapItems.Select(s => s.SequenceData.Length).FirstOrDefault();
            Dictionary<string, int> headersColumns =
                new Dictionary<string, int>
                {
                    { "№", 1 },
                    { "Технологическая операция", 2 },
                    { "Персонал", 3 },
                    { "Примечание", 4 },
                };
            headersColumns.Add("Время выполнения, мин", headersColumns["Примечание"] + 2);
            headersColumns.Add("конец", headersColumns["Время выполнения, мин"] + colimnCountTime);

            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();

            _exporter.AddTableHeader(sheet, "8. Дорожная карта", headRow - 1, columnNums);
            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            int row = headRow + 1;
            var order = 1;

            var orderedList = roadMapItems.OrderBy(r => r.Order);

            foreach (var item in orderedList)
            {
                sheet.Cells[row, headersColumns["№"]].Value = order;
                sheet.Cells[row, headersColumns["Технологическая операция"]].Value = item.TOName;
                sheet.Cells[row, headersColumns["Персонал"]].Value = item.Staffs;
                sheet.Cells[row, headersColumns["Примечание"]].Value = item.Note;
                sheet.Cells[row, headersColumns["Примечание"], row, headersColumns["Примечание"] + 1].Merge = true;

                for (int i = headersColumns["Время выполнения, мин"]; i < headersColumns["Время выполнения, мин"] + colimnCountTime; i++)
                {
                    if(item.SequenceData[i - 6] != 0)
                    {
                        sheet.Cells[row, i].Value = item.SequenceData[i - 6] == -1 ? "" : item.SequenceData[i - 6];
                        sheet.Cells[row, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[row, i].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }
                }
                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, row, _defaultRowHeight, _columnWidths, columnNums);
                RowFormat();
                row++;
                order++;
            }
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], row - 1, columnNums[columnNums.Length - 1] - 1]);


            void RowFormat()
            {
                // Применяем стили для всех ячеек
                _exporter.ApplyCellFormatting(sheet.Cells[row, columnNums[0], row, columnNums[columnNums.Length - 1] - 1]);
                FormatCells(sheet.Cells[row, columnNums[0], row, columnNums[columnNums.Length - 1] - 1]);
                //sheet.Cells[row, columnNums[1], row, columnNums[4] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }
            void FormatCells(ExcelRange range)
            {
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Включаем перенос слов в ячейке
                range.Style.WrapText = true;
            }
        }

        #region SetSheetSettings

        private void CompliteColumnsWidth(int newColumnNum)
        {
            var lastColumn = _columnWidths.Keys.Max();
            for (int i = 1; i <= newColumnNum; i++)
            {
                _columnWidths.Add(lastColumn + 1, _defaultColumnWidth);
                lastColumn++;
            }
        }
        private void SetColumnWigth(ExcelWorksheet sheet)
        {
            foreach (var columnWidth in _columnWidths)
            {
                sheet.Column(columnWidth.Key).Width = columnWidth.Value * 1.15;
            }
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
        }

        #endregion
    }
}
