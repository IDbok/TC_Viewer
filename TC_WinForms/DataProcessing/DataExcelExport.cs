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
        private DbConnector dbCon = new DbConnector();

        public DataExcelExport() 
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
        }

        public async Task SaveTCtoExcelFile(string filePath, int tcId)
        {
            try
            {

                TechnologicalCardRepository tcRepository = new TechnologicalCardRepository();
                var tc = await tcRepository.GetTechnologicalCardToExportAsync(tcId);
                var outlayList = await tcRepository.GetTCOutlayDataForPrint(tcId);
                var dtwList = await tcRepository.GetDTWDataForPrint(tcId);

                if (tc == null)
                {
                    MessageBox.Show("Ошибка при загрузки данных из БД", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                var excelPackage = CreateNewExcelPackage(filePath);

                TCExcelExporter.ExportTCtoFile(excelPackage, tc);
                OutlayExcelExporter.ExportOutlatytoFile(excelPackage, outlayList);
                DiagramExcelExporter.ExportDiadramToExel(excelPackage, tc, dtwList);

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


        private ExcelPackage CreateNewExcelPackage(string filePath)
        {
            // Создание нового файла Excel (если файл уже существует, он будет перезаписан)
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            return new ExcelPackage(fileInfo);
        }
    }
}
