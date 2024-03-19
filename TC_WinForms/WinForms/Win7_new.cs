using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TC_WinForms.DataProcessing;
using TC_WinForms.Interfaces;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms
{

    public partial class Win7_new : Form
    {
        private readonly int _accessLevel;

        private readonly Dictionary<WinNumber, Form> _forms = new Dictionary<WinNumber, Form>();

        private WinNumber? _currentWinNumber = WinNumber.TC;

        private bool _isAllFormsLoading = false;

        public Win7_new(int accessLevel)
        {
            _accessLevel = accessLevel;
            InitializeComponent();
            AccessInitialization(accessLevel);


            //this.Shown += async (sender, e) => await LoadAllForms();

            this.KeyPreview = true;

            this.KeyDown += ControlSaveEvent;

            SetTagsToButtons();
        }

        private async void Win7_new_Load(object sender, EventArgs e)
        {
            //SetLoadingState(true);
            //await LoadAllForms();

            //this.BeginInvoke((MethodInvoker)(() =>
            //{
            //    AddFormToPanel(WinNumber.TC);
            //}));

            //SetLoadingState(false);

            btnTechCard_Click(sender,e);
        }
        private void AccessInitialization(int accessLevel)
        {
            var controlAccess = new Dictionary<int, Action>
            {
                [0] = () => { pnlNavigationBtns.Visible = false; },
                [1] = () => { HideAllButtonsExcept(btnTechCard); },
                [2] = () => { HideAllButtonsExcept(btnProject); },
            };
            controlAccess.TryGetValue(accessLevel, out var action);
            action?.Invoke();
        }
        private void HideAllButtonsExcept(Button visibleButton)
        {
            foreach (Control btn in pnlNavigationBtns.Controls)
            {
                btn.Visible = btn == visibleButton;
            }
        }
        private void Win7_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClosingForms(sender, e);

            WinProcessing.CloseingApp(e);

        }



        /////////////////////////////////////////////////////////////////////////////////////////////////
        private async Task LoadAllForms()
        {

            var tasks = new List<Task>();
            foreach (var winNumber in Enum.GetValues(typeof(WinNumber)))
            {
                tasks.Add(LoadForm((WinNumber)winNumber));
            }

            await Task.WhenAll(tasks);
        }
        private async Task LoadFormInPanel(WinNumber winNumber)
        {
            SetLoadingState(true);

            var form = await LoadForm(winNumber);

            pnlDataViewer.Controls.Clear();
            pnlDataViewer.Controls.Add(form);

            form.Show();

            _currentWinNumber = winNumber;
            form.BringToFront();
            UpdateButtonsState(winNumber);

            SetLoadingState(false);
        }
        private async Task<Form> LoadForm(WinNumber winNumber)
        {
            if (!_forms.TryGetValue(winNumber, out var form))
            {
                form = CreateForm(winNumber);
                _forms[winNumber] = form;
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;

                var loadDataTask = (form as ILoadDataAsyncForm)?.LoadDataAsync();
                if (loadDataTask != null)
                {
                    await loadDataTask; //.ConfigureAwait(false);
                }
            }
            return form;
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
                case WinNumber.Project:
                    return new Win7_2_Prj(_accessLevel);
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
                default:
                    return null;
            }
        }

        private void ClosingForms(object sender, EventArgs e)
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

        private async void updateToolStripButton_Click(object sender, EventArgs e)
        {
            // close all forms and load them again
            foreach (var frm in _forms)
            {
                var form = frm.Value;
                form.Close();
            }
            _forms.Clear();

            await LoadAllForms();
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
            TechTransition = 9
        }

    }
}
