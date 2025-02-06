using System;
using System.Collections.Generic;
using System.Data.Common;
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
                            var excelExporter = new SummOutlayExcelExporter();
                            excelExporter.ExportSummOutlayoFile(saveFileDialog.FileName, summaryOutlayDataGridItems);

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
    }
}
