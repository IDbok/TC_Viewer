using OfficeOpenXml;
using TcDbConnector;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace ExcelParsing.DataProcessing;

public class DictionaryParser
{
    private readonly MyDbContext context;

    public DictionaryParser(MyDbContext context)
    {
        ExcelPackage.LicenseContext = LicenseContext.Commercial;

        this.context = context;
    }

    public void ParseDictionaries(string filePath)
    {
        using var package = new ExcelPackage(new FileInfo(filePath));

        var techOperations = MapTechOperations(package, "Перечень ТО");

        var techTransitions = MapTechTransitions(package, "Типовые переходы");

        var techTransitionTypicals = MapTechTransitionTypicals(package, "Типовые ТО", techOperations, techTransitions);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {
                context.AddRange(techOperations);
                context.AddRange(techTransitions);
                context.AddRange(techTransitionTypicals);

                context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new Exception("Ошибка при сохранении данных в БД", e);
            }
        }

        //DbCreator.AddTechOperations(techOperations);
    }


    public List<TechOperation> MapTechOperations(ExcelPackage package, string sheetName)
    {

        var worksheet = GetWorksheet(package, sheetName);

        // проверка наличия столбцов
        var columnNames = new List<string>
        {
            "№",
            "Наименование",
            "Категория"
        };

        CheckAllColumnsExists(worksheet, columnNames);

        int rowCount = worksheet.Dimension.Rows;
        int currentId = 1;

        var techOperations = new List<TechOperation>();
        for (int row = 2; row <= rowCount; row++)
        {
            string? name = Convert.ToString(worksheet.Cells[row, 2].Value);
            string? Category = Convert.ToString(worksheet.Cells[row, 3].Value);

            if(string.IsNullOrEmpty(name)) // todo: add logger
            {
                //this.Logger.LogWarning($"Строка {row} не содержит наименование ТО");
                continue;
            }

            var obj = new TechOperation
            {
                Name = name,

                Category = string.IsNullOrEmpty(Category) ? "ТО" : Category,

                CreatedTCId = currentId,

                IsReleased = true,
            };

            techOperations.Add(obj);
            currentId++;
        }

        return techOperations;
    }

    public List<TechTransition> MapTechTransitions(ExcelPackage package, string sheetName)
    {
        var worksheet = GetWorksheet(package, sheetName);

        // получение названий категорий их столбец. В первой строке должны быть названия столбцов с категориями
        var categories = new Dictionary<string, int>();
        for(int i = 1; i <= worksheet.Dimension.Columns; i++)
        {
            var categoryName = Convert.ToString(worksheet.Cells[1, i].Value);
            if (string.IsNullOrEmpty(categoryName))
            {
                continue;
            }
            categories.Add(categoryName, i);
        }
        // для каждой категории получаем список переходов
        var techTransitions = new List<TechTransition>();
        int currentId = 1;
        int emptyNameCount = 2;
        foreach (var category in categories)
        {
            var colNumber = category.Value;
            for (int row = 3; row <= worksheet.Dimension.Rows; row++)
            {
                var cellName = worksheet.Cells[row, colNumber];
                string? name = Convert.ToString(cellName.Value)?.Trim();
                string? commentName = cellName.Comment?.Text;

                if (string.IsNullOrEmpty(name)) // todo: добавить проверку на наличие времени и установить его для NA значений
                {

                    emptyNameCount--;
                    //this.Logger.LogWarning($"Строка {row} не содержит наименование ТО");
                    if (emptyNameCount == 0)
                    {
                        Console.WriteLine($"Тех переход категории {category} завершился на строке {row - 2}");
                        emptyNameCount = 2;
                        break;
                    }
                    continue;
                }

                var celltimeExecution = worksheet.Cells[row, colNumber + 1];
                string? timeExecutionStr = celltimeExecution.Value?.ToString();
                bool isTimeExecutionDouble = double.TryParse(timeExecutionStr, out double timeExecution);
                string? commentTimeExecution = celltimeExecution.Comment?.Text; // todo: избавить от имени создателя комментария


                
                if (!isTimeExecutionDouble)
                {
                    timeExecution = 1;
                    Console.WriteLine($"Тех переход {name} с нераспознанным временем ({timeExecutionStr})");
                }

                CleanComments(ref commentName);
                CleanComments(ref commentTimeExecution);

                var obj = new TechTransition
                {
                    Id = currentId,
                    Name = name,
                    TimeExecution = timeExecution,

                    CommentName = commentName,
                    CommentTimeExecution = commentTimeExecution,

                    Category = category.Key,

                    CreatedTCId = currentId,

                    IsReleased = true,
                };

                techTransitions.Add(obj);
                currentId++;
            }
        }

        return techTransitions;
    }

    public List<TechTransitionTypical> MapTechTransitionTypicals(ExcelPackage package, string sheetName,
        List<TechOperation> techOperations, List<TechTransition> techTransitions)
    {
        var worksheet = GetWorksheet(package, sheetName);

        // проверка наличия столбцов
        var columnNames = new List<string>
        {
            "№",
            "Наименование технологической операции",
            "Технологические переходы",
            "Время выполнения действия, мин.",
            "Время выполнения этапа (параллельная работа), мин.",
            "Примечание",
        };
        CheckAllColumnsExists(worksheet, columnNames);

        int nameToColumNum = 2;
        int nameTpColumNum = 3;
        //int timeExecutionColumNum = 4;
        //int timeExecutionStageColumNum = 5;
        int rowCount = worksheet.Dimension.Rows;
        //int currentId = 1;

        var emptyRowCounter = 2;

        var techTransitionTypicals = new List<TechTransitionTypical>();
        int currentToId = 0;
        TechOperation? currentTo = null;
        string? lastEtapFormula = null;
        for (int row = 2; row <= rowCount; row++)
        {
            string nameTO = worksheet.Cells[row, nameToColumNum].Text.Trim();
            string nameTP = worksheet.Cells[row, nameTpColumNum].Text.Trim();
            //string timeExecutionStr = worksheet.Cells[row, 4].Text.Trim();
            string timeExecutionFormula = worksheet.Cells[row, 4].Formula;
            // string timeExecutionStageStr = worksheet.Cells[row, 5].Text.Trim();
            string timeExecutionStageFormula = worksheet.Cells[row, 5].Formula;
            string comment = worksheet.Cells[row, 6].Text.Trim();

            if(string.IsNullOrEmpty(nameTO) && string.IsNullOrEmpty(nameTP))
            {
                emptyRowCounter--;
                if (emptyRowCounter == 0)
                {
                    Console.WriteLine($"Технологическая операция завершилась на строке {row - 1}");
                    break;
                }
                //this.Logger.LogWarning($"Строка {row} не содержит наименование ТО или ТП");
                continue;
            }

            try
            {
                var(techOperation, techTransition) = GetTechOperationAndTransition(nameTO, nameTP, nameToColumNum, nameTpColumNum, techOperations, techTransitions, ref currentToId);
                if (techOperation != null)
                    currentTo = techOperation; currentTo!.Category = "Типовая ТО";


                var coefficient = WorkParser.GetStringBeforeAtSymbol(timeExecutionFormula);

                string? etap = null, posled = null;
                // Получение данных о этапах и последовательностях
                if(!string.IsNullOrEmpty(timeExecutionStageFormula) || !string.IsNullOrEmpty(lastEtapFormula))
                {
                    (etap, posled) = GetStages(row, timeExecutionStageFormula, ref lastEtapFormula);
                    // обнуляем формулу, если она не актуальна
                    if(string.IsNullOrEmpty(etap) && string.IsNullOrEmpty(posled))
                    {
                        lastEtapFormula = null;
                    }
                }

                var newTechTransitionTypical = new TechTransitionTypical
                {
                    //Id = currentId,
                    TechOperationId = currentTo!.Id,
                    TechOperation = currentTo!,
                    TechTransitionId = techTransition!.Id,
                    TechTransition = techTransition,

                    Coefficient = coefficient,
                    Comments = comment,

                    Etap = etap ?? "",
                    Posled = posled ?? "",
                };

                techTransitionTypicals.Add(newTechTransitionTypical);
            }
            catch (Exception e)
            {

               //this.Logger.LogError(e, $"Ошибка при обработке строки {row}");
                continue;
            }
        }
        return techTransitionTypicals;
    }
    private void GetValueAndFormula(ExcelWorksheet sheet, int row, int columnNum, out string value, out string formula)
    {
        value = sheet.Cells[row, columnNum].Value?.ToString() ?? "";
        formula = sheet.Cells[row, columnNum].Formula;
    }
    private (TechOperation? techOperation, TechTransition? techTransition) GetTechOperationAndTransition(
        //int row, //ExcelWorksheet sheet,
        string techOperationName, string techTransitionName,
        int columnNumTO, int columnNumTp,
        List<TechOperation> techOperations,
        List<TechTransition> techTransitions,
        ref int currentToId)
    {
        TechOperation? techOperation = null;
        TechTransition? techTransition = null;

        if (!string.IsNullOrEmpty(techOperationName))
        {
            techOperation = techOperations.FirstOrDefault(to => to.Name == techOperationName);
            if (techOperation == null)
            {
                throw new Exception($"Технологическая операция {techOperationName} не найдена в кэше.");
            }
            currentToId = techOperation.Id;
        }

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
                        ThrowExceptionTpNotFound(techTransitionName);
                    }

                    techTransition!.CommentName = techTransitionName;
                }
                else if (techTransitionName.Contains("Повторить п."))
                {
                    techTransition = techTransition = techTransitions.FirstOrDefault(tt => tt.Name == "Повторить п.");
                    if (techTransition == null)
                    {
                        ThrowExceptionTpNotFound(techTransitionName);
                    }

                    techTransition!.CommentName = techTransitionName;
                }
                else
                {
                    ThrowExceptionTpNotFound(techTransitionName);
                }

            }
        }

        return (techOperation, techTransition);

        void ThrowExceptionTpNotFound(string techTransitionName)
        {
            throw new Exception($"Технологический переход {techTransitionName} не найден в кэше.");
        }
    }
    private (string etap, string posled) GetStages(
        int row,
        string formulaEtap,
        ref string lastEtapFormula)
    {
        if (!string.IsNullOrEmpty(formulaEtap))
        {
            lastEtapFormula = formulaEtap;
        }
        else if (string.IsNullOrEmpty(lastEtapFormula))
        {
            return ("", "");
        }

        return WorkParser.ParseStageFormula(lastEtapFormula, row, out _);
    }

    private ExcelWorksheet GetWorksheet(ExcelPackage package, string sheetName)
    {
        var worksheet = package.Workbook.Worksheets[sheetName];

        if (worksheet == null)
        {
            throw new ArgumentException($"Лист {sheetName} не найден!");
        }

        return worksheet;
    }
    private void CleanComments(ref string? comment)
    {
        if (!string.IsNullOrEmpty(comment))
        {
            // разделить на две части до первого ":" и взять вторую часть
            var commentParts = comment.Split(':');
            if (commentParts.Length > 1)
            {
                //убрать пробелы в начале и конце
                comment = commentParts[1].Trim();
            }
        }
    }

    private void CheckAllColumnsExists(ExcelWorksheet worksheet, List<string> columnNames)
    {
        for (int i = 1; i <= columnNames.Count(); i++)
        {
            var cellValue = worksheet.Cells[1, i].Value?.ToString()?.Trim();
            var columnName = columnNames[i - 1];
            if (cellValue != columnName)
            {
                throw new ArgumentException($"Лист {worksheet.Name} не содержит необходимых столбцов!");
            }
        }
    }
}
