
using System.Data;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Staff : Form, ISaveEventForm
    {
        private DbConnector dbCon = new DbConnector();

        private int _tcId;
        private TechnologicalCard _tc;

        private List<Staff_TC> newItems = new List<Staff_TC>();
        private List<Staff_TC> deletedItems = new List<Staff_TC>();
        private List<Staff_TC> changedItems = new List<Staff_TC>();

        public Win6_Staff(int tcId)
        {
            InitializeComponent();
            this._tcId = tcId;

            new DGVEvents().AddGragDropEvents(dgvMain);
            new DGVEvents().SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);
        }

        private void Win6_Staff_Load(object sender, EventArgs e)
        {
            var objects = Task.Run(() => dbCon.GetIntermediateObjectList<Staff_TC, Staff>(_tcId));

            DGVProcessing.SetDGVColumnsNamesAndOrder(
                GetColumnNames(), 
                GetColumnOrder(), 
                dgvMain,
                Staff_TC.GetChangeablePropertiesNames);

            SetDGVColumnsSettings();

            DGVProcessing.AddNewRowsToDGV(objects.Result, dgvMain);
        }

        private void Win6_Staff_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

        private Dictionary<string, string> GetColumnNames()
        {
            var colNames = new Dictionary<string, string>();
            foreach (var item in Staff_TC.GetPropertiesNames)
            {
                colNames.Add(item.Key, item.Value);
            }
            foreach (var item in Staff.GetPropertiesNames)
            {
                colNames.Add(item.Key, item.Value);
            }
            return colNames;
        }
        private Dictionary<string, int> GetColumnOrder()
        {
            var colOrder = new Dictionary<string, int>();
            foreach (var item in Staff.GetPropertiesOrder)
            {
                colOrder.Add(item.Key, item.Value);
            }
            foreach (var item in Staff_TC.GetPropertiesOrder)
            {
                colOrder.Add(item.Key, item.Value);
            }
            return colOrder;
        }
        void SetDGVColumnsSettings()
        {

            // автоподбор ширины столбцов под ширину таблицы
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.RowHeadersWidth = 25;

            // автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // ширина столбцов по содержанию
            var autosizeColumn = new List<string>
            {
                "Order",
                "Symbol",
                "Id",
                "Name",
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            // autosize only comment column to fill all free space
            dgvMain.Columns["Comment"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgvMain.Columns["Functions"].Width = 180;
            dgvMain.Columns["CombineResponsibility"].Width = 140;

            dgvMain.Columns["Qualification"].Width = 250;
            dgvMain.Columns["Comment"].Width = 100;

            dgvMain.Columns["Type"].Width = 70;

            // make columns readonly
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.ReadOnly = true;
            }
            // make columns editable
            dgvMain.Columns["Order"].ReadOnly = false;
            dgvMain.Columns["Symbol"].ReadOnly = false;

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AddNewObjects(List<Staff> newObjs)
        {
            int i = 1;
            foreach (Staff obj in newObjs)
            {
                var sttc = new Staff_TC
                {
                    ParentId = _tcId,
                    ChildId = obj.Id,
                    Child = obj,
                    Order = dgvMain.Rows.Count + i,
                    Symbol = "-"
                };
                newItems.Add(sttc);
                i++;
            }

            DGVProcessing.AddNewRowsToDGV(newItems, dgvMain);
        }
        public void SaveChanges()
        {
            MessageBox.Show("SaveChanges in Staff");

            dgvMain.CurrentCell = null;// stops editing in dgvMain and save changes


            // check if all rows have unique combination of childId and Symbol fields
            if (!DGVProcessing.CheckUniqueColumnsValues(dgvMain, new List<string> { "Id", "Symbol" }))
            {
                MessageBox.Show("Не все строки имеют уникальную комбинацию полей ID и Символ");
                return;
            }

            DetectChanges(); // check if rows in dgvMain have difference values in copy columns and add them to newItems, deletedItems, changedItems

            var dbCon = new DbConnector();

            //// save new rows in db
            if (newItems.Count > 0)
                dbCon.Add<Staff_TC, Staff>(newItems); newItems.Clear();

            // Delete deleted rows from db
            if (deletedItems.Count > 0)
                dbCon.Delete<Staff_TC>(deletedItems); deletedItems.Clear();

            // save changes in db
            if (changedItems.Count > 0)
                dbCon.Update<Staff_TC>(changedItems); changedItems.Clear();

            // change values of copy columns to original
            DGVProcessing.SetCopyColumnsValues(dgvMain, Staff_TC.GetChangeablePropertiesNames);
        }
        private void DetectChanges()
        {
            // check if rows in dgvMain have difference values in copy columns
            foreach (var row in dgvMain.Rows.Cast<DataGridViewRow>())
            {
                CheckSymbolChanged(row);

                CheckOrderChanged(row);
            }
        }
        private void CheckOrderChanged(DataGridViewRow row)
        {
            string order = row.Cells["Order"].Value.ToString();
            string order_copy = row.Cells["Order_copy"].Value.ToString();
            if (order != order_copy)
            {
                var sttc = new Staff_TC
                {
                    ParentId = _tcId,
                    ChildId = int.Parse(row.Cells["Id"].Value.ToString()),
                    Symbol = (string)row.Cells["Symbol"].Value,
                    Order = int.Parse(row.Cells["Order"].Value.ToString())
                };
                changedItems.Add(sttc);
            }
        }
        private void CheckSymbolChanged(DataGridViewRow row)
        {
            string symbol = row.Cells["Symbol"].Value.ToString();
            string symbol_copy = row.Cells["Symbol_copy"].Value.ToString();

            if (symbol != symbol_copy)
            {
                var sttc_new = new Staff_TC
                {
                    ParentId = _tcId,
                    ChildId = (int)row.Cells["Id"].Value,
                    Symbol = (string)row.Cells["Symbol"].Value,
                    Order = (int)row.Cells["Order"].Value
                };
                newItems.Add(sttc_new);
                if (symbol_copy != "-")
                {
                    var sttc_old = new Staff_TC
                    {
                        ParentId = _tcId,
                        ChildId = (int)row.Cells["Id"].Value,
                        Symbol = (string)row.Cells["Symbol_copy"].Value
                    };
                    deletedItems.Add(sttc_old);
                }
            }
        }
        ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            // load new form Win7_3_Staff as dictonary
            var newForm = new Win7_3_Staff();
            newForm.SetAsAddingForm();
            newForm.ShowDialog();
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count > 0)
            {
                List<DataGridViewRow> rowsToDelete = DGVProcessing.GetSelectedRows(dgvMain);

                string message;
                if (rowsToDelete.Count == 1)
                    message = "Вы действительно хотите удалить выбранные объект?\n";
                else
                    message = "Вы действительно хотите удалить выбранные объекты?\n";

                DialogResult result = MessageSender.SendQuestionDeliteObjects(message, rowsToDelete, "Id");

                if (result == DialogResult.Yes)
                {
                    foreach (var row in rowsToDelete)
                    {
                        var sttc = new Staff_TC
                        {
                            ParentId = _tcId,
                            ChildId = (int)row.Cells["Id"].Value,
                            Order = (int)row.Cells["Order"].Value,
                            Symbol = (string)row.Cells["Symbol"].Value
                        };
                        deletedItems.Add(sttc);
                    }
                    DGVProcessing.DeleteRows(rowsToDelete, dgvMain);
                }
                dgvMain.Refresh();
            }
        }

        private void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvMain.Rows[e.RowIndex];

            // if changed cell is in Order column then reorder dgv
            if (e.ColumnIndex == dgvMain.Columns["Order"].Index)
            {
                // check if new cell value is a number
                if (int.TryParse(row.Cells["Order"].Value.ToString(), out int orderValue))
                {
                    if (orderValue == e.RowIndex + 1) { return; }
                    if (orderValue > 0 && orderValue <= dgvMain.Rows.Count)
                    { DGVProcessing.ReorderRows(row, orderValue, dgvMain); }
                    else
                    { DGVProcessing.ReorderRows(row, dgvMain.Rows.Count, dgvMain); } // insert row to the end of dgv

                }
                else
                {
                    MessageBox.Show("Ошибка ввода в поле \"№\"");
                    row.Cells["Order"].Value = e.RowIndex + 1;
                }
            }
            else if (e.ColumnIndex == dgvMain.Columns["Symbol"].Index)
            {
                string symbol = (string)row.Cells["Symbol"].Value;
                string symbol_copy = (string)row.Cells["Symbol_copy"].Value;
                if (symbol != symbol_copy)
                {
                    // check if new symbol is unique
                    if (!DGVProcessing.CheckUniqueColumnsValues(dgvMain, new List<string> { "Symbol" }))
                    {
                        MessageBox.Show("Символ должен быть уникальным!");
                        row.Cells["Symbol"].Value = symbol_copy;
                    }
                }
            }
        }
    }

}
