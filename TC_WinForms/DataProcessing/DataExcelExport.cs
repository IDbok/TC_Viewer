using ExcelParsing.DataProcessing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcDbConnector.Repositories;

namespace TC_WinForms.DataProcessing
{
    public class DataExcelExport
    {
        public DataExcelExport() 
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
        }

        public async Task SaveTCtoExcelFile(string filePath, int tcId)
        {
            try
            {

                TechnologicalCardRepository tcRepository = new TechnologicalCardRepository();
                var tc = await tcRepository.GetTCDataAsync(tcId);
                var outlayList = await tcRepository.GetOutlayDataAsync(tcId);
                var dtwList = await tcRepository.GetDTWDataAsync(tcId);
                var roadMapItems = await tcRepository.GetRoadMapItemsDataAsync(tc.TechOperationWorks.Select(s => s.Id).ToList());
                roadMapItems.OrderBy(r => r.Order);
                string imageBase64 = "";
                if(tc.ExecutionSchemeImageId != null)
                    imageBase64 = await tcRepository.GetImageBase64Async((long)tc.ExecutionSchemeImageId);

                if (tc == null)
                {
                    MessageBox.Show("Ошибка при загрузки данных из БД", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                var excelPackage = new ExcelPackage();

                var coverExport = new TcCoverExcelExporter();
                coverExport.ExportCoverToExcel(excelPackage, tc, imageBase64);

                var tcExport = new TCExcelExporter();
                tcExport.ExportTCtoFile(excelPackage, tc);

                var outlayExport = new OutlayExcelExporter();
                outlayExport.ExportOutlatytoFile(excelPackage, outlayList);

                var diagramExport = new DiagramExcelExporter();
                diagramExport.ExportDiadramToExel(excelPackage, tc, dtwList);

                var roadMapExport = new RoadMapExcelExporter();
                roadMapExport.ExportRoadMaptoFile(excelPackage, roadMapItems);

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
