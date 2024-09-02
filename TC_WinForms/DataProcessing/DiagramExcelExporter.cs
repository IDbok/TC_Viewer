using ExcelParsing.DataProcessing;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing;
using System.IO;
using TcDbConnector;
using TcModels.Models.TcContent;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace TC_WinForms.DataProcessing
{
    internal class DiagramExcelExporter
    {
        private ExcelPackage _excelPackage;
        private ExcelExporter _exporter;

        private List<DiagamToWork> _diagramToWorks;

        private int _pageCount;
        private int _currentParallelShag, _currentParallelTO;
        private int _startCollumn;
        private int _parallelToCount;


        private readonly int _currentPrintHeigth = 43;
        private readonly int _currentPrintWidgth = 18;

        private bool isNextShagIsParallel = false;
        private bool isNextTOParallel = false;

        private Dictionary<string, Color> _toColors = new Dictionary<string, Color> ();
        private Dictionary<int, int>  _nextPagesLastRow = new Dictionary<int, int>();
        private Dictionary<int, int> _nextPagesLastCollumn = new Dictionary<int, int>();



        public DiagramExcelExporter()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
            _exporter = new ExcelExporter();
            _diagramToWorks = new List<DiagamToWork>();
        }

        public void ExportDiadramToExel(int tcId, string fileFolderPath)
        {
            string filePath = fileFolderPath;

            CreateNewFile(filePath);
            var context = new MyDbContext();

            var technologicalCard = context.TechnologicalCards.Single(x => x.Id == tcId);
            string article = technologicalCard.Article;

            _diagramToWorks = context.DiagamToWork.Where(w => w.technologicalCard == technologicalCard)
                                                        .Include(i => i.ListDiagramParalelno)
                                                            .ThenInclude(ie => ie.techOperationWork)
                                                        .Include(i => i.ListDiagramParalelno)
                                                            .ThenInclude(i => i.ListDiagramPosledov)
                                                            .ThenInclude(i => i.ListDiagramShag)
                                                            .ThenInclude(i => i.ListDiagramShagToolsComponent)
                                                        .OrderBy(o => o.Order)
                                                        .ToList();


            var dTOWGroups = _diagramToWorks
                .GroupBy(g => g.ParallelIndex != null ? g.GetParallelIndex() : g.Order.ToString())
                .ToList();

            dTOWGroups = dTOWGroups.OrderBy(o => o.FirstOrDefault()!.Order).ToList();

            var sheet = _excelPackage.Workbook.Worksheets[article] ?? _excelPackage.Workbook.Worksheets.Add(article);
            sheet.PrinterSettings.Scale = 80;
            sheet.PrinterSettings.Orientation = eOrientation.Landscape;
            sheet.PrinterSettings.RightMargin = 0.3M / 2.54M; //выделение места для объявления столбца с номером листа печати

            var currentRow = 2; //стартовая строчка расположения диаграм
            _pageCount = 1;//Присваиваем значение счетчику старниц, нумерация с 1 страницы

            foreach (var dTOWGroup in dTOWGroups)
            {
                bool isNull = dTOWGroup.Key == null;

                if (!isNull)
                    currentRow = AddTODiadramsToExcel(dTOWGroup.OrderBy(x => x.Order).ToList(), currentRow, sheet);
                else
                    currentRow = AddTODiadramsToExcel(dTOWGroup.ToList(), currentRow, sheet);
                    
            }

            Save();
            Close();

            MessageBox.Show("Блок схема сохранена");
        }

        #region AddDiagramToExcel


        private int AddTODiadramsToExcel(List<DiagamToWork> diagramsTO, int currentRow, ExcelWorksheet sheet)
        {
            var context = new MyDbContext();
            _startCollumn = 1;
            _currentParallelTO = 1;
            _parallelToCount = 1;

            int resultRow = currentRow, workRow = currentRow;
            var toPosledovGroups = diagramsTO
                                    .GroupBy(g => g.GetSequenceIndex() != null ? g.GetSequenceIndex() : g.ParallelIndex+"_")
                                    .ToList();//получение списка последовательных ТО

            var currentStatus = isNextTOParallel;
            if(toPosledovGroups.Count == 1 && toPosledovGroups[0].Key.Contains("_") && toPosledovGroups[0].ToList().Count() > 1)
                isNextTOParallel = true;
            else
                isNextTOParallel = toPosledovGroups.Count > 1 ? true : false;

            if (currentStatus != isNextTOParallel)
                currentRow = AddTOParallel(sheet, currentRow, Modulo(-_startCollumn, _currentPrintWidgth) + _startCollumn - _currentPrintWidgth + 6);
            //Вывод статуса отображения TO - паралелльно или последовательно

            foreach (var diagram in toPosledovGroups)
            {   
                workRow = AddPoslsedDiagramTO(diagram.ToList(), currentRow, sheet);
                resultRow = workRow > resultRow ? workRow : resultRow;
            }

            return resultRow;
        }

        private int AddPoslsedDiagramTO(List<DiagamToWork> diagramsTO, int currentRow, ExcelWorksheet sheet)
        {
            var context = new MyDbContext();
            int resultRow = currentRow, workRow = currentRow;
            int pageRow = 1;
            int pageCollumn = 1;
            var isTOListisPosledow = diagramsTO.ToList().All(g => g.GetSequenceIndex() != null);


            foreach (var diagram in diagramsTO)
            {
                var techOperation = context.TechOperationWorks.Where(t => t.Id == diagram.techOperationWorkId)
                                                                .Include(t => t.techOperation).FirstOrDefault();

                Color toColor = GetTOColor();
                _toColors.TryAdd(techOperation.techOperation.Name, toColor);
                while (_toColors.ContainsValue(toColor))
                {
                    toColor = GetTOColor();
                    _toColors.TryAdd(techOperation.techOperation.Name, toColor);
                }

                if (_startCollumn > _currentPrintWidgth)
                {
                    pageRow = Modulo(-currentRow, _currentPrintHeigth) + currentRow - _currentPrintHeigth + 2;
                    pageCollumn = Modulo(-_startCollumn, _currentPrintWidgth) + _startCollumn - _currentPrintWidgth + 1;

                    if (_parallelToCount > 1 && !isTOListisPosledow)
                    {
                        int tempColl = _startCollumn + ((_parallelToCount - 1) * 9);
                        pageCollumn = Modulo(-tempColl, _currentPrintWidgth) + tempColl - _currentPrintWidgth + 1;

                        if (_nextPagesLastCollumn.TryGetValue(pageCollumn, out int collCount))
                            _startCollumn = collCount > _startCollumn ? collCount : _startCollumn;
                        else
                            _nextPagesLastCollumn.Add(pageCollumn, _startCollumn);
                    }

                    pageCollumn = _startCollumn;

                    if (_nextPagesLastRow.TryGetValue(pageCollumn, out int rowCount))
                        pageRow = rowCount > pageRow ? rowCount : pageRow;
                    else
                        _nextPagesLastRow.Add(pageCollumn, pageRow);

                    pageRow = AddParallelesToExel(diagram.ListDiagramParalelno, pageRow, techOperation.techOperation.Name, sheet);
                    _nextPagesLastRow[pageCollumn] = pageRow;
                }
                else
                {
                    workRow = AddParallelesToExel(diagram.ListDiagramParalelno, currentRow, techOperation.techOperation.Name, sheet);
                    resultRow = workRow > resultRow ? workRow : resultRow;
                    if (isTOListisPosledow)
                        currentRow = resultRow;
                }

                if (!isTOListisPosledow)
                    _startCollumn += 9;

                if (isNextTOParallel && !isTOListisPosledow)
                    _currentParallelTO++;
            }
            
            if (isNextTOParallel)
            {
                _currentParallelTO++;
                _startCollumn += 9;
            }
            return resultRow;
        }

        private int AddParallelesToExel(List<DiagramParalelno> parallelesList, int currentRow, string TOName, ExcelWorksheet sheet)
        {
            int resultRow = currentRow, workRow = currentRow;
            int currentCollumn = _startCollumn;
            int pageRow = 1;
            int pageCollumn = _startCollumn;
            int pageCollumnindex = 0;


            foreach (DiagramParalelno parallel in parallelesList)
            {
                var currentStatus = isNextShagIsParallel;
                isNextShagIsParallel = parallel.ListDiagramPosledov.Count > 1 ? true : false;
                if(currentStatus != isNextShagIsParallel)
                    currentRow = AddParallelStatus(sheet, currentRow, _startCollumn);

                foreach (DiagramPosledov posledov in parallel.ListDiagramPosledov)
                {
                    _currentParallelShag = posledov.Order;

                    if (parallel.ListDiagramPosledov.Count == 1)
                    {
                        workRow = AddShagToExcel(posledov.ListDiagramShag, currentRow, _startCollumn, TOName, sheet, true);
                        pageCollumn = _startCollumn;
                    }
                    else
                    {
                        _parallelToCount = parallel.ListDiagramPosledov.Count > _parallelToCount ? parallel.ListDiagramPosledov.Count : _parallelToCount;

                        if (currentCollumn > _currentPrintWidgth)
                        {
                            pageRow = Modulo(-currentRow, _currentPrintHeigth) + currentRow - _currentPrintHeigth + 2;
                            pageCollumn = Modulo(-currentCollumn, _currentPrintWidgth) + currentCollumn - _currentPrintWidgth + 1;
                            pageCollumnindex = pageCollumn;

                            if (_nextPagesLastCollumn.TryGetValue(pageCollumnindex, out int collCount))
                                pageCollumn = collCount > pageCollumn ? collCount : pageCollumn;
                            else
                                _nextPagesLastCollumn.Add(pageCollumnindex, pageCollumn);

                            if (_nextPagesLastRow.TryGetValue(pageCollumn, out int rowCount))
                                pageRow = rowCount > pageRow ? rowCount : pageRow;
                            else
                                _nextPagesLastRow.Add(pageCollumn, pageRow);

                            pageRow = Modulo(-currentRow, _currentPrintHeigth) + currentRow - _currentPrintHeigth + 2;
                            
                            pageRow = AddShagToExcel(posledov.ListDiagramShag, pageRow, pageCollumn, TOName, sheet, false);
                            _nextPagesLastRow[pageCollumn] = pageRow;
                        }
                        else
                        {
                            workRow = AddShagToExcel(posledov.ListDiagramShag, currentRow, currentCollumn, TOName, sheet, false);
                            pageCollumn = currentCollumn;
                        }
                        
                    }

                    currentCollumn = pageCollumn + 9;
                    pageCollumn = Modulo(-currentCollumn, _currentPrintWidgth) + currentCollumn - _currentPrintWidgth + 1;

                    if (_nextPagesLastCollumn.TryGetValue(pageCollumn, out int collumnCount))
                    {
                        currentCollumn = collumnCount > currentCollumn ? collumnCount : currentCollumn;
                        pageCollumn = Modulo(-currentCollumn, _currentPrintWidgth) + currentCollumn - _currentPrintWidgth + 1;
                        _nextPagesLastCollumn[pageCollumn] = currentCollumn;
                    }
                    else
                        _nextPagesLastCollumn.Add(pageCollumn, currentCollumn);

                    resultRow = workRow > resultRow ? workRow : resultRow;

                }

                currentRow = resultRow > currentRow ? resultRow : currentRow;

            }

            return currentRow;

        }

        private int AddShagToExcel(List<DiagramShag> shagList, int currentRow, int currentColumn, string TOName, ExcelWorksheet sheet, bool isOneParalell)
        {
            if (isOneParalell && !isNextTOParallel )
                currentColumn = Modulo(-currentColumn, _currentPrintWidgth) + currentColumn - _currentPrintWidgth + 6;

            foreach (DiagramShag shag in shagList)
            {
                AddPageCount(currentRow, currentColumn, sheet);//проверка выполняется дважды
                
                currentRow = AddTONameToExcel(TOName, sheet, currentRow, currentColumn);

                if (!isOneParalell)
                    currentRow = AddCurrentParallelShagNum(sheet, currentRow-1, currentColumn);
                if (isNextTOParallel && !isOneParalell)
                    currentRow = AddCurrentParallelTONum(sheet, currentRow - 2, currentColumn);
                if (isNextTOParallel && isOneParalell)
                    currentRow = AddCurrentParallelTONum(sheet, currentRow - 1, currentColumn);

                currentRow = AddShagDescriptionToExcel(shag, sheet, currentRow, currentColumn);
                currentRow = AddToolDataToExcel(shag, sheet, currentRow, currentColumn);
                currentRow = AddPictureToExcel(shag, sheet, currentRow, currentColumn, TOName);

                AddPageCount(currentRow, currentColumn, sheet);
            }

            return currentRow;
        }

        #endregion
       
        #region AddShagDetails

        private int AddTONameToExcel(String TOName, ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            var printScaleDifference = Modulo(-headRow, _currentPrintHeigth);
            if (printScaleDifference < 5) //Проверка наличия хотя бы 5 строк в конце листка, чтобы корректно отображались данные шага
            {
                headRow = headRow + printScaleDifference + 2;
            }

            int[] columnNums = { currentColumn+1, currentColumn + 7};

            sheet.Cells[headRow, columnNums[0]].Value = TOName;
            if(_toColors.TryGetValue(TOName, out Color toColor))
            {
                sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.BackgroundColor.SetColor(toColor);
            }

            AddStyleAlignment(headRow, columnNums, sheet);

            return headRow+2;
        }
        private int AddShagDescriptionToExcel(DiagramShag shag, ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            headRow = AddShagName(shag, sheet, headRow, currentColumn) - 1;
            
            int[] rowsNums = { headRow, headRow + 1 };
            int[] columnNums = { currentColumn, currentColumn + 7 };

            sheet.Cells[rowsNums[0], columnNums[0]].Value = shag.Deystavie != "" ? shag.Deystavie : "Нет описания действий шага";

            if (shag.Deystavie != "")
            {
                rowsNums[rowsNums.Length - 1] = GetRowsCountByData(shag.Deystavie, sheet, columnNums) + headRow;
            }

            sheet.Cells[rowsNums[0], columnNums[0], rowsNums[0], columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Cells[rowsNums[0], columnNums[0], rowsNums[0], columnNums[1]].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            sheet.Cells[rowsNums[0], columnNums[0], rowsNums[1], columnNums[1]].Merge = true;
            sheet.Cells[rowsNums[0], columnNums[0], rowsNums[0], columnNums[1]].Style.WrapText = true;

            _exporter.ApplyCellFormatting(sheet.Cells[rowsNums[0], columnNums[0], rowsNums[1], columnNums[1]]);

            return rowsNums[rowsNums.Length - 1] + 2;

        }
        private int AddToolDataToExcel(DiagramShag shag , ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            if (shag.ListDiagramShagToolsComponent.Count == 0)//Таблица не выводится, если пустая
                return headRow;

            List<DiagramShagToolsComponent> toolComponent_DiagramList = shag.ListDiagramShagToolsComponent;

            Dictionary<string, int> headersColumns = new Dictionary<string, int>
            {
                { "№", currentColumn },
                { "Наименование", currentColumn+1 },
                { "Тип (исполнение)", currentColumn+4 },
                { "Ед. Изм.", currentColumn+6 },
                { "Кол-во", currentColumn+7 },
                {"конец", currentColumn+8 }
            };

            string[] headers = headersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => headersColumns[x]).ToArray();
            int[] columnNums = headersColumns.Values.OrderBy(x => x).ToArray();

            headRow = AddTableToolNumber(shag, sheet, headRow, currentColumn);

            _exporter.AddTableHeader(sheet, "Таблица с материалами, инструментами", headRow - 1, columnNums);

            _exporter.AddTableHeaders(sheet, headers, headRow, columnNums);

            int currentRow = headRow + 1;
            int i = 1;

            List<(dynamic tableObject, double Quantity)> tableItems = new List<(dynamic tableObject, double Quantity)>();

            foreach (var toolComponent in toolComponent_DiagramList)//упорядочивание элементов, чтобы компоненты падали вниз
            {
                (dynamic tableObject, double Quantity) obj = GetItemFromList(toolComponent);
                if (toolComponent.toolWorkId != null)
                    tableItems.Insert(0, obj);
                else
                    tableItems.Add(obj);
            }

            foreach(var item in tableItems)
            {
                if (item != (null, 0))
                {
                    sheet.Cells[currentRow, columnNums[0]].Value = i;

                    sheet.Cells[currentRow, columnNums[1]].Value = item.tableObject.Name;
                    sheet.Cells[currentRow, columnNums[2]].Value = item.tableObject.Type;
                    sheet.Cells[currentRow, columnNums[3]].Value = item.tableObject.Unit;
                    sheet.Cells[currentRow, columnNums[4]].Value = item.Quantity;

                    // Форматирование ячеек
                    AddStyleAlignment(currentRow, columnNums, sheet);

                    // Включаем перенос слов в ячейке
                    sheet.Cells[currentRow, columnNums[1]].Style.WrapText = true;
                    sheet.Cells[currentRow, columnNums[2]].Style.WrapText = true;

                    sheet.Cells[currentRow, columnNums[1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    // Объединение ячеек между столбцами
                    _exporter.MergeRowCellsByColumns(sheet, currentRow, columnNums);

                    sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[4]].Style.Fill.PatternType = ExcelFillStyle.Solid;

                    if (item.tableObject.GetType().Name == "Tool")//Окраживание ячеек в зависимоти от типа
                        sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[4]].Style.Fill.BackgroundColor.SetColor(Color.Aquamarine);
                    else
                        sheet.Cells[currentRow, columnNums[0], currentRow, columnNums[4]].Style.Fill.BackgroundColor.SetColor(Color.Salmon);

                    // Переход к следующей строке
                    currentRow++;
                    i++;
                }
            }
            _exporter.ApplyCellFormatting(sheet.Cells[headRow - 1, columnNums[0], currentRow - 1, columnNums[columnNums.Length - 1] - 1]);

            return currentRow + 1;
        }
        private int AddPictureToExcel(DiagramShag shag, ExcelWorksheet sheet, int headRow, int currentColumn, string diagramTo_name)
        {
            if(shag.ImageBase64 == "")
            {
                return headRow;
            }
            int[] columnNums = { currentColumn, currentColumn + 8 };
            var rowHeightPixels = sheet.Row(headRow).Height / 72 * 96d;

            var bytepath = Convert.FromBase64String(shag.ImageBase64);
            Image bitmapImage = LoadImage(bytepath);

            //var memoryStream = new MemoryStream();
            using(MemoryStream ms = new MemoryStream())
            {
                if (bitmapImage.Width > 600 || bitmapImage.Height > 500)
                {
                    Bitmap resized = new Bitmap(bitmapImage, new Size((int)(bitmapImage.Width / 1.5d), (int)(bitmapImage.Height / 1.5d)));
                    resized.Save(ms, ImageFormat.Png);
                }
                else
                    bitmapImage.Save(ms, ImageFormat.Png);

                ExcelPicture excelImage = null;
                excelImage = sheet.Drawings.AddPicture(shag.NameImage + headRow, ms);
                excelImage.SetSize(100);

                int printScaleDifference = Modulo(-headRow, _currentPrintHeigth);
                var currentRow = (int)Math.Ceiling(excelImage.Image.Bounds.Height / rowHeightPixels) + 2;//Высота изображения с учетом вставки наименований в строках

                if (printScaleDifference < currentRow)
                {
                    headRow += printScaleDifference;
                    headRow = AddTONameToExcel(diagramTo_name, sheet, headRow, currentColumn);
                    headRow = AddShagName(shag, sheet, headRow, currentColumn) - 1;
                }
                currentRow = (int)Math.Ceiling(excelImage.Image.Bounds.Height / rowHeightPixels) + headRow + 2;//Текущая строка с учетом высоты изображения

                excelImage.From.Column = columnNums[0] - 1;
                excelImage.From.Row = headRow;
                excelImage.From.ColumnOff = columnNums[1];

                AddImageNameNum(shag, sheet, currentRow, currentColumn);

                return currentRow + 2;

            }
        }

        #endregion

        #region SupportMethods
       
        private int AddShagName(DiagramShag shag, ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            int[] columnNums = { currentColumn + 3, currentColumn + 5 };
            sheet.Cells[headRow, columnNums[0]].Value = "Шаг " + shag.Nomer;

            AddStyleAlignment(headRow, columnNums, sheet);

            return headRow + 2;
        }
        private int GetRowsCountByData(string data, ExcelWorksheet sheet, int[] collumnNums)
        {
            var collumnsWidth = (collumnNums[collumnNums.Length - 1] - collumnNums[0]) * sheet.Column(collumnNums[collumnNums.Length - 1]).Width;
            var rowCount = data.Length / collumnsWidth;
            rowCount = Math.Ceiling(rowCount) + 1;
            return (int)rowCount;
        }
        private int AddTableToolNumber(DiagramShag shag, ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            int[] columnNums = { currentColumn + 5, currentColumn + 8 };
            sheet.Cells[headRow, columnNums[0]].Value = "Таблица " + shag.Nomer;

            AddStyleAlignment(headRow, columnNums, sheet);

            return headRow + 2;
        }
        private (dynamic, double) GetItemFromList(DiagramShagToolsComponent item)
        {
            var context = new MyDbContext();
            if (item.toolWorkId != null)
            {
                var toolWorksObj = context.ToolWorks.Where(t => t.Id == item.toolWorkId).FirstOrDefault();
                var tool = context.Tools.Where(t => t.Id == toolWorksObj.toolId).FirstOrDefault();
                (dynamic tableObject, double Quantity) tableItem = (tool, item.Quantity);
                return tableItem;
            }
            else if (item.componentWorkId != null)
            {
                var componentWorksObj = context.ComponentWorks.Where(t => t.Id == item.componentWorkId).FirstOrDefault();
                var component = context.Components.Where(t => t.Id == componentWorksObj.componentId).FirstOrDefault();
                (dynamic tableObject, double Quantity) tableItem = (component, item.Quantity);
                return tableItem;
            }
            return (null, 0);
        }
        private int AddImageNameNum(DiagramShag shag, ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            int[] columnNumsNumb = { currentColumn, currentColumn + 3 };
            int[] columnNumsName = { currentColumn + 3, currentColumn + 8 };

            sheet.Cells[headRow, columnNumsNumb[0]].Value = "Рисунок " + shag.Nomer;
            sheet.Cells[headRow, columnNumsName[0]].Value = shag.NameImage;

            AddStyleAlignment(headRow, columnNumsNumb, sheet);
            AddStyleAlignment(headRow, columnNumsName, sheet);

            return headRow + 2;
        }
        public Image LoadImage(byte[] imageData)
        {
            Image image;
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                image = Image.FromStream(ms);
            }

            return image;
        }
        private void AddStyleAlignment(int headRow, int[] columnNums, ExcelWorksheet sheet)
        {
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1]].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            _exporter.MergeRowCellsByColumns(sheet, headRow, columnNums);
            _exporter.ApplyCellFormatting(sheet.Cells[headRow, columnNums[0], headRow, columnNums[1] - 1]);
        }
        private int Modulo(int p, int q)
        {
            q = Math.Abs(q);
            var result = p % q;
            if (result < 0)
            {
                result += q;
            }
            return result;
        }
        private int AddParallelStatus(ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            var printScaleDifference = Modulo(-headRow, _currentPrintHeigth);
            if (printScaleDifference < 5)
            {
                headRow = headRow + printScaleDifference + 2;
            }

            int[] columnNums = { currentColumn + 1, currentColumn + 7 };
            sheet.Cells[headRow, columnNums[0]].Value = isNextShagIsParallel 
                                                            ? "Далее шаги выполняются параллельно"
                                                            :"Далее шаги выполняются последовательно";

            AddStyleAlignment(headRow, columnNums, sheet);

            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] -1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] -1].Style.Fill.BackgroundColor.SetColor(Color.DarkSeaGreen);

            return headRow + 2;
        }
        private int AddCurrentParallelShagNum(ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            int[] columnNums = { currentColumn + 1, currentColumn + 4 };
            sheet.Cells[headRow, columnNums[0]].Value = "Параллель шага № " + _currentParallelShag;

            AddStyleAlignment(headRow, columnNums, sheet);

            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.BackgroundColor.SetColor(Color.PaleGreen);

            return headRow + 2;
        }
        private int AddCurrentParallelTONum(ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            int[] columnNums = { currentColumn + 4, currentColumn + 7 };
            sheet.Cells[headRow, columnNums[0]].Value = "Параллель ТО № " + _currentParallelTO;

            AddStyleAlignment(headRow, columnNums, sheet);

            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.BackgroundColor.SetColor(Color.BlanchedAlmond);

            return headRow + 2;
        }
        private int AddTOParallel(ExcelWorksheet sheet, int headRow, int currentColumn)
        {
            var printScaleDifference = Modulo(-headRow, _currentPrintHeigth);
            if (printScaleDifference < 5)
            {
                headRow = headRow + printScaleDifference + 2;
            }

            int[] columnNums = { currentColumn + 1, currentColumn + 7 };
            if(sheet.Cells[headRow, columnNums[0]].Value == null)
                sheet.Cells[headRow, columnNums[0]].Value = isNextTOParallel
                                                                ? "Далее ТО выполняются параллельно"
                                                                : "Далее ТО выполняются последовательно";

            AddStyleAlignment(headRow, columnNums, sheet);

            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1] - 1].Style.Fill.BackgroundColor.SetColor(Color.Bisque);

            return headRow + 2;
        }
        private void AddPageCount(int currentRow, int currentColumn, ExcelWorksheet sheet)
        {
            var isCurrentPageNew = Modulo(-(currentRow - 2), _currentPrintHeigth) == 0;

            if (isCurrentPageNew)
            {
                return;
            }

            var pageCollumn = Modulo(-currentColumn, _currentPrintWidgth) + currentColumn;
            var pageRow = Modulo(-currentRow, _currentPrintHeigth) + currentRow - _currentPrintHeigth + 1;

            if (sheet.Cells[pageRow, pageCollumn].Value == null
                    && (_currentParallelShag == 1 || _currentParallelShag == 3))
            {
                sheet.Cells[pageRow, pageCollumn].Value = "Лист №" + _pageCount;
                sheet.Cells[pageRow, pageCollumn, pageRow + _currentPrintHeigth - 1, pageCollumn].Merge = true;
                sheet.Cells[pageRow, pageCollumn, pageRow + _currentPrintHeigth - 1, pageCollumn].Style.TextRotation = 90;
                sheet.Cells[pageRow, pageCollumn, pageRow + _currentPrintHeigth - 1, pageCollumn].Style.Font.Size = 24;

                _pageCount++;
            }
        }

        private Color GetTOColor()
        {
            Random colorRnd = new Random();
            int red = colorRnd.Next(240);
            int green = colorRnd.Next(90, 240);
            int blue = colorRnd.Next(90, 160);
            red = Modulo(-red, 30) + red;
            green = Modulo(-green, 30) + green;
            blue = Modulo(-blue, 30) + blue;


            Color color = Color.FromArgb(red, green, blue);

            return color;
        }
        #endregion

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

    }
}
