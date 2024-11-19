using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Serilog;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TC_WinForms.Interfaces;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using static TC_WinForms.DataProcessing.AuthorizationService;

namespace TC_WinForms.WinForms
{

    public partial class Win7_new : Form
    {
        //private readonly int _accessLevel;
        private readonly User.Role _accessLevel;

        private readonly Dictionary<WinNumber, Form> _forms = new Dictionary<WinNumber, Form>();

        private WinNumber? _currentWinNumber = WinNumber.TC;

        private bool _isAllFormsLoading = false;

        public void SetPageInfo(int startRecord, int endRecord, int totalRecords)
        {
            lblPageInfo.Text = $"Показаны результаты с {startRecord} по {endRecord} из {totalRecords}";
        }

        public Win7_new(User.Role accessLevel)
        {
            Log.Information("Инициализация окна Win7_new");

            StaticWinForms.Win7_new = this;

            _accessLevel = accessLevel;
            InitializeComponent();

            AccessInitialization();

            //this.Shown += async (sender, e) => await LoadAllForms();

            this.KeyPreview = true;

            this.KeyDown += ControlSaveEvent;

            SetTagsToButtons();
        }

        private async void Win7_new_Load(object sender, EventArgs e)
        {
            if (_currentWinNumber != null)
                await LoadFormInPanel(_currentWinNumber.Value).ConfigureAwait(false);
        }
        private void AccessInitialization()
        {
            var controlAccess = new Dictionary<User.Role, Action>
            {
                [User.Role.Lead] = () => { _currentWinNumber = WinNumber.TC; },

                [User.Role.Implementer] = () => { _currentWinNumber = WinNumber.TC; },

                [User.Role.ProjectManager] = () =>
                {
                    _currentWinNumber = WinNumber.Project;

                    HideAllButtonsExcept(new List<Button> { btnProject, btnTechCard });
                },

                [User.Role.User] = () =>
                {
                    _currentWinNumber = WinNumber.TC;
                    HideAllButtonsExcept(new List<Button> { btnTechCard });
                }
            };

            controlAccess.TryGetValue(_accessLevel, out var action);
            action?.Invoke();
        }
        private void HideAllButtonsExcept(List<Button> visibleButtons)
        {
            foreach (var button in pnlNavigationBtns.Controls.OfType<Button>())
            {
                button.Visible = visibleButtons.Contains(button);
            }
        }
        private void Win7_FormClosing(object sender, FormClosingEventArgs e)
        {
            WinProcessing.ClosingApp(e, ClosingForms);
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////
        //private async Task LoadAllForms()
        //{
        //    var tasks = new List<Task>();
        //    foreach (var winNumber in Enum.GetValues(typeof(WinNumber)))
        //    {
        //        tasks.Add(LoadForm((WinNumber)winNumber));
        //    }

        //    await Task.WhenAll(tasks);
        //}
        private async Task LoadFormInPanel(WinNumber winNumber)
        {
            Log.Information("Начало загрузки формы {WinNumber}", winNumber);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            SetLoadingState(true);

            // выделение нажатой кнопки
            UpdateButtonsState(winNumber);

            try
            {
                var form = await LoadForm(winNumber);

                SetPaginationBar(form);

                pnlDataViewer.Controls.Clear();
                pnlDataViewer.Controls.Add(form);

                form.Show();

                _currentWinNumber = winNumber;
                form.BringToFront();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при загрузке формы {CurrentWinNumber}", winNumber);
                MessageBox.Show("Ошибка при загрузке формы");
            }
            finally
            {
                SetLoadingState(false);
            }

            stopwatch.Stop();
            Log.Information("Форма {WinNumber} загружена за {ElapsedMilliseconds} мс",
                winNumber, stopwatch.ElapsedMilliseconds);
        }
        private async Task<Form> LoadForm(WinNumber winNumber)
        {
            if (!_forms.TryGetValue(winNumber, out var form))
            {
                try
                {
                    form = CreateForm(winNumber);
                    _forms[winNumber] = form;
                    form.TopLevel = false;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.Dock = DockStyle.Fill;

                    Log.Information("Создана форма {WinNumber}", winNumber);

                    SubscribeToPageInfoChanged(form as IPaginationControl);

                    var loadDataTask = (form as ILoadDataAsyncForm)?.LoadDataAsync();
                    if (loadDataTask != null)
                    {
                        await loadDataTask; //.ConfigureAwait(false);
                        Log.Information("Данные формы {WinNumber} загружены успешно", winNumber);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при создании формы {WinNumber}", winNumber);
                    throw;
                }

            }

            return form;
        }

        private void SetPaginationBar(Form? form)
        {
            if (form is IPaginationControl paginationForm )
            {
                paginationForm.RaisePageInfoChanged();

                pnlPageControls.Visible = true;
            }
            else { pnlPageControls.Visible = false; }
        }

        private void AddFormToPanel(WinNumber winNumber)
        {
            pnlDataViewer.Controls.Clear(); // Очищаем предыдущие формы
            var form = _forms[winNumber];
            if (form != null)
            {
                pnlDataViewer.Controls.Add(form); // Добавляем загруженную форму
                form.Show();
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            _isAllFormsLoading = isLoading;

            if (isLoading)
            {
                progressBarLoad.Maximum = Enum.GetNames(typeof(WinNumber)).Length + 1;
                progressBarLoad.Value = 0;
            }
            else
            {
                progressBarLoad.Value = progressBarLoad.Maximum;
            }
            Cursor.Current = isLoading ? Cursors.WaitCursor : Cursors.Default;
            progressBarLoad.Visible = isLoading;

            pnlDataViewer.Visible = !isLoading;

            pnlNavigationBlok.Enabled = !isLoading;
        }
        private Form CreateForm(WinNumber winNumber)
        {
            switch (winNumber)
            {
                case WinNumber.TC:
                    return new Win7_1_TCs(_accessLevel);
                //case WinNumber.Project:
                //    return new Win7_2_Prj(_accessLevel);
                case WinNumber.Staff:
                    return new Win7_3_Staff(_accessLevel);
                case WinNumber.Component:
                    return new Win7_4_Component(_accessLevel);
                case WinNumber.Machine:
                    return new Win7_5_Machine(_accessLevel);
                case WinNumber.Protection:
                    return new Win7_7_Protection(_accessLevel);
                case WinNumber.Tool:
                    return new Win7_6_Tool(_accessLevel);
                case WinNumber.TechOperation:
                    return new Win7_TechOperation(_accessLevel);
                case WinNumber.TechTransition:
                    return new Win7_TechTransition(_accessLevel);
                case WinNumber.Project:
                    return new Win7_Process(_accessLevel);
                default:
                    return null;
            }
        }

        private void ClosingForms()
        {
            foreach (Form frm in _forms.Values)
            {
                frm.Close();
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async void btnTechCard_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.TC).ConfigureAwait(false);
        private async void btnStaff_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.Staff).ConfigureAwait(false);
        private async void btnComponent_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.Component).ConfigureAwait(false);
        private async void btnMachine_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.Machine).ConfigureAwait(false);
        private async void btnProtection_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.Protection).ConfigureAwait(false);
        private async void btnTool_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.Tool).ConfigureAwait(false);
        private async void btnTechOperation_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.TechOperation).ConfigureAwait(false);
        private async void btnWorkStep_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.TechTransition).ConfigureAwait(false);

        private async void btnProject_Click(object sender, EventArgs e) => await LoadFormInPanel(WinNumber.Project).ConfigureAwait(false);


        private void UpdateButtonsState(WinNumber activeModelType)
        {
            foreach (Control control in pnlNavigationBtns.Controls)
            {
                if (control is Button button)
                {
                    button.BackColor = SystemColors.Control;
                    button.ForeColor = SystemColors.ControlText;

                    if (button.Tag is WinNumber buttonModelType && buttonModelType == activeModelType)
                    {
                        button.BackColor = Color.FromArgb(10, 107, 88);
                        button.ForeColor = Color.White;
                    }
                }
            }
        }
        private void SetTagsToButtons()
        {
            btnTechCard.Tag = WinNumber.TC;
            btnProject.Tag = WinNumber.Project;
            btnStaff.Tag = WinNumber.Staff;
            btnComponent.Tag = WinNumber.Component;
            btnMachine.Tag = WinNumber.Machine;
            btnProtection.Tag = WinNumber.Protection;
            btnTool.Tag = WinNumber.Tool;
            btnTechOperation.Tag = WinNumber.TechOperation;
            btnWorkStep.Tag = WinNumber.TechTransition;

            //btnProcess.Tag = WinNumber.Process;
        }


        private async void ControlSaveEvent(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                await Save();
            }
        }

        private async Task Save()
        {
            foreach (var frm in _forms)
            {
                var form = frm.Value;
                if (form is ISaveEventForm saveEventForm)
                {
                    await saveEventForm.SaveChanges();
                }
            }
            MessageBox.Show("Сохранено!");
        }

        private async void toolStripButton5_Click(object sender, EventArgs e)
        {
            await Save();
        }

        // Методы нигде не используются
        //public async void UpdateTС(TechnologicalCard tc)
        //{
        //    // найти ТК в форме с данным id и обновить ее
        //    if (_forms.TryGetValue(WinNumber.TC, out var form))
        //    {
        //        if (form is Win7_1_TCs tcForm)
        //        {
        //            //await tcForm.UpdateTc(tc);
        //        }
        //    }
        //}

        //public async void UpdateTC()
        //{
        //    _forms.Remove(WinNumber.TC);
        //    //LoadForm(WinNumber.TC);

        //    if (_currentWinNumber != null)
        //        await LoadFormInPanel(_currentWinNumber.Value).ConfigureAwait(false);
        //}

        public async void UpdateTO()
        {
            _forms.Remove(WinNumber.TechOperation);
            //LoadForm(WinNumber.TechOperation); 
            // не вижу сценария, при котором окно с ТО не является _currentWinNumber
            // при вызове этой функции

            if (_currentWinNumber != null)
                await LoadFormInPanel(_currentWinNumber.Value).ConfigureAwait(false);
        }


        public async void updateToolStripButton_Click(object sender, EventArgs e)
        {
            Log.Information("Вызвано обновление данных из формы {CurrentWinNumber}", 
                _currentWinNumber);

            bool next = true;
            foreach (var frm in _forms)
            {
                if (frm.Value is ISaveEventForm)
                {
                    if (((ISaveEventForm)frm.Value).GetDontSaveData() == true)
                    {
                        next = false;
                    }
                }
            }

            if (next == false)
            {
                var result = MessageBox.Show("Сохранить изменения перед закрытием?", "Сохранение",
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    foreach (var frm in _forms)
                    {
                        if (frm.Value is ISaveEventForm)
                        {
                            await ((ISaveEventForm)frm.Value).SaveChanges();
                        }
                    }
                }
                else
                {
                    foreach (var frm in _forms)
                    {
                        if (frm.Value is ISaveEventForm)
                        {
                            ((ISaveEventForm)frm.Value).CloseFormsNoSave = true;
                        }
                    }
                }
            }

            // close all forms and load them again
            foreach (var frm in _forms)
            {
                var form = frm.Value;
                form.Close();
            }

            _forms.Clear();

            // await LoadAllForms(); // кажется, заная загрузка при обновлении безколезна,
            // т.к. активная форма загружается ниже

            if (_currentWinNumber != null)
                await LoadFormInPanel(_currentWinNumber.Value).ConfigureAwait(false);

            Log.Information("Данные обновлены");
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (_forms.TryGetValue(_currentWinNumber.Value, out var activeForm) && activeForm is IPaginationControl paginationControl)
            {
                paginationControl.GoToNextPage();
            }
        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            if (_forms.TryGetValue(_currentWinNumber.Value, out var activeForm) && activeForm is IPaginationControl paginationControl)
            {
                paginationControl.GoToPreviousPage();
            }
        }

        private void SubscribeToPageInfoChanged(IPaginationControl? form)
        {
            if (form == null)
                return;

            form.PageInfoChanged += Form_PageInfoChanged;
        }

        private void Form_PageInfoChanged(object sender, PageInfoEventArgs? e)
        {
            if (e == null)
                return;
            // Может потребоваться использовать Invoke для обновления UI из другого потока.
            lblPageInfo.Invoke((MethodInvoker)delegate
            {
                lblPageInfo.Text = $"Показаны результаты с {e.StartRecord} по {e.EndRecord} из {e.TotalRecords}";
            });
        }

        private void infoToolStripButton_Click(object sender, EventArgs e)
        {
            Log.Information("Вызвано отображение информации о программе");
            // сообщение с информацией о программе
            MessageBox.Show(ApplicationInfoService.GetApplicationInfo(), "О программе");
        }

        enum WinNumber
        {
            TC = 1,
            Project = 2,
            Staff = 3,
            Component = 4,
            Machine = 5,
            Protection = 6,
            Tool = 7,

            TechOperation = 8,
            TechTransition = 9,

            //Process = 10
        }

    }
}
