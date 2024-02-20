
using System.ComponentModel;
using TC_WinForms.DataProcessing;
using TcModels.Models;

namespace TC_WinForms.WinForms
{

    public partial class Win7_new : Form
    {
        private DbConnector dbCon = new DbConnector();
        private int _accessLevel;

        private List<Form> forms = new List<Form>();

        private Win7_1_TCs winTCs;
        private Win7_2_Prj winPrj;
        private Win7_3_Staff winStaff;
        private Win7_4_Component winComponent;
        private Win7_5_Machine winMachine;
        private Win7_6_Tool winTool;
        private Win7_7_Protection winProtection;

        private WinNumber activeWin = 0;

        private List<TechnologicalCard> tcList = new List<TechnologicalCard>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<TechnologicalCard> changedCards = new List<TechnologicalCard>();
        private TechnologicalCard newCard;

        public Win7_new(int accessLevel)
        {
            _accessLevel = accessLevel;
            InitializeComponent();
            AccessInitialization(accessLevel);

            // btnTechCard_Click event
            btnTechCard_Click(null, null);

        }
        private void AccessInitialization(int accessLevel)
        {
            // todo - reverse accessibility
            if (accessLevel == 0) // viewer
            {
                // hide all buttons
                //pnlControlBtns.Visible = false;

                pnlNavigationBtns.Visible = false;

                //btnAddNewTC.Enabled = false;
                //btnUpdateTC.Enabled = false;
                //btnDeleteTC.Enabled = false;

            }
            else if (accessLevel == 1) // TC editor
            {
                // false visibility of all buttons in pnlControlBtns except btnAddNewTC
                foreach (Control btn2 in pnlNavigationBtns.Controls)
                { btn2.Visible = false; }
                btnTechCard.Visible = true;

            }
            else if (accessLevel == 2) // Progect editor
            {
                foreach (Control btn2 in pnlNavigationBtns.Controls)
                { btn2.Visible = false; }
                btnProject.Visible = true;
            }
        }

        private void Win7_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClosingForms(sender, e);

            WinProcessing.CloseingApp(e);

        }

        private void UpdateChangedTcList()
        {
            foreach (var item in changedItemId)
            {
                changedCards.Add(tcList.Find(x => x.Id == item));
            }
        }




        private void AddChangedItemToList(int id)
        {
            if (!changedItemId.Contains(id))
            {
                changedItemId.Add(id);
            }
        }

        private void btnUpdateTC_Click(object sender, EventArgs e)
        {
            UpdateChangedTcInDb();
        }

        private void UpdateChangedTcInDb()
        {
            UpdateChangedTcList();
            dbCon.UpdateTcList(changedCards);

            changedItemId.Clear();
            changedCards.Clear();
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

        /////////////////////////////////////////////////////////////////////////////////////////////////


        private void LoadFormInPanel(Form form)
        {
            // hide all forms in panel
            foreach (Form frm in pnlDataViewer.Controls)
            {
                frm.Hide();
            }

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            this.pnlDataViewer.Controls.Add(form);
            form.Show();

            //forms.Add(form);
        }

        private void ClosingForms(object sender, EventArgs e)
        {
            foreach (Form frm in pnlDataViewer.Controls)
            {
                frm.Close();
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void btnTechCard_Click(object sender, EventArgs e)
        {
            if (winTCs == null)
            {
                winTCs = new Win7_1_TCs(_accessLevel);
            }
            LoadFormInPanel(winTCs);
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            if (winStaff == null)
            {
                winStaff = new Win7_3_Staff(_accessLevel);
            }
            LoadFormInPanel(winStaff);
        }

        private void btnComponent_Click(object sender, EventArgs e)
        {
            if (winComponent == null)
            {
                winComponent = new Win7_4_Component(_accessLevel);
            }
            LoadFormInPanel(winComponent);
        }

        private void btnMachine_Click(object sender, EventArgs e)
        {
            if (winMachine == null)
            {
                winMachine = new Win7_5_Machine(_accessLevel);
            }
            LoadFormInPanel(winMachine);
        }

        private void btnProtection_Click(object sender, EventArgs e)
        {
            if (winProtection == null)
            {
                winProtection = new Win7_7_Protection(_accessLevel);
            }
            LoadFormInPanel(winProtection);
        }

        private void btnTool_Click(object sender, EventArgs e)
        {
            if (winTool == null)
            {
                winTool = new Win7_6_Tool(_accessLevel);
            }
            LoadFormInPanel(winTool);
        }
    }
}
