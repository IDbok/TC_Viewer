using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using TcDbConnector;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;



namespace ExcelParsing.DataProcessing;

public class WorkParser
{
   // List<TechTransition> _newTransitions;

    private string _notes;
    private const int StartRow = 2;
    private readonly string[] _stepColumns = {
        "Id ТО", "Id ТП", "TC_ID", "Артикул", "№",
        "Технологические операции", "Исполнитель", "Технологические переходы",
        "Время действ., мин.", "Время выполнения этапа, мин.", "№ СЗ",
        "Примечание", "Индекс", "Категория ТО", "Инструменты", "Тип", "Этап, формула", "Строка", "Machinery"
    };
    public WorkParser()
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
    }

    public void CacheDbData(out List<TechOperation> techOperations, out List<TechTransition> techTransitions/*, out List<TechnologicalCard> tcs*/)
    {
        using (var context = new MyDbContext())
        {
            techOperations = context.TechOperations.ToList();
            techTransitions = context.TechTransitions.ToList();
            //tcs = context.TechnologicalCards
            //    //.Include(tc => tc.Tool_TCs)
            //    //.Include(tc => tc.Component_TCs)
            //    .ToList();
        }
    }
    public void ParseTcWorkSteps(string tcFilePath, string tcArticle, ref string notes, CachedData? cachedData = null)
    {
        var fileInfo = new FileInfo(tcFilePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"Файл {tcFilePath} не найден.");
        }

        if (notes != null)
        {
            this._notes = notes;
        }

        var objList = new List<TechOperationWork>();

        using (var package = new ExcelPackage(fileInfo))
        {

            var stepSheet = package.Workbook.Worksheets[tcArticle];
            if (stepSheet == null)
            {
                throw new Exception($"Лист {tcArticle} не найден в файле.");
            }

            // Кэшируем данные из БД
            var techOperationsCache = new List<TechOperation>();
            var techTransitionsCache = new List<TechTransition>();
            if (cachedData == null)
                CacheDbData(out techOperationsCache, out techTransitionsCache/*, out var tcs*/);
            else
            {
                techOperationsCache = cachedData.TechOperations;
                techTransitionsCache = cachedData.TechTransitions;
            }

            var currentTc = GetCurrentTechnologicalCard(tcArticle);
            var startRow = FindStartRow(stepSheet, "Выполнение работ");
            var stepColumnsNumbers = ExcelParser.GetColumnsNumbers(startRow, stepSheet);
            var (newStepColumnsNumbers, machinaryColumnsNumbers) = MapStepColumns(stepColumnsNumbers);

            // Находим конец таблицы хода работ
            var endRow = FindEndRow(stepSheet, startRow, newStepColumnsNumbers);

            int row = 0;

            try
            {
                ParseRows(stepSheet, startRow, endRow, newStepColumnsNumbers, machinaryColumnsNumbers, techOperationsCache, techTransitionsCache, currentTc, objList, ref row);
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка при парсинге строки {row} хода работ. {e.Message}");
            }

            // Сохраняем данные в БД
            using (var context = new MyDbContext())
            {
                //List<TechOperationWork> objToAdd = objList.Take(1).ToList();
                foreach (var techOperationWork in objList)
                {
                    foreach (var executionWork in techOperationWork.executionWorks)
                    {
                        var staff_tcs = new List<Staff_TC>();
                        foreach (var staff in executionWork.Staffs)
                        {
                            var staff_tc = context.Staff_TCs.FirstOrDefault(st => st.ChildId == staff.ChildId && st.ParentId == staff.ParentId);
                            if(staff_tc == null)
                            {
                                throw new Exception($"Сотрудник {staff.ChildId} не найден в БД.");
                            }

                            staff_tcs.Add(staff_tc);
                        }
                        executionWork.Staffs = staff_tcs;

                        var protection_tcs = new List<Protection_TC>();
                        foreach (var protection in executionWork.Protections)
                        {
                            var protection_tc = context.Protection_TCs.FirstOrDefault(pr => pr.ChildId == protection.ChildId && pr.ParentId == protection.ParentId);
                            if (protection_tc == null)
                            {
                                throw new Exception($"СЗ {protection.ChildId} не найден в БД.");
                            }
                            protection_tcs.Add(protection_tc);
                        }
                        executionWork.Protections = protection_tcs;

                        var machine_tcs = new List<Machine_TC>();
                        foreach (var machine in executionWork.Machines)
                        {
                            var machine_tc = context.Machine_TCs.FirstOrDefault(m => m.ChildId == machine.ChildId && m.ParentId == machine.ParentId);
                            if (machine_tc == null)
                            {
                                throw new Exception($"Механизм {machine.ChildId} не найден в БД.");
                            }
                            machine_tcs.Add(machine_tc);
                        }
                        executionWork.Machines = machine_tcs;

                    }

                    context.TechOperationWorks.Add(techOperationWork);

                }
                context.SaveChanges();
            }

        }
    }

    public void ParseExecutionPictures(string tcFilePath, string tcArticle, TechnologicalCardRepository? tcRepository = null)
    {
        Console.WriteLine($"Парсинг схемы исполнения ТК {tcArticle}");

        var fileInfo = new FileInfo(tcFilePath);

        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"Файл {tcFilePath} не найден.");
        }

        var card = new TechnologicalCard();
        

        using (var package = new ExcelPackage(fileInfo))
        {

            var stepSheet = package.Workbook.Worksheets[tcArticle];
            if (stepSheet == null)
            {
                throw new Exception($"Лист {tcArticle} не найден в файле.");
            }
            Dictionary<string, int> startRows = new Dictionary<string, int>
            {
                { "Требования к составу бригады и квалификации", 0 },
                { "Требования к материалам и комплектующим", 0 },
                { "Требования к механизмам", 0 },
                { "Требования к средствам защиты", 0 },
                { "Требования к инструментам и приспособлениям", 0 },
                { "Выполнение работ", 0 }
            };

            foreach (var tableName in startRows)
            {
                startRows[tableName.Key] = FindStartRow(stepSheet, tableName.Key);
            }
            var startRow = startRows["Требования к механизмам"]; // заголовок 3. Требования к механизмам
            var endRow = startRows["Выполнение работ"]; // заголовок 6. Выполнение работ

            var startColumn = 1; // столбец Стоим., руб. без НДС таблицы 2. Требования к материалам и комплектующим
            var endColumn = stepSheet.Dimension.Columns;// конец диапазона

            
            var imageBytes = GetPictureFromExcelByRange(stepSheet, startRow, startColumn, endRow, endColumn);

            if(tcRepository == null)
            {
                tcRepository = new TechnologicalCardRepository(new MyDbContext());
            }

            tcRepository.UpdateExecutionScheme(tcArticle, imageBytes);

            //using (var context = new MyDbContext())
            //{
            //    card = context.TechnologicalCards.Where(tc => tc.Article == tcArticle)
            //        .FirstOrDefault();

            //    if (card == null)
            //    {
            //        throw new Exception($"Технологическая карта {tcArticle} не найдена в БД.");
            //    }

            //    if (imageBytes != null)
            //    {
            //        card.ExecutionScheme = imageBytes;
            //    }

            //    context.SaveChanges();
            //}

            
        }
    }
    public void ParseExecutionPictures(string filePath, List<string> tcArticles, TechnologicalCardRepository? tcRepository = null)
    {
        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"Файл {filePath} не найден.");
        }

        using (var package = new ExcelPackage(fileInfo))
        {
            ParseExecutionPictures (package, tcArticles, tcRepository);
        }
    }
    private void ParseExecutionPictures(ExcelPackage package, List<string> tcArticles, TechnologicalCardRepository? tcRepository = null)
    {
        Dictionary<string, int> startRows = new Dictionary<string, int>
        {
            { "Требования к составу бригады и квалификации", 0 },
            { "Требования к материалам и комплектующим", 0 },
            { "Требования к механизмам", 0 },
            { "Требования к средствам защиты", 0 },
            { "Требования к инструментам и приспособлениям", 0 },
            { "Выполнение работ", 0 }
        };

        foreach (var tcArticle in tcArticles)
        {
            Console.WriteLine($"Парсинг схемы исполнения ТК {tcArticle}");
            
            var stepSheet = package.Workbook.Worksheets[tcArticle];
            if (stepSheet == null)
            {
                throw new Exception($"Лист {tcArticle} не найден в файле.");
            }
            foreach (var tableName in startRows)
            {
                startRows[tableName.Key] = FindStartRow(stepSheet, tableName.Key);
            }
            var startRow = startRows["Требования к механизмам"]; // заголовок 3. Требования к механизмам
            var endRow = startRows["Выполнение работ"]; // заголовок 6. Выполнение работ

            var startColumn = 1; // столбец Стоим., руб. без НДС таблицы 2. Требования к материалам и комплектующим
            var endColumn = stepSheet.Dimension.Columns;// конец диапазона


            var imageBytes = GetPictureFromExcelByRange(stepSheet, startRow, startColumn, endRow, endColumn);

            if (tcRepository == null)
            {
                tcRepository = new TechnologicalCardRepository(new MyDbContext());
            }

            tcRepository.UpdateExecutionScheme(tcArticle, imageBytes);
        }
    }

    public byte[] GetPictureFromExcelByRange(ExcelWorksheet worksheet, int startRow, int startColumn, int endRow, int endColumn)
    {
        var pictures = new List<ExcelPicture>();
        string outputDirectory = @"C:\Tests\pictures";
        // Перебор всех рисунков на листе
        foreach (var drawing in worksheet.Drawings)
        {
            if (drawing is ExcelPicture picture)
            {
                // Проверка, находится ли картинка в заданном диапазоне
                if (picture.From.Row >= startRow && picture.From.Row <= endRow &&
                    picture.From.Column >= startColumn && picture.From.Column <= endColumn)
                {
                    pictures.Add(picture);
                }
            }

        }
        if (pictures.Count > 0)
        {
            // Определение размеров объединенного изображения
            int width = 0;
            int height = 0;

            var images = new List<Image>();

            foreach (var pic in pictures)
            {
                var image = Image.FromStream(new MemoryStream(pic.Image.ImageBytes));
                images.Add(image);
                width = Math.Max(width, image.Width);
                height += image.Height;
            }

            using (var finalImage = new Bitmap(width, height))
            using (var g = Graphics.FromImage(finalImage))
            {
                g.Clear(Color.White); // Фон белого цвета, можно изменить при необходимости

                int currentHeight = 0;
                foreach (var img in images)
                {
                    g.DrawImage(img, 0, currentHeight, img.Width, img.Height);
                    currentHeight += img.Height;
                    img.Dispose(); // Освобождение ресурсов изображения
                }

                // Сохранение изображения в байтовый массив
                using (var ms = new MemoryStream())
                {
                    finalImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
        return null;
    }

    private TechnologicalCard? GetCurrentTechnologicalCard(string tcArticle)
    {
        using(var context = new MyDbContext())
        {
            var fullTc = context.TechnologicalCards.Where(tc => tc.Article == tcArticle)
                .Include(tc => tc.Staff_TCs).ThenInclude(st => st.Child)
                .Include(tc => tc.Protection_TCs).ThenInclude(pr => pr.Child)
                .Include(tc => tc.Machine_TCs).ThenInclude(m => m.Child)
                .Include(tc => tc.Tool_TCs).ThenInclude(t => t.Child)
                .Include(tc => tc.Component_TCs).ThenInclude(c => c.Child)
                .FirstOrDefault();

            if (fullTc == null)
            {
                throw new Exception($"Технологическая карта {tcArticle} не найдена в БД.");
            }
            return fullTc;

        }
    }

    private int FindStartRow(ExcelWorksheet sheet, string tableName)
    {
        int stepRowCount = sheet.Dimension.Rows;
        for (int row = 1; row <= stepRowCount; row++)
        {
            if (sheet.Cells[row, 1].Text.Contains(tableName))
            {
                return row + 1;
            }
        }
        throw new Exception($"Таблица {tableName} не найдена в листе.");
    }

    private int FindEndRow(ExcelWorksheet sheet, int startRow, Dictionary<string, int> columnsNumbers)
    {
        int stepRowCount = sheet.Dimension.Rows;
        for (int row = startRow + 1; row <= stepRowCount; row++)
        {
            if (sheet.Cells[row, columnsNumbers["№"]].Text == "")
            {
                if (sheet.Cells[row, columnsNumbers["Технологические операции"]].Text == "Наименование")
                {
                    //Console.WriteLine("Таблица 6 заканчивается на строке: " + (row - 1));
                    return row - 1;
                }
                else
                {
                    throw new Exception($"Есть пробелы в нумерации тех переходов");
                }
                
            }
        }
        throw new Exception($"Конец таблицы не найден в листе.");
    }

    private (Dictionary<string, int> newStepColumnsNumbers, Dictionary<string, int> machinaryColumnsNumbers) MapStepColumns(Dictionary<string, int> stepColumnsNumbers)
    {
        string[] stepColumns = {
            "№", "Технологические операции", "Исполнитель", "Технологические переходы",
            "Время выполнения действия, мин.", "Время действ., мин.",
            "Время выполнения этапа, мин.", "Время этапа, мин.",
            "№ СЗ", "Примечание"
        };

        var newStepColumnsNumbers = new Dictionary<string, int>();
        var machinaryColumnsNumbers = new Dictionary<string, int>();

        foreach (var column in stepColumns)
        {
            foreach (var key in stepColumnsNumbers.Keys)
            {
                if (CompareStrings(key, column, maxDistance: 1))
                {
                    newStepColumnsNumbers[column] = stepColumnsNumbers[key];
                }
            }
        }

        foreach (var key in stepColumnsNumbers.Keys)
        {
            if (!newStepColumnsNumbers.ContainsValue(stepColumnsNumbers[key]))
            {
                if (key.Contains("Время"))
                    machinaryColumnsNumbers[key] = stepColumnsNumbers[key];
            }
        }

        return (newStepColumnsNumbers, machinaryColumnsNumbers);
    }

    private void ParseRows(ExcelWorksheet stepSheet,
        int startRow, int endRow,
        Dictionary<string, int> newStepColumnsNumbers,
        Dictionary<string, int> machinaryColumnsNumbers,
        List<TechOperation> techOperationsCache,
        List<TechTransition> techTransitionsCache,
        TechnologicalCard currentTc,
        List<TechOperationWork> TOList,
        ref int currentRow
        )
    {
        

        //int stepRowCount = stepSheet.Dimension.Rows;
        int currentToId = 0, previousToId = 0;
        int lastToOrderInTc = 1, rowNumber;
        string lastEtapFormula = "";
        
        var exWorks = new List<ExecutionWork>();

        var machineNames = GetMachinesNames2(machinaryColumnsNumbers);
        // (string,int,bool) - name, column number, is active in current stage
        Dictionary<Machine_TC, (string, int, bool)> machines = GetStageMachines2(currentTc.Machine_TCs, machineNames);

        for (int row = startRow + 1; row <= endRow; row++)
        {
            //if (row == 28)
            //    Console.WriteLine();

            currentRow = row;
            if (!int.TryParse(stepSheet.Cells[row, newStepColumnsNumbers["№"]].Text.Trim(), out rowNumber)) 
            {
                throw new Exception($"Номер строки хода работ не является числом. Строка {row}");
            }

            string name = stepSheet.Cells[row, newStepColumnsNumbers["Технологические переходы"]].Text.Trim();

            string listStaff = stepSheet.Cells[row, newStepColumnsNumbers["Исполнитель"]].Text;

            // разделить строку на массив строк по символу переноса строки и пробелу

            string[] separators = new string[] { "\n", " " };
            string[] staffSymbols = listStaff.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //string[] staffSymbols = listStaff.Split("\n");


            if (!string.IsNullOrEmpty(listStaff))
            {

                var (techOperation, techTransition) = GetTechOperationAndTransition(row, stepSheet, newStepColumnsNumbers, techOperationsCache, techTransitionsCache, ref currentToId);

                //bool isTOChanges = currentToId != previousToId;

                var (etap, posled, formulaStep, valueStep) = GetFormulasAndStages(row, stepSheet, newStepColumnsNumbers, ref lastEtapFormula , out _);
                
                // Проверка участия механизмов в этапе, если механизм участвует, то третий элемент картежа - true
                if (currentToId != previousToId & machines.Count != 0)
                    GetMachineParticipation(row, stepSheet, machines);

                string protectionRange = stepSheet.Cells[row, newStepColumnsNumbers["№ СЗ"]].Text;

                string coefficient;

                if (techTransition!.Name == "Выполнить в соответствии с ТК" 
                    || techTransition!.Name == "Повторить п.")
                {
                    coefficient = ExtractExpressionOutsideFunction(formulaStep);// valueStep;
                }
                else
                {
                    coefficient = GetStringBeforeAtSymbol(formulaStep);
                }

                AddOrUpdateTechOperationWork(row, startRow, ref currentToId, ref previousToId, currentTc, ref lastToOrderInTc, TOList);

                string comments = stepSheet.Cells[row, newStepColumnsNumbers["Примечание"]].Text;

                ExecutionWork exWork = CreateExecutionWork(techTransition, rowNumber, etap, posled, coefficient, comments);

                if (techTransition!.Name == "Повторить п.")
                {
                    //var repeatObjs = GetExecutionWorksToRepeat(name, row, stepSheet, newStepColumnsNumbers, exWorks);
                    GetExecutionWorksToRepeat_new(exWork, name, row, exWorks);
                    //exWork.ListexecutionWorkRepeat2 = repeatObjs;
                    exWork.Coefficient = ""; // Удаляем коэффициент, т.к. он дублируется для каждого шага внутри повторения
                }

                // добавление персонала в ExecutionWork
                exWork.Staffs = FindStaff_TCsBySymbol(staffSymbols, currentTc.Id, currentTc.Staff_TCs);

                // добавление механизмов в ExecutionWork
                foreach (var key in machines.Keys)
                {
                    if (machines[key].Item3)
                    {
                        exWork.Machines.Add(key);
                    }
                }

                // добавление СЗ в ExecutionWork
                exWork.Protections = FindProtection_TCByOrder(protectionRange, currentTc.Id, currentTc.Protection_TCs);

                exWorks.Add(exWork);

                TOList[TOList.Count - 1].executionWorks.Add(exWork);
                exWork.techOperationWork = TOList[TOList.Count - 1];

            }
            else if (name.Contains("Повторить п."))
            {
                var exWork = CreateExecutionWork(null, rowNumber /*lastEwOrderInTc++*/, "", "", "");
                GetExecutionWorksToRepeat_new(exWork, name, row, exWorks);
                
                //var repeatObjs = GetExecutionWorksToRepeat(name, row, stepSheet, newStepColumnsNumbers, exWorks);

                //exWork.ListexecutionWorkRepeat2 = repeatObjs;

                exWorks.Add(exWork);

                TOList[TOList.Count - 1].executionWorks.Add(exWork);
                exWork.techOperationWork = TOList[TOList.Count - 1];

            }
            else // если нет исполнителя, то это инструмент или компонент
            {
                (var tool, var component) = GetToolOrComponent(row, stepSheet, currentTc, newStepColumnsNumbers, out bool? isTool);
                if (isTool != null)
                {
                    if(isTool.Value && tool != null)
                    {
                        //AddOrUpdateTechOperationWork(row, startRow, ref currentToId, ref previousToId, currentTc, ref lastToOrderInTc, objList);
                        TOList[TOList.Count - 1].ToolWorks.Add(tool);
                    }
                    else if(component != null)
                    {
                        TOList[TOList.Count - 1].ComponentWorks.Add(component);
                    }
                }
            }
        }
    }
    private List<ExecutionWork> GetExecutionWorksToRepeat(string name, int row, ExcelWorksheet sheet, Dictionary<string, int> columnsNumbers, List<ExecutionWork> executionWorksCache)
    {
        List<ExecutionWork> exWorksToRepeat = new List<ExecutionWork>();
        if (name.Contains("Повторить п."))
        {
            var repeatObjsNumbers = GetExecutinWorksNumbers(name);
            foreach (var n in repeatObjsNumbers)
            {
                var repeatingObj = executionWorksCache.Find(e => e.Order == n);
                if (repeatingObj != null)
                {
                    exWorksToRepeat.Add(repeatingObj);
                }
                else 
                {                         
                    throw new Exception($"Техпереход с номером {n} не найден в кэше. {name} cтрока {row}");
                }
            }
        }

        return exWorksToRepeat;
    }
    private void GetExecutionWorksToRepeat_new(ExecutionWork executionWorkParent, string name, int row, List<ExecutionWork> executionWorksCache)
    {
        //List<ExecutionWorkRepeat> exWorksToRepeat = new List<ExecutionWorkRepeat>();
        if (name.Contains("Повторить п."))
        {
            var repeatObjsNumbers = GetExecutinWorksNumbers(name);
            foreach (var n in repeatObjsNumbers)
            {
                var repeatingObj = executionWorksCache.Find(e => e.Order == n);
                if (repeatingObj != null)
                {
                    var exWorkRepeat = new ExecutionWorkRepeat
                    {
                        //ParentExecutionWorkId = executionWorkParent.Id,
                        ParentExecutionWork = executionWorkParent,
                        //ChildExecutionWorkId = repeatingObj.Id,
                        ChildExecutionWork = repeatingObj,
                        NewCoefficient = executionWorkParent.Coefficient ?? "",
                    };
                    executionWorkParent.ExecutionWorkRepeats.Add(exWorkRepeat);
                    //exWorksToRepeat.Add(repeatingObj);
                }
                else
                {
                    throw new Exception($"Техпереход с номером {n} не найден в кэше. {name} cтрока {row}");
                }
            }
        }
    }

    private (ToolWork?, ComponentWork?) GetToolOrComponent(int row, ExcelWorksheet sheet,TechnologicalCard tc, Dictionary<string, int> columnsNumbers, out bool? isTool)
    {
        ToolWork? tool = null;
        ComponentWork? component = null;

        isTool = null;

        var colNumber = columnsNumbers["Технологические переходы"];

        var name = sheet.Cells[row, colNumber].Text.Trim();
        var type = sheet.Cells[row, colNumber + 1 ].Text.Trim();
        var unit = sheet.Cells[row, colNumber + 2].Text.Trim();

        double quantity = 0;
        try
        {
            quantity = Convert.ToDouble(sheet.Cells[row, colNumber + 3].Value);
        }
        catch (Exception e)
        {
            if (sheet.Cells[row, colNumber + 3].Text == "По месту")
                quantity = 0;
            else
                throw new Exception($"Ошибка при получении количества инструмента или компонента. Строка {row}. {e.Message}");
        }
        
           

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type) && IntermediateTablesParser.toolExceptions.ContainsKey((name, type)))
        {
            (name, type) = IntermediateTablesParser.toolExceptions[(name, type)];
        }

        var toolTc = tc.Tool_TCs.FirstOrDefault(t => t.Child.Name == name && t.Child.Type == type);
        var componentTc = tc.Component_TCs.FirstOrDefault(c => c.Child.Name == name && c.Child.Type == type);


        if (toolTc != null)
        {
            tool = new ToolWork
            {
                toolId = toolTc.ChildId,
                Quantity = quantity,
            };

            isTool= true;
        }
        else if(componentTc != null)
        {
            component = new ComponentWork
            {
                componentId = componentTc.ChildId,
                Quantity = quantity,
            };
            isTool= false;
        }
        else
        {
            _notes += $"Инструмент или компонент {name} (строка {row}) не найден в кэше.\n";
            //throw new Exception($"Инструмент или компонент {name} (строка {row}) не найден в кэше.");
        }

        return ( tool, component);
               
    }

    private (TechOperation? techOperation, TechTransition? techTransition) GetTechOperationAndTransition(
        int row, ExcelWorksheet sheet,
        Dictionary<string, int> columnsNumbers,
        List<TechOperation> techOperations,
        List<TechTransition> techTransitions,
        ref int currentToId)
    {
        TechOperation? techOperation = null;
        TechTransition? techTransition = null;

        string techOperationName = sheet.Cells[row, columnsNumbers["Технологические операции"]].Text.Trim();
        if (!string.IsNullOrEmpty(techOperationName))
        {
            techOperation = techOperations.FirstOrDefault(to => to.Name == techOperationName);
            if (techOperation == null)
            {
                throw new Exception($"Технологическая операция {techOperationName} (строка {row}) не найдена в кэше.");
            }
            currentToId = techOperation.Id;
            // Console.WriteLine($"------------------ ТО: {techOperation.Name}");
        }

        string techTransitionName = sheet.Cells[row, columnsNumbers["Технологические переходы"]].Text.Trim();
        if (!string.IsNullOrEmpty(techTransitionName))
        {
            techTransition = techTransitions.FirstOrDefault(tt => tt.Name == techTransitionName);
            if (techTransition == null)
            {
                if (techTransitionName.Contains("Выполнить в соответствии с ТК"))
                {
                    techTransition = techTransition = techTransitions.FirstOrDefault(tt => tt.Name == "Выполнить в соответствии с ТК");
                    if (techTransition == null)
                    {
                        throw new Exception($"Технологический переход {techTransitionName} (строка {row}) не найден в кэше.");
                    }

                    techTransition.CommentName = techTransitionName;
                }
                else if (techTransitionName.Contains("Повторить п."))
                {
                    techTransition = techTransition = techTransitions.FirstOrDefault(tt => tt.Name == "Повторить п.");
                    if (techTransition == null)
                    {
                        throw new Exception($"Технологический переход {techTransitionName} (строка {row}) не найден в кэше.");
                    }

                    techTransition.CommentName = techTransitionName;
                }
                else
                {
                    throw new Exception($"Технологический переход {techTransitionName} (строка {row}) не найден в кэше.");
                }

            }
            //Console.WriteLine($"    ТП: {techTransition.Name} строка {row}");
        }

        return (techOperation, techTransition);
    }

    public void Test()
    {

        var article = "ТКР10_1.1.6";
        var filePath = @"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\ТК\Исходные\ТКР_v4.0.xlsx";

        Console.WriteLine($"Парсинг ход работ ТК {article}");

        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"Файл {filePath} не найден.");
        }

        var objList = new List<TechOperationWork>();

        using (var package = new ExcelPackage(fileInfo))
        {

            var stepSheet = package.Workbook.Worksheets[article];
            if (stepSheet == null)
            {
                throw new Exception($"Лист {article} не найден в файле.");
            }

            Dictionary<string, int> newStepColumnsNumbers = new Dictionary<string, int>()
            {
                {"Время выполнения этапа, мин.", 8},
                {"Время выполнения действия, мин.", 7},
            };

            var row = 93;
            //var (etap, posled, formulaStep, formulaEtap) = GetFormulasAndStages(row, stepSheet, newStepColumnsNumbers, "lastEtapFormula", out _);

            //var formulaEtap = "=MAX(SUM(G93:G95);SUM(G97:G98);G99;SUM(G100:G101))";
            //var (etap, posled) = ParseStageFormula2(formulaEtap, row, out _);

        }
    }

    private (string etap, string posled, string formulaStep, string valueStep) GetFormulasAndStages(
        int row, ExcelWorksheet sheet,
        Dictionary<string, int> columnsNumbers,
        ref string lastEtapFormula,
        out List<int> allRowNumbersInEtap)
    {
        string timeExecutionStepColumn = "Время выполнения действия, мин.";
        string timeExecutionEtapColumn = "Время выполнения этапа, мин.";

        if (!columnsNumbers.ContainsKey(timeExecutionStepColumn))
        {
            timeExecutionStepColumn = "Время действ., мин.";
        }
        if (!columnsNumbers.ContainsKey(timeExecutionEtapColumn))
        {
            timeExecutionEtapColumn = "Время этапа, мин.";
        }
        string formulaStep;
        string formulaEtap;

        string valueStep;
        try
        {
            formulaStep = sheet.Cells[row, columnsNumbers[timeExecutionStepColumn]].Formula;
            formulaEtap = sheet.Cells[row, columnsNumbers[timeExecutionEtapColumn]].Formula;

            valueStep = sheet.Cells[row, columnsNumbers[timeExecutionStepColumn]].Value?.ToString() ?? "";
        }
        catch (Exception e)
        {
            throw new Exception($"Ошибка при получении формулы или значения времени выполнения действия или этапа. Строка {row}. {e.Message}");
        }
        

        if (string.IsNullOrEmpty(formulaStep))
        {
            formulaStep = valueStep;
        }

        if (!string.IsNullOrEmpty(formulaEtap))
        {
            lastEtapFormula = formulaEtap;
        }
        else if (string.IsNullOrEmpty(lastEtapFormula))
        {
            throw new Exception($"Формула этапа не найдена в строке {row}");
        }

        var (etap, posled) = ParseStageFormula(lastEtapFormula!, row, out allRowNumbersInEtap);

        return (etap, posled, formulaStep, valueStep);
    }

    private void GetMachineParticipation(
        int row, ExcelWorksheet sheet,
        Dictionary<Machine_TC, (string, int, bool)> machinesDict)
    {
        foreach (var key in machinesDict.Keys)
        {
            machinesDict[key] = (machinesDict[key].Item1, machinesDict[key].Item2, sheet.Cells[row, machinesDict[key].Item2].Text.Length >= 1);
        }
    }

    private void AddOrUpdateTechOperationWork(
        int row, int startRow,
        ref int currentToId, ref int previousToId,
        TechnologicalCard currentTc,
        ref int lastToOrderInTc,
        List<TechOperationWork> objList)
    {
        if (row == startRow + 1 || currentToId != previousToId)
        {
            previousToId = currentToId;

            var techOperationWork = new TechOperationWork
            {
                techOperationId = currentToId,
                TechnologicalCardId = currentTc.Id,
                Order = lastToOrderInTc++,
            };

            objList.Add(techOperationWork);
        }
    }

    public static ExecutionWork CreateExecutionWork(
        TechTransition techTransition, int order,
        string etap, string posled,
        string coefficient, string comment = "")
    {
        return new ExecutionWork
        {
            techTransitionId = techTransition.Id,
            Order = order,
            Staffs = new List<Staff_TC>(),
            Protections = new List<Protection_TC>(),
            Machines = new List<Machine_TC>(),
            Repeat = techTransition.Name == "Повторить п.",
            Etap = etap,
            Posled = posled,
            Coefficient = string.IsNullOrEmpty(coefficient) ? "" : "*" + coefficient,

            Value = string.IsNullOrEmpty(coefficient) ? techTransition.TimeExecution : EvaluateExpression(techTransition.TimeExecution + "*" + coefficient),

            Comments = comment,
        };
    }

    public List<TechOperation> ParseExcelToObjectsTechOperation(string filePath, string sheetName = "Перечень ТО")
    {
        var objList = new List<TechOperation>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[sheetName];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = StartRow; row <= rowCount; row++)
            {

                var obj = new TechOperation
                {
                    Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                    Name = Convert.ToString(worksheet.Cells[row, 2].Value),
                    Category = Convert.ToString(worksheet.Cells[row, 3].Value),

                    IsReleased = true,
                };

                objList.Add(obj);
            }
        }

        return objList;
    }

    public List<TechTransition> ParseExcelToObjectsTechTransition(string filePath, string sheetName = "Типовые переходы")
    {
        var objList = new List<TechTransition>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[sheetName];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = StartRow; row <= rowCount; row++)
            {

                var isTimeExist = double.TryParse(Convert.ToString(worksheet.Cells[row, 4].Value), out double exTime);
                var obj = new TechTransition
                {
                    Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                    Name = Convert.ToString(worksheet.Cells[row, 3].Value),
                    Category = Convert.ToString(worksheet.Cells[row, 2].Value),
                    TimeExecution = isTimeExist ? exTime : 1,
                    TimeExecutionChecked = Convert.ToBoolean(worksheet.Cells[row, 8].Value),
                    CommentName = Convert.ToString(worksheet.Cells[row, 6].Value),
                    CommentTimeExecution = Convert.ToString(worksheet.Cells[row, 7].Value),

                    IsReleased = true,
                };

                objList.Add(obj);
            }
        }

        return objList;
    }
    public List<TechOperationWork> ParseExcelToObjectsTechOperationWork(string filePath, string stepSheetName)
    {
        var objList = new List<TechOperationWork>();
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"Файл {filePath} не найден.");
        }

        using (var package = new ExcelPackage(fileInfo))
        {
            var stepSheet = package.Workbook.Worksheets[stepSheetName];
            if (stepSheet == null)
            {
                throw new Exception($"Лист {stepSheetName} не найден в файле.");
            }

            int stepRowCount = stepSheet.Dimension.Rows;
            var stepColumnsNumbers = GetColumnsNumbers(_stepColumns, 1, stepSheet);

            int lastToOrderInTc = 0;
            int previousToId = 0;
            int previousTcId = 0;

            var executionWorks = new List<ExecutionWork>();

            for (int row = StartRow; row <= stepRowCount; row++)
            {
                ProcessRow(stepSheet, row, stepColumnsNumbers,
                    ref executionWorks,
                    ref lastToOrderInTc, ref previousToId, ref previousTcId, objList);
            }
        }

        return objList;
    }

    private void ProcessRow(ExcelWorksheet stepSheet, int row, Dictionary<string, int> stepColumnsNumbers, 
        ref List<ExecutionWork> executionWorks,
        ref int lastToOrderInTc, ref int previousToId, ref int previousTcId, List<TechOperationWork> objList)
    {
        int currentTcId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["TC_ID"]].Value);
        int currentToId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Id ТО"]].Value);

        ExecutionWork exWork = ParseTechExecutionWork(stepSheet, row, stepColumnsNumbers, currentTcId, 
            ref executionWorks,
            out var tool, out var component);


        if (row == StartRow || currentToId != previousToId || currentTcId != previousTcId)
        {
            if (currentTcId != previousTcId)
            {
                previousTcId = currentTcId;
                lastToOrderInTc = 0;
            }

            lastToOrderInTc++;
            previousToId = currentToId;

            var techOperationWork = new TechOperationWork
            {
                techOperationId = currentToId,
                TechnologicalCardId = currentTcId,
                Order = lastToOrderInTc,
            };

            objList.Add(techOperationWork);
        }

        if (exWork != null)
        {
            objList[objList.Count - 1].executionWorks.Add(exWork);
            exWork.techOperationWork = objList[objList.Count - 1];
            executionWorks.Add(exWork);
        }
        else if (component != null)
        {
            objList[objList.Count - 1].ComponentWorks.Add(component);
        }
        else if (tool != null)
        {
            objList[objList.Count - 1].ToolWorks.Add(tool);
        }
    }
    public ExecutionWork? ParseTechExecutionWork(ExcelWorksheet stepSheet, int row, Dictionary<string, int> stepColumnsNumbers, int tcId, 
        ref List<ExecutionWork> executionWorks,
        out ToolWork? tool,
        out ComponentWork? component)
    {
        tool = null;
        component = null;

        int order = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["№"]].Value);

        TechTransition techTransition;
        int idTP = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Id ТП"]].Value);
        
        string name = stepSheet.Cells[row, stepColumnsNumbers["Технологические переходы"]].Text;

        string timeExecutionString = stepSheet.Cells[row, stepColumnsNumbers["Время действ., мин."]].Text;
        bool timeExecutionChecked = double.TryParse(timeExecutionString, out double timeExecution);

        string comments = stepSheet.Cells[row, stepColumnsNumbers["Примечание"]].Text;

        string listStaff = stepSheet.Cells[row, stepColumnsNumbers["Исполнитель"]].Text;
        string[] staffSymbols = listStaff.Split("\n");
        List<Staff_TC> staffIds = FindStaff_TCIds(staffSymbols, tcId);

        string protectionRange = stepSheet.Cells[row, stepColumnsNumbers["№ СЗ"]].Text;
        List<Protection_TC> protections = FindProtection_TCByOrder(protectionRange, tcId);

        string machineRange = stepSheet.Cells[row, stepColumnsNumbers["Machinery"]].Text;
        List<Machine_TC> machines = FindMachine_TCByOrder(machineRange, tcId);

        bool repeat = false;
        var repeatObjs = new List<ExecutionWork>();

        // check if idTP is null or empty
        if (idTP == 0)
        {
            // check if it "Повторить" item or Tools and Components
            if(name.Contains("Повторить п.") )
            {
                repeat = true;
                var repeatObjsNumbers = GetExecutinWorksNumbers(name);
                foreach (var n in repeatObjsNumbers)
                {
                    var repeatingObj = executionWorks.Find(e => e.techOperationWork.TechnologicalCardId == tcId && e.Order == n && n != order);
                    if (repeatingObj != null)
                    {
                        repeatObjs.Add(repeatingObj);
                    }
                }
            }
            else if (stepSheet.Cells[row, stepColumnsNumbers["Тип"]].Text == "Component")
            {
                component = new ComponentWork
                {
                    componentId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Инструменты"]].Value),
                    Quantity = timeExecution,
                };

                return null;
            }
            else if (stepSheet.Cells[row, stepColumnsNumbers["Тип"]].Text == "Tool")
            {
                tool = new ToolWork
                {
                    toolId = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Инструменты"]].Value),
                    Quantity = timeExecution,
                };

                return null;
            }
        }

        string formula = stepSheet.Cells[row, stepColumnsNumbers["Этап, формула"]].Text;
        int rowNum = Convert.ToInt32(stepSheet.Cells[row, stepColumnsNumbers["Строка"]].Value);

        (string stage, string parallelIndex) = ParseStageFormula(formula, rowNum, out _);

        var obj = new ExecutionWork
        {
            techTransitionId = idTP == 0? null : idTP,

            Order = order,

            Value = timeExecution,
            Comments = comments,

            Staffs = staffIds,
            Protections = protections,
            Machines = machines,

            Repeat = repeat,

            Etap = stage.ToString(),
            Posled = parallelIndex.ToString(),

        };


        if (obj.Repeat)
        {
            obj.ListexecutionWorkRepeat2 = repeatObjs;
        }

        return obj;
    }

    public List<int> GetExecutinWorksNumbers(string repeatName)
    {
        var numbers = new List<int>();
        var split = repeatName.Split(" ");
        if (split.Length > 1)
        {
            foreach (var s in split)
            {
                ExtractRange(s, numbers);
            }
        }
        return numbers;
    }
    private void ExtractRange(string textRange, List<int> numbers)
    {
        try
        {
            if (textRange.Contains("-") || textRange.Contains(","))
            {
                if (textRange.Contains(","))
                {
                    var range = textRange.Split(",");
                    foreach (var r in range)
                    {
                        SplitRange(r).ForEach(n => numbers.Add(n));
                    }
                }
                SplitRange(textRange).ForEach(n => numbers.Add(n));
            }
            else if (int.TryParse(textRange, out int n))
            {
                numbers.Add(n);
            }
        }catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    private List<int> SplitRange(string range)
    {
        var numbers = new List<int>();

        var split = range.Split("-");
        if (split.Length > 1)
        {
            if (int.TryParse(split[0], out int start) && int.TryParse(split[1], out int end))
            {
                for (int i = start; i <= end; i++)
                {
                    numbers.Add(i);
                }
            }
            
        }
        else if (int.TryParse(range, out int n))
        {
            //int.TryParse(range, out int n);
            numbers.Add(n);
        }
        return numbers;
    }
    public List<Staff_TC> FindStaff_TCIds(string[] staff_TcsSymbols, int tcId)
    {
        List<Staff_TC> staff_TCs = new List<Staff_TC>();

        using (var context = new TcDbConnector.MyDbContext())
        {
            foreach (var symbol in staff_TcsSymbols)
            {
                var staff_tc = context.Staff_TCs.Where(st => st.Symbol == symbol && st.ParentId == tcId).FirstOrDefault();
                if (staff_tc != null)
                {
                    staff_TCs.Add(staff_tc);
                }
                else
                {
                    throw new Exception($"Сотрудник {symbol} не найден в БД.");
                }
            }
        }

        return staff_TCs;
    }
    public List<Staff_TC> FindStaff_TCsBySymbol(string[] staff_TcsSymbols, int tcId, List<Staff_TC>? staff_TCsCache = null)
    {
        List<Staff_TC> staff_TCs = new List<Staff_TC>();

        if (staff_TCsCache == null || staff_TCsCache.Count == 0)
        {
            using (var context = new TcDbConnector.MyDbContext())
            {
                foreach (var symbol in staff_TcsSymbols)
                {
                    var staff_tc = context.Staff_TCs.Where(st => st.Symbol == symbol && st.ParentId == tcId).FirstOrDefault();
                    if (staff_tc != null)
                    {
                        staff_TCs.Add(staff_tc);
                    }
                    else
                    {
                        throw new Exception($"Сотрудник {symbol} не найден в БД.");
                    }
                }
            }
        }
        else
        {
            foreach (var symbol in staff_TcsSymbols)
            {
                var staff_tc = staff_TCsCache.Find(st => st.Symbol == symbol && st.ParentId == tcId);
                if (staff_tc != null)
                {
                    staff_TCs.Add(staff_tc);
                }
                else
                {
                    throw new Exception($"Сотрудник {symbol} не найден в кэше.");
                }
            }
        }
        

        return staff_TCs;
    }
    public List<Protection_TC> FindProtection_TCByOrder(string protectionRange, int tcId, List<Protection_TC>? protection_TCsCache = null)
    {
        List<Protection_TC> protections = new List<Protection_TC>();

        var  protectionIdsList = new List<int>();
        ExtractRange(protectionRange, protectionIdsList);

        if (protection_TCsCache == null || protection_TCsCache.Count == 0)
        {
            using (var context = new TcDbConnector.MyDbContext())
            {
                foreach (var order in protectionIdsList)
                {
                    var protection = context.Protection_TCs.Where(pr => pr.Order == order && pr.ParentId == tcId).FirstOrDefault();
                    if (protection != null)
                    {
                        protections.Add(protection);
                    }
                }
            }
        }
        else
        {
            foreach (var order in protectionIdsList)
            {
                var protection = protection_TCsCache.Find(pr => pr.Order == order && pr.ParentId == tcId);
                if (protection != null)
                {
                    protections.Add(protection);
                }
            }
        }
        

        return protections;
    }

    public List<Machine_TC> FindMachine_TCByOrder(string machineNames, int tcId)
    {
        string[] names = GetMachinesNames(machineNames);

        List<Machine_TC> machinesExist = GetMachinesFromDB(tcId);

        return GetStageMachines(machinesExist, names);
    }

    public static string[] GetMachinesNames(string machineNames)
    {
        string[] textToDelete = new string[] { "Время работы", "мин.", "," };
        string separator = " | ";
        string[] machines = machineNames.Split(separator);

        for (int i = 0; i < machines.Length; i++)
        {
            foreach (var text in textToDelete)
            {
                machines[i] = machines[i].Replace(text, "").Trim();
            }
        }
        return machines;
    }
    public static Dictionary<string, int> GetMachinesNames2(Dictionary<string, int> machineNames)
    {
        Dictionary < string, int> machinesDict = new Dictionary<string, int>();
        string[] textToDelete = new string[] { "Время работы", "Время", "мин.", "," };
       
        foreach (var kvp in machineNames)
        {
            string cleanKey = kvp.Key;
            foreach (var text in textToDelete)
            {
                cleanKey = cleanKey.Replace(text, "").Trim();
            }

            if (!machinesDict.ContainsKey(cleanKey))
            {
                machinesDict[cleanKey] = kvp.Value;
            }
            else
            {
                throw new Exception($"Механизм {cleanKey} дублируется в карте.");
            }
        }
        return machinesDict;
    }
    public List<Machine_TC> GetMachinesFromDB(int tcId)
    {
        List<Machine_TC> machines = new List<Machine_TC>();
        using (var db = new MyDbContext())
        {
            machines = db.Machine_TCs.Where(m => m.ParentId == tcId)
                .Include(m => m.Child)
                .ToList();
        }
        return machines;
    }

    public static List<Machine_TC> GetStageMachines(List<Machine_TC> existMacines, string[] machinesNames)
    {
        var stageMachines = new List<Machine_TC>();
        int maxDistance = 2;
        foreach (var machineName in machinesNames)
        {
            var machine = existMacines.FirstOrDefault(m => CompareStrings(m.Child.Name, machineName, maxDistance));
            if (machine != null)
            {
                stageMachines.Add(machine);
            }
        }
        return stageMachines;
    }
    public Dictionary<Machine_TC, (string, int, bool)> GetStageMachines2(List<Machine_TC> existMacines, Dictionary<string, int> machinesNames)
    {
        var stageMachines = new Dictionary<Machine_TC, (string, int, bool)>();
        int maxDistance = 2;

        var exceptions = new Dictionary<string, string>()
            {
                {"П11", "Грузовик П-11" },
                {"грузовика", "Грузовик П-11" },

                {"автомобиля ЭТЛ","Автолаборатория (КЛ)" },
            };



        foreach (var machineName in machinesNames)
        {
            Machine_TC machine;
            if (exceptions.ContainsKey(machineName.Key))
            {
                var machineNameExc = exceptions[machineName.Key];
                machine = existMacines.FirstOrDefault(m => CompareStrings(m.Child.Name, machineNameExc, maxDistance));
            }
            else
            {

                machine = existMacines.FirstOrDefault(m => CompareStrings(m.Child.Name, machineName.Key, maxDistance));
            }

            if (machine != null)
            {
                stageMachines.Add(machine, (machineName.Key, machineName.Value, false));
            }
            else
            {
                _notes += $"Механизм {machineName.Key} не найден в карте.\n";
            }
        }

        if (existMacines.Count() != 0 && stageMachines.Count == 0)
            _notes += $"Ни одного механизма найдено не было! Количество механизмов в карте {existMacines.Count()}\n";

        return stageMachines;
    }
    static bool CompareStrings(string source, string target, int maxDistance)
    {
        source = source.ToLowerInvariant().Replace(" ", "");
        target = target.ToLowerInvariant().Replace(" ", "");

        int distance = ComputeLevenshteinDistance(source, target);
        return distance <= maxDistance;
    }
    static int ComputeLevenshteinDistance(string source, string target)
    {
        var n = source.Length;
        var m = target.Length;
        var matrix = new int[n + 1, m + 1];

        if (n == 0)
            return m;

        if (m == 0)
            return n;

        for (int i = 0; i <= n; matrix[i, 0] = i++) { }
        for (int j = 0; j <= m; matrix[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[n, m];
    }
    public Dictionary<string, int> GetColumnsNumbers(string[] columns, int columnRow, ExcelWorksheet worksheet)
    {
        var columnsNumbers = new Dictionary<string, int>();
        for (int i = 1; i <= worksheet.Dimension.Columns; i++)
        {
            string value = worksheet.Cells[columnRow, i].Text;
            if (Array.IndexOf(columns, value) >= 0)
            {
                columnsNumbers[value] = i;
            }

        }
        return columnsNumbers;
    }
    public Dictionary<string, int> GetColumnsNumbers(int columnRow, ExcelWorksheet worksheet)
    {
        var columnsNumbers = new Dictionary<string, int>();
        for (int i = 1; i <= worksheet.Dimension.Columns; i++)
        {
            string value = worksheet.Cells[columnRow, i].Text;
            if (value.Length >= 1)
            {
                columnsNumbers[value] = i;
            }

        }
        return columnsNumbers;
    }
    public static (string stage, string parallelIndex) ParseStageFormula(string formula, int rowNum, out List<int> allNumbers)
    {
        // Инициализируем переменные для хранения результатов
        string stage = "0";
        string parallelIndex = "0";

        // Инициализируем список для хранения всех номеров
        allNumbers = new List<int>();

        // Ищем максимальноt и минимально число в формуле и присваиваем их в stage
        (int minValue, int maxValue) = WorkParser.GetMinAndMaxValue(formula);
        // Если минимальное и максимальное значение равны, то возвращаем нули
        if (minValue == maxValue)
        {
            return (stage, parallelIndex);
        }
        stage = $"{minValue}{maxValue}";

        // Убираем из строки все, кроме цифр и диапазонов
        string cleanFormula = new string(formula.Where(c => char.IsDigit(c) || c == ';' || c == '+' || c == ':' || c == ',').ToArray());

        // Разделяем на диапазоны
        string[] ranges = cleanFormula.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        int posledIndex = 0;
        foreach (string range in ranges)
        {
            posledIndex++; // Увеличиваем индекс последовательности для каждого диапазона

            if (range.Contains('+') || range.Contains(';'))
            {
                // Получаем начало и конец диапазона
                string[] ranges2 = range.Split(new char[] { '+', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string range2 in ranges2)
                {
                    // Проверяем, является ли текущий элемент диапазоном
                    if (range2.Contains(':'))
                    {
                        // Получаем начало и конец диапазона
                        int start = int.Parse(range2.Split(':')[0]);
                        int end = int.Parse(range2.Split(':')[1]);

                        // Добавляем все номера из диапазона в список
                        for (int i = start; i <= end; i++)
                        {
                            allNumbers.Add(i);
                        }

                        // Проверяем, попадает ли rowNum в диапазон
                        if (rowNum >= start && rowNum <= end)
                        {
                            parallelIndex = posledIndex.ToString();
                            break;
                        }
                    }
                    else
                    {
                        // Обработка случая, когда указан один номер строки
                        int line = int.Parse(range2);

                        // Добавляем номер в список
                        allNumbers.Add(line);

                        if (rowNum == line)
                        {
                            parallelIndex = posledIndex.ToString();
                            break;
                        }
                    }
                }
            }
            else if (!formula.Contains("SUM"))
            {
                // Проверяем, является ли текущий элемент диапазоном
                if (range.Contains(':'))
                {
                    // Получаем начало и конец диапазона
                    int start = int.Parse(range.Split(':')[0]);
                    int end = int.Parse(range.Split(':')[1]);

                    // Добавляем все номера из диапазона в список
                    for (int i = start; i <= end; i++)
                    {
                        allNumbers.Add(i);
                    }

                    // Проверяем, попадает ли rowNum в диапазон
                    if (rowNum >= start && rowNum <= end)
                    {
                        parallelIndex = "0";
                        break;
                    }
                }
                else
                {
                    // Обработка случая, когда указан один номер строки
                    int line = int.Parse(range);

                    // Добавляем номер в список
                    allNumbers.Add(line);

                    if (rowNum == line)
                    {
                        parallelIndex = "0"; // Для одиночных строк индекс последовательности равен 0
                        break;
                    }
                }
            }
            else 
            {
                // Проверяем, является ли текущий элемент диапазоном
                if (range.Contains(':'))
                {
                    // Получаем начало и конец диапазона
                    int start = int.Parse(range.Split(':')[0]);
                    int end = int.Parse(range.Split(':')[1]);

                    // Добавляем все номера из диапазона в список
                    for (int i = start; i <= end; i++)
                    {
                        allNumbers.Add(i);
                    }

                    // Проверяем, попадает ли rowNum в диапазон
                    if (rowNum >= start && rowNum <= end)
                    {
                        parallelIndex = posledIndex.ToString();
                        break;
                    }
                }
                else
                {
                    // Обработка случая, когда указан один номер строки
                    int line = int.Parse(range);

                    // Добавляем номер в список
                    allNumbers.Add(line);

                    if (rowNum == line)
                    {
                        parallelIndex = "0"; // Для одиночных строк индекс последовательности равен 0
                        break;
                    }
                }
            }

        }

        // Возвращаем индекс параллельности и последовательности

        // Если номер строки не входит в диапазон, то возвращаем 0
        if (rowNum < allNumbers.Min() || rowNum > allNumbers.Max())
        {
            return ("", "");
        }

        return (stage, parallelIndex);
    }
    public static (int min, int max) GetMinAndMaxValue(string formula)
    {
        string text = formula;

        // Использование регулярного выражения для извлечения всех чисел из текста
        var matches = Regex.Matches(text, @"\d+");

        // Преобразование найденных чисел из строки в int
        var numbers = matches.Cast<Match>().Select(m => int.Parse(m.Value)).ToList();

        // Поиск минимального и максимального значения
        int min = numbers.Min();
        int max = numbers.Max();

        return (min, max);
    }
    public static string GetStringBeforeAtSymbol(string input, string symbol = "*INDEX")
    {
        string result = string.Empty;
        if (string.IsNullOrEmpty(input))
        {
            return result;
        }

        int atIndex = input.IndexOf(symbol);
        if (atIndex == -1)
        {
            return result; // If '@' not found, return the entire string
        }

        return input.Substring(0, atIndex); // Return substring before '@' and after '='
    }
    public static string ExtractExpressionOutsideFunction(string input)
    {
        // Regular expression to match function and extract remaining part
        string pattern = @"^[^)]*\)(.*)$";
        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            var expression = match.Groups[1].Value;
            // если первый символ "*" удалить его
            if(expression.Count() > 0)
            {
                while (expression[0] == '*')
                {
                    expression = expression.Substring(1);
                }
            }
            
            return expression;
        }
        return "1";
    }
    public static double EvaluateExpression(string expression)
    {
        try
        {
            expression = expression.Trim().Replace(",", ".");
            // разделить выражение на части по разделителю "*" удаляя пустые значения
            var parts = expression.Split(new[] { '*' }, StringSplitOptions.RemoveEmptyEntries);

            // собираем выражение заново
            expression = string.Join("*", parts);

            var table = new DataTable();

            var value = table.Compute(expression, string.Empty);
            return Math.Round(Convert.ToDouble(value),2);
        }
        catch
        {
            throw new Exception($"Произошла ошибка при расчёте выражения {expression}");
            
        }
        
    }
}
