using ExcelParsing.DataProcessing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_WinForms.DataProcessing
{
    public class DataExcelExport
    {
        private DbConnector dbCon = new DbConnector();
        private ExcelPackage _excelPackage;

        public DataExcelExport()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            _excelPackage = new ExcelPackage();
        }

        public async Task SaveTCtoExcelFile(string tcArticle, int tcId)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    // Настройка диалога сохранения файла
                    saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    saveFileDialog.FileName = tcArticle;

                    // Показ диалога пользователю и проверка, что он нажал кнопку "Сохранить"
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            var tc = await dbCon.GetTechnologicalCardToExportAsync(tcId);
                            var outlays = await dbCon.GetOutlayListToExportAsync(tcId);

                            if (tc == null)
                            {
                                MessageBox.Show("Ошибка при загрузки данных из БД", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            CreateNewFile(saveFileDialog.FileName);

                            var tcExporter = new TCExcelExporter(_excelPackage);
                            tcExporter.ExportTCtoFile(saveFileDialog.FileName, tc, outlays);

                            var outlayExporter = new OutlayExcelExporter(_excelPackage);
                            outlayExporter.ExportOutlatytoFile(saveFileDialog.FileName, tcArticle, outlays);

                            var diagramExporter = new DiagramExcelExporter(_excelPackage);
                            await diagramExporter.ExportDiadramToExel(tc.Id, saveFileDialog.FileName);

                            Save();
                            Close();

                            MessageBox.Show($"Файл успешно сохранен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Произошла ошибка при загрузке данных: \n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при сохранении файла: \n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
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
