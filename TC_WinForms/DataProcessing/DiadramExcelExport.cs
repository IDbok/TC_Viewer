using ExcelParsing.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models;

namespace TC_WinForms.DataProcessing
{
    class DiadramExcelExport
    {
        private DbConnector dbCon = new DbConnector();
        public DiadramExcelExport() { }

        public async Task SaveDiagramToExelFile(string dgmArticle, int tcID)
        {
            try 
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog()) 
                {
                    saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    saveFileDialog.FileName = dgmArticle+"_БС";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            var tc = dbCon.GetObject<TechnologicalCard>(tcID);
                            if (tc == null)
                            {
                                MessageBox.Show("Ошибка при загрузки данных из БД", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            var excelExporter = new DiagramExcelExporter();
                            await excelExporter.ExportDiadramToExel(tcID, saveFileDialog.FileName);
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
