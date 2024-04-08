using ExcelParsing.DataProcessing;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TcDbConnector;
using TcModels.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ComboBox = System.Windows.Forms.ComboBox;
using TextBox = System.Windows.Forms.TextBox;

namespace TC_WinForms.WinForms
{
    public partial class Win7_1_TCs_Window : Form
    {
        public MyDbContext context;
        private DbConnector dbCon = new DbConnector();

        private List<object> AllEllement = new List<object>();

        private TechnologicalCard LocalCard = null;

        public Win7_1_TCs_Window(int? tcId = null)
        {
            InitializeComponent();

            context = new MyDbContext();

            ConfigureComboBox();

            AllEllement.Add(txtArticle);
            AllEllement.Add(cbxType);
            AllEllement.Add(cbxNetworkVoltage);

            SubscribeToChanges(); // подписываемся на изменения сразу всех полей

            if (tcId != null)
            {
                LocalCard = context.TechnologicalCards.Single(s => s.Id == tcId);
                load(LocalCard);
            }

        }


        public void load(TechnologicalCard LocalCard)
        {
            txtArticle.Text = LocalCard.Article;
            cbxType.Text = LocalCard.Type;
            cbxNetworkVoltage.Text = LocalCard.NetworkVoltage.ToString();
            txtTechProcessType.Text = LocalCard.TechnologicalProcessType;
            txtTechProcess.Text = LocalCard.TechnologicalProcessName;
            txtParametr.Text = LocalCard.Parameter;
            txtFinalProduct.Text = LocalCard.FinalProduct;
            txtApplicability.Text = LocalCard.Applicability;
            txtNote.Text = LocalCard.Note;
            chbxIsCompleted.Checked = LocalCard.IsCompleted;
        }

        private void ConfigureComboBox()
        {
            cbxType.Items.AddRange(new object[] { "Ремонтная", "Монтажная", "Точка Трансформации", "Нет данных" });
            cbxNetworkVoltage.Items.AddRange(new object[] { 35f, 10f, 6f, 0.4f, 0f });
        }

        bool NoEmptiness()
        {
            bool ValueRet = true;

            foreach (object obj in AllEllement)
            {
                if (obj is TextBox)
                {
                    var tb = (TextBox)obj;
                    if (tb.Text == "")
                    {
                        tb.BackColor = Color.Red;
                        ValueRet = false;
                    }
                }

                if (obj is ComboBox)
                {
                    var cb = (ComboBox)obj;
                    if (cb.SelectedIndex == -1)
                    {
                        cb.BackColor = Color.Red;
                        ValueRet = false;
                    }
                }
            }
            return ValueRet;
        }


        bool Save()
        {
            if (LocalCard == null)
            {
                LocalCard = new TechnologicalCard();
                context.TechnologicalCards.Add(LocalCard);
            }

            LocalCard.Article = txtArticle.Text;
            LocalCard.Type = cbxType.Text;
            LocalCard.NetworkVoltage = float.Parse(cbxNetworkVoltage.Text);
            LocalCard.TechnologicalProcessType = txtTechProcessType.Text;
            LocalCard.TechnologicalProcessName = txtTechProcess.Text;
            LocalCard.Parameter = txtParametr.Text;
            LocalCard.FinalProduct = txtFinalProduct.Text;
            LocalCard.Applicability = txtApplicability.Text;
            LocalCard.Note = txtNote.Text;
            LocalCard.IsCompleted = chbxIsCompleted.Checked;

            try
            {
                context.SaveChanges();
                StaticWinForms.Win7_new.UpdateTC();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return false;
            }
            return true;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (NoEmptiness())
            {
                if (Save())
                {
                    MessageBox.Show("Сохранено!");
                }
            }
        }

        private void btnSaveAndOpen_Click(object sender, EventArgs e)
        {
            if (NoEmptiness())
            {
                if (HasChanges()) 
                    if (Save()) return;

                var nn = LocalCard.Id;
                var editorForm = new Win6_new(nn);
                editorForm.Show();
                this.Close();
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ((TextBox)sender).BackColor = Color.White;
        }

        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((ComboBox)sender).BackColor = Color.White;
        }

        private async void btnExportExcel_Click(object sender, EventArgs e)
        {
            // спрашиваем у пользователя о пути сохранения файла
            await SaveTCtoExcelFile();
        }
        public async Task SaveTCtoExcelFile()
        {
            

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Настройка диалога сохранения файла
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                saveFileDialog.FileName = LocalCard.Article;

                // Показ диалога пользователю и проверка, что он нажал кнопку "Сохранить"
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var tc = await dbCon.GetTechnologicalCardToExportAsync(LocalCard.Id);
                        if (tc == null)
                        {
                            MessageBox.Show("Ошибка при загрузки данных из БД", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        var excelExporter = new TCExcelExporter();
                        excelExporter.ExportTCtoFile(saveFileDialog.FileName, tc);
                    }
                    catch (Exception ex)
                    {
                    MessageBox.Show("Произошла ошибка при загрузке данных: \n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

            }
            }
        }
        private bool HasChanges()
        {
            if (LocalCard == null) return false;

            bool hasChanges = false;

            hasChanges |= LocalCard.Article != txtArticle.Text;
            hasChanges |= LocalCard.Type != cbxType.Text;
            hasChanges |= LocalCard.NetworkVoltage.ToString() != cbxNetworkVoltage.Text;
            hasChanges |= LocalCard.TechnologicalProcessType != txtTechProcessType.Text;
            hasChanges |= LocalCard.TechnologicalProcessName != txtTechProcess.Text;
            hasChanges |= LocalCard.Parameter != txtParametr.Text;
            hasChanges |= LocalCard.FinalProduct != txtFinalProduct.Text;
            hasChanges |= LocalCard.Applicability != txtApplicability.Text;
            hasChanges |= LocalCard.Note != txtNote.Text;
            hasChanges |= LocalCard.IsCompleted != chbxIsCompleted.Checked;

            return hasChanges;
        }
        private void SubscribeToChanges()
        {
            txtArticle.TextChanged += ComponentChanged;
            txtTechProcessType.TextChanged += ComponentChanged;
            txtTechProcess.TextChanged += ComponentChanged;
            txtParametr.TextChanged += ComponentChanged;
            txtFinalProduct.TextChanged += ComponentChanged;
            txtApplicability.TextChanged += ComponentChanged;
            txtNote.TextChanged += ComponentChanged;

            cbxType.SelectedIndexChanged += ComponentChanged;
            cbxNetworkVoltage.SelectedIndexChanged += ComponentChanged;

            chbxIsCompleted.CheckedChanged += ComponentChanged;
        }

        private void ComponentChanged(object sender, EventArgs e)
        {
            if(HasChanges())
            {
                btnSaveAndOpen.Text = "Сохранить и открыть";
            }else
            {
                btnSaveAndOpen.Text = "Открыть";
            }
        }


    }
}
