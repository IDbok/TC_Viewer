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

namespace ExcelParsing.DataProcessing
{
    public class SummOutlayExcelExporter
    {
        #region Fields

        private ExcelPackage _excelPackage;
        private ExcelExporter _exporter;

        private readonly Color _lightGreen = Color.FromArgb(197, 224, 180);

        private Dictionary<int, double> _columnWidths = new Dictionary<int, double>
            {
                { 1, 3.45 },    // № Записи
                { 2, 11 },      // Наименование ТК
                { 4, 11  },    // Тип ТК
                { 6, 6.82 },   // Параметр ТК
            };

        private Dictionary<int, double> _addingColumnWidths = new Dictionary<int, double>
            {
                { 1, 6.82 }, // Стоимость материалов
                { 3, 11 }, // Сводные затраты(ед изм.)
                { 5, 11 }, // Сводные затраты(стоимость)
            };

        private readonly double _defaultRowHeight = 14.5;
        private readonly double _defaultColumnWidth = 6.82;

        private List<string> _uniqueStaffName = new List<string>();
        private List<(string MachineName, double MachineOutlay, int MachineId)> _uniqueMachines = new List<(string MachineName, double MachineOutlay, int MachineId)>();

        private Dictionary<string, int> machineColumnNumber = new Dictionary<string, int>();
        private Dictionary<string, int> staffColumnNumber = new Dictionary<string, int>();

        #endregion

        #region Constructor

        public SummOutlayExcelExporter()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
            _exporter = new ExcelExporter();
        }

        #endregion

        #region SetSheetSettings

        private void CompliteColumnsWidth(int newColumnNum, int newColumnNum2)
        {
            var lastColumn = _columnWidths.Keys.Max();
            for (int i = 1; i <= newColumnNum; i++)
            {
                _columnWidths.Add(lastColumn + 2, _defaultColumnWidth);
                lastColumn += 2;
            }

            lastColumn++;

            for (int i = 1; i <= newColumnNum2; i++)
            {
                _columnWidths.Add(lastColumn + 1, _defaultColumnWidth);
                lastColumn += 1;
            }

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
            string filePath = fileFolderPath;// + article + ".xlsx";

            _uniqueMachines = summaryOutlayDataGridItems.Select(s => s.listMachStr)
                                                        .SelectMany(innerList => innerList)
                                                        .GroupBy(machine => (machine.MachineName, machine.MachineId))
                                                        .Select(group => group.First())
                                                        .ToList();

            _uniqueStaffName = summaryOutlayDataGridItems.Select(s => s.listStaffStr)
                                                         .SelectMany(innerList => innerList.Select(tuple => tuple.StaffName.Split(" ")[0]))
                                                         .Distinct()
                                                         .ToList();

            CompliteColumnsWidth(_uniqueMachines.Count(), _uniqueStaffName.Count());

            CreateNewFile(filePath);

            var sheet = _excelPackage.Workbook.Worksheets[name] ?? _excelPackage.Workbook.Worksheets.Add(name);

            SetColumnWigth(sheet);

            AddSummaryOutlayDataToExcel(sheet, summaryOutlayDataGridItems, 2);

            // Установка параметров для вывода на печать
            SetPrinterSettings(sheet);

            Save();
            Close();
        }

        private void AddSummaryOutlayDataToExcel(ExcelWorksheet sheet, List<SummaryOutlayDataGridItem> summaryOutlayDataGridItems, int headRow)
        {
            if (summaryOutlayDataGridItems.Count == 0)
                return;

            Dictionary<string, int> headersColumns = new Dictionary<string, int>
            {
                { "№", 1 },//0
                { "Технологическая карта", 2 },//1
                { "Тип тех. процесса", 4 },//2
                { "Параметр", 6 },//3
            };

            Dictionary<string, int> additinghHadersColumns = new Dictionary<string, int>
            { {"Стоимость материалов" ,1}, { "Сводные затраты(ед. измерения)", 3 }, { "Сводные затраты(стоимость)", 5 }, { "конец", 7 } };

            int lastColumn = headersColumns["Параметр"];
            var machineColumnNums = new List<int>();

            List<(int MachineColumnNum, int MachineChildId)> machinePairs = new List<(int MachineColumnNum, int MachineChildId)>();

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
                sheet.Cells[currentRow, headersColumns["Тип тех. процесса"]].Value = item.TechProcessType;
                sheet.Cells[currentRow, headersColumns["Параметр"]].Value = item.Parameter;
                sheet.Cells[currentRow, headersColumns["Стоимость материалов"]].Value = item.ComponentOutlay;
                sheet.Cells[currentRow, headersColumns["Сводные затраты(ед. измерения)"]].Value = item.UnitType.ToString();
                sheet.Cells[currentRow, headersColumns["Сводные затраты(стоимость)"]].Value = item.SummaryOutlayCost;

                for (int i = headersColumns["Параметр"] + 1; i < headersColumns["Стоимость материалов"]; i++)
                {
                    sheet.Cells[currentRow, i].Value = " - ";
                }

                foreach (var staff in item.listStaffStr)
                {
                    if (headersColumns[staff.StaffName.Split(" ")[0]] != null)
                    {
                        sheet.Cells[currentRow, headersColumns[staff.StaffName.Split(" ")[0]]].Value = staff.StaffOutlay;
                    }
                }

                foreach (var machine in item.listMachStr)
                {
                    var columnNum = machinePairs.Where(s => s.MachineChildId == machine.MachineId).FirstOrDefault();
                    if (columnNum != (0, 0))
                    {
                        sheet.Cells[currentRow, columnNum.MachineColumnNum].Value = machine.MachineOutlay;
                    }
                }

                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);
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
                    var machineColumnNum = 2 + lastColumn;
                    headersColumns.Add($"{machine.MachineName}(Id{machine.MachineId})", machineColumnNum); // todo: форматировать название механизма в соответствии с его склонением
                    machineColumnNumber.Add($"{machine.MachineName}(Id{machine.MachineId})", machineColumnNum);
                    machinePairs.Add((machineColumnNum, machine.MachineId));
                    lastColumn = machineColumnNum;
                }

                lastColumn++;

                foreach (var staff in _uniqueStaffName)
                {
                    var staffColumnNum = ++lastColumn;
                    headersColumns.Add(staff, staffColumnNum); // todo: форматировать название механизма в соответствии с его склонением
                    staffColumnNumber.Add(staff, staffColumnNum);
                }

                foreach (var additinghHader in additinghHadersColumns)
                {
                    headersColumns.Add(additinghHader.Key, additinghHader.Value + lastColumn);
                }
            }
        }

        #endregion

        #region SupportMethods

       

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

        #endregion

    }
}
