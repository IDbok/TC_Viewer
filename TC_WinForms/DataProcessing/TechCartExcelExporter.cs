using ExcelParsing.DataProcessing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC_WinForms.Extensions;
using TC_WinForms.WinForms.Work;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.DataProcessing
{
    public class TechCartExcelExporter
    {
        #region Fields
        private ExcelExporter _exporter;
        private Dictionary<long, string> _relatedTcNames;

        private readonly Color _lightGey = Color.FromArgb(242, 242, 242);

        private readonly Color _toolColor = Color.FromArgb(221, 235, 247);
        private readonly Color _componentColor = Color.FromArgb(252, 228, 214);

        private readonly Color _yellow = Color.FromArgb(255, 255, 0);

        private Dictionary<int, double> _columnWidths = new Dictionary<int, double>
            {
                { 1, 3.45 },
                { 2, 11 },
                { 3, 4.27 },
                { 4, 19.27 },
                { 5, 16.27 },
                { 6, 6.27 },
                { 7, 6.82 },
                { 8, 6.82}, // Квалификация, Стоим., руб. без НДС, Время выполнения этапа, мин.
            };
        private Dictionary<int, double> _addingColumnWidths = new Dictionary<int, double>
            {
                { 1, 6.82 }, // № СЗ
                { 2, 32 }, // Примечание
                { 5, 6.82 }, // Обозначение, Рисунок
            };

        // Название столбцов для таблицы и их номер столбца в Excel для Механизмов, СЗ и Интсрументов
        private Dictionary<string, int> structHeadersColumns = new Dictionary<string, int>
            {
                { "№", 1 },
                { "Наименование", 2 },
                { "Тип (исполнение)", 5 },
                { "Ед. Изм.", 6 },
                { "Кол-во", 7 },
                { "конец", 8 }
            };

        private Dictionary<Machine_TC, int> machineColumnNumber = new Dictionary<Machine_TC, int>();

        private readonly double _defaultRowHeight = 14.5;
        private readonly double _defaultColumnWidth = 6.82;
        private readonly double _defaulHeaderHeight = 40;
        #endregion

        #region Contructor
        public TechCartExcelExporter()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            _exporter = new ExcelExporter();
        }
        #endregion

        #region ExportToExcel

        public void ExportTCtoFile(ExcelPackage _excelPackage, TechnologicalCard tc, List<TechOperationDataGridItem> tcRowItems, Color tabColor, Dictionary<long, string> relatedTcNames)
        {
            string sheetName = $"Ход работ {tc.Article}";
            var machine_TCs = tc.Machine_TCs.OrderBy(x => x.Order).ToList();
            _relatedTcNames = relatedTcNames;

            CompliteColumnsWidthWithMachines(machine_TCs.Count);

            var sheet = _excelPackage.Workbook.Worksheets[sheetName] ?? _excelPackage.Workbook.Worksheets.Add(sheetName);

            sheet.TabColor = tabColor;
            sheet.HeaderFooter.OddHeader.CenteredText = tc.Article;
            sheet.HeaderFooter.OddFooter.CenteredText = "Лист &P";

            SetColumnWigth(sheet);

            var lastRow = AddStaffDataToExcel(tc.Staff_TCs.OrderBy(x => x.Order).ToList(), sheet, 3);

            lastRow = AddComponentDataToExcel(tc.Component_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            var headerMachineRow = lastRow;
            lastRow = AddMachineDataToExcel(machine_TCs, sheet, lastRow + 1);

            lastRow = AddProtectionDataToExcel(tc.Protection_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            lastRow = AddToolDataToExcel(tc.Tool_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);
            lastRow = AddTechOperationDataToExcel(tcRowItems, machine_TCs, sheet, lastRow + 1);

            HideMachineColumns(sheet);
            SetTCName(sheet, tc);
            SetPrinterSettings(sheet);
        }

        #endregion

        #region TcPrintMethods

        public int AddStaffDataToExcel(List<Staff_TC> staff_tcList, ExcelWorksheet sheet, int headRow)
        {
            Dictionary<string, int> headersColumns = new Dictionary<string, int>
            {
                { "№", 1 },
                { "Наименование", 2 },
                { "Тип (исполнение)", 5 },
                { "Возможность совмещения обязанностей", 6 },
                { "Квалификация", 8 },
                { "Обозначение в ТК", _columnWidths.Keys.Max() },
                {"конец", _columnWidths.Keys.Max()+1 }
            };
            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "1. Требования к составу бригады и квалификации", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var staff_tc in staff_tcList)
            {
                var staff = staff_tc.Child;
                // Номер и наименование
                sheet.Cells[currentRow, headersColumns["№"]].Value = staff_tc.Order;
                sheet.Cells[currentRow, headersColumns["Наименование"]].Value = staff?.Name;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Value = staff?.Type;

                // Возможность совмещения обязанностей и квалификация
                sheet.Cells[currentRow, headersColumns["Возможность совмещения обязанностей"]].Value = staff?.CombineResponsibility;
                sheet.Cells[currentRow, headersColumns["Квалификация"]].Value = staff?.Qualification;

                // Обозначение в ТК
                sheet.Cells[currentRow, headersColumns["Обозначение в ТК"]].Value = staff_tc.Symbol;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.WrapText = true;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Style.WrapText = true;
                sheet.Cells[currentRow, headersColumns["Возможность совмещения обязанностей"]].Style.WrapText = true;
                sheet.Cells[currentRow, headersColumns["Квалификация"]].Style.WrapText = true;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet.Cells[currentRow, headersColumns["Квалификация"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet.Cells[currentRow, headersColumns["Обозначение в ТК"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = _defaulHeaderHeight;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[5], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddComponentDataToExcel(List<Component_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            Dictionary<string, int> headersColumns = new Dictionary<string, int>
            {
                { "№", 1 },
                { "Наименование", 2 },
                { "Тип (исполнение)", 5 },
                { "Ед. Изм.", 6 },
                { "Кол-во", 7 },
                { "Стоим., руб. без НДС", 8 },
                { "Примечание", 9 },
                {"конец", _columnWidths.Keys.Max() - 1 }
            };
            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();

            // Добавление заголовков
            //string[] headers = new string[] { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во"};
            //int[] columnNums = new int[] { 1, 2, 4, 5, 6, 7 }; // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "2. Требования к материалам и комплектующим", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var obj_tc in object_tcList)
            {
                var child = obj_tc.Child;

                sheet.Cells[currentRow, headersColumns["№"]].Value = obj_tc.Order;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Value = child?.Name;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Value = child?.Type;
                sheet.Cells[currentRow, headersColumns["Ед. Изм."]].Value = child?.Unit;

                sheet.Cells[currentRow, headersColumns ["Кол-во"]].Value = obj_tc.Quantity;

                sheet.Cells[currentRow, headersColumns["Стоим., руб. без НДС"]].Value = obj_tc.Child?.Price;
                sheet.Cells[currentRow, headersColumns["Примечание"]].Value = obj_tc.Note;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.WrapText = true;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Style.WrapText = true;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet.Cells[currentRow, headersColumns["Примечание"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                //_exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = _defaulHeaderHeight;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddMachineDataToExcel(List<Machine_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            Dictionary<string, int> headersColumns = structHeadersColumns;
            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "3. Требования к механизмам", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var obj_tc in object_tcList)
            {
                var child = obj_tc.Child;

                sheet.Cells[currentRow, headersColumns["№"]].Value = obj_tc.Order;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Value = child?.Name;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Value = child?.Type;
                sheet.Cells[currentRow, headersColumns["Ед. Изм."]].Value = child?.Unit;

                sheet.Cells[currentRow, headersColumns["Кол-во"]].Value = obj_tc.Quantity;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.WrapText = true;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Style.WrapText = true;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = _defaulHeaderHeight;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddProtectionDataToExcel(List<Protection_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            Dictionary<string, int> headersColumns = structHeadersColumns;
            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "4. Требования к средствам защиты", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var obj_tc in object_tcList)
            {
                var child = obj_tc.Child;

                sheet.Cells[currentRow, headersColumns["№"]].Value = obj_tc.Order;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Value = child?.Name;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Value = child?.Type;
                sheet.Cells[currentRow, headersColumns["Ед. Изм."]].Value = child?.Unit;

                sheet.Cells[currentRow, headersColumns["Кол-во"]].Value = obj_tc.Quantity;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.WrapText = true;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Style.WrapText = true;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                //_exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = _defaulHeaderHeight;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddToolDataToExcel(List<Tool_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            Dictionary<string, int> headersColumns = structHeadersColumns;
            //    new Dictionary<string, int>
            //{
            //    { "№", 1 },
            //    { "Наименование", 2 },
            //    { "Тип (исполнение)", 5 },
            //    { "Ед. Изм.", 6 },
            //    { "Кол-во", 7 },
            //    {"конец", 8 }
            //};
            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "5. Требования к инструментам и приспособлениям", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            // Добавление данных
            int currentRow = headRow + 1;
            foreach (var obj_tc in object_tcList)
            {
                var child = obj_tc.Child;

                sheet.Cells[currentRow, headersColumns["№"]].Value = obj_tc.Order;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Value = child?.Name;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Value = child?.Type;
                sheet.Cells[currentRow, headersColumns["Ед. Изм."]].Value = child?.Unit;

                sheet.Cells[currentRow, headersColumns["Кол-во"]].Value = obj_tc.Quantity;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.WrapText = true;
                sheet.Cells[currentRow, headersColumns["Тип (исполнение)"]].Style.WrapText = true;

                sheet.Cells[currentRow, headersColumns["Наименование"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                //_exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

                // Переход к следующей строке
                currentRow++;
            }

            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = _defaulHeaderHeight;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddTechOperationDataToExcel(List<TechOperationDataGridItem> tcRowItems, List<Machine_TC> machine_TCs, ExcelWorksheet sheet, int headRow)
        {
            if (tcRowItems.Count == 0)
            {
                return headRow;
            }

            Dictionary<string, int> headersColumns = new Dictionary<string, int>
            {
                { "№", 1 },//0
                { "Технологические операции", 2 },//1
                { "Исполнитель", 3 },//2
                { "Технологические переходы", 4 },//3
                { "Время действ., мин.", 7 },//4
                { "Время этапа, мин.", 8 },//5
            };

            Dictionary<string, int> additinghHadersColumns = new Dictionary<string, int>
            { {"№ СЗ" ,1}, { "Примечание", 2 }, { "Рисунок", 5 }, { "конец", 6 } };

            int lastColumn = headersColumns["Время этапа, мин."];

            var machineColumnNums = new List<int>();
            foreach (var machine_tc in machine_TCs)
            {
                var machineColumnNum = ++lastColumn;
                headersColumns.Add($"Время {machine_tc.Child.Name.ToLower()}({machine_tc.Child.Type}), мин.", machineColumnNum); // todo: форматировать название механизма в соответствии с его склонением
                machineColumnNumber.Add(machine_tc, machineColumnNum);

            }

            foreach (var additinghHader in additinghHadersColumns)
            {
                headersColumns.Add(additinghHader.Key, additinghHader.Value + lastColumn);
            }

            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.ToArray(); // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "6. Выполнение работ", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            _exporter.ApplyCellFormatting(sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1]);

            int currentRow = headRow + 1;

            string currentTOName = tcRowItems.FirstOrDefault().TechOperation;
            string currentTimeEtap = tcRowItems.FirstOrDefault().TimeEtap;
            int startTO = currentRow;
            int startTimeEtap = currentRow;

            foreach (var rowItem in tcRowItems)
            {
                sheet.Cells[currentRow, headersColumns["№"]].Value = rowItem.Nomer;
                sheet.Cells[currentRow, headersColumns["Технологические операции"]].Value = rowItem.TechOperation;
                sheet.Cells[currentRow, headersColumns["Исполнитель"]].Value = rowItem.Staff;

                if(rowItem.WorkItem is ExecutionWork executionWork && executionWork.Repeat)
                {
                    var listRepeatedOtemOrders = ((ExecutionWork)rowItem.WorkItem).ExecutionWorkRepeats
                        .Select(x => x.ChildExecutionWork.RowOrder).ToList();

                    var name = $"{rowItem.TechTransition}" +
                        $" {(((ExecutionWork)rowItem.WorkItem).techTransition.IsRepeatAsInTcTransition()
                            ? "п." + _relatedTcNames[((ExecutionWork)rowItem.WorkItem).RepeatsTCId.Value]
                            : string.Empty)
                            } {ConvertListToRangeString(listRepeatedOtemOrders)}";

                    sheet.Cells[currentRow, headersColumns["Технологические переходы"]].Value = name;

                    ColorizeRange(sheet.Cells[currentRow, headersColumns["Технологические переходы"]], _yellow);
                }
                else
                    sheet.Cells[currentRow, headersColumns["Технологические переходы"]].Value = rowItem.TechTransition;

                sheet.Cells[currentRow, headersColumns["Время действ., мин."]].Value = rowItem.TechTransitionValue;
                sheet.Cells[currentRow, headersColumns["Время этапа, мин."]].Value = rowItem.TimeEtap.Equals("-1") ? "" : rowItem.TimeEtap;
                sheet.Cells[currentRow, headersColumns["№ СЗ"]].Value = rowItem.Protections;

                if (rowItem.WorkItem is ExecutionWork ew && ew.ImageList.Count > 1 != null)
                {
                    var pictureNums = ew.ImageList
                    .Select(img => img.Number)
                    .Where(n => n > 0)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();
                    // Рисунок
                    sheet.Cells[currentRow, headersColumns["Рисунок"]].Value = pictureNums.Count == 0 ? "" : "Рис." + ConvertListToRangeString(pictureNums);
                }

                sheet.Cells[currentRow, headersColumns["Примечание"]].Value = rowItem.Comments;
                ColorizeRange(sheet.Cells[currentRow, headersColumns["Примечание"], currentRow, headersColumns.Last().Value - 1], _lightGey);
                AddMachineData(sheet, currentRow, rowItem, machine_TCs);

                TryMergeToColumn(rowItem);
                TryMergeTimeColumn(rowItem);

                if(rowItem.WorkItemType == WorkItemType.ComponentWork)
                {
                    ColorizeRange(sheet.Cells[currentRow, headersColumns["Технологические переходы"], currentRow, headersColumns["Время этапа, мин."]], _componentColor);
                }
                else if (rowItem.WorkItemType == WorkItemType.ToolWork)
                {
                    ColorizeRange(sheet.Cells[currentRow, headersColumns["Технологические переходы"], currentRow, headersColumns["Время этапа, мин."]], _toolColor);
                }

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

                RowFormat();

                currentRow++;
            }

            return currentRow;

            void RowFormat()
            {
                // Применяем стили для всех ячеек
                _exporter.ApplyCellFormatting(sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1]);

                FormatCells(sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1]);
                //_exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);
                sheet.Cells[currentRow, columnNums[1], currentRow, columnNums[4] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                //sheet.Row(currentRow).CustomHeight = true; // Разрешить изменение высоты
                //sheet.Row(currentRow).Height = -1;
            }

            void FormatCells(ExcelRange range)
            {
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Включаем перенос слов в ячейке
                range.Style.WrapText = true;
            }

            void ColorizeRange(ExcelRange range, Color color)
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(color);

            }

            void TryMergeToColumn(TechOperationDataGridItem rowItem)
            {
                if ((currentTOName != rowItem.TechOperation && startTO != currentRow) || tcRowItems.Last() == rowItem)
                {
                    currentTOName = rowItem.TechOperation;

                    if (tcRowItems.Last() == rowItem)
                        sheet.Cells[startTO, headersColumns["Технологические операции"], currentRow, headersColumns["Технологические операции"]].Merge = true;
                    else
                        sheet.Cells[startTO, headersColumns["Технологические операции"], currentRow - 1, headersColumns["Технологические операции"]].Merge = true;
                    //sheet.Cells[startTO, headersColumns["Время этапа, мин."], currentRow - 1, headersColumns["Время этапа, мин."]].Merge = true;
                    startTO = currentRow;
                }
            }

            void TryMergeTimeColumn(TechOperationDataGridItem rowItem)
            {
                if ((!currentTimeEtap.Equals(rowItem.TimeEtap) && !rowItem.TimeEtap.Equals("-1") && startTimeEtap != currentRow) || tcRowItems.Last() == rowItem)
                {
                    currentTimeEtap = rowItem.TimeEtap;

                    if (tcRowItems.Last() != rowItem)
                        sheet.Cells[startTimeEtap, headersColumns["Время этапа, мин."], currentRow - 1, headersColumns["Время этапа, мин."]].Merge = true;
                    else
                        sheet.Cells[startTimeEtap, headersColumns["Время этапа, мин."], currentRow, headersColumns["Время этапа, мин."]].Merge = true;

                    startTimeEtap = currentRow;
                }
            }
        }

        #endregion

        #region SupportMethods

        string ConvertListToRangeString(List<int> numbers)
        {
            if (numbers == null || !numbers.Any())
                return string.Empty;

            // Сортировка списка
            numbers.Sort();

            StringBuilder stringBuilder = new StringBuilder();
            int start = numbers[0];
            int end = start;

            for (int i = 1; i < numbers.Count; i++)
            {
                // Проверяем, идут ли числа последовательно
                if (numbers[i] == end + 1)
                {
                    end = numbers[i];
                }
                else
                {
                    // Добавляем текущий диапазон в результат
                    if (start == end)
                        stringBuilder.Append($"{start}, ");
                    else
                        stringBuilder.Append($"{start}-{end}, ");

                    // Начинаем новый диапазон
                    start = end = numbers[i];
                }
            }

            // Добавляем последний диапазон
            if (start == end)
                stringBuilder.Append($"{start}");
            else
                stringBuilder.Append($"{start}-{end}");

            return stringBuilder.ToString().TrimEnd(',', ' ');


        }

        private void AddMachineData(ExcelWorksheet sheet, int currentRow, TechOperationDataGridItem rowItem, List<Machine_TC> mach)
        {
            for (var index = 0; index < mach.Count; index++)
            {
                if (rowItem.listMach.Count > 0)
                {
                    bool isMachineChecked = rowItem.listMach[index];
                    var machColumn = machineColumnNumber.Where(m => m.Key.Child.Name == mach[index].Child.Name).Select(m => m.Value).FirstOrDefault();
                    if (machColumn != null && isMachineChecked)
                    {
                        sheet.Cells[currentRow, machColumn].Value = rowItem.TechTransitionValue;
                    }
                }
            }
        }

        private void HideMachineColumns(ExcelWorksheet sheet)
        {
            var machineColumnNums = machineColumnNumber.Values.ToList();

            if (machineColumnNums.Count > 0)
            {
                foreach (var machineColumnNum in machineColumnNums)
                {
                    var machineCol = sheet.Columns[machineColumnNum];
                    machineCol.Hidden = true;
                }
            }
        }

        private void SetTCName(ExcelWorksheet sheet, TechnologicalCard tc)
        {
            int[] columns = new int[] { 1, _columnWidths.Keys.Max() };
            // Объединить первую строку листа
            // Объединяем ячейки для текущего диапазона
            var cells = sheet.Cells[1, columns.First(), 1, columns.Last()];
            cells.Merge = true;

            // Установить имя ТК в объединённую ячейку
            cells.Value = string.IsNullOrEmpty(tc.Name) ? tc.Article : $"{tc.Name} ({tc.Article})";
            // Выровнять текст по центру

            cells.Style.Font.Bold = true;

            cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            cells.Style.Border.BorderAround(ExcelBorderStyle.Double);
            cells.Style.Fill.SetBackground(_lightGey);

        }

        #endregion

        #region PrintSettings
        public void CompliteColumnsWidthWithMachines(int newColumnNum)
        {
            var lastColumn = _columnWidths.Keys.Max();
            for (int i = 1; i <= newColumnNum; i++)
            {
                _columnWidths.Add(++lastColumn, _defaultColumnWidth);
            }

            foreach (var columnWidth in _addingColumnWidths)
            {
                _columnWidths.Add(columnWidth.Key + lastColumn, columnWidth.Value);
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
            printerSettings.HeaderMargin = 0.3m / 2.54m;
            printerSettings.FooterMargin = 0.3m / 2.54m;


            // Повторение строк заголовков на каждой странице печати
            printerSettings.RepeatRows = sheet.Cells["1:1"];

            //// Повторение столбцов на каждой странице печати
            //printerSettings.RepeatColumns = sheet.Cells["A:A"];
        }
        
        private void SetColumnWigth(ExcelWorksheet sheet)
        {
            foreach (var columnWidth in _columnWidths)
            {
                sheet.Column(columnWidth.Key).Width = columnWidth.Value * 1.15;
            }
        }

        #endregion
    }
}
