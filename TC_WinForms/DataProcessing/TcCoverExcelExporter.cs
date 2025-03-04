using ExcelParsing.DataProcessing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.IntermediateTables;

namespace TC_WinForms.DataProcessing
{
    public class TcCoverExcelExporter
    {
        private ExcelExporter _exporter;

        private Dictionary<string, int> structHeadersColumns = new Dictionary<string, int>
            {
                { "№", 2 },
                { "Код", 3 },
                { "Значение", 4 },
                { "Наименование", 5 },
                { "Описание", 7 },
                { "конец", 10 }
            };

        private const int ImageColumnConst = 11;
        private readonly int _currentRixelWidgthShag = 440;

        public TcCoverExcelExporter()
        {
            _exporter = new ExcelExporter();
        }

        public async void ExportCoverToExcel(ExcelPackage _excelPackage, TechnologicalCard technologicalCard, string base64Image)
        {
            string sheetName = "Обложка технологической карты";
            var sheet = _excelPackage.Workbook.Worksheets[sheetName] ?? _excelPackage.Workbook.Worksheets.Add(sheetName);
            int headRow = 7;

            sheet.PrinterSettings.Scale = 80;
            sheet.PrinterSettings.Orientation = eOrientation.Landscape;

            InsertImageIntoSheet(sheet, base64Image, headRow);
            headRow = InsertTCInfoIntoSheet(sheet, technologicalCard, headRow + 1);
            InsertCoefficientTableIntoSHeet(sheet, technologicalCard, headRow + 3);
        }

        private void InsertImageIntoSheet(ExcelWorksheet sheet, string base64Image, int headRow)
        {
            if (string.IsNullOrEmpty(base64Image))
                return;

            int[] imageColumnNum = { ImageColumnConst, ImageColumnConst + 8 };
            var rowHeightPixels = sheet.Row(headRow).Height / 72 * 96d;

            var bytepath = Convert.FromBase64String(base64Image);
            Image bitmapImage = LoadImage(bytepath);

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    bitmapImage.Save(ms, ImageFormat.Png);
                }
                catch //Обход ошибки сохранения. Она связана с размером изображения и одновременным открытием изображения GDI+. Изображение будет с нуля отрисовано в копии и все сторонние процессы будут закрыты.
                {
                    Bitmap bitmap = new Bitmap(bitmapImage, bitmapImage.Width, bitmapImage.Height); //Создаем копию текущего изображения
                    Graphics g = Graphics.FromImage(bitmap);
                    g.DrawImage(bitmapImage, 0f, 0f, (float)bitmapImage.Width, (float)bitmapImage.Height);
                    g.Dispose();
                    bitmapImage.Dispose(); // завершаем процесс оригинального объекта, чтобы не было ошибки, связанной с уже изначально открытым процессом в GDI+
                    bitmap.Save(ms, ImageFormat.Png);
                }

                ExcelPicture excelImage = null;
                excelImage = sheet.Drawings.AddPicture("TcCover", ms);
                if ((excelImage.Size.Width / 9525) > _currentRixelWidgthShag) //Сравниваем, войдет ли по ширине пикселей текущий масштаб изображения в заданную ширину шага. Ширину изображения делим на указанное в формуле число соотношения к пикселям
                {
                    int i = 100;//Число для задания масштаба, по умолчанию 100
                    while ((excelImage.Size.Width / 9525) > _currentRixelWidgthShag) //Уменьшаем масштаб, пока изображения не поместится
                    {
                        excelImage.SetSize(i);
                        i -= 5;
                    }
                }
                else
                    excelImage.SetSize(100); //задаем масштаб изображения по умолчанию

                excelImage.From.Column = imageColumnNum[0] - 1;
                excelImage.From.Row = headRow;
                excelImage.From.ColumnOff = imageColumnNum[1];
            }
        }
        private Image LoadImage(byte[] imageData)
        {
            Image image;
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                image = Image.FromStream(ms);
            }

            return image;
        }
        private int InsertTCInfoIntoSheet(ExcelWorksheet sheet, TechnologicalCard tc, int headRow)
        {
            int[] columnNums = { 2, 5, 8}; //Начало ячейки названия, конец ячейки названия, начало ячейки значени, конец ячейки значения

            var currentRow = headRow;
            sheet.Cells[currentRow, columnNums[0]].Value = "Артикул:";
            sheet.Cells[currentRow, columnNums[1]].Value = tc.Article;

            AddStyleAlignment(currentRow, columnNums, sheet);
            currentRow++;

            int[] rowsNums = { currentRow, currentRow + 1 };
            sheet.Cells[rowsNums[0], columnNums[0]].Value = "Тех.процесс/Параметр:";

            sheet.Cells[rowsNums[0], columnNums[1]].Value = string.IsNullOrEmpty(tc.TechnologicalProcessName)
                ? "N/A"
                : tc.TechnologicalProcessName;

            sheet.Cells[rowsNums[0], columnNums[1]].Value += string.IsNullOrEmpty(tc.Parameter)
                ? ": N/A"
                : ": " + tc.Parameter;

            AddRowStyle();
            currentRow = rowsNums[rowsNums.Length-1] + 1;

            rowsNums = [ currentRow, currentRow + 1 ];

            sheet.Cells[rowsNums[0], columnNums[0]].Value = "Примечание:";
            sheet.Cells[rowsNums[0], columnNums[1]].Value = string.IsNullOrEmpty(tc.Note) ? "N/A" : tc.Note;
            AddRowStyle();

            return rowsNums[rowsNums.Length-1]+1;

            void AddRowStyle()
            {
                rowsNums[rowsNums.Length - 1] = GetRowsCountByData(sheet.Cells[rowsNums[0], columnNums[1]].Value.ToString(), sheet, [columnNums[1], columnNums[2]]) + currentRow;

                sheet.Cells[rowsNums[0], columnNums[1], rowsNums[0], columnNums[2]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                sheet.Cells[rowsNums[0], columnNums[1], rowsNums[0], columnNums[2]].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                sheet.Cells[rowsNums[0], columnNums[1], rowsNums[1], columnNums[2] - 1].Merge = true;
                sheet.Cells[rowsNums[0], columnNums[1], rowsNums[0], columnNums[2]].Style.WrapText = true;

                _exporter.MergeRowCellsByColumns(sheet, currentRow, [columnNums[0], columnNums[1]]);
                _exporter.ApplyCellFormatting(sheet.Cells[rowsNums[0], columnNums[0], rowsNums[0], columnNums[1]]);
                _exporter.ApplyCellFormatting(sheet.Cells[rowsNums[0], columnNums[1], rowsNums[1], columnNums[2] - 1]);
            }
        }
        private void InsertCoefficientTableIntoSHeet(ExcelWorksheet sheet, TechnologicalCard tc, int headRow)
        {
            if (!tc.IsDynamic)
                return;

            string[] headers = structHeadersColumns.Keys.Where(x => !x.Contains("конец")).OrderBy(x => structHeadersColumns[x]).ToArray();
            int[] columnNums = structHeadersColumns.Values.OrderBy(x => x).ToArray();

            _exporter.AddTableHeader(sheet, "Коэффициенты", headRow, columnNums);
            _exporter.AddTableHeaders(sheet, headers, headRow +1, columnNums);

            var currentRow = headRow + 2;

            for(int i = 1; i <= tc.Coefficients.Count; i++)
            {
                sheet.Cells[currentRow, columnNums[0]].Value = i;
                sheet.Cells[currentRow, columnNums[1]].Value = tc.Coefficients[i-1].Code;
                sheet.Cells[currentRow, columnNums[2]].Value = tc.Coefficients[i-1].Value;
                sheet.Cells[currentRow, columnNums[3]].Value = tc.Coefficients[i - 1].ShortName;
                sheet.Cells[currentRow, columnNums[4]].Value = tc.Coefficients[i - 1].Description;

                AddStyleAlignment(currentRow, columnNums, sheet);
                currentRow++;
            }
        }
        private void AddStyleAlignment(int headRow, int[] columnNums, ExcelWorksheet sheet)
        {
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1]].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length - 1]].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            _exporter.MergeRowCellsByColumns(sheet, headRow, columnNums);
            _exporter.ApplyCellFormatting(sheet.Cells[headRow, columnNums[0], headRow, columnNums[columnNums.Length-1] - 1]);
        }

        private int GetRowsCountByData(string data, ExcelWorksheet sheet, int[] collumnNums)
        {
            var collumnsWidth = (collumnNums[collumnNums.Length - 1] - collumnNums[0]) * sheet.Column(collumnNums[collumnNums.Length - 1]).Width;
            var rowCount = data.Length / collumnsWidth;
            rowCount = rowCount <= 0.85 ? Math.Floor(rowCount) : Math.Ceiling(rowCount);
            return (int)rowCount;
        }
    }
}
