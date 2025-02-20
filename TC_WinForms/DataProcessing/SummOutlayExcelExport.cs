using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC_WinForms.DataProcessing;
using TC_WinForms.WinForms.Win7.Work;
using TcModels.Models.TcContent;

namespace ExcelParsing.DataProcessing
{
    public class SummOutlayExcelExport
    {
        private DbConnector dbCon = new DbConnector();
        private ExcelPackage _excelPackage;
        public SummOutlayExcelExport()
        {

        }

        public async Task SaveSummOutlaytoExcelFile(List<SummaryOutlayDataGridItem> summaryOutlayDataGridItems)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    // Настройка диалога сохранения файла
                    saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    saveFileDialog.FileName = $"Сводная таблица затрат {DateTime.Today.ToString("yyyy-MM-dd")}";

                    // Показ диалога пользователю и проверка, что он нажал кнопку "Сохранить"
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            if (summaryOutlayDataGridItems == null)
                            {
                                MessageBox.Show("Ошибка при загрузки данных из БД", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            CreateNewFile(saveFileDialog.FileName);

                            var excelExporter = new SummOutlayExcelExporter(_excelPackage);
                            excelExporter.ExportSummOutlayoFile(summaryOutlayDataGridItems);

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

        private void CreateNewFile(string filePath)
        {
            try
            {
                // Создание нового файла Excel (если файл уже существует, он будет перезаписан)
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
                _excelPackage = new ExcelPackage(fileInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при создании файла: \n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }
    }
}
