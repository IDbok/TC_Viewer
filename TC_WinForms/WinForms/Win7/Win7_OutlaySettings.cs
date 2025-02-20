using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Win7.Work;

namespace TC_WinForms.WinForms
{ 
    public partial class Win7_OutlaySettings : Form
    {
        private readonly ILogger _logger;

        public Win7_OutlaySettings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _logger.Information("Форма Win7_OutlaySettings инициалирована и открыта.");

            if (!File.Exists("SummaryOutlaySettings.json"))
            {
                _logger.Warning("Файл SummaryOutlaySettings.json не был найден в папке проекта.");
                MessageBox.Show("Файл с настройками для сводной таблицы не обнаружен", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }

            string jsonString = File.ReadAllText("SummaryOutlaySettings.json");
            SummaryOutlaySettings settings = JsonSerializer.Deserialize<SummaryOutlaySettings>(jsonString);

            try
            {
                // Обновление настроек
                settings.LeaderSallary = (float)Convert.ToDouble(txtLead.Text);
                settings.RegularSallary = (float)Convert.ToDouble(txtRegular.Text);
            }
            catch (Exception ex) 
            {
                _logger.Error($"Ошибка при обновлении данных настроек: {ex.Message}");
                MessageBox.Show("Ошибка перезаписи данных: " + ex.Message);
                return;
            }

            // Перезапись файла
            jsonString = JsonSerializer.Serialize(settings);
            File.WriteAllText("SummaryOutlaySettings.json", jsonString);

            var openedForm = CheckOpenFormService.FindOpenedForm<Win7_SummaryOutlay>();
            if (openedForm != null)
            {
                openedForm.LoadData();
                MessageBox.Show("Данные перезаписаны, записи в таблице пересчитаны.", "Данные обновлены", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _logger.Information($"Данные перезаписаны, новые данные: Оплата бригадира - {settings.LeaderSallary}; Оплата прочего персонала - {settings.RegularSallary}.");
                return;
            }
        }

        private void Win7_OutlaySettings_Load(object sender, EventArgs e)
        {
            _logger.Information("Форма Win7_OutlaySettings инициалирована и открыта.");

            if (!File.Exists("SummaryOutlaySettings.json"))
            {
                _logger.Warning("Файл SummaryOutlaySettings.json не был найден в папке проекта.");
                MessageBox.Show("Файл с настройками для сводной таблицы не обнаружен", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }

            string jsonString = File.ReadAllText("SummaryOutlaySettings.json");
            SummaryOutlaySettings settings = JsonSerializer.Deserialize<SummaryOutlaySettings>(jsonString);

            txtLead.Text = settings.LeaderSallary.ToString();
            txtRegular.Text = settings.RegularSallary.ToString();
        }
    }
}
