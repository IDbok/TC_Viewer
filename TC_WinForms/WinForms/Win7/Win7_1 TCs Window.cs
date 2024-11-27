using Serilog;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.Services;
using TcDbConnector;
using TcModels.Models;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TechnologicalCard;
using ComboBox = System.Windows.Forms.ComboBox;
using TextBox = System.Windows.Forms.TextBox;

namespace TC_WinForms.WinForms
{
    public partial class Win7_1_TCs_Window : Form
    {
        private readonly ILogger _logger;

        private readonly User.Role _accessLevel;
        public MyDbContext context;
        private DbConnector dbCon = new DbConnector();
        private CheckOpenFormService _checkOpenFormService = new CheckOpenFormService();

        private List<object> AllEllement = new List<object>();
        private TechnologicalCard OriginCard = null!;

        private TechnologicalCard LocalCard = new();

        public delegate Task PostSaveAction<TModel>(TModel modelObject) where TModel : TechnologicalCard;
        public PostSaveAction<TechnologicalCard>? AfterSave { get; set; }

        public Win7_1_TCs_Window(int? tcId = null, bool win6Format = false, User.Role role = User.Role.Lead)
        {
            _logger = Log.ForContext<Win7_1_TCs_Window>();

            if (tcId != null)
                _logger.Information("Инициализация окна Win7_1_TCs_Window с ID={TcId}, Win6Format={Win6Format}, Роль={Role}", tcId, win6Format, role);
            else
                _logger.Information("Создание новой технологической карты");

            _accessLevel = role;

            InitializeComponent();

            try
            {
                context = new MyDbContext();
                _logger.Information("Подключение к контексту базы данных установлено");
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка подключения к базе данных: {ExceptionMessage}", ex.Message);
                throw;
            }

            ConfigureComboBox();
            ConfigureStatusComboBox();

            AllEllement.Add(txtArticle);
            AllEllement.Add(cbxType);
            AllEllement.Add(cbxNetworkVoltage);

            SubscribeToChanges(); // подписываемся на изменения сразу всех полей

            if (tcId != null)
            {
                try
                {
                    OriginCard = context.TechnologicalCards.FirstOrDefault(s => s.Id == tcId);
                    if (OriginCard != null)
                    {
                        LocalCard.Id = OriginCard.Id;
                        LocalCard.ApplyUpdates(OriginCard);
                        load(LocalCard);
                        _logger.Information("Данные технологической карты с ID={TcId} загружены", tcId);
                        this.Text = "Редактирование технологической карты";
                    }
                    else
                    {
                        _logger.Warning("Технологическая карта с ID={TcId} не найдена", tcId);
                        MessageBox.Show("Технологическая карта не найдена");
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Ошибка при загрузке технологической карты с ID={TcId}: {ExceptionMessage}", tcId, ex.Message);
                    throw;
                }
            }
            else
            {
                OriginCard = new TechnologicalCard();

                btnSaveAndOpen.Text = "Сохранить и открыть";
                this.Text = "Создание новой технологической карты";
            }

            if (win6Format)
            {

                btnSaveAndOpen.Visible = false;
                btnExportExcel.Visible = false;
                btnSave.Visible = false;
            }


            if (_accessLevel == User.Role.Lead)
            {
                _logger.Information("Роль Lead: включение отображения статуса");

                cbxStatus.Visible = true;
                lblStatus.Visible = true;
            }
        }


        public void load(TechnologicalCard LocalCard)
        {
            txtName.Text = LocalCard.Name;
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

            cbxStatus.SelectedItem = LocalCard.Status.GetDescription();
        }

        private void ConfigureComboBox() // todo: обновить список в соответствии с разрешенными значениями (нужно их ещё ввести)
        {
            cbxType.Items.AddRange(new object[] { "Ремонтная", "Монтажная", "Точка трансформации", "Подстанции", "Нет данных" });
            cbxNetworkVoltage.Items.AddRange(new object[] { 35f, 10f, 6f, 0.4f });
        }
        private void ConfigureStatusComboBox()
        {
            FillComboBoxWithEnumDescriptions(cbxStatus, typeof(TechnologicalCardStatus));
        }
        private void FillComboBoxWithEnumDescriptions(ComboBox comboBox, Type enumType)
        {
            var enumValues = Enum.GetValues(enumType).Cast<Enum>().ToList();
            var descriptions = enumValues.Select(e => e.GetDescription()).ToList();

            comboBox.DataSource = descriptions;

            // Сохраняем значения енама для последующего использования
            comboBox.Tag = enumValues;
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
        async Task<bool> SaveAsync()
        {
            _logger.Information("Начало сохранения данных технологической карты");
            //if (LocalCard == null)
            //    LocalCard = new TechnologicalCard();

            LocalCard.Name = txtName.Text;
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

            if (_accessLevel == User.Role.Lead)
            {
                var selectedDescription = cbxStatus.SelectedItem.ToString();
                var enumValues = cbxStatus.Tag as List<Enum>;

                if (enumValues != null)
                {
                    var selectedEnumValue = enumValues.FirstOrDefault(e => e.GetDescription() == selectedDescription);
                    LocalCard.Status = (TechnologicalCardStatus)selectedEnumValue;
                }
            }


            try
            {

                _logger.Information("Проверка уникальности данных технологической карты");
                // проверка полей на уникальность
                if ( !await UniqueFieldChecker<TechnologicalCard>.IsPropertiesUnique(LocalCard))
                {
                    _logger.Warning("Нарушена уникальность полей технологической карты");
                    return false;
                }

                OriginCard.ApplyUpdates(LocalCard);
                await dbCon.AddOrUpdateTCAsync(OriginCard);

                if (AfterSave != null)
                {
                    await AfterSave(OriginCard);
                }

                _logger.Information("Сохранение данных технологической карты выполнено успешно");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка при сохранении данных технологической карты: {ExceptionMessage}", ex.Message);

                MessageBox.Show(ex.Message);
                return false;
            }
        }


        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (NoEmptiness())
            {
                if (await SaveAsync())
                {
                    this.BringToFront();
                    MessageBox.Show("Сохранено!");
                }
            }
        }

        private async void btnSaveAndOpen_Click(object sender, EventArgs e)
        {
            if (NoEmptiness())
            {
                var checkinFormType = "Win6_new";
                _checkOpenFormService.SetFormType(checkinFormType);
                var openedForm = _checkOpenFormService.AreFormOpen(LocalCard.Id);

                if (openedForm != null)
                {
                    openedForm.BringToFront();
                    return;
                }

                if (HasChanges())
                {
                    if (!await SaveAsync())
                        return;
                }

                var nn = OriginCard.Id;
                var editorForm = new Win6_new(nn, role: _accessLevel);
                this.Close();
                editorForm.Show();
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
            //await SaveTCtoExcelFile();

            var tcExporter = new ExExportTC();

            await tcExporter.SaveTCtoExcelFile(OriginCard.Article, OriginCard.Id);

        }

        //private bool FieldsIsNotEmpty()
        //{
        //    if (txtName.Text != "" ||
        //            txtArticle.Text != "" ||
        //            cbxType.Text != "" ||
        //            cbxNetworkVoltage.Text != "" ||
        //            txtTechProcessType.Text != "" ||
        //            txtTechProcess.Text != "" ||
        //            txtParametr.Text != "" ||
        //            txtFinalProduct.Text != "" ||
        //            txtApplicability.Text != "" ||
        //            txtNote.Text != "" ||
        //            chbxIsCompleted.Checked)
        //        return true;
        //    else
        //        return false;
        //}
        private bool HasChanges()
        {
            //if (OriginCard == null)
            //{
            //    return FieldsIsNotEmpty();

            //}

            bool hasChanges = false;

            hasChanges |= OriginCard.Name != txtName.Text;
            hasChanges |= OriginCard.Article != txtArticle.Text;
            hasChanges |= OriginCard.Type != cbxType.Text;
            hasChanges |= OriginCard.NetworkVoltage.ToString() != cbxNetworkVoltage.Text;
            hasChanges |= OriginCard.TechnologicalProcessType != txtTechProcessType.Text;
            hasChanges |= OriginCard.TechnologicalProcessName != txtTechProcess.Text;
            hasChanges |= OriginCard.Parameter != txtParametr.Text;
            hasChanges |= OriginCard.FinalProduct != txtFinalProduct.Text;
            hasChanges |= OriginCard.Applicability != txtApplicability.Text;
            hasChanges |= OriginCard.Note != txtNote.Text;
            hasChanges |= OriginCard.IsCompleted != chbxIsCompleted.Checked;
            hasChanges |= OriginCard.Status.GetDescription() != cbxStatus.SelectedItem.ToString();

            return hasChanges;
        }
        private void SubscribeToChanges()
        {
            txtName.TextChanged += ComponentChanged;
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

            cbxStatus.SelectedIndexChanged += ComponentChanged;
        }

        private void ComponentChanged(object sender, EventArgs e)
        {
            if (HasChanges())
            {
                btnSaveAndOpen.Text = "Сохранить и открыть";
            }
            else
            {
                btnSaveAndOpen.Text = "Открыть";
            }
        }

    }
}
