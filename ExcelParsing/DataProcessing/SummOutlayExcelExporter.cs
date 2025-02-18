using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.TcContent;
using TcModels.Models;
using TC_WinForms.WinForms.Win7.Work;
using TcModels.Models.IntermediateTables;
using System.Collections;
using OfficeOpenXml.Style;
using TcDbConnector;
using static TcModels.Models.TechnologicalCard;

namespace ExcelParsing.DataProcessing
{
    public class SummOutlayExcelExporter
    {
        #region Fields

        private ExcelPackage _excelPackage;
        private ExcelExporter _exporter;

        private Dictionary<int, double> _columnWidths = new Dictionary<int, double>
            {
                { 1, 6.82 },    // № Записи
                { 2, 20 },      // Наименование ТК
                { 3, 20  },    // Тип ТК
                { 4, 20 },   // Параметр ТК
            };

        private Dictionary<int, double> _addingColumnWidths = new Dictionary<int, double>
            {
                { 1, 11 }, // Стоимость материалов
                { 2, 20 }, // Сводные затраты(ед изм.)
                { 3, 20 }, // Сводные затраты(стоимость)
            };

        private readonly double _defaultRowHeight = 14.5;
        private readonly double _defaultColumnWidth = 20;

        private List<string> _uniqueStaffName = new List<string>();
        private List<string> _uniqueMachines = new List<string>();

        private Dictionary<string, int> staffHeaders = new Dictionary<string, int> //Список обязательных для вывода кодировок персонала
            {
                { "ЭР1 Затраты электромонтера", 1 },//0
                { "ЭР2 Затраты электромонтера", 2 },//1
                { "ЭР3 Затраты электромонтера", 3 },//2
                { "ЭР4 Затраты электромонтера", 4 },//3
                { "ЭР5 Затраты электромонтера", 5 },//4
                { "М1 Затраты монтера", 6 },//5
                { "М2 Затраты монтера", 7 },//6
                { "М3 Затраты монтера", 8 },//7
                { "А1 Затраты арматурщика", 9 },//8
                { "А2 Затраты арматурщика", 10 },//9
                { "С1 Затраты сварщика", 11 },//10
                { "Г1 Затраты геодезиста", 12 },//11
                { "Г2 Затраты геодезиста", 13 },//12
                { "ПТО Затраты инженера", 14 },//13

            };
        #endregion

        #region Constructor

        public SummOutlayExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
            _exporter = new ExcelExporter();
        }

        #endregion

        #region SetSheetSettings

        private void CompliteColumnsWidth(int newColumnNum)
        {
            var lastColumn = _columnWidths.Keys.Max();
            for (int i = 1; i <= newColumnNum; i++)
            {
                _columnWidths.Add(lastColumn + 1, _defaultColumnWidth);
                lastColumn++;
            }

            foreach (var columnWidth in staffHeaders)
            {
                _columnWidths.Add(columnWidth.Value + lastColumn, _defaultColumnWidth);
            }

            lastColumn = _columnWidths.Last().Key;

            foreach (var columnWidth in _addingColumnWidths)
            {
                _columnWidths.Add(columnWidth.Key + lastColumn, columnWidth.Value);
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

        #region ExportToExcel
        public void ExportSummOutlayoFile(string fileFolderPath, List<SummaryOutlayDataGridItem> summaryOutlayDataGridItems)
        {
            string name = $"Сводная таблица затрат {DateTime.Today.ToString("yyyy-MM-dd")}";
            string filePath = fileFolderPath;

            using (MyDbContext context = new MyDbContext())
            {
                _uniqueMachines = context.Machines.Select(m => $"{m.Name}({m.Type})").ToList();
            }
            
            _uniqueStaffName = summaryOutlayDataGridItems.Select(s => s.listStaffStr)
                                                         .SelectMany(innerList => innerList.Select(tuple => tuple.StaffName.Split(" ")[0]))
                                                         .Distinct()
                                                         .Except(staffHeaders.Keys.Select(s => s.Split(' ')[0]).ToList())
                                                         .ToList();//Получаем список новых кодировок персонала, которые не указывались в забитом ранее списке

            CompliteColumnsWidth(_uniqueMachines.Count() + _uniqueStaffName.Count());
            CreateNewFile(filePath);

            var sheet = _excelPackage.Workbook.Worksheets[name] ?? _excelPackage.Workbook.Worksheets.Add(name);
            SetColumnWigth(sheet);

            AddSummaryOutlayDataToExcel(sheet, summaryOutlayDataGridItems, 2);

            // Установка параметров для вывода на печать
            SetPrinterSettings(sheet);

            _excelPackage.Save();
            _excelPackage.Dispose();
        }

        private void AddSummaryOutlayDataToExcel(ExcelWorksheet sheet, List<SummaryOutlayDataGridItem> summaryOutlayDataGridItems, int headRow)
        {
            if (summaryOutlayDataGridItems.Count == 0)
                return;

            Dictionary<string, int> headersColumns = new Dictionary<string, int>
            {
                { "№", 1 },//0
                { "Технологическая карта", 2 },//1
                { "Тип тех. процесса", 3 },//2
                { "Параметр", 4 },//3
            };

            Dictionary<string, int> additinghHadersColumns = new Dictionary<string, int>
            { {"Стоимость материалов" ,1}, { "Сводные затраты(ед. измерения)", 2 }, { "Сводные затраты(стоимость)", 3 }, { "конец", 4 } };

            int lastColumn = headersColumns.Last().Value;
            SetColumnHeaders();

            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.ToArray(); // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            _exporter.AddTableHeader(sheet, "Сводная таблица затрат", headRow - 1, columnNums);
            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);
            _exporter.ApplyCellFormatting(sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1]);
            _exporter.AutoFitRowHeightForMergedCells(sheet, headRow, _defaultRowHeight, _columnWidths, columnNums);

            var currentRow = headRow + 1;
            var rowOrder = 1;
            foreach (var item in summaryOutlayDataGridItems)
            {
                sheet.Cells[currentRow, headersColumns["№"]].Value = rowOrder;
                sheet.Cells[currentRow, headersColumns["Технологическая карта"]].Value = item.TcName;
                sheet.Cells[currentRow, headersColumns["Тип тех. процесса"]].Value = item.TechProcess;
                sheet.Cells[currentRow, headersColumns["Параметр"]].Value = item.Parameter;
                sheet.Cells[currentRow, headersColumns["Стоимость материалов"]].Value = item.ComponentOutlay;
                sheet.Cells[currentRow, headersColumns["Сводные затраты(ед. измерения)"]].Value = GetDescriptionUnit(item.UnitType);
                sheet.Cells[currentRow, headersColumns["Сводные затраты(стоимость)"]].Value = item.SummaryOutlayCost;

                foreach (var staff in item.listStaffStr)
                {
                    sheet.Cells[currentRow, FindValueByPartialKey(headersColumns, staff.StaffName.Split(" ")[0])].Value = staff.StaffOutlay;
                }

                foreach (var machine in item.listMachStr)
                {
                    sheet.Cells[currentRow, FindValueByPartialKey(headersColumns, machine.MachineName)].Value = machine.MachineOutlay;
                }

                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);
                RowFormat();

                currentRow++;
                rowOrder++;
            }

            void RowFormat()
            {
                // Применяем стили для всех ячеек
                _exporter.ApplyCellFormatting(sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1]);
                FormatCells(sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1]);
                sheet.Cells[currentRow, columnNums[1], currentRow, columnNums[4] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            void FormatCells(ExcelRange range)
            {
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Включаем перенос слов в ячейке
                range.Style.WrapText = true;
            }

            void SetColumnHeaders()
            {
                foreach (var machine in _uniqueMachines)
                {
                    var machineColumnNum = ++lastColumn;
                    headersColumns.Add(machine, machineColumnNum); 
                }

                foreach (var staff in staffHeaders)
                {
                    headersColumns.Add(staff.Key, staff.Value + lastColumn);
                }

                lastColumn = headersColumns.Last().Value;

                foreach (var staff in _uniqueStaffName)
                {
                    var staffColumnNum = ++lastColumn;
                    headersColumns.Add(staff, staffColumnNum);
                }

                foreach (var additinghHader in additinghHadersColumns)
                {
                    headersColumns.Add(additinghHader.Key, additinghHader.Value + lastColumn);
                }
            }
        }

        #endregion

        #region SupportMethods

        public int FindValueByPartialKey(Dictionary<string, int> headerColumns, string partialKey)
        {
            // Ищем ключ, который содержит partialKey
            var match = headerColumns
                .FirstOrDefault(x => x.Key.Contains(partialKey, StringComparison.OrdinalIgnoreCase));

            // Если ключ найден, возвращаем значение
            if (!string.IsNullOrEmpty(match.Key))
            {
                return match.Value;
            }
            else
                return 0;
        }

        public string GetDescriptionUnit(TechnologicalCardUnit technologicalCardUnit)
        {
            switch(technologicalCardUnit)
            {
                case TechnologicalCardUnit.Pieces:
                    return "Шт.";
                case TechnologicalCardUnit.FiveHundredM:
                    return "500 м.";
                case TechnologicalCardUnit.OneHundredM:
                    return "100 м.";
                case TechnologicalCardUnit.Kilometer:
                    return "1 км.";
                default:
                    return "NA";
            }
        }
        #endregion

        #region ExcelMethods

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

        #endregion

    }
}
