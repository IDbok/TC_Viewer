using OfficeOpenXml;
using ExcelParsing.DataProcessing;
using TcModels.Models;
using TcModels.Models.TcContent;
using System.Diagnostics;

namespace TC_WinForms.DataProcessing
{
    public class FromExelToObjectMapper
    {
        const int maxRow = 1000;
        const int maxColumn = 20;

        // create dictionarys with start and end rows for EModelType from keyValuePairs
        private Dictionary<EModelType?, int> modelStartRows = new();
        private Dictionary<EModelType?, int> modelEndRows = new();

        public Dictionary<EModelType, List<IModelStructure>> mapFrom(Dictionary<string, EModelType> keyValuePairs, ExcelWorksheet worksheet) 
        {
            Dictionary<EModelType, List<IModelStructure>> modelsList = new();

            EModelType[] eModels = new EModelType[keyValuePairs.Count];
            keyValuePairs.Values.CopyTo(eModels, 0);
            var parser = new ExcelParser();
            //parser.FindTableBorderRows(keyValuePairs, worksheet, out modelStartRows,out modelEndRows);
            foreach (EModelType eModel in eModels)
            {
                int startRow = modelStartRows[eModel];
                int endRow = modelEndRows[eModel];
                switch (eModel)
                {
                    //case EModelType.Staff:
                    //    modelsList[eModel] = ParseStaffs(startRow, endRow, worksheet);
                    //    break;
                    //case EModelType.Component:
                    //    //modelsList[eModel] = ParseComponetns(startRow, endRow, worksheet);
                    //    break;
                    //case EModelType.Machine:
                    //    modelsList[eModel] = ParseMachines(startRow, endRow, worksheet);
                    //    break;
                    //case EModelType.Protection:
                    //    modelsList[eModel] = ParseProtections(startRow, endRow, worksheet);
                    //    break;
                    //case EModelType.Tool:
                    //    modelsList[eModel] = ParseTools(startRow, endRow, worksheet);
                    //    break;
                    //case EModelType.WorkStep:
                    //    modelsList[eModel] = ParseStaffs(startRow, endRow, worksheet);
                    //    break;
                    default:
                        break;
                }
            }
            return modelsList;
        }

        
        //todo - parser to worksteps
        //todo - method to find start row of table
        //todo - method to find end row of table
        //todo - method to mapp parsed data to object
        //todo - method to save object to json, database, or smth else
        private List<IModelStructure> ParseStaffs(string filepath, string sheetName, int startRow, int endRow)
        {
            var structs = new List<IModelStructure>();

            List<string> columnsNames = new List<string>
            { "№", "Наименование", "Тип (исполнение)", "Возможность совмещения обязанностей",
                "Группа ЭБ, не ниже", "Разряд,\r\nне ниже", "Квалификация","Обозначение в ТК"};

            var parser = new ExcelParser();
            var parsedData = parser.ParseRowsToStrings(columnsNames, filepath, sheetName, startRow: startRow, endRow: endRow);

            foreach (var row in parsedData)
            {
                string num = row[0];
                string name = row[1];
                string type = row[2];
                string combineResponsibility = row[3];
                string elSaftyGroup = row[4];
                string grade = row[5];
                string competence = row[6];
                string symbol = row[7];
                
                //structs.Add(new Staff
                //{
                //    //Num = int.Parse(num),
                //    Name = name,
                //    Type = type,
                //    CombineResponsibility = combineResponsibility,
                //    ElSaftyGroup = elSaftyGroup,
                //    Grade = grade,
                //    Competence = competence,
                //    // Symbol = symbol,

                //}
                //);
            
            }
            return structs;
        }
        public List<IModelStructure> ParseComponetns(string filepath, string sheetName, int startRow, int endRow)
        {
            var structs = new List<IModelStructure>();

            List<string> columnsNames = new List<string> 
            { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во", "Стоимость, руб. без НДС" };

            var parser = new ExcelParser();
            var parsedData = parser.ParseRowsToStrings(columnsNames,filepath,sheetName,startRow: startRow, endRow: endRow);

            int itemCounter = 0;
            int itemWhithConponents = 0;
            
            foreach (var row in parsedData)
            {
                string num = row[0];//columnsNames.IndexOf("№")];
                string name = row[1];// columnsNames.IndexOf("Наименование")];
                string? type = row[2];// columnsNames.IndexOf("Тип (исполнение)")];
                string unit = row[3];// columnsNames.IndexOf("Ед. Изм.")];
                string? amount = row[4];// columnsNames.IndexOf("Кол-во")];
                string? price = row[5];// columnsNames.IndexOf("Стоимость, руб. без НДС")];

                if (name.ToLower().Contains("в составе") || unit.ToLower().Contains("комплект")) 
                    itemWhithConponents = itemCounter;

                if (itemCounter > itemWhithConponents && num == "-")
                {
                    //((Component)structs[itemWhithConponents]).AddComplectItem(new Component
                    //{
                    //    Num = 0,
                    //    Name = name,
                    //    Type = type,
                    //    Unit = unit,
                    //    Amount = double.Parse(amount),
                    //    Price = price != null ? float.Parse(price) : null
                    //});
                }
                else
                {
                    if (itemWhithConponents != 0 && itemCounter > itemWhithConponents) 
                        itemWhithConponents = 0; // Сброс счетчика (исключаем попадание компонентов из другого списка)

                    structs.Add(new Component
                    {
                        //Num = int.Parse(num),
                        Name = name,
                        Type = type,
                        Unit = unit,
                        //Amount = double.Parse(amount),
                        Price = price != null ? float.Parse(price) : null
                    });

                }

                itemCounter++;
            }

            return structs;
        }
        private List<IModelStructure> ParseMachines(string filepath, string sheetName, int startRow, int endRow)
        {
            var structs = new List<IModelStructure>();

            List<string> columnsNames = new List<string>
            { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во","Стоимость, руб. без НДС" };

            var parser = new ExcelParser();
            var parsedData = parser.ParseRowsToStrings(columnsNames, filepath, sheetName, startRow: startRow, endRow: endRow);

            foreach (var row in parsedData)
            {
                string num = row[0];//columnsNames.IndexOf("№")];
                string name = row[1];// columnsNames.IndexOf("Наименование")];
                string? type = row[2];// columnsNames.IndexOf("Тип (исполнение)")];
                string unit = row[3];// columnsNames.IndexOf("Ед. Изм.")];
                string? amount = row[4];// columnsNames.IndexOf("Кол-во")];
                string? price = row[5];// columnsNames.IndexOf("Стоимость, руб. без НДС")];

                structs.Add(new Machine
                {
                    //Num = int.Parse(num),
                    Name = name,
                    Type = type,
                    Unit = unit,
                    //Amount = double.Parse(amount),
                    Price = price != null ? float.Parse(price) : null
                });
            }
            return structs;
        }
        private List<IModelStructure> ParseProtections(string filepath, string sheetName, int startRow, int endRow)
        {
            var structs = new List<IModelStructure>();

            List<string> columnsNames = new List<string>
            { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во","Стоимость, руб. без НДС" };

            var parser = new ExcelParser();
            var parsedData = parser.ParseRowsToStrings(columnsNames, filepath, sheetName, startRow: startRow, endRow: endRow);

            foreach (var row in parsedData)
            {
                string num = row[0];//columnsNames.IndexOf("№")];
                string name = row[1];// columnsNames.IndexOf("Наименование")];
                string? type = row[2];// columnsNames.IndexOf("Тип (исполнение)")];
                string unit = row[3];// columnsNames.IndexOf("Ед. Изм.")];
                string? amount = row[4];// columnsNames.IndexOf("Кол-во")];
                string? price = row[5];// columnsNames.IndexOf("Стоимость, руб. без НДС")];

                structs.Add(new Protection
                {
                    //Num = int.Parse(num),
                    Name = name,
                    Type = type,
                    Unit = unit,
                    //Amount = double.Parse(amount),
                    Price = price != null ? float.Parse(price) : null
                });
            }
            return structs;
        }
        private List<IModelStructure> ParseTools(string filepath, string sheetName, int startRow, int endRow)
        {
            var structs = new List<IModelStructure>();

            List<string> columnsNames = new List<string>
            { "№", "Наименование", "Тип (исполнение)", "Ед. Изм.", "Кол-во","Стоимость, руб. без НДС" };

            var parser = new ExcelParser();
            var parsedData = parser.ParseRowsToStrings(columnsNames, filepath, sheetName, startRow: startRow, endRow: endRow);

            foreach (var row in parsedData)
            {
                string num = row[0];//columnsNames.IndexOf("№")];
                string name = row[1];// columnsNames.IndexOf("Наименование")];
                string? type = row[2];// columnsNames.IndexOf("Тип (исполнение)")];
                string unit = row[3];// columnsNames.IndexOf("Ед. Изм.")];
                string? amount = row[4];// columnsNames.IndexOf("Кол-во")];
                string? price = row[5];// columnsNames.IndexOf("Стоимость, руб. без НДС")];

                structs.Add(new Tool
                {
                    //Num = int.Parse(num),
                    Name = name,
                    Type = type,
                    Unit = unit,
                    //Amount = double.Parse(amount),
                    Price = price != null ? float.Parse(price) : null
                });
            }
            return structs;
        }

    }
}
