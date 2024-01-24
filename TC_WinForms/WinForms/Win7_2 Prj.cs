using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TcModels.Models;

namespace TC_WinForms.WinForms
{
    public partial class Win7_2_Prj : Form
    {
        private DbConnector dbCon = new DbConnector();

        private List<TechnologicalCard> tcList = new List<TechnologicalCard>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<TechnologicalCard> changedCards = new List<TechnologicalCard>();
        private TechnologicalCard newCard;
        public Win7_2_Prj(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }

        private void AccessInitialization(int accessLevel)
        {
            // todo - reverse accessibility
            if (accessLevel == 0) // viewer
            {
                // hide all buttons
                pnlControlBtns.Visible = false;
                // pnlNavigationBtns.Visible = false;
                btnAddNewTC.Enabled = false;
                btnUpdateTC.Enabled = false;
                btnDeleteTC.Enabled = false;

            }
            else if (accessLevel == 1) // TC editor
            {

            }
            else if (accessLevel == 2) // Progect editor
            {
            }
        }

        private void Win7_2_Prj_Load(object sender, EventArgs e)
        {
            dgvMain.Columns.Clear();

            tcList = dbCon.GetObjectList<TechnologicalCard>();

            var bindingList = new BindingList<TechnologicalCard>(tcList);
            dgvMain.DataSource = bindingList;

            // set columns names
            WinProcessing.SetTableHeadersNames(TechnologicalCard.GetPropertiesNames(), dgvMain);

            // set columns order and visibility
            WinProcessing.SetTableColumnsOrder(TechnologicalCard.GetPropertiesOrder(), dgvMain);
        }
    }
}
