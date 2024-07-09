
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Runtime.CompilerServices;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace ExcelParsing.DataProcessing;

public class IntermediateTablesParser
{
    private string? _notes;

    List<string> tablesNames = new List<string> { "Требования к составу бригады и квалификации", 
                 "Требования к материалам и комплектующим",
                 "Требования к механизмам",
                 "Требования к средствам защиты",
                 "Требования к инструментам и приспособлениям",
                 "Выполнение работ"};

    public static Dictionary<string,string> protectionExceptions = new Dictionary<string, string>
    {
        { "Стеклопластиковый колышек", "12x1500" },
        { "Сумка монтажника", "C-10" },
    };
    public static Dictionary<(string, string), (string, string)> toolExceptions = new Dictionary<(string, string), (string, string)>
    {
        { ("Цепной строп", "2СЦ-3,0-3000"),("Цепной строп","2СЦ-3,0-5000") },
    };


    public IntermediateTablesParser()
    {
        ExcelPackage.LicenseContext = LicenseContext.Commercial;
    }

    public void ParseIntermediateObjects(string tcFilePath, string tcArticle, ref string? notes, CachedData? cachedData = null)
    {

        if (notes != null)
        {
            _notes = notes;
        }

        var fileInfo = new FileInfo(tcFilePath);

        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"Файл {tcFilePath} не найден.");
        }

        using (var package = new ExcelPackage(fileInfo))
        {
            var stepSheet = package.Workbook.Worksheets[tcArticle];
            if (stepSheet == null)
            {
                throw new Exception($"Лист {tcArticle} не найден в файле.");
            }

            if (cachedData == null)
            {
                // Кэшируем данные из БД
                CacheDbData(out var staffsCache, out var machinesCache, out var componentsCache, out var toolsCache, out var protectionsCache);
                cachedData = new CachedData(staffsCache, componentsCache, toolsCache, machinesCache, protectionsCache, new List<TechOperation>(), new List<TechTransition>());
            }
            //// Кэшируем данные из БД
            //CacheDbData(out var staffsCache, out var machinesCache, out var componentsCache, out var toolsCache, out var protectionsCache);

            var currentTc = GetCurrentTechnologicalCard(tcArticle);

            Dictionary<string,int> startRows = new Dictionary<string, int>
            {
                { "Требования к составу бригады и квалификации", 0 },
                { "Требования к материалам и комплектующим", 0 },
                { "Требования к механизмам", 0 },
                { "Требования к средствам защиты", 0 },
                { "Требования к инструментам и приспособлениям", 0 },
                { "Выполнение работ", 0 }
            };

            foreach (var startRow in startRows)
            {
                startRows[startRow.Key] = FindStartRow(stepSheet, startRow.Key);
            }

            Dictionary<string, Dictionary<string, int>> tableColumnNumbers = new Dictionary<string, Dictionary<string, int>>
            {
                { "Требования к составу бригады и квалификации", new Dictionary<string, int>()
                    {
                        { "№", 0 },
                        { "Наименование", 0 },
                        { "Тип (исполнение)", 0 },
                        { "Возможность совмещения обязанностей", 0 },
                        { "Квалификация", 0 },
                        { "Обозначение", 0 },
                    } 
                },
                { "Требования к материалам и комплектующим", new Dictionary<string, int>()
                    {
                        { "№", 0 },
                        { "Наименование", 0 },
                        { "Тип (исполнение)", 0},
                        { "Ед. Изм.", 0 },
                        { "Кол-во", 0 },
                        { "Стоим., руб. без НДС", 0 },
                        { "Примечание", 0 }
                    } 
                },
                { "Требования к механизмам", new Dictionary<string, int>()
                    {
                        { "№", 0 },
                        { "Наименование", 0 },
                        { "Тип (исполнение)", 0},
                        { "Ед. Изм.", 0 },
                        { "Кол-во", 0 },
                    } 
                },
                { "Требования к средствам защиты", new Dictionary<string, int>()
                    {
                        { "№", 0 },
                        { "Наименование", 0 },
                        { "Тип (исполнение)", 0},
                        { "Ед. Изм.", 0 },
                        { "Кол-во", 0 },
                    }

                },
                { "Требования к инструментам и приспособлениям", new Dictionary<string, int>()
                    {
                        { "№", 0 },
                        { "Наименование", 0 },
                        { "Тип (исполнение)", 0},
                        { "Ед. Изм.", 0 },
                        { "Кол-во", 0 },
                    }
                }
            };

            foreach (var table in tableColumnNumbers)
            {
                if (table.Key != "Выполнение работ")
                {
                    //tableColumnNumbers[table.Key] = ExcelParser.GetColumnsNumbers(startRows[table.Key], stepSheet);
                    var colNumbers = ExcelParser.GetColumnsNumbers(startRows[table.Key], stepSheet);
                    foreach(var colName in table.Value)
                    {
                        if (table.Key == "Требования к составу бригады и квалификации" && colName.Key == "Обозначение")
                        {
                            if (colNumbers.ContainsKey("Обозначение в ТК"))
                            {
                                table.Value[colName.Key] = colNumbers["Обозначение в ТК"];
                            }
                            else
                            {
                                table.Value[colName.Key] = colNumbers[colName.Key];
                            }
                        }
                        else if (table.Key == "Требования к составу бригады и квалификации" && colName.Key == "Возможность совмещения обязанностей")
                        {
                            if (colNumbers.ContainsKey("Возможность совмещения"))
                            {
                                table.Value[colName.Key] = colNumbers["Возможность совмещения"];
                            }
                            else
                            {
                                table.Value[colName.Key] = colNumbers[colName.Key];
                            }
                        }
                        else if (table.Key == "Требования к материалам и комплектующим" && colName.Key == "Примечание")
                        {
                            if (colNumbers.ContainsKey("Примечание"))
                            {
                                table.Value[colName.Key] = colNumbers[colName.Key];
                            }
                            else
                            {
                                // удалить колонку "Примечание"
                                table.Value.Remove(colName.Key);
                            }
                        }
                        else if (table.Key == "Требования к материалам и комплектующим" && colName.Key == "Стоим., руб. без НДС")
                        {
                            if (colNumbers.ContainsKey("Стоимость, руб. без НДС"))
                            {
                                table.Value[colName.Key] = colNumbers["Стоимость, руб. без НДС"]; 
                            }
                            else
                            {
                                table.Value[colName.Key] = colNumbers[colName.Key];
                            }
                        }
                        else
                        {
                            table.Value[colName.Key] = colNumbers[colName.Key];
                        }
                    }
                }
            }

            // проверка все ли колонки найдены
            foreach (var table in tableColumnNumbers)
            {
                foreach (var column in table.Value)
                {
                    if (column.Value == 0)
                    {
                        throw new Exception($"Столбец {column.Key} не найден в таблице {table.Key}");
                    }
                }
            }

            // создать транзакцию с БД и добавить все объекты
            ParseTables(stepSheet, startRows, tableColumnNumbers, currentTc,
                cachedData.Staffs, cachedData.Machines, cachedData.Components, cachedData.Tools, cachedData.Protections);
        }
    }

    
    public void CacheDbData(out List<Staff> staffs,
        out List<Machine> machines,
        out List<Component> components,
        out List<Tool> tools,
        out List<Protection> protections)
    {
        using (var context = new MyDbContext())
        {
            staffs = context.Staffs.ToList();
            machines = context.Machines.ToList();
            components = context.Components.ToList();
            tools = context.Tools.ToList();
            protections = context.Protections.ToList();

        }
    }
    private TechnologicalCard GetCurrentTechnologicalCard(string tcArticle)
    {
        using (var context = new MyDbContext())
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
                //Console.WriteLine("Таблица 6 начинается на строке: " + (row + 1));
                return row + 1;
            }
        }
        throw new Exception($"Таблица {tableName} не найдена в листе.");
    }

    private void ParseTables(ExcelWorksheet stepSheet, 
        Dictionary<string, int> startRows, 
        Dictionary<string, Dictionary<string, int>> tableColumnNumbers,
        TechnologicalCard currentTc,
        List<Staff> staffsCache, List<Machine> machinesCache, 
        List<Component> componentsCache, List<Tool> toolsCache, 
        List<Protection> protectionsCache)
    {
        using (var context = new MyDbContext())
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // очистить все связанные таблицы с текущей ТК
                    context.Staff_TCs.RemoveRange(context.Staff_TCs.Where(st => st.ParentId == currentTc.Id));
                    context.Machine_TCs.RemoveRange(context.Machine_TCs.Where(st => st.ParentId == currentTc.Id));
                    context.Component_TCs.RemoveRange(context.Component_TCs.Where(st => st.ParentId == currentTc.Id));
                    context.Tool_TCs.RemoveRange(context.Tool_TCs.Where(st => st.ParentId == currentTc.Id));
                    context.Protection_TCs.RemoveRange(context.Protection_TCs.Where(st => st.ParentId == currentTc.Id));


                    // Парсинг и добавление новых объектов
                    var tableName = "Требования к составу бригады и квалификации";
                    try
                    {
                        var staffTCs = ParseRows(startRows[tableName] + 1, startRows["Требования к материалам и комплектующим"] - 2,
                        stepSheet, tableColumnNumbers[tableName], staffsCache, currentTc);
                        context.Staff_TCs.AddRange(staffTCs);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("ОШИБКА при парсинге таблицы 1.\n" + e.Message);
                    }

                    tableName = "Требования к материалам и комплектующим";
                    try
                    {
                        var componentTCs = ParseRows(startRows[tableName] + 1, startRows["Требования к механизмам"] - 2,
                        stepSheet, tableColumnNumbers[tableName], componentsCache, currentTc);
                        context.Component_TCs.AddRange(componentTCs);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("ОШИБКА при парсинге таблицы 2.\n" + e.Message);
                    }

                    tableName = "Требования к механизмам";
                    try
                    {
                        var machineTCs = ParseRows(startRows[tableName] + 1, startRows["Требования к средствам защиты"] - 2,
                        stepSheet, tableColumnNumbers[tableName], machinesCache, currentTc);
                        context.Machine_TCs.AddRange(machineTCs);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("ОШИБКА при парсинге таблицы 3.\n" + e.Message);
                    }

                    tableName = "Требования к средствам защиты";
                    try
                    {
                        var protectionTCs = ParseRows(startRows[tableName] + 1, startRows["Требования к инструментам и приспособлениям"] - 2,
                        stepSheet, tableColumnNumbers[tableName], protectionsCache, currentTc);
                        context.Protection_TCs.AddRange(protectionTCs);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("ОШИБКА при парсинге таблицы 4.\n" + e.Message);
                    }

                    tableName = "Требования к инструментам и приспособлениям";
                    try
                    {
                        var toolTCs = ParseRows(startRows[tableName] + 1, startRows["Выполнение работ"] - 2,
                        stepSheet, tableColumnNumbers[tableName], toolsCache, currentTc);
                        context.Tool_TCs.AddRange(toolTCs);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("ОШИБКА при парсинге таблицы 5.\n" + e.Message);
                    }

                    context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }
        }
    }

    private List<Staff_TC> ParseRows(int startRow, int endRow, ExcelWorksheet sheet, Dictionary<string, int> columnsNumbers, List<Staff> staffsCache, TechnologicalCard currentTc)
    {
        var objList = new List<Staff_TC>();

        var parentId = currentTc.Id;

        for (int row = startRow; row <= endRow; row++)
        {
            var name = Convert.ToString(sheet.Cells[row, columnsNumbers["Наименование"]].Value)?.Trim();
            var type = Convert.ToString(sheet.Cells[row, columnsNumbers["Тип (исполнение)"]].Value)?.Trim();
            // определение объекта staff
            var obj = staffsCache.FirstOrDefault(st => st.Name == name && st.Type == type);
            if (obj == null)
            {
                throw new Exception($"Объект {typeof(Staff_TC)} {name} {type} не найден в БД. Строка  {row}");
            }

            int.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["№"]].Value), out var order);


            string? symbol  = Convert.ToString(sheet.Cells[row, columnsNumbers["Обозначение"]].Value);
            
            if (order != 0 && !string.IsNullOrEmpty(symbol))
            {
                var obj_tc = new Staff_TC
                {

                    ParentId = parentId,
                    ChildId = obj.Id,
                    Order = order,
                    Symbol = symbol,
                };

                objList.Add(obj_tc);

                // Console.WriteLine($"    {order}. {name} {type} ({symbol})");
            }
            else
            {
                throw new Exception($"Не заполнено поле стоимость для объекта {typeof(Staff_TC)} {name} {type}. Строка {row}");
            }
        }

        return objList;
    }
    private List<Component_TC> ParseRows(int startRow, int endRow, ExcelWorksheet sheet, Dictionary<string, int> columnsNumbers, List<Component> componentsCache, TechnologicalCard currentTc)
    {
        var objList = new List<Component_TC>();

        var parentId = currentTc.Id;

        for (int row = startRow; row <= endRow; row++)
        {
            var name = Convert.ToString(sheet.Cells[row, columnsNumbers["Наименование"]].Value)?.Trim();
            var type = Convert.ToString(sheet.Cells[row, columnsNumbers["Тип (исполнение)"]].Value)?.Trim();
            // определение объекта staff
            var obj = componentsCache.FirstOrDefault(st => st.Name == name && st.Type == type);
            if (obj == null)
            {
                throw new Exception($"Объект {typeof(Component_TC)} {name} {type} не найден в БД. Строка   {row}");
            }

            int.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["№"]].Value), out var order);
            double.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["Кол-во"]].Value), out var quantity);
            string note = "";
            if (columnsNumbers.ContainsKey("Примечание"))
                note = Convert.ToString(sheet.Cells[row, columnsNumbers["Примечание"]].Value)?? "";

            if (order != 0)
            {
                var obj_tc = new Component_TC
                {

                    ParentId = parentId,
                    ChildId = obj.Id,
                    Order = order,
                    Quantity = quantity,
                    Note = note,
                };

                objList.Add(obj_tc);
            }
            
        }

        return objList;
    }
    private List<Machine_TC> ParseRows(int startRow, int endRow, ExcelWorksheet sheet, Dictionary<string, int> columnsNumbers, List<Machine> machinesCache, TechnologicalCard currentTc)
    {
        var objList = new List<Machine_TC>();

        var parentId = currentTc.Id;

        for (int row = startRow; row <= endRow; row++)
        {
            var name = Convert.ToString(sheet.Cells[row, columnsNumbers["Наименование"]].Value)?.Trim();
            var type = Convert.ToString(sheet.Cells[row, columnsNumbers["Тип (исполнение)"]].Value)?.Trim();
            // определение объекта staff
            var staff = machinesCache.FirstOrDefault(st => st.Name == name && st.Type == type);
            if (staff == null)
            {
                throw new Exception($"Объект {typeof(Machine_TC)} {name} {type} не найден в БД. Строка {row}");
            }

            int.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["№"]].Value), out var order);
            int.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["Кол-во"]].Value), out var quantity);

            

            if (order != 0)
            {
                var obj = new Machine_TC
                {

                    ParentId = parentId,
                    ChildId = staff.Id,
                    Order = order,
                    Quantity = quantity,

                };

                objList.Add(obj);
            }

            if ( quantity != 0)
                _notes += $"Не заполнено поле стоимость для объекта {typeof(Machine_TC)} {name} {type}. Строка {row}\n";
        }

        return objList;
    }
    private List<Protection_TC> ParseRows(int startRow, int endRow, ExcelWorksheet sheet, Dictionary<string, int> columnsNumbers, List<Protection> protectionsCache, TechnologicalCard currentTc)
    {
        var objList = new List<Protection_TC>();

        var parentId = currentTc.Id;

        for (int row = startRow; row <= endRow; row++)
        {
            var name = Convert.ToString(sheet.Cells[row, columnsNumbers["Наименование"]].Value)?.Trim();
            var type = Convert.ToString(sheet.Cells[row, columnsNumbers["Тип (исполнение)"]].Value)?.Trim();
            // определение объекта staff
            var obj = protectionsCache.FirstOrDefault(st => st.Name == name && st.Type == type);
            if (obj == null)
            {
                throw new Exception($"Объект {typeof(Protection_TC)} {name} {type} не найден в БД. Строка {row}");
            }

            int.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["№"]].Value), out var order);
            int.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["Кол-во"]].Value), out var quantity);

            

            if (order != 0)
            {
                var obj_tc = new Protection_TC
                {
                    ParentId = parentId,
                    ChildId = obj.Id,
                    Order = order,
                    Quantity = quantity,
                };

                objList.Add(obj_tc);
            }
            if (quantity != 0)
                _notes += $"Не заполнено поле стоимость для объекта {typeof(Protection_TC)} {name} {type}. Строка {row}\n";
        }

        return objList;
    }
    private List<Tool_TC> ParseRows(int startRow, int endRow, ExcelWorksheet sheet, Dictionary<string, int> columnsNumbers, List<Tool> toolsCache, TechnologicalCard currentTc)
    {
        var objList = new List<Tool_TC>();

        var parentId = currentTc.Id;

        for (int row = startRow; row <= endRow; row++)
        {
            var name = Convert.ToString(sheet.Cells[row, columnsNumbers["Наименование"]].Value)?.Trim();
            var type = Convert.ToString(sheet.Cells[row, columnsNumbers["Тип (исполнение)"]].Value)?.Trim();
            // определение объекта staff
            var obj = toolsCache.FirstOrDefault(st => st.Name == name && st.Type == type);
            if (obj == null)
            {
                if(!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type) && toolExceptions.ContainsKey((name, type)))
                {
                    (name, type) = toolExceptions[(name, type)];
                    obj = toolsCache.FirstOrDefault(st => st.Name == name && st.Type == type);

                    if (obj == null)
                    {
                        throw new Exception($"Объект {typeof(Tool_TC)} {name} {type} не найден в БД. Строка  {row}");
                    }
                }
                else
                {
                    throw new Exception($"Объект {typeof(Tool_TC)} {name} {type} не найден в БД. Строка  {row}");
                }

            }

            int.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["№"]].Value), out var order);
            int.TryParse(Convert.ToString(sheet.Cells[row, columnsNumbers["Кол-во"]].Value), out var quantity);

            if (order != 0)
            {
                var obj_tc = new Tool_TC
                {
                    ParentId = parentId,
                    ChildId = obj.Id,
                    Order = order,
                    Quantity = quantity,
                };

                objList.Add(obj_tc);
            }
            if (quantity != 0)
                _notes += $"Не заполнено поле стоимость для объекта {typeof(Tool_TC)} {name} {type}. Строка {row}\n";
        }

        return objList;
    }
}
