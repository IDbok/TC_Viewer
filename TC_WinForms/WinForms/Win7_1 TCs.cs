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

        }

        private void btnDeleteTC_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count > 0)
            {
                List<DataGridViewRow> rowsToDelete = GetSelectedRows();

                // add articles to message
                string message = "Вы действительно хотите удалить выбранные карты?\n";
                foreach (var row in rowsToDelete)
                {
                    message += row.Cells["Article"].Value.ToString() + "\n";
                }

                string caption = "Удаление Технологических карт";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;
                result = MessageBox.Show(message, caption, buttons);


                if (result == DialogResult.Yes)
                {
                    foreach (var row in rowsToDelete)
                    {
                        dbCon.Delete<TechnologicalCard>((int)row.Cells["Id"].Value);
                    }
                    DeleteRows(rowsToDelete);
                }

                dgvMain.Refresh();
            }


        }
        private List<DataGridViewRow> GetSelectedRows()
        {
            List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dgvMain.SelectedRows)
            {
                selectedRows.Add(row);
            }
            return selectedRows;
        }
        private void DeleteRows(List<DataGridViewRow> rowsToDelete)
        {
            foreach (DataGridViewRow row in rowsToDelete)
            {
                dgvMain.Rows.Remove(row);
            }
        }
    }
}
