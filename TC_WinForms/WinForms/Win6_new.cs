using TC_WinForms.DataProcessing;
using TC_WinForms.Interfaces;
using TC_WinForms.WinForms.Work;
using TcModels.Models;
using TcModels.Models.Interfaces;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TechnologicalCard;

namespace TC_WinForms.WinForms
{
    public partial class Win6_new : Form, IViewModeable
    {
        private bool _isViewMode = true;

        private User.Role _accessLevel;

        private Dictionary<EModelType, Form> _formsCache = new Dictionary<EModelType, Form>();
        private EModelType? _activeModelType = null;
        private Form _activeForm = null;

        //TechOperationForm techOperationForm;

        EModelType? activeModelType = null;
        private TechnologicalCard _tc = null!;
        private int _tcId;
        private DbConnector db = new DbConnector();




        public Win6_new(int tcId, User.Role role = User.Role.Lead, bool viewMode = false)
        {
            _tcId = tcId;
            _accessLevel = role;
            _isViewMode = viewMode;

            InitializeComponent();

            // download TC from db
            _tc = db.GetObject<TechnologicalCard>(tcId);
            if (_tc == null)
            {
                MessageBox.Show("Технологическая карта не найдена");
                this.Close();
            }

            this.KeyDown += ControlSaveEvent;

            SetTagsToButtons();

            AccessInitialization();

            SetViewMode();

            SetTCStatusAccess();
        }
        private void AccessInitialization()
        {
            var controlAccess = new Dictionary<User.Role, Action>
            {
                //[User.Role.Lead] = () => { },

                [User.Role.Implementer] = () => 
                {
                    setApprovedStatusToolStripMenuItem.Visible = false;
                    //setApprovedStatusToolStripMenuItem.Enabled = false;
                    if (_tc.Status != TechnologicalCardStatus.Remarked)
                    {
                        setRemarksModeToolStripMenuItem.Enabled = false;
                    }
                },

                [User.Role.ProjectManager] = () =>
                {
                    SetOnlyViewModeRoleAccess();
                },

                [User.Role.User] = () =>
                {
                    SetOnlyViewModeRoleAccess();
                }
            };

            controlAccess.TryGetValue(_accessLevel, out var action);
            action?.Invoke();
        }
        private void SetOnlyViewModeRoleAccess()
        {
            updateToolStripMenuItem.Visible = false;
            if (!_isViewMode)
            {
                MessageBox.Show("Доступен только режим просмотра!");
                SetViewMode(true);
            }
            SaveChangesToolStripMenuItem.Visible = false;
            updateToolStripMenuItem.Visible = false;
            actionToolStripMenuItem.Visible = false;
            setRemarksModeToolStripMenuItem.Visible = false;
        }
        private void SetTCStatusAccess()
        {
            var controlAccess = new Dictionary<TechnologicalCardStatus, Action>
            {

                [TechnologicalCardStatus.Approved] = () =>
                {
                    actionToolStripMenuItem.Visible = false;
                    setRemarksModeToolStripMenuItem.Visible = false;
                    // SaveChangesToolStripMenuItem.Enabled = false;
                },

                [TechnologicalCardStatus.Remarked] = () =>
                {
                    setDraftStatusToolStripMenuItem.Enabled = false;
                },

                [TechnologicalCardStatus.Draft] = () =>
                {
                    setDraftStatusToolStripMenuItem.Enabled = false;
                },

                [TechnologicalCardStatus.Created] = () =>
                {
                    //setApprovedStatusToolStripMenuItem.Enabled = false;
                }
            };

            controlAccess.TryGetValue(_tc.Status, out var action);
            action?.Invoke();

        }

        public void SetViewMode(bool? isViewMode = null)
        {
            if (isViewMode != null)
            {
                _isViewMode = (bool)isViewMode;
            }

            SaveChangesToolStripMenuItem.Visible = !_isViewMode;

            updateToolStripMenuItem.Text = _isViewMode ? "Редактировать" : "Просмотр";

            //btnInformation.Visible = !_isViewMode;

            //if (_isViewMode)
            //    CheckForChanges();

            foreach (var form in _formsCache.Values)
            {
                // is form is ISaveEventForm
                if (form is IViewModeable cashForms)
                {
                    cashForms.SetViewMode(_isViewMode);
                }
            }
        }

        private async void Win6_new_Load(object sender, EventArgs e)
        {
            this.Text = $"{_tc.Name} ({_tc.Article})";
            if (_tc.Status != TechnologicalCardStatus.Approved)
            {
                this.Text += $" - {_tc.Status.GetDescription()}";
            }
            await ShowForm(EModelType.WorkStep);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            WinProcessing.BackFormBtn(this);
        }

        private async void Win6_new_FormClosing(object sender, FormClosingEventArgs e)
        {
            // проверка на наличие изменений во всех формах
            CheckForChanges();

            //close all inner forms
            foreach (Form frm in pnlDataViewer.Controls) // todo - move to WinProcessing and run it asynch
            {
                frm.Close();
            }
            this.Dispose();

        }

        private void cmbTechCardName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private async Task ShowForm(EModelType modelType)
        {

            if (_activeModelType == modelType) return;

            bool isSwitchingFromOrToWorkStep = _activeModelType == EModelType.WorkStep || modelType == EModelType.WorkStep;


            if (isSwitchingFromOrToWorkStep)
            {
                CheckForChanges();

                // Удаляем формы из кеша для их обновления при следующем доступе
                foreach (var formKey in _formsCache.Keys.ToList())
                {
                    if (_formsCache[formKey] != null)
                    {
                        _formsCache[formKey].Close();
                        _formsCache[formKey].Dispose();
                    }
                }
                _formsCache.Clear(); // Очищаем кеш
            }



            if (!_formsCache.TryGetValue(modelType, out var form))
            {
                form = CreateForm(modelType);
                _formsCache[modelType] = form;
            }

            SwitchActiveForm(form);
            _activeModelType = modelType;

            UpdateButtonsState(modelType);
        }

        private void CheckForChanges()
        {
            // проверка на наличие изменений во всех формах
            bool hasUnsavedChanges = false;

            foreach (var fm in _formsCache.Values)
            {
                if (fm is ISaveEventForm saveForm && saveForm.HasChanges)
                {
                    hasUnsavedChanges = true;
                    break;
                }
            }

            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show("Вы хотите сохранить изменения?", "Сохранение изменений", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    foreach (var fm in _formsCache.Values.OfType<ISaveEventForm>().Where(f => f.HasChanges))
                    {
                        fm.SaveChanges();
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
        }

        private Form CreateForm(EModelType modelType)
        {
            switch (modelType)
            {
                case EModelType.Staff:
                    return new Win6_Staff(_tcId, _isViewMode);
                case EModelType.Component:
                    return new Win6_Component(_tcId, _isViewMode);
                case EModelType.Machine:
                    return new Win6_Machine(_tcId, _isViewMode);
                case EModelType.Protection:
                    return new Win6_Protection(_tcId, _isViewMode);
                case EModelType.Tool:
                    return new Win6_Tool(_tcId, _isViewMode);
                case EModelType.WorkStep:
                    return new TechOperationForm(_tcId, _isViewMode);
                //case EModelType.TechnologicalCard:
                //    return new Win7_1_TCs_Window(_tcId, win6Format: true);
                default:
                    throw new ArgumentOutOfRangeException(nameof(modelType), "Неизвестный тип модели");
            }
        }
        private void SwitchActiveForm(Form form) // todo - move to WinProcessing and add to win7
        {
            if (_activeForm != null)
            {
                _activeForm.Hide();
            }
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            pnlDataViewer.Controls.Add(form);
            form.Show();
            _activeForm = form;
        }

        private async void btnShowStaffs_Click(object sender, EventArgs e) => await ShowForm(EModelType.Staff);
        private async void btnShowComponents_Click(object sender, EventArgs e) => await ShowForm(EModelType.Component);
        private async void btnShowMachines_Click(object sender, EventArgs e) => await ShowForm(EModelType.Machine);
        private async void btnShowProtections_Click(object sender, EventArgs e) => await ShowForm(EModelType.Protection);
        private async void btnShowTools_Click(object sender, EventArgs e) => await ShowForm(EModelType.Tool);
        private async void btnShowWorkSteps_Click(object sender, EventArgs e) => await ShowForm(EModelType.WorkStep);
        //private async void btnInformation_Click(object sender, EventArgs e) => await ShowForm(EModelType.TechnologicalCard);

        private void UpdateButtonsState(EModelType activeModelType)
        {
            foreach (Control control in pnlControls.Controls)
            {
                if (control is Button button)
                {
                    button.BackColor = SystemColors.Control;
                    button.ForeColor = SystemColors.ControlText;

                    if (button.Tag is EModelType buttonModelType && buttonModelType == activeModelType)
                    {
                        button.BackColor = Color.FromArgb(10, 107, 88);
                        button.ForeColor = Color.White;
                    }
                }
            }
        }

        private void SetTagsToButtons()
        {
            btnShowStaffs.Tag = EModelType.Staff;
            btnShowComponents.Tag = EModelType.Component;
            btnShowMachines.Tag = EModelType.Machine;
            btnShowProtections.Tag = EModelType.Protection;
            btnShowTools.Tag = EModelType.Tool;
            btnShowWorkSteps.Tag = EModelType.WorkStep;
        }

        private void toolStripButton4_Click(object sender, EventArgs e) => SaveAllChanges();
        private void SaveAllChanges()
        {
            foreach (var form in _formsCache.Values)
            {
                // is form is ISaveEventForm
                if (form is ISaveEventForm saveForm)
                {
                    saveForm.SaveChanges();
                }
            }
            MessageBox.Show("Изменения сохранены"); // todo: add bool return value and show message only if changes were saved
        }
        private void ControlSaveEvent(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveAllChanges();
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SetViewMode(!_isViewMode);
        }

        private async void printToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var tcExporter = new ExExportTC();

            await tcExporter.SaveTCtoExcelFile(_tc.Article, _tc.Id);
        }

        private void SaveChangesToolStripMenuItem_Click(object sender, EventArgs e) => SaveAllChanges();

        private async void setDraftStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await db.UpdateStatusTc(_tc, TechnologicalCardStatus.Draft);
        }

        private async void setApprovedStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await db.UpdateStatusTc(_tc, TechnologicalCardStatus.Approved);
        }

        private async void setRemarksModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tc.Status == TechnologicalCardStatus.Draft && _accessLevel == User.Role.Lead)
            {
                await db.UpdateStatusTc(_tc, TechnologicalCardStatus.Remarked);
            }

        }

        enum WinNumber
        {
            Staff = 1,
            Component = 2,
            Machine = 3,
            Protection = 4,
            Tool = 5
        }
    }

}

