using TC_WinForms.DataProcessing;
using TC_WinForms.WinForms.Work;
using TcModels.Models;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms
{
    public partial class Win6_new : Form
    {
        private Dictionary<EModelType, Form> _formsCache = new Dictionary<EModelType, Form>();
        private EModelType? _activeModelType = null;
        private Form _activeForm = null;

        TechOperationForm techOperationForm;

        EModelType? activeModelType = null;
        private TechnologicalCard _tc;
        private int _tcId;
        private DbConnector db = new DbConnector();


        public Win6_new(int tcId)
        {
            _tcId = tcId;
            InitializeComponent();

            // download TC from db
            _tc = db.GetObject<TechnologicalCard>(tcId);// Task.Run(()=>db.GetObject<TechnologicalCard>(tcId));

            this.KeyDown += ControlSaveEvent;
        }

        private void Win6_new_Load(object sender, EventArgs e)
        {
            this.Text = $"{_tc.Name} ({_tc.Article})";
            ShowForm(EModelType.Staff); 
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            WinProcessing.BackFormBtn(this);
        }

        private void Win6_new_FormClosing(object sender, FormClosingEventArgs e)
        {
            //close all inner forms
            foreach (Form frm in pnlDataViewer.Controls) // todo - move to WinProcessing and run it asynch
            {
                frm.Close();
            }
            this.Dispose();
            // todo - if there are some changes - ask user if he wants to save them
        }

        private void cmbTechCardName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void ShowForm(EModelType modelType)
        {
            if (_activeModelType == modelType) return;

            if (!_formsCache.TryGetValue(modelType, out var form))
            {
                form = CreateForm(modelType);
                _formsCache[modelType] = form;
            }

            SwitchActiveForm(form);
            _activeModelType = modelType;
        }

        private Form CreateForm(EModelType modelType)
        {
            switch (modelType)
            {
                case EModelType.Staff:
                    return new Win6_Staff(_tcId);
                case EModelType.Component:
                    return new Win6_Component(_tcId);
                case EModelType.Machine:
                    return new Win6_Machine(_tcId);
                case EModelType.Protection:
                    return new Win6_Protection(_tcId);
                case EModelType.Tool:
                    return new Win6_Tool(_tcId);
                case EModelType.WorkStep:
                    return new TechOperationForm(_tcId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(modelType), "Неизвестный тип модели");
            }
        }
        private void SwitchActiveForm(Form form) // todo - move to WinProcessing and add to win7
        {
            pnlDataViewer.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            pnlDataViewer.Controls.Add(form);
            form.Show();
            _activeForm = form;
        }

        private void btnShowStaffs_Click(object sender, EventArgs e)
        {
            ShowForm(EModelType.Staff);
        }

        private void btnShowComponents_Click(object sender, EventArgs e)
        {
            ShowForm(EModelType.Component); 
        }

        private void btnShowMachines_Click(object sender, EventArgs e)
        {
            ShowForm(EModelType.Machine); 
        }

        private void btnShowProtections_Click(object sender, EventArgs e)
        {
            ShowForm(EModelType.Protection); 
        }

        private void btnShowTools_Click(object sender, EventArgs e)
        {
            ShowForm(EModelType.Tool); 
        }

        private void btnShowWorkSteps_Click(object sender, EventArgs e)
        {
            ShowForm(EModelType.WorkStep);
            //if (_activeForm is TechOperationForm) return;
            //if (techOperationForm == null)
            //    techOperationForm = new TechOperationForm(_tcId);
            //_activeForm = techOperationForm;
            //LoadFormInPanel(_activeForm);


            //if (activeModelType == EModelType.WorkStep) return;
            ////SaveDataFromDGV(activeModelType);
            //DGVNewStructure(EModelType.WorkStep);
            //activeModelType = EModelType.WorkStep;
            //if (sender is Button)
            //    WinProcessing.ColorizeOnlyChosenButton(sender as Button, pnlControls);
        } // todo - make it work


        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            SaveAllChanges();
        }
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
        }
        private void ControlSaveEvent(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveAllChanges();
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

