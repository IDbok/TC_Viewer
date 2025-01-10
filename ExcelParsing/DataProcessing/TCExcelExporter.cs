using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace ExcelParsing.DataProcessing
{
    public class TCExcelExporter
    {
        private ExcelPackage _excelPackage;
        private ExcelExporter _exporter;

        private readonly Color _lightGreen = Color.FromArgb(197, 224, 180);
        private readonly Color _lightYellow = Color.FromArgb(237, 125, 49);
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

        private Dictionary<Machine_TC,int> machineColumnNumber = new Dictionary<Machine_TC, int>();

        private readonly  double _defaultRowHeight = 14.5;
        private readonly double _defaultColumnWidth = 6.82;

        public TCExcelExporter()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
            _exporter = new ExcelExporter();
        }
        public void CompliteColumnsWidthWithMachines( int newColumnNum)
        {
            var lastColumn = _columnWidths.Keys.Max();
            for(int i = 1; i<= newColumnNum; i++)
            {
                _columnWidths.Add(++lastColumn, _defaultColumnWidth);
            }

            foreach (var columnWidth in _addingColumnWidths)
            {
                _columnWidths.Add(columnWidth.Key + lastColumn, columnWidth.Value);
            }
        }

        public void ExportTCtoFile(string fileFolderPath, TechnologicalCard tc, List<Outlay> outlays)
        {
            string article = tc.Article;
            string filePath = fileFolderPath;// + article + ".xlsx";
            var machine_TCs = tc.Machine_TCs.OrderBy(x => x.Order).ToList();

            CompliteColumnsWidthWithMachines(machine_TCs.Count());

            CreateNewFile(filePath);

            // todo: add header of the table
            var sheet = _excelPackage.Workbook.Worksheets[article] ?? _excelPackage.Workbook.Worksheets.Add(article);

            SetColumnWigth(sheet);



            var lastRow = AddStaffDataToExcel(tc.Staff_TCs.OrderBy(x => x.Order).ToList(), sheet, 3);

            lastRow = AddComponentDataToExcel(tc.Component_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            var headerMachineRow = lastRow;
            lastRow = AddMachineDataToExcel(machine_TCs, sheet, lastRow + 1);

            lastRow = AddProtectionDataToExcel(tc.Protection_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            lastRow = AddToolDataToExcel(tc.Tool_TCs.OrderBy(x => x.Order).ToList(), sheet, lastRow + 1);

            var headerWorkStepsRow = lastRow;
            lastRow = AddTechOperationDataToExcel(tc.TechOperationWorks.OrderBy(x => x.Order).ToList(), machine_TCs, sheet, lastRow + 1);

            lastRow = AddOutlayDataToExel(sheet, lastRow + 1, outlays);

            // Скрытие столбцов механизмов в Таблице хода работ
            HideMachineColumns(sheet);

            // Добавление схемы исполнения на уровне с таблицей механизмы
            AddExecutionSchemeToExcel(sheet, tc.ExecutionSchemeImageId, headerMachineRow , structHeadersColumns.Values.Max(), headerWorkStepsRow - 1);

            // Установка заголовка (шапки) ТК
            SetTCName(sheet, tc);

            // Установка параметров для вывода на печать
            SetPrinterSettings(sheet);

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
            foreach (var columnWidth in _columnWidths)
            {
                sheet.Column(columnWidth.Key).Width = columnWidth.Value * 1.15;
            }
        }
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
            string[] headers = headersColumns.Keys.Where(x=> !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
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
                sheet.Cells[currentRow, columnNums[1]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[2]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[3]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[4]].Style.WrapText = true;

                sheet.Cells[currentRow, columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet.Cells[currentRow, columnNums[4]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet.Cells[currentRow, columnNums[5]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

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
            row.Height = 40;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow-1, columnNums[0], currentRow-1, columnNums[columnNums.Length - 1] - 1]);

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

                sheet.Cells[currentRow, columnNums[0]].Value = obj_tc.Order;

                sheet.Cells[currentRow, columnNums[1]].Value = child?.Name;
                sheet.Cells[currentRow, columnNums[2]].Value = child?.Type;
                sheet.Cells[currentRow, columnNums[3]].Value = child?.Unit;

                sheet.Cells[currentRow, columnNums[4]].Value = obj_tc.Quantity;

                sheet.Cells[currentRow, columnNums[5]].Value = obj_tc.Child?.Price;
                sheet.Cells[currentRow, columnNums[6]].Value = obj_tc.Note;

                // Форматирование ячеек
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Включаем перенос слов в ячейке
                sheet.Cells[currentRow, columnNums[1]].Style.WrapText = true;
                sheet.Cells[currentRow, columnNums[2]].Style.WrapText = true;

                sheet.Cells[currentRow, columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet.Cells[currentRow, columnNums[6]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

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
            row.Height = 40;

            // Применяем стили для всех ячеек
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, columnNums[4], headRow + 1, currentRow - 1);

            return currentRow;
        }

        public int AddMachineDataToExcel(List<Machine_TC> object_tcList, ExcelWorksheet sheet, int headRow)
        {
            Dictionary<string, int> headersColumns = structHeadersColumns;
            //    new Dictionary<string, int>
            //{
            //    { "№", 1 },
            //    { "Наименование", 2 },
            //    { "Тип (исполнение)", 5 },
            //    { "Ед. Изм.", 6 },
            //    { "Кол-во", 7 },
            //    { "конец", 8 }
            //};
            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();

            //// Добавление заголовков
            //string[] headers = new string[] { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во" };
            //int[] columnNums = new int[] { 1, 2, 4, 5, 6, 7 }; // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

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
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

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
                //_exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

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
                //_exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, 15);
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

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

        public int AddTechOperationDataToExcel(List<TechOperationWork> object_tcList, List<Machine_TC> machine_TCs, ExcelWorksheet sheet, int headRow)
        {

            if (object_tcList.Count == 0)
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
                headersColumns.Add($"Время {machine_tc.Child.Name.ToLower()}, мин." , machineColumnNum); // todo: форматировать название механизма в соответствии с его склонением
                machineColumnNumber.Add(machine_tc, machineColumnNum);

            }

            foreach (var additinghHader in additinghHadersColumns)
            {
                headersColumns.Add(additinghHader.Key, additinghHader.Value + lastColumn);
            }

            // Добавление заголовков
            string[] headers = headersColumns.Keys.Where(x=> !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.ToArray(); // начала для всех столбцов, для последнего столбца указана позиция начала и конца после последней ячейки

            // Добавляем заголовок всей таблицы
            _exporter.AddTableHeader(sheet, "6. Выполнение работ", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            _exporter.ApplyCellFormatting(sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1]);

            // Добавление данных
            int currentRow = headRow + 1;
            int rowOrder = 1;

            string currentStage = "";
            int startStageRow = 0;
            ExecutionWork startStageEW = new ExecutionWork();
            List<ExecutionWork> currentStageEWList = new List<ExecutionWork>();

            var startItemRow =0;
            int itemCount = 0;

            int startTechOperationRow = 0;
            foreach (var obj_tc in object_tcList)
            {
                startTechOperationRow = currentRow;

                foreach (var executionWork in obj_tc.executionWorks)
                {
                    SetExecutionWorkData(executionWork, obj_tc, currentRow, columnNums,
                            ref startStageRow, ref currentStage, currentStageEWList);
                    // Переход к следующей строке
                    NextRow();
                }

                startItemRow = currentRow;
                itemCount = 0;

                // добавляем Компоненты и инструменты
                foreach (var component in obj_tc.ComponentWorks)
                {

                    sheet.Cells[currentRow, columnNums[0]].Value = rowOrder;

                    sheet.Cells[currentRow, 4].Value = component.component.Name;
                    sheet.Cells[currentRow, 5].Value = component.component.Type;
                    sheet.Cells[currentRow, 6].Value = component.component.Unit;
                    sheet.Cells[currentRow, 7].Value = component.Quantity;

                    ColorizeRange(sheet.Cells[currentRow, 4, currentRow, 7], _componentColor);

                    RowFormat();
                    sheet.Cells[currentRow, headersColumns["Примечание"], currentRow, headersColumns["Рисунок"] - 1].Merge = true;

                    itemCount++;
                    NextRow();
                }
                foreach (var component in obj_tc.ToolWorks)
                {
                    sheet.Cells[currentRow, columnNums[0]].Value = rowOrder;

                    sheet.Cells[currentRow, 4].Value = component.tool.Name;
                    sheet.Cells[currentRow, 5].Value = component.tool.Type;
                    sheet.Cells[currentRow, 6].Value = component.tool.Unit;
                    sheet.Cells[currentRow, 7].Value = component.Quantity;

                    ColorizeRange(sheet.Cells[currentRow, 4, currentRow, 7], _toolColor);

                    RowFormat();
                    sheet.Cells[currentRow, headersColumns["Примечание"], currentRow, headersColumns["Рисунок"] - 1].Merge = true;

                    itemCount++;
                    NextRow();
                }

                

                // Объединение ячеек между строками в столбце "Технологические операции"
                sheet.Cells[startTechOperationRow, columnNums[1], currentRow - 1, columnNums[1]].Merge = true;

                // Объединение ячеек между строками в столбце "Исполнитель" для компонентов и инструментов
                if (startItemRow <= currentRow - 1)
                {
                    if(startItemRow != currentRow - 1)// если в строке больше чем один элемент
                    {
                        SetBordersForRange(range: sheet.Cells[startItemRow, headersColumns["Исполнитель"], currentRow - 1, headersColumns["Исполнитель"]]);

                        SetBordersForRange(range: sheet.Cells[startItemRow, headersColumns["Время этапа, мин."], currentRow - 1, headersColumns["№ СЗ"]-1]);
                        //sheet.Cells[startItemRow, columnNums[2], currentRow - 1, columnNums[2]].Merge = true;Исполнитель
                    }
                }
            }

            SetStageTimes();


            // Высота строки заголовка
            var row = sheet.Row(headRow);
            row.CustomHeight = true;
            row.Height = 63;


            // Подсветка столбца с обозначением в ТК
            _exporter.ColorizeEditableColumn(sheet, headersColumns["Примечание"], headRow + 1, currentRow - 1);
            sheet.Cells[headRow + 1, headersColumns["Примечание"], currentRow - 1, headersColumns["Примечание"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            _exporter.ColorizeEditableColumn(sheet, headersColumns["Рисунок"], headRow + 1, currentRow - 1);
            sheet.Cells[headRow + 1, headersColumns["Рисунок"], currentRow - 1, headersColumns["Рисунок"]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            return currentRow;

            void NextRow()
            {
                currentRow++;
                rowOrder++;
            }

            void SetExecutionWorkData(ExecutionWork executionWork, TechOperationWork obj_tc, int currentRow, int[] columnNums, 
                ref int startStageRow, ref string currentStage, List<ExecutionWork> currentStageEWList)
            {
                if (executionWork.Repeat)
                {
                    // Номер и наименование
                    sheet.Cells[currentRow, headersColumns["№"]].Value = rowOrder;

                    sheet.Cells[currentRow, headersColumns["Технологические операции"]].Value = obj_tc.techOperation?.Name;

                    //var listRepeatedOtemOrders = executionWork.ListexecutionWorkRepeat2.Select(x => x.Order).ToList();
                    var listRepeatedOtemOrders = executionWork.ExecutionWorkRepeats
                        .Select(x => x.ChildExecutionWork.Order).ToList();
                    sheet.Cells[currentRow, headersColumns["Технологические переходы"]].Value = "Повторить п. "
                        + ConvertListToRangeString(listRepeatedOtemOrders);

                    ColorizeRange(sheet.Cells[currentRow, headersColumns["Технологические переходы"]], _yellow);

                    // Время действия и времени этапа
                    //var repeatTime = executionWork.ListexecutionWorkRepeat2.Sum(x => x.Value);
                    var repeatTime = executionWork.ExecutionWorkRepeats
                        .Sum(x => x.ChildExecutionWork.Value);
                    sheet.Cells[currentRow, headersColumns["Время действ., мин."]].Value = executionWork.Value != 0 ? executionWork.Value : repeatTime;

                }
                else
                {
                    // Номер и наименование
                    sheet.Cells[currentRow, headersColumns["№"]].Value = rowOrder;//executionWork.Order; columnNums[0]].Value

                    sheet.Cells[currentRow, headersColumns["Технологические операции"]].Value = obj_tc.techOperation?.Name ?? "error";

                    // Исполнитель
                    sheet.Cells[currentRow, headersColumns["Исполнитель"]].Value = SetStaffsString(executionWork.Staffs);

                    // Технологические переходы
                    sheet.Cells[currentRow, headersColumns["Технологические переходы"]].Value = executionWork.techTransition?.Name ?? "error";

                    // Время действия и времени этапа
                    sheet.Cells[currentRow, headersColumns["Время действ., мин."]].Value = executionWork.Value;
                }

                if (executionWork.Etap != currentStage)
                {
                    SetStageTimes();

                    startStageRow = currentRow;
                    currentStage = executionWork.Etap;

                    currentStageEWList.Clear();
                    currentStageEWList.Add(executionWork);
                    startStageEW = executionWork;
                }
                else
                {
                    currentStageEWList.Add(executionWork);
                }

                // Номер СЗ
                sheet.Cells[currentRow, headersColumns["№ СЗ"]].Value = ConvertListToRangeString(ExtractProtectionOrder(executionWork.Protections));

                // Примечание
                sheet.Cells[currentRow, headersColumns["Примечание"]].Value = executionWork.Comments;

                // Рисунок
                sheet.Cells[currentRow, headersColumns["Рисунок"]].Value = executionWork.PictureName;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

                RowFormat();
            }

            void SetRepeatData(ExecutionWork executionWork, TechOperationWork obj_tc, int currentRow, int[] columnNums)
            {
                // Номер и наименование
                sheet.Cells[currentRow, headersColumns["№"]].Value = rowOrder;

                sheet.Cells[currentRow, headersColumns["Технологические операции"]].Value = obj_tc.techOperation?.Name;

                var listRepeatedOtemOrders = executionWork.ListexecutionWorkRepeat2.Select(x => x.Order).ToList();
                sheet.Cells[currentRow, headersColumns["Технологические переходы"]].Value = "Повторить п. "
                    + ConvertListToRangeString(listRepeatedOtemOrders);

                // Время действия и времени этапа
                var repeatTime = executionWork.ListexecutionWorkRepeat2.Sum(x => x.Value);
                sheet.Cells[currentRow, headersColumns["Время действ., мин."]].Value = executionWork.Value != 0 ? executionWork.Value : repeatTime;

                if (executionWork.Etap != currentStage)
                {
                    if (startStageRow != 0)
                    {
                        sheet.Cells[startStageRow, headersColumns["Время этапа, мин."], currentRow - 1, headersColumns["Время этапа, мин."]].Merge = true;
                        double previousStageTime = GetTimeExecution(currentStageEWList);
                        sheet.Cells[startStageRow, headersColumns["Время этапа, мин."]].Value = previousStageTime;

                        // устанавливаем время этапа для механизмов
                        int column = 0;
                        foreach (var machine_tc in machine_TCs)
                        {
                            var machine = machineColumnNumber.Keys.Where(x => x.ChildId == machine_tc.ChildId).FirstOrDefault();
                            column = machineColumnNumber[machine];
                            sheet.Cells[startStageRow, column, currentRow - 1, column].Merge = true;

                            // устанавливаем время этапа для механизмов, которые учавствуют в этапе
                            if (executionWork.Machines.Select(x => x.ChildId).ToList().Contains(machine_tc.ChildId))
                            {
                                sheet.Cells[startStageRow, column].Value = previousStageTime;
                            }
                        }
                    }
                    startStageRow = currentRow;
                    currentStage = executionWork.Etap;
                    sheet.Cells[currentRow, columnNums[5]].Value = $"этап {executionWork.Etap}";//executionWork.TempTimeExecution;

                    currentStageEWList.Clear();
                    currentStageEWList.Add(executionWork);
                }
                else
                {
                    currentStageEWList.Add(executionWork);
                }

                // Номер СЗ
                sheet.Cells[currentRow, headersColumns["№ СЗ"]].Value = ConvertListToRangeString(ExtractProtectionOrder(executionWork.Protections));

                // Примечание
                sheet.Cells[currentRow, headersColumns["Примечание"]].Value = executionWork.Comments;

                // Объединение ячеек между столбцами
                _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                // Установка высоты строки
                _exporter.AutoFitRowHeightForMergedCells(sheet, currentRow, _defaultRowHeight, _columnWidths, columnNums);

                RowFormat();

                ColorizeRange(sheet.Cells[currentRow, headersColumns["Технологические переходы"]], _yellow);
            }

            void SetStageTimes()
            {
                if (startStageRow != 0)
                {
                    if(startItemRow + itemCount == currentRow)
                    {
                        sheet.Cells[startStageRow, headersColumns["Время этапа, мин."], currentRow - 1 - itemCount, headersColumns["Время этапа, мин."]].Merge = true;
                    } // если этап закончился до добавления компонентов и инструментов
                    else
                    {
                        sheet.Cells[startStageRow, headersColumns["Время этапа, мин."], currentRow - 1, headersColumns["Время этапа, мин."]].Merge = true;
                    }

                    double previousStageTime = GetTimeExecution(currentStageEWList);
                    sheet.Cells[startStageRow, headersColumns["Время этапа, мин."]].Value = previousStageTime;

                    // устанавливаем время этапа для механизмов
                    int column = 0;
                    foreach (var machine_tc in machine_TCs)
                    {
                        var machine = machineColumnNumber.Keys.Where(x => x.ChildId == machine_tc.ChildId).FirstOrDefault();
                        column = machineColumnNumber[machine];

                        if (startItemRow + itemCount == currentRow)
                        {
                            sheet.Cells[startStageRow, column, currentRow - 1 - itemCount, column].Merge = true;
                            _exporter.ApplyCellFormatting(sheet.Cells[startStageRow, column, currentRow - 1 - itemCount, column]);
                        }
                        else
                        {
                            sheet.Cells[startStageRow, column, currentRow - 1, column].Merge = true;
                            _exporter.ApplyCellFormatting(sheet.Cells[startStageRow, column, currentRow - 1, column]);
                        }
                        
                        // устанавливаем время этапа для механизмов, которые учавствуют в этапе
                        if (startStageEW.Machines.Select(x => x.ChildId).ToList().Contains(machine_tc.ChildId))
                        {
                            sheet.Cells[startStageRow, column].Value = previousStageTime;
                        }
                    }
                }
            }

            void RowFormat()
            {
                // Применяем стили для всех ячеек
                _exporter.ApplyCellFormatting(sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1]);

                FormatCells(sheet.Cells[currentRow , columnNums[0], currentRow, columnNums[columnNums.Length - 1] - 1]);
                sheet.Cells[currentRow, columnNums[1], currentRow, columnNums[4] - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }
            double GetTimeExecution(List<ExecutionWork> executionWorks)
            {
                var etapGroups = executionWorks.GroupBy(w => w.Etap);
                if (etapGroups.Count() != 1)
                {
                    Console.WriteLine("Ошибка в группировке по этапам");
                    return 0;
                }

                var posledGroups = executionWorks.GroupBy(w => w.Posled);
                double etapTotalTime = 0;

                foreach (var posledGroup in posledGroups)
                {
                    if (posledGroup.Key == "0" || posledGroup.Count() == 1) // Если Posled = 0 или группа уникальна, работаем как с параллельными задачами
                    {
                        etapTotalTime = Math.Max(etapTotalTime, posledGroup.Max(w => w.Value));
                    }
                    else // Если Posled не 0 и есть повторяющиеся элементы, работаем как с последовательными задачами
                    {
                        etapTotalTime = Math.Max(etapTotalTime, posledGroup.Sum(w => w.Value));
                    }
                }

                return etapTotalTime;
            }

            string SetStaffsString(List<Staff_TC> staffs_Tc)
            {
                var staffsSymbol = staffs_Tc.Select(x => x.Symbol).ToList();
                //var staffsNames = staffs.Select(x => x.).ToList();
                return string.Join(", ", staffsSymbol);
            }

            List<int> ExtractProtectionOrder(List<Protection_TC> protection_TCs)
            {
                return protection_TCs.Select(x => x.Order).ToList();
            }

            void ColorizeRange(ExcelRange range, Color color)
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(color);

            }
           
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

            void FormatCells(ExcelRange range)
            {
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Включаем перенос слов в ячейке
                range.Style.WrapText = true;
            }

            void UnFormatCells(ExcelRange cells)
            {
                // удалить границы
                cells.Style.Border.Top.Style = ExcelBorderStyle.None;
                cells.Style.Border.Left.Style = ExcelBorderStyle.None;
                cells.Style.Border.Right.Style = ExcelBorderStyle.None;
                cells.Style.Border.Bottom.Style = ExcelBorderStyle.None;
            }

            void SetBordersForRange(ExcelRange range)
            {
                // Получение адресов начальной и конечной ячеек диапазона
                var start = range.Start;
                var end = range.End;
                
                if (start == end)
                {
                    _exporter.ApplyCellFormatting(range.Worksheet.Cells[start.Row, start.Column]);
                    return;
                }
                UnFormatCells(range);

                // Установка границ для верхней строки диапазона
                for (int col = start.Column; col <= end.Column; col++)
                {
                    range.Worksheet.Cells[start.Row, col].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                }

                // Установка границ для нижней строки диапазона
                for (int col = start.Column; col <= end.Column; col++)
                {
                    range.Worksheet.Cells[end.Row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Установка границ для левого столбца диапазона
                for (int row = start.Row; row <= end.Row; row++)
                {
                    range.Worksheet.Cells[row, start.Column].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                }

                // Установка границ для правого столбца диапазона
                for (int row = start.Row; row <= end.Row; row++)
                {
                    range.Worksheet.Cells[row, end.Column].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
        }
        
        public int AddOutlayDataToExel(ExcelWorksheet sheet, int headRow, List<Outlay> outlays)
        {
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

            _exporter.AddTableHeader(sheet, "7. Таблица затрат", headRow-1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            //// Добавление данных
            int currentRow = headRow + 1;
            var order = 1;
            foreach (Outlay outlay in outlays)
            {
                sheet.Cells[currentRow, columnNums[0]].Value = order;
                sheet.Cells[currentRow, columnNums[1]].Value = outlay.Name == null
                                                               ? GetDescription(outlay.Type)
                                                               : $"{GetDescription(outlay.Type)} ({outlay.Name})";
                sheet.Cells[currentRow, columnNums[2]].Value = GetDescription(outlay.OutlayUnitType);
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

            return currentRow;
        }

        public void AddExecutionSchemeToExcel(ExcelWorksheet sheet, long? executionSchemeImageId, int row, int column, int endRow)
        {
            if (executionSchemeImageId != null)
            {
                ImageStorage? image;
                // Загрузить изображение схемы выполнения
                using (var dbCon = new MyDbContext())
                {
                    image = dbCon.ImageStorage.Where(i => i.Id == executionSchemeImageId).FirstOrDefault();
                }

                // Сохранить изображение в Excel по указанным координатам
                if (image != null)
                {
                    _exporter.AddImageToExcel(image, sheet, row +1, column, endRow, _columnWidths.Keys.Max());
                }

                // добавить подпись в начало изображения
                var headRow = row;
                var header = "Схема исполнения";
                sheet.Cells[headRow, column].Value = header;
                var mergeRange = sheet.Cells[headRow, column, headRow, _columnWidths.Keys.Max()];
                mergeRange.Merge = true;
                mergeRange.Style.Font.Bold = true;
                mergeRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                mergeRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                mergeRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 0, 0));
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

        private void SetTCName(ExcelWorksheet sheet, TechnologicalCard tc)
        {
            int[] columns = new int[] { 1, _columnWidths.Keys.Max()};
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

        public string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
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
