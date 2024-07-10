using OfficeOpenXml;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace ExcelParsing.DataProcessing;

public class ExcelParser
{
    public const int maxRow = 10000;
    public const int maxColumn = 20;

    public const int addToTitleRow = 1;
    public const int addToTable = addToTitleRow + 1;
    public ExcelParser()
    {
        ExcelPackage.LicenseContext = LicenseContext.Commercial;
    }

    private CachedData GetChachedDate()
    {
        var interParser = new IntermediateTablesParser();
        var wsParser = new WorkParser();

        interParser.CacheDbData(out List<Staff> staffs,
            out List<Machine> machines,
            out List<Component> components,
            out List<Tool> tools,
            out List<Protection> protections);

        wsParser.CacheDbData(out List<TechOperation> techOperations,
                               out List<TechTransition> techTransitions);

        return new CachedData(staffs, components, tools, machines, protections, techOperations, techTransitions);
    }
    
    public void ParseAllTCs(string folderPath, List<string> fileNames, string historyFileFolderPath, string historyFileName,  string? sheetName = null)
    {
        List<LogInfor> logInfos = new List<LogInfor>
        {
            new LogInfor($"{DateTime.Now}")
        };

        var cacheDb = GetChachedDate();

        var tcInfos = new List<TcParserInfo>(); //ParseTcParserInfo(@"C:\Users\bokar\OneDrive\Работа\Таврида\Технологические карты\ТК\Парсинг.xlsx");

        foreach (var fileName in fileNames)
        {

            Console.Write($"Файл {fileName}:");

            var tcFilePath = Path.Combine(folderPath, fileName);
            var fileInfo = new FileInfo(tcFilePath);

            if (!fileInfo.Exists)
            {
                logInfos.Add(LogError(fileName, "", "Файл не найден"));
                Console.WriteLine(" Файл не найден");
                //LogError(logSheet, fileName, "", "Файл не найден");
                continue;
            }

            using (var package = new ExcelPackage(fileInfo))
            {
                var sheetNames = package.Workbook.Worksheets.Select(x => x.Name).ToList();
                var tcSheetNames = sheetNames.Where(x => x.StartsWith("ТК")).ToList();

                // оставить только лист с названием "ТК"
                if (sheetName != null) tcSheetNames = tcSheetNames.Where(x => x == sheetName).ToList();

                if (tcSheetNames.Count == 0)
                {
                    //LogError(logSheet, fileName, "", "Листы с названием начинающимся на 'ТК' не найдены");
                    logInfos.Add(LogError(fileName, "", "Листы с названием начинающимся на 'ТК' не найдены"));
                    
                    if (sheetName != null)
                        Console.WriteLine($"Листы {sheetName} не найдены");
                    else
                        Console.WriteLine("Листы с названием начинающимся на 'ТК' не найдены");

                    continue;
                }

                foreach (var tcSheetName in tcSheetNames)
                {
                    Console.WriteLine();
                    Console.Write($"   {tcSheetName}:");
                    var tcInfo = tcInfos.Where(x => x.SheetName == tcSheetName).FirstOrDefault();
                    if (tcInfo != null && (tcInfo.IsParsed || tcInfo.IsExcluded))
                    {
                        if (tcInfo.IsExcluded)
                            Console.Write($" Исключён из парсинга. {tcInfo.Note}");
                        else if (tcInfo.IsParsed)
                            Console.Write($" Уже спарсен");

                        continue;
                    }

                    string article = tcSheetName;
                    string parsingResult1to5 = "";
                    string parsingResult6 = "";
                    string notes = "";

                    try
                    {
                        var tcRepo = new TechnologicalCardRepository(new TcDbConnector.MyDbContext());
                        if (tcInfo == null || !tcInfo.IsIntermediateParsed)
                            tcRepo.DeleteInnerEntitiesAsync(article);

                        Console.WriteLine();
                        Console.Write("      Таблицы 1-5:");

                        try
                        {
                            
                            if (tcInfo == null || !tcInfo.IsIntermediateParsed)
                            {
                                var interParser = new IntermediateTablesParser();
                                interParser.ParseIntermediateObjects(tcFilePath, article, ref notes);
                                Console.Write(" Успешно");

                            }
                            else
                            {
                                Console.Write(" Уже спарсены");
                            }

                            parsingResult1to5 = "+";

                            try
                            {
                                Console.WriteLine();
                                Console.Write("      Таблица 6:");
                                if (tcInfo == null || !tcInfo.IsExecutionParsed)
                                {
                                    var wsParser = new WorkParser();
                                    wsParser.ParseTcWorkSteps(tcFilePath, article, ref notes, cacheDb);
                                    Console.Write(" Успешно");
                                }
                                else
                                {
                                    Console.Write(" Уже спарсена");
                                }
                                parsingResult6 = "+";
                            }
                            catch (Exception ex)
                            {
                                parsingResult6 = ex.Message;
                                Console.WriteLine(ex.Message);
                            }

                            tcRepo.UpdateStatus(article, TechnologicalCard.TechnologicalCardStatus.Draft);
                        }
                        catch (Exception ex)
                        {
                            parsingResult1to5 = ex.Message;
                            Console.WriteLine(ex.Message);
                            parsingResult6 = "-";
                        }

                    }
                    catch (Exception ex)
                    {
                        notes += $"Ошибка при парсинге ТК {fileName}: {ex.Message}\n";

                        Console.WriteLine($" ОШИБКА {ex.Message}");

                        logInfos.Add(LogError(fileName, article, notes));

                        //LogError(logSheet, fileName, article, notes);
                        //File.AppendAllText(historyFilePath, notes + "\n");
                    }

                    logInfos.Add(LogResult(fileName, article, parsingResult1to5, parsingResult6, notes));
                    //LogResult(logSheet, fileName, article, parsingResult1to5, parsingResult6, notes);
                }
            }
        }

        var historyFilePath = Path.Combine(historyFileFolderPath, historyFileName);
        FileInfo logFile = new FileInfo(historyFilePath);
        try
        {
            SaveLogFile(logInfos, historyFilePath);
        }
        catch (Exception ex)
        {
            var newHistoryFileName = $"{historyFileName}_{DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss")}.xlsx";
            historyFilePath = Path.Combine(historyFileFolderPath, newHistoryFileName);

            SaveLogFile(logInfos, historyFilePath);
        }

    }

    public void FindOldFormTC(string folderPath, List<string> fileNames, string historyFileFolderPath, string historyFileName, string? sheetName = null)
    {
        List<(string, string)> articlesWithExecutorColumn = new List<(string, string)>();

        foreach (var fileName in fileNames)
        {
            Console.Write($"Файл {fileName}:");

            var tcFilePath = Path.Combine(folderPath, fileName);
            var fileInfo = new FileInfo(tcFilePath);

            if (!fileInfo.Exists)
            {
                Console.WriteLine(" Файл не найден");
                continue;
            }

            using (var package = new ExcelPackage(fileInfo))
            {
                var sheetNames = package.Workbook.Worksheets.Select(x => x.Name).ToList();
                var tcSheetNames = sheetNames.Where(x => x.StartsWith("ТК")).ToList();

                if (sheetName != null)
                {
                    tcSheetNames = tcSheetNames.Where(x => x == sheetName).ToList();
                }

                if (tcSheetNames.Count == 0)
                {
                    Console.WriteLine(" Листы с названием начинающимся на 'ТК' не найдены");
                    continue;
                }

                foreach (var tcSheetName in tcSheetNames)
                {
                    Console.WriteLine();
                    Console.Write($"   {tcSheetName}:");

                    var stepSheet = package.Workbook.Worksheets[tcSheetName];
                    if (stepSheet == null)
                    {
                        Console.WriteLine(" Лист не найден");
                        continue;
                    }

                    var startRow = FindStartRow(stepSheet, "Выполнение работ");
                    if (startRow == 0)
                    {
                        Console.WriteLine(" Таблица 'Выполнение работ' не найдена");
                        continue;
                    }

                    var stepColumnsNumbers = ExcelParser.GetColumnsNumbers(startRow, stepSheet);
                    if (!stepColumnsNumbers.ContainsKey("Исполнитель"))
                    {
                        if (stepColumnsNumbers.ContainsKey("Технологические операции"))
                        {
                            // проверка наличия формулы в первой строке Тех операции
                            var formulaCell = stepSheet.Cells[startRow + 1, stepColumnsNumbers["Технологические операции"]].Formula;
                            if (formulaCell != null)
                            {
                                var formula = formulaCell.ToString();
                                if (!formula.Contains("="))
                                {
                                    articlesWithExecutorColumn.Add((tcSheetName, " ТК старого формата"));
                                    Console.WriteLine(" ТК старого формата");

                                    continue;
                                }
                            }


                        }

                        articlesWithExecutorColumn.Add((tcSheetName, " ТК, вероятно, старого формата"));
                        Console.WriteLine(" ТК, вероятно, старого формат");
                    }
                }
            }
        }

        SaveArticlesWithExecutor(historyFileFolderPath, historyFileName, articlesWithExecutorColumn);
    }

    private void SaveArticlesWithExecutor(string folderPath, string fileName, List<(string, string)> articles)
    {
        var historyFilePath = Path.Combine(folderPath, fileName);
        var fileInfo = new FileInfo(historyFilePath);
        try
        {
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets.Add("History");
                worksheet.Cells[1, 1].Value = "Артикул";
                worksheet.Cells[1, 2].Value = "Комментарий";
                for (int i = 0; i < articles.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = articles[i].Item1;
                    worksheet.Cells[i + 2, 2].Value = articles[i].Item2;
                }
                package.Save();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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

        return 0;
    }

    private void SaveLogFile(List<LogInfor> logInfos, string historyFilePath)
    {
        FileInfo logFile = new FileInfo(historyFilePath);
        
        using (ExcelPackage logPackage = new ExcelPackage(logFile))
        {
            var logSheet = logPackage.Workbook.Worksheets.FirstOrDefault() ?? logPackage.Workbook.Worksheets.Add("Log");

            if (logSheet.Dimension == null)
            {
                logSheet.Cells[1, 1].Value = "Файл";
                logSheet.Cells[1, 2].Value = "Артикул ТК";
                logSheet.Cells[1, 3].Value = "Парсинг таблиц 1-5";
                logSheet.Cells[1, 4].Value = "Парсинг таблицы 6";
                logSheet.Cells[1, 5].Value = "Примечания";
            }

            foreach (var logInfo in logInfos)
            {
                int newRow = logSheet.Dimension.End.Row + 1;
                logSheet.Cells[newRow, 1].Value = logInfo.FileName;
                logSheet.Cells[newRow, 2].Value = logInfo.Article;
                logSheet.Cells[newRow, 3].Value = logInfo.ParsingResult1to5;
                logSheet.Cells[newRow, 4].Value = logInfo.ParsingResult6;
                logSheet.Cells[newRow, 5].Value = logInfo.Notes;
            }

            logPackage.Save();
        }
    }   

    private void LogResult(ExcelWorksheet sheet, string fileName, string article, string result1to5, string result6, string notes)
    {
        int newRow = sheet.Dimension.End.Row + 1;
        sheet.Cells[newRow, 1].Value = fileName;
        sheet.Cells[newRow, 2].Value = article;
        sheet.Cells[newRow, 3].Value = result1to5;
        sheet.Cells[newRow, 4].Value = result6;
        sheet.Cells[newRow, 5].Value = notes;
    }

    private void LogError(ExcelWorksheet sheet, string fileName, string article, string error)
    {
        int newRow = sheet.Dimension.End.Row + 1;
        sheet.Cells[newRow, 1].Value = fileName;
        sheet.Cells[newRow, 2].Value = article;
        sheet.Cells[newRow, 5].Value = error;
    }
    private LogInfor LogResult(string fileName, string article, string result1to5, string result6, string notes)
    {
        return new LogInfor(fileName)
        {
            Article = article,
            ParsingResult1to5 = result1to5,
            ParsingResult6 = result6,
            Notes = notes,
        };
    }

    private LogInfor LogError( string fileName, string article, string error)
    {
        return new LogInfor(fileName)
        {
            Article = article,
            Notes = error,
        };
    }
    
    public List<Staff_TC> ParseExcelToObjectsStaff_TC(string filePath, out List<string> metaList)
    {

        var objList = new List<Staff_TC>();
        metaList = new List<string>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var sheetName = typeof(Staff_TC).Name;

            var worksheet = package.Workbook.Worksheets[sheetName];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                int.TryParse(Convert.ToString(worksheet.Cells[row, 1].Value), out var isIndex);
                int.TryParse(Convert.ToString(worksheet.Cells[row, 2].Value), out var isParentId);
                int.TryParse(Convert.ToString(worksheet.Cells[row, 10].Value), out var isChildId);
                int.TryParse(Convert.ToString(worksheet.Cells[row, 4].Value), out var isOrder);
                var isSymbol = Convert.ToString(worksheet.Cells[row, 9].Value);

                if (isParentId != 0 && isChildId != 0 && isOrder != 0 && isSymbol != "")
                {
                    var obj = new Staff_TC
                    {

                        ParentId = isParentId,
                        ChildId = isChildId,
                        Order = isOrder,
                        Symbol = isSymbol,
                    };

                    objList.Add(obj);
                }
                else
                    metaList.Add($"Ошибка парсинга строке {isIndex}");
            }
        }
        return objList;
    }
    public List<T> ParseExcelToIntermediateStructObjects<T,C>(string filePath, out List<string> metaList)
        where T: IStructIntermediateTable<TechnologicalCard,C>, new()
    {

        var objList = new List<T>();
        metaList = new List<string>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var sheetName = typeof(T).Name;
            

            var worksheet = package.Workbook.Worksheets[sheetName];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                int.TryParse(Convert.ToString(worksheet.Cells[row, 1].Value),out var isIndex) ;
                int.TryParse(Convert.ToString(worksheet.Cells[row, 2].Value), out var isParentId);
                int.TryParse(Convert.ToString(worksheet.Cells[row, 8].Value), out var isChildId);
                int.TryParse(Convert.ToString(worksheet.Cells[row, 4].Value), out var isOrder);
                float.TryParse(Convert.ToString(worksheet.Cells[row, 10].Value), out var isQuantity);
                var isNote = Convert.ToString(worksheet.Cells[row, 11].Value);

                if (isParentId != 0 && isChildId != 0 && isOrder != 0 && isQuantity != 0)
                {
                    var obj = new T
                    {

                        ParentId = isParentId,
                        ChildId = isChildId,
                        Order = isOrder,
                        Quantity = isQuantity,
                        Note = isNote == "" ? null : isNote,
                    };


                    // check if object with the same id pairs already exists in the list
                    
                    if (objList.Exists(x => x.ParentId == obj.ParentId && x.ChildId == obj.ChildId))
                    {
                        var exObj = objList.Where(x => x.ParentId == obj.ParentId && x.ChildId == obj.ChildId).FirstOrDefault();
                        exObj.Quantity += obj.Quantity;
                    }
                    else 
                    { 
                        objList.Add(obj); 
                    }
                }
                else
                    metaList.Add($"Ошибка парсинга строке {isIndex}");
            }
        }
        return objList;
    }
    
    public List<TechnologicalCard> ParseExcelToObjectsTc(string filePath)
    {
        var objList = new List<TechnologicalCard>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets["ТК"];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var isCompleted = Convert.ToString(worksheet.Cells[row, 13].Value);
                var nameTc = Convert.ToString(worksheet.Cells[row, 14].Value);
                string NoName = "Без названия";

                var obj = new TechnologicalCard
                {
                    Id = Convert.ToInt32(worksheet.Cells[row, 16].Value),

                    Article = Convert.ToString(worksheet.Cells[row, 1].Value),

                    Name = nameTc=="" ? NoName : nameTc,

                    Type = Convert.ToString(worksheet.Cells[row, 2].Value),
                    NetworkVoltage = Convert.ToSingle(worksheet.Cells[row, 3].Value),
                    TechnologicalProcessType = Convert.ToString(worksheet.Cells[row, 4].Value),
                    TechnologicalProcessName = Convert.ToString(worksheet.Cells[row, 5].Value),
                    Parameter = Convert.ToString(worksheet.Cells[row, 6].Value),
                    TechnologicalProcessNumber = Convert.ToString(worksheet.Cells[row, 7].Value),
                    FinalProduct = Convert.ToString(worksheet.Cells[row, 8].Value),
                    Applicability = Convert.ToString(worksheet.Cells[row, 9].Value),
                    Note = Convert.ToString(worksheet.Cells[row, 10].Value),
                    DamageType = Convert.ToString(worksheet.Cells[row, 11].Value),
                    RepairType = Convert.ToString(worksheet.Cells[row, 12].Value),

                    IsCompleted = isCompleted == "Есть" ? true : false,

                    //Status = TechnologicalCard.TechnologicalCardStatus.Approved,

                };


                objList.Add(obj);
            }
        }
        return objList;
    }
    public List<Staff> ParseExcelToObjectsStaff(string filePath)
    {
        var staffList = new List<Staff>();

        // Первый проход для создания списка staffList и обработки связей
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets["Staff"];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var staff = new Staff
                {
                    Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                    Name = Convert.ToString(worksheet.Cells[row, 2].Value),
                    Type = Convert.ToString(worksheet.Cells[row, 3].Value),
                    Functions = Convert.ToString(worksheet.Cells[row, 5].Value),
                    CombineResponsibility = Convert.ToString(worksheet.Cells[row, 6].Value),
                    Qualification = Convert.ToString(worksheet.Cells[row, 7].Value),
                    Comment = Convert.ToString(worksheet.Cells[row, 8].Value),

                    IsReleased = true,
                };

                staffList.Add(staff);
            }

            
        }

        //// Установка связей между объектами
        //foreach (var staff in staffList)
        //{
        //    if (!string.IsNullOrWhiteSpace(staff.CombineResponsibility) && staff.CombineResponsibility.Count() > 2)
        //    {
        //        var relatedStaffNames = staff.CombineResponsibility.Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);

        //        foreach (var relatedStaffName in relatedStaffNames)
        //        {
        //            var trimmedName = relatedStaffName.Trim();
        //            var relatedStaff = staffList.FirstOrDefault(s => $"{s.Name}: {s.Type}".Equals(trimmedName, StringComparison.OrdinalIgnoreCase));
        //            if (relatedStaff != null)
        //            {
        //                staff.AddRelatedStaff(relatedStaff);
        //            }
        //        }
        //    }
        //}

        return staffList;
    }

    public List<Protection> ParseExcelToObjectsProtection(string filePath)
    {
        var objList = new List<Protection>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets["Protect"];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var linkValie = Convert.ToString(worksheet.Cells[row, 9].Value);
                var manufacturer = Convert.ToString(worksheet.Cells[row, 8].Value);

                var obj = new Protection
                {
                    Id = Convert.ToInt32(worksheet.Cells[row, 11].Value),
                    Name = Convert.ToString(worksheet.Cells[row, 2].Value),
                    Type = Convert.ToString(worksheet.Cells[row, 3].Value),
                    Unit = Convert.ToString(worksheet.Cells[row, 4].Value),
                    
                    Description = Convert.ToString(worksheet.Cells[row, 7].Value),

                    Manufacturer = manufacturer,
                    Links = ParseLinks2(linkValie, manufacturer),

                    ClassifierCode = Convert.ToString(worksheet.Cells[row, 6].Value),


                    IsReleased = true,
                };

                objList.Add(obj);
            }
        }

        return objList;
    }
    public List<Machine> ParseExcelToObjectsMachine(string filePath)
    {
        var objList = new List<Machine>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets["Machinery"];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var linkValie = Convert.ToString(worksheet.Cells[row, 8].Value);
                var manufacturer = Convert.ToString(worksheet.Cells[row, 7].Value);

                var obj = new Machine
                {
                    Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                    Name = Convert.ToString(worksheet.Cells[row, 2].Value),
                    Type = Convert.ToString(worksheet.Cells[row, 3].Value),
                    Unit = Convert.ToString(worksheet.Cells[row, 4].Value),

                    Description = Convert.ToString(worksheet.Cells[row, 6].Value),

                    Manufacturer = manufacturer,
                    Links = ParseLinks2(linkValie, manufacturer),

                    ClassifierCode = Convert.ToString(worksheet.Cells[row, 5].Value),


                    IsReleased = true,
                };

                objList.Add(obj);
            }
        }

        return objList;
    }
    public List<Component> ParseExcelToObjectsComponent(string filePath)
    {
        var objList = new List<Component>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets["Component"];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var linkValie = Convert.ToString(worksheet.Cells[row, 9].Value);
                var manufacturer = Convert.ToString(worksheet.Cells[row, 8].Value);

                var obj = new Component
                {
                    //Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                    Name = Convert.ToString(worksheet.Cells[row, 2].Value),
                    Type = Convert.ToString(worksheet.Cells[row, 3].Value),
                    Unit = Convert.ToString(worksheet.Cells[row, 4].Value),
                    Price = Convert.ToSingle(worksheet.Cells[row, 6].Value),
                    Description = Convert.ToString(worksheet.Cells[row, 7].Value),

                    Manufacturer = manufacturer,
                    Links = ParseLinks2(linkValie, manufacturer),

                    Categoty = Convert.ToString(worksheet.Cells[row, 10].Value),

                    ClassifierCode = Convert.ToString(worksheet.Cells[row, 5].Value),


                    IsReleased = true,
                };

                objList.Add(obj);
            }
        }

        return objList;
    }
    public List<Tool> ParseExcelToObjectsTool(string filePath)
    {
        var objList = new List<Tool>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets["Tools"];
            int rowCount = worksheet.Dimension.Rows;
            
            for (int row = 2; row <= rowCount; row++)
            {
                var linkValie = Convert.ToString(worksheet.Cells[row, 9].Value);
                var manufacturer = Convert.ToString(worksheet.Cells[row, 8].Value);


                var obj = new Tool
                {
                    //Id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                    Name = Convert.ToString(worksheet.Cells[row, 2].Value),
                    Type = Convert.ToString(worksheet.Cells[row, 3].Value),
                    Unit = Convert.ToString(worksheet.Cells[row, 4].Value),
                    //Price = Convert.ToSingle(worksheet.Cells[row, 6].Value),
                    Description = Convert.ToString(worksheet.Cells[row, 7].Value),

                    Manufacturer = manufacturer,
                    Links = ParseLinks2(linkValie, manufacturer),

                    Categoty = Convert.ToString(worksheet.Cells[row, 10].Value),

                    ClassifierCode = Convert.ToString(worksheet.Cells[row, 6].Value),


                    IsReleased = true,
                };

                objList.Add(obj);
            }
        }

        return objList;
    }

    public List<string> ParseLinks(string linkValie, string manufacturer) 
    { 
        var objList = new List<string>();
        if (linkValie != "NoLink" && linkValie != "")
        {
            objList.Add(linkValie);
        }
        if (manufacturer != "")
        {
            if (manufacturer.Contains("http"))
            {
                var links = manufacturer.Split('\n');
                foreach (var link in links)
                {
                    if (link != linkValie)
                        objList.Add(link.Trim());
                }
            }
        }
        return objList;
    }
    public List<LinkEntety> ParseLinks2(string linkValie, string manufacturer)
    {
        var objList = new List<LinkEntety>();
        if (linkValie != "NoLink" && linkValie != "")
        {
            var link = new LinkEntety
            {
                Link = linkValie,
            };
            objList.Add(link);
        }
        if (manufacturer != "")
        {
            if (manufacturer.Contains("http"))
            {
                var links = manufacturer.Split('\n');
                foreach (var linkValue in links)
                {
                    var link = new LinkEntety
                    {
                        Link = linkValue.Trim(),
                    };
                    if (linkValue != linkValie)
                        objList.Add(link);
                }
            }
        }
        return objList;
    }

    public List<List<string>> ParseRowsToStrings(List<string> columnsNames, string filepath, 
        string? sheetName = null, int sheetNumber = 0, int startRow = 1, int endRow = maxRow)
    {
        List<List<string>> structs = new();

        if (sheetName == null && sheetNumber == 0) sheetNumber = 1;
        if (sheetName != null && sheetNumber != 0) throw new Exception("sheetName and sheetNumber can't be set at the same time");
        
        using (var package = new ExcelPackage(new FileInfo(filepath)))
        {
            // determine sheet name by number
            if (sheetName == null)
            {
                // get sheets names
                List<string> sheetsNames = new();
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    sheetsNames.Add(sheet.Name);
                }
                sheetName = sheetsNames[sheetNumber - 1];
            }

            var worksheet = package.Workbook.Worksheets[sheetName];
            
            // find end row as last not empty row
            if (endRow == maxRow) endRow = worksheet.Dimension.End.Row;

            int columnNameRow = startRow + addToTitleRow;

            var columnsNums = FindListColumn(columnsNames, columnNameRow, worksheet);
            // todo - check if all columnsNums equals 0 - throw exception
            // todo - check whick columnsNums equals 0 - throw exception with columns names

            // todo - don't breake the code if columnsNums contains 0

            if (columnsNums.Contains(0)) throw new Exception("Can't find column name in the table");
            for (int i = startRow + addToTable; i < endRow; i++)
            {
                // todo - add try catch block to get exceptions in row parsing and save them to history
                List<string> rowValues = GetCellsValue(worksheet, columnsNums, i);
                structs.Add(rowValues);
            }
        }
        return structs;
    }

    private List<int> FindListColumn(List<string> columnNameList, int columnRow, ExcelWorksheet worksheet)
    {
        List<int> numsColumns = new List<int>();
        for (int i = 0; i < columnNameList.Count; i++)
        {
            numsColumns.Add(FindColumn(columnNameList[i], columnRow, worksheet));
        }
        return numsColumns;
    }
    private int FindColumn(string columnName, int columnRow, ExcelWorksheet worksheet)
    {
        string[] columnNameList = { columnName };
        return FindColumn(columnNameList, columnRow, worksheet);
    }
    private int FindColumn(string[] columnNameList, int columnRow, ExcelWorksheet worksheet)
    {
        int numColumn = 0;
        for (int i = 1; i < maxColumn; i++)
        {
            if (worksheet.Cells[columnRow, i].Value != null && columnNameList.Contains(worksheet.Cells[columnRow, i].Value.ToString()))
            { numColumn = i; break; }
        }
        return numColumn;
    }
    public static Dictionary<string, int> GetColumnsNumbers(int columnRow, ExcelWorksheet worksheet)
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
    private string GetCellValue(int row, int column, ExcelWorksheet worksheet)
    {
        return worksheet.Cells[row, column].Value?.ToString()?.Trim();
    }
    private string? GetCellValue(int row, int column, ExcelWorksheet worksheet, string? defaultValue)
    {
        return GetCellValue(row, column, worksheet) ?? defaultValue; // 
    }
    private List<string> GetCellsValue(ExcelWorksheet worksheet, List<int> columns, int row)
    {
        List<string> values = new List<string>();
        foreach (var column in columns)
        {
            values.Add(GetCellValue(row, column, worksheet, ""));
        }
        return values;
    }

    public void FindTableBorderRows(Dictionary<string, EModelType> keyValuePairs, string filepath, 
        out Dictionary<EModelType?, int> modelStartRows,
        out Dictionary<EModelType?, int> modelEndRows,
        string? sheetName = null, int sheetNumber = 0)
    {
        modelStartRows = new();
        modelEndRows = new();

        if (sheetName == null && sheetNumber == 0) sheetNumber = 1;
        if (sheetName != null && sheetNumber != 0) throw new Exception("sheetName and sheetNumber can't be set at the same time");

        foreach (var item in keyValuePairs)
        {
            modelStartRows.Add(item.Value, 0);
            modelEndRows.Add(item.Value, 0);
        }

        EModelType? currentTableType = null;

        bool lastTableStarts = false;

        using (var package = new ExcelPackage(new FileInfo(filepath)))
        {
            // determine sheet name by number
            if (sheetName == null)
            {
                // get sheets names
                List<string> sheetsNames = new();
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    sheetsNames.Add(sheet.Name);
                }
                sheetName = sheetsNames[sheetNumber - 1];
            }
            var worksheet = package.Workbook.Worksheets[sheetName];
            for (int i = 1; i < maxRow; i++)
            {
                string valueCell = worksheet.Cells[i, 1].Value != null ? worksheet.Cells[i, 1].Value.ToString() : "";

                if (keyValuePairs.Keys.Contains(valueCell))
                {
                    foreach (var item in keyValuePairs)
                    {
                        if (item.Key == valueCell)
                        {
                            if (currentTableType != null) modelEndRows[currentTableType] = i - 1;
                            currentTableType = item.Value;
                            modelStartRows[item.Value] = i;

                            // check if it is last table (as if it no one 0 value in modelStartRows)
                            if (!modelStartRows.Values.Contains(0)) { lastTableStarts = true; }
                            break;
                        }
                    }
                }
                // looking for last row of last table as first row whith null or unparsable it double value in "A" column
                if (lastTableStarts && i > modelStartRows[currentTableType] + 1)
                {
                    if (valueCell == "" || !double.TryParse(valueCell, out double _))
                    {
                        modelEndRows[currentTableType] = i - 1;
                        break;
                    }
                }
            }
        }
    }

    public List<TcParserInfo> ParseTcParserInfo(string filePath)
    {
        var parserInfos = new List<TcParserInfo>();

        FileInfo fileInfo = new FileInfo(filePath);
        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[1]; // Предполагаем, что данные на первом листе

            int rowCount = worksheet.Dimension.Rows;
            int colFilePath = 1;
            int colSheetName = 2;
            int colIntermediateParsed = 3;
            int colExecutionParsed = 4;
            int notes = 5;
            int colParsed = 6;
            int colExcluded = 8;

            for (int row = 2; row <= rowCount; row++) // Начинаем со второй строки, т.к. первая строка это заголовок
            {
                string file = worksheet.Cells[row, colFilePath].Text;
                string sheetName = worksheet.Cells[row, colSheetName].Text;
                bool isIntermediateParsed = worksheet.Cells[row, colIntermediateParsed].Text == "+";
                bool isExecutionParsed = worksheet.Cells[row, colExecutionParsed].Text == "+";
                bool isParsed = worksheet.Cells[row, colParsed].Text == "1";
                bool isExcluded = worksheet.Cells[row, colExcluded].Text == "1";
                string note = worksheet.Cells[row, notes].Text;

                var info = new TcParserInfo
                {
                    FilePath = file,
                    SheetName = sheetName,
                    IsIntermediateParsed = isIntermediateParsed,
                    IsExecutionParsed = isExecutionParsed,
                    IsParsed = isParsed,
                    IsExcluded = isExcluded,
                    Note = note,
                };

                parserInfos.Add(info);
            }
        }

        return parserInfos;
    }

    private class LogInfor
    {
        public string FileName { get; set; } = null!;
        public string? Article { get; set; }
        public string? ParsingResult1to5 { get; set; }
        public string? ParsingResult6 { get; set; }
        public string? Notes { get; set; }

        public LogInfor(string FileName)
        {
            this.FileName = FileName;
        }
    }
}

public class TcParserInfo
{
    public string FilePath { get; set; }
    public string SheetName { get; set; }
    public bool IsIntermediateParsed { get; set; }
    public bool IsExecutionParsed { get; set; }
    public bool IsParsed { get; set; }
    public bool IsExcluded { get; set; }
    public string? Note { get; set; }
}
