using ExcelParsing.DataProcessing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcDbConnector.Repositories;
using TcModels.Models.Helpers;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.RoadMap;

namespace TC_WinForms.DataProcessing
{
    public class DataExcelExport
    {
        public DataExcelExport() 
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
        }

        public async Task SaveTCtoExcelFile(string filePath, List<TcPrinterSettings> printSettings)
        {
            try
            {
                if (printSettings == null || printSettings.Count == 0)
                    throw new Exception("Нет данных для печати");

                TechnologicalCardRepository tcRepository = new TechnologicalCardRepository();
                var excelPackage = new ExcelPackage();

                foreach (var settings in printSettings)
                {
                    if (settings.TcId == null)
                        throw new Exception("Карты не существует, ошибка печати");

                    if(!settings.PrintWorkSteps && !settings.PrintDiagram && !settings.PrintOutlay && !settings.PrintRoadMap)
                        continue;

                    var tc = await tcRepository.GetTCDataAsync((int)settings.TcId);
                    string imageBase64 = "";

                    if (tc.ExecutionSchemeImageId != null)
                        imageBase64 = await tcRepository.GetImageBase64Async((long)tc.ExecutionSchemeImageId);

                    var coverExport = new TcCoverExcelExporter();
                    coverExport.ExportCoverToExcel(excelPackage, tc, imageBase64);

                    if (settings.PrintWorkSteps)
                    {
                        var tcExport = new TCExcelExporter();
                        tcExport.ExportTCtoFile(excelPackage, tc);
                    }

                    if (settings.PrintOutlay)
                    {
                        var outlayList = await tcRepository.GetOutlayDataAsync((int)settings.TcId);
                        var outlayExport = new OutlayExcelExporter();
                        outlayExport.ExportOutlatytoFile(excelPackage, outlayList, tc.Article);
                    }

                    if (settings.PrintDiagram)
                    {
                        var dtwList = await tcRepository.GetDTWDataAsync((int)settings.TcId);
                        var diagramExport = new DiagramExcelExporter();
                        diagramExport.ExportDiadramToExel(excelPackage, tc, dtwList, tc.Article);
                    }

                    if (settings.PrintRoadMap)
                    {
                        var roadMapItems = await tcRepository.GetRoadMapItemsDataAsync(tc.TechOperationWorks.Select(s => s.Id).ToList());
                        roadMapItems.OrderBy(r => r.Order);
                        var roadMapExport = new RoadMapExcelExporter();
                        roadMapExport.ExportRoadMaptoFile(excelPackage, roadMapItems, tc.Article);
                    }
                }
                excelPackage.File = new FileInfo(filePath);//после того, как создание/обновление всех листов происходит(в случае ошибок создания листов мы не дойдем до этого места) мы присваиваем адрес excelPackage, это обеспечивает перезаписывание файла без его удаления
                excelPackage.Save();
                excelPackage.Dispose();

                MessageBox.Show($"Файл успешно сохранен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Нет доступа к директории.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при сохранении файла: \n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
