using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
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
        private readonly SummaryOutlaySettings _settings = new SummaryOutlaySettings { LeaderSallary = 1134, RegularSallary = 794 };//На случай отсуствия доступа к файлу настроек можно считать данные из этого объекта

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

            SummaryOutlaySettings settings;
            try
            {
                string jsonString = File.ReadAllText("SummaryOutlaySettings.json");
                settings = JsonSerializer.Deserialize<SummaryOutlaySettings>(jsonString);
                // Обновление настроек
                settings.LeaderSallary = (float)Convert.ToDouble(txtLead.Text);
                settings.RegularSallary = (float)Convert.ToDouble(txtRegular.Text);
                // Перезапись файла
                jsonString = JsonSerializer.Serialize(settings);
                File.WriteAllText("SummaryOutlaySettings.json", jsonString);
            }
            catch (Exception ex) 
            {
                _logger.Error($"Ошибка при обновлении данных настроек: {ex.Message}");
                MessageBox.Show("Ошибка перезаписи данных: " + ex.Message);
                return;
            }

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

            SummaryOutlaySettings settings = new SummaryOutlaySettings();
            try
            {
                string jsonString = File.ReadAllText("SummaryOutlaySettings.json");
                settings = JsonSerializer.Deserialize<SummaryOutlaySettings>(jsonString);
            }
            catch (FileNotFoundException fe)
            {
                _logger.Error($"Ошибка при загрузке данных настроек, файл не найден: {fe.Message}");
                MessageBox.Show("Ошибка при загрузке данных настроек, файл не найден: \n" + fe.Message);
                sender = _settings;
            }
            catch (IOException io)
            {
                _logger.Error($"Ошибка ввода-вывода. Файл может быть недоступен: {io.Message}");
                MessageBox.Show("Ошибка ввода-вывода. Файл может быть недоступен: \n" + io.Message);
                sender = _settings;
            }
            catch (UnauthorizedAccessException un)
            {
                _logger.Error($"Нет прав доступа к файлу: {un.Message}");
                MessageBox.Show("Нет прав доступа к файлу.");
                sender = _settings;
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при загрузке данных настроек: {ex.Message}");
                MessageBox.Show("Ошибка загрузки данных настроек: " + ex.Message);
                sender = _settings;
            }

            txtLead.Text = settings.LeaderSallary.ToString();
            txtRegular.Text = settings.RegularSallary.ToString();
        }
    }
}
