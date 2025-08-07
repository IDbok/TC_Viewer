using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcDbConnector.Repositories;
using TcModels.Models.TcContent;
using TcModels.Models.Helpers;
using TcDbConnector;
using Microsoft.EntityFrameworkCore;
using TC_WinForms.DataProcessing;
using Serilog;

namespace TC_WinForms.WinForms.PrinterSettings
{
    public partial class TcPrintForm : Form
    {
        private int _mainTcId;
        private string _mainTcArticle;
        private List<TcPrinterSettings> _printerSettings = new List<TcPrinterSettings>();
        private Dictionary<long?, string> _allPrintedTcDict = new Dictionary<long?, string>();
        private bool _isFormLoaded = false;
        private readonly ILogger _logger;

        public TcPrintForm(int tcId)
        {
            _logger = Log.ForContext<TcPrintForm>();

            _mainTcId = tcId;
            InitializeComponent();
            Load += PrinterSettings_Load;
        }

        private async void PrinterSettings_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
            setListBox();
            _isFormLoaded = true;
            lbxTc_SelectedIndexChanged(null, null);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using (MyDbContext context = new MyDbContext())
                {
                    var mainTc = await context.TechnologicalCards
                        .Where(x => x.Id == _mainTcId)
                        .Include(e => e.TechOperationWorks)
                        .ThenInclude(e => e.executionWorks)
                        .FirstOrDefaultAsync();

                    _mainTcArticle = mainTc.Article;
                    _allPrintedTcDict.Add(_mainTcId, _mainTcArticle);
                    _printerSettings.Add(new TcPrinterSettings { TcId = _mainTcId, TcName = mainTc.Article });

                    if (mainTc != null)
                    {
                        var relatedTcList = mainTc.TechOperationWorks
                            .SelectMany(e => e.executionWorks)
                            .Where(e => e.RepeatsTCId != null)
                            .Select(e => e.RepeatsTCId)
                            .Distinct()
                            .ToList();

                        relatedTcList.ForEach
                            (id =>
                            {
                                var tcArticle = context.TechnologicalCards
                                .Where(x => x.Id == id)
                                .Select(x => $"{x.Article} {x.TechnologicalProcessName} {x.Parameter}")
                                .FirstOrDefault();
                                _allPrintedTcDict.Add(id, $"{tcArticle}()");
                                _printerSettings.Add(new TcPrinterSettings { TcId = id, TcName = tcArticle });
                            }
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке данных для TcId={TcId}", _mainTcId);
                MessageBox.Show("Возникла ошибка при попытке загрузки данных: \n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }

        }


        private void setListBox()
        {
            lbxTc.DataSource = _allPrintedTcDict.ToList();
            lbxTc.DisplayMember = "Value";
            lbxTc.ValueMember = "Key";
            lbxTc.ScrollAlwaysVisible = true;
        }

        private void ckbxWorkStep_CheckedChanged(object sender, EventArgs e)
        {
            var tcSettings = _printerSettings.Where(s => s.TcId == (long?)lbxTc.SelectedValue).FirstOrDefault();
            tcSettings.PrintWorkSteps = ckbxWorkStep.Checked;
        }

        private void ckbxDiagram_CheckedChanged(object sender, EventArgs e)
        {
            var tcSettings = _printerSettings.Where(s => s.TcId == (long?)lbxTc.SelectedValue).FirstOrDefault();
            tcSettings.PrintDiagram = ckbxDiagram.Checked;
        }

        private void ckbxOutlay_CheckedChanged(object sender, EventArgs e)
        {
            var tcSettings = _printerSettings.Where(s => s.TcId == (long?)lbxTc.SelectedValue).FirstOrDefault();
            tcSettings.PrintOutlay = ckbxOutlay.Checked;
        }

        private void ckbxRoadMap_CheckedChanged(object sender, EventArgs e)
        {
            var tcSettings = _printerSettings.Where(s => s.TcId == (long?)lbxTc.SelectedValue).FirstOrDefault();
            tcSettings.PrintRoadMap = ckbxRoadMap.Checked;
        }

        private async void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    // Настройка диалога сохранения файла
                    saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    saveFileDialog.FileName = _mainTcArticle;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var tcExporter = new DataExcelExport();
                        await tcExporter.SaveTCtoExcelFile(saveFileDialog.FileName, _printerSettings);
                    }
                }

                _logger.Information("Печать успешно завершена для TcId={TcId}", _mainTcId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при печати для TcId={TcId}", _mainTcId);
                MessageBox.Show(ex.Message);
            }
        }

        private void lbxTc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoaded)
                return;

            var tcSettings = _printerSettings.Where(s => s.TcId == (long?)lbxTc.SelectedValue).FirstOrDefault();
            if (tcSettings != null)
            {
                ckbxWorkStep.Checked = tcSettings.PrintWorkSteps;
                ckbxDiagram.Checked = tcSettings.PrintDiagram;
                ckbxOutlay.Checked = tcSettings.PrintOutlay;
                ckbxRoadMap.Checked = tcSettings.PrintRoadMap;
            }
        }
    }
}
