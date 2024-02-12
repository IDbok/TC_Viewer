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
    public partial class Win7_1_TCs : Form
    {
        public DbConnector dbCon = new DbConnector();

        private List<TechnologicalCard> tcList = new List<TechnologicalCard>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<TechnologicalCard> changedCards = new List<TechnologicalCard>();
        public TechnologicalCard newCard;

        public Win7_1_TCs(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }


        private void Win7_1_TCs_Load(object sender, EventArgs e)
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

        /////////////////////////////// btnNavigation events /////////////////////////////////////////////////////////////////
        

        private void btnAddNewTC_Click(object sender, EventArgs e)
        {
            if (newCard != null) // todo - add check for all required fields (ex. Type can be only as "Ремонтная", "Монтажная", "ТТ")
                if (newCard.Article == ""
                  || newCard.Type == ""
                  || newCard.NetworkVoltage == 0)
                {
                    MessageBox.Show($"Новая карта с id {newCard.Id} ещё не заполнена." +
                        $"\nВнесите обязательные поля (Артикул, Тип карты, Сеть, кВ)");
                    return;
                };
            // create new object and add it to newCard if newCard is null
            if (DataProcessing.DataProcessing.addNewTC(this))
            {
                tcList.Insert(0, newCard);

                // scroll to new row and refresh dgvMain
                dgvMain.FirstDisplayedScrollingRowIndex = 0;
                dgvMain.Refresh();
            }

            
        }

        private void btnUpdateTC_Click(object sender, EventArgs e)
        {
            // check if one and only one row is selected
            if (dgvMain.SelectedRows.Count == 1)
            {
                // get selected row
                DataGridViewRow selectedRow = dgvMain.SelectedRows[0];

                // get id of selected row
                int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);

                // open new form with selected object
                Win6_new win6 = new Win6_new(id);
                //win6.ShowDialog();
                win6.Show();
                //this.Enabled = false;
            }
            else
            {
                MessageBox.Show("Выберите одну строку для редактирования");
            }
        }

        private void btnDeleteTC_Click(object sender, EventArgs e)
        {
            //if (dgvMain.SelectedRows.Count > 0)
            //{
            //    List<DataGridViewRow> rowsToDelete = DataGridProcessing.GetSelectedRows(dgvMain);

            //    // add articles to message
            //    string message = "Вы действительно хотите удалить выбранные карты?\n";

            //    DialogResult result = MessageSender.SendQuestionDeliteObjects(message,rowsToDelete,"Article");

                
            //    if (result == DialogResult.Yes)
            //    {
            //        DataGridProcessing.DeleteRowsById(rowsToDelete, dgvMain, dbCon);
            //    }

            //    dgvMain.Refresh();
            //}
        }

        /////////////////////////////// dgvMain events /////////////////////////////////////////////////////////////////
        
    }
}
