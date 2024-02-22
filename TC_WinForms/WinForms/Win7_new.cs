using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms
{

    public partial class Win7_new : Form
    {
        private readonly int _accessLevel;

        private readonly Dictionary<WinNumber, Form> _forms = new Dictionary<WinNumber, Form>();
        
        public Win7_new(int accessLevel)
        {
            _accessLevel = accessLevel;
            InitializeComponent();
            AccessInitialization(accessLevel);


            btnTechCard_Click(null, null);
            this.KeyPreview = true;

            this.KeyDown += ControlSaveEvent;
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
        private void LoadFormInPanel(WinNumber winNumber)
        {
            if (!_forms.TryGetValue(winNumber, out var form))
            {
                form = CreateForm(winNumber);
                _forms[winNumber] = form;
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                pnlDataViewer.Controls.Add(form);
            }

            foreach (var frm in pnlDataViewer.Controls.OfType<Form>())
            {
                frm.Hide();
            }

            form.Show();
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
                default:
                    return null;
            }
        }

        private void ClosingForms(object sender, EventArgs e)
        {
            foreach (Form frm in pnlDataViewer.Controls)
            {
                frm.Close();
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        private void btnTechCard_Click(object sender, EventArgs e) => LoadFormInPanel(WinNumber.TC);
        private void btnStaff_Click(object sender, EventArgs e) => LoadFormInPanel(WinNumber.Staff);
        private void btnComponent_Click(object sender, EventArgs e) => LoadFormInPanel(WinNumber.Component);
        private void btnMachine_Click(object sender, EventArgs e) => LoadFormInPanel(WinNumber.Machine);
        private void btnProtection_Click(object sender, EventArgs e) => LoadFormInPanel(WinNumber.Protection);
        private void btnTool_Click(object sender, EventArgs e) => LoadFormInPanel(WinNumber.Tool);


        private async void ControlSaveEvent(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                await Save(); 
            }
        }

        private async Task Save()
        {
            // todo - save all changes in all forms 

            
            foreach (var frm in _forms)
            {
                var form = frm.Value;
                if (form is ISaveEventForm saveEventForm)
                {
                    await saveEventForm.SaveChanges();
                }
            }
            //_forms.TryGetValue(WinNumber.TC, out var form);
            //if (form is ISaveEventForm saveEventForm)
            //{
            //    await saveEventForm.SaveChanges();
            //}
            MessageBox.Show("Сохранено!");
        }

        enum WinNumber
        {
            TC = 1,
            Project = 2,
            Staff = 3,
            Component = 4,
            Machine = 5,
            Protection = 6,
            Tool = 7
        }

    }
}
