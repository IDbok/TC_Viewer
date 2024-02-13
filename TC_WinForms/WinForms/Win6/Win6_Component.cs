
using OfficeOpenXml.Style;
using System.Data;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Component : Form, ISaveEventForm
    {
        private DbConnector dbCon = new DbConnector();

        private int _tcId;
        private TechnologicalCard _tc;

        private List<Component_TC> newItems = new List<Component_TC>();
        private List<Component_TC> deletedItems = new List<Component_TC>();
        private List<Component_TC> changedItems = new List<Component_TC>();

        public Win6_Component(int tcId)
        {
            InitializeComponent();
            this._tcId = tcId;

            new DGVEvents().AddGragDropEvents(dgvMain);
            new DGVEvents().SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);
        }

        private void Win6_Component_Load(object sender, EventArgs e)
        {
            var objects = Task.Run(() => dbCon.GetIntermediateObjectList<Component_TC, Component>(_tcId));

            DGVProcessing.SetDGVColumnsNamesAndOrder(
                GetColumnNames(), 
                GetColumnOrder(), 
                dgvMain,
                Component_TC.GetChangeablePropertiesNames);

            SetDGVColumnsSettings();

            DGVProcessing.AddNewRowsToDGV(objects.Result, dgvMain);
        }

        private void Win6_Component_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

        private Dictionary<string, string> GetColumnNames()
        {
            //var colNames = new Dictionary<string, string>();
            //foreach (var item in Component_TC.GetPropertiesNames)
            //{
            //    colNames.Add(item.Key, item.Value);
            //}
            //foreach (var item in Component.GetPropertiesNames)
            //{
            //     colNames.Add(item.Key, item.Value);
            //}
            var colNames = new Dictionary<string, string>
            {
                { "ChildId", "ID Комплектующие" },
                { "ParentId", "ID тех. карты" },
                { "Order", "№" },
                { "Quantity", "Количество" },
                { "Note", "Примечание" },
                { "Id", "ID" },
                { "Name", "Наименование" },
                { "Type", "Тип" },
                { "Unit", "Ед.изм." },
                { "Price", "Стоимость,\nруб. без НДС" },
            };
            return colNames;
        }
        private Dictionary<string, int> GetColumnOrder()
        {
            //var colOrder = new Dictionary<string, int>();
            //foreach (var item in Component.GetPropertiesOrder)
            //{
            //    colOrder.Add(item.Key, item.Value);
            //}
            //foreach (var item in Component_TC.GetPropertiesOrder)
            //{
            //    colOrder.Add(item.Key, item.Value);
            //}
            var colOrder = new Dictionary<string, int>
            {
                { "ChildId", -1 },
                { "ParentId", -1 },
                { "Order", 0 },
                { "Quantity", 1 },
                { "Note", 2 },
                { "Id", 3 },
                { "Name", 4 },
                { "Type", 5 },
                { "Unit", 6 },
                { "Price", 7 },
            };
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
                "Quantity",
                "Id",
                "Name",
            };
            foreach (var column in autosizeColumn)
            {
                dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }

            // autosize only comment column to fill all free space
            dgvMain.Columns["Note"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            //dgvMain.Columns["Functions"].Width = 180;
            //dgvMain.Columns["CombineResponsibility"].Width = 140;

            //dgvMain.Columns["Qualification"].Width = 250;
            //dgvMain.Columns["Comment"].Width = 100;

            dgvMain.Columns["Type"].Width = 70;

            // make columns readonly
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.ReadOnly = true;
            }
            // make columns editable
            foreach (var column in Component_TC.GetChangeablePropertiesNames)
            {
                dgvMain.Columns[column].ReadOnly = false;
            }

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AddNewObjects(List<Component> newObjs)
        {
            int i = 1;
            foreach (Component obj in newObjs)
            {
                var sttc = new Component_TC
                {
                    ParentId = _tcId,
                    ChildId = obj.Id,
                    Child = obj,
                    Order = dgvMain.Rows.Count + i,
                    Quantity = 0,
                };
                newItems.Add(sttc);
                i++;
            }

            DGVProcessing.AddNewRowsToDGV(newItems, dgvMain);
        }
        public void SaveChanges()
        {
            MessageBox.Show("SaveChanges in Component");

            dgvMain.CurrentCell = null;// stops editing in dgvMain and save changes


            // check if all rows have unique combination of childId and Symbol fields
            if (!DGVProcessing.CheckUniqueColumnsValues(dgvMain, new List<string> { "Id", "Symbol" }))
            {
                MessageBox.Show("Не все строки имеют уникальную комбинацию полей ID и Символ");
                return;
            }

            DetectChanges(); // check if rows in dgvMain have difference values in copy columns and add them to newItems, deletedItems, changedItems

            var dbCon = new DbConnector();

            // if (!CheckOrder()) return;

            //// save new rows in db
            if (newItems.Count > 0)
                dbCon.Add<Component_TC, Component>(newItems); newItems.Clear();

            // Delete deleted rows from db
            if (deletedItems.Count > 0)
                dbCon.Delete(deletedItems); deletedItems.Clear();

            // save changes in db
            if (changedItems.Count > 0)
                dbCon.Update(changedItems); changedItems.Clear();

            // change values of copy columns to original
            DGVProcessing.SetCopyColumnsValues(dgvMain, Component_TC.GetChangeablePropertiesNames);
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
        private void CheckOrderChanged(DataGridViewRow row) // todo - add try catch block or check if Order and quantity is a numbers
        {
            string order = row.Cells["Order"].Value.ToString();
            string order_copy = row.Cells["Order_copy"].Value.ToString();
            if (order != order_copy)
            {
                var sttc = new Component_TC
                {
                    ParentId = _tcId,
                    ChildId = int.Parse(row.Cells["Id"].Value.ToString()),
                    Order = int.Parse(row.Cells["Order"].Value.ToString()),
                    Quantity = double.Parse(row.Cells["Quantity"].Value.ToString()),
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
                var sttc_new = new Component_TC
                {
                    ParentId = _tcId,
                    ChildId = (int)row.Cells["Id"].Value,
                    //Symbol = (string)row.Cells["Symbol"].Value,
                    Order = (int)row.Cells["Order"].Value
                };
                newItems.Add(sttc_new);
                if (symbol_copy != "-")
                {
                    var sttc_old = new Component_TC
                    {
                        ParentId = _tcId,
                        ChildId = (int)row.Cells["Id"].Value,
                        //Symbol = (string)row.Cells["Symbol_copy"].Value
                    };
                    deletedItems.Add(sttc_old);
                }
            }
        }
        ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            // load new form Win7_3_Component as dictonary
            var newForm = new Win7_4_Component();
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
                        var sttc = new Component_TC
                        {
                            ParentId = _tcId,
                            ChildId = (int)row.Cells["Id"].Value,
                            Order = (int)row.Cells["Order"].Value,
                            //Symbol = (string)row.Cells["Symbol"].Value
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
