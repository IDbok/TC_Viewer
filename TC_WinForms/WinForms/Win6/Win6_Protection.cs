
using OfficeOpenXml.Style;
using System.Data;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using static TC_WinForms.DataProcessing.DGVProcessing;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Protection : Form, ISaveEventForm
    {
        private DbConnector dbCon = new DbConnector();

        private int _tcId;
        private TechnologicalCard _tc;

        private List<Protection_TC> newItems = new List<Protection_TC>();
        private List<Protection_TC> deletedItems = new List<Protection_TC>();
        private List<Protection_TC> changedItems = new List<Protection_TC>();

        public Win6_Protection(int tcId)
        {
            InitializeComponent();
            this._tcId = tcId;

            new DGVEvents().AddGragDropEvents(dgvMain);
            new DGVEvents().SetRowsUpAndDownEvents(btnMoveUp, btnMoveDown, dgvMain);
        }

        private void Win6_Protection_Load(object sender, EventArgs e)
        {
            var objects = Task.Run(() => dbCon.GetIntermediateObjectList<Protection_TC, Protection>(_tcId));

            SetDGVColumnsNamesAndOrder(
                GetColumnNames(), 
                GetColumnOrder(), 
                dgvMain,
                Protection_TC.GetChangeablePropertiesNames);

            SetDGVColumnsSettings();

            AddNewRowsToDGV(objects.Result, dgvMain);
        }

        private void Win6_Protection_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        ////////////////////////////////////////////////////// * DGV settings * ////////////////////////////////////////////////////////////////////////////////////

        private Dictionary<string, string> GetColumnNames()
        {
            //var colNames = new Dictionary<string, string>();
            //foreach (var item in Protection_TC.GetPropertiesNames)
            //{
            //    colNames.Add(item.Key, item.Value);
            //}
            //foreach (var item in Protection.GetPropertiesNames)
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
            //foreach (var item in Protection.GetPropertiesOrder)
            //{
            //    colOrder.Add(item.Key, item.Value);
            //}
            //foreach (var item in Protection_TC.GetPropertiesOrder)
            //{
            //    colOrder.Add(item.Key, item.Value);
            //}
            var colOrder = new Dictionary<string, int>
            {
                { "ChildId", -1 },
                { "ParentId", -1 },
                { "Order", 0 },
                { "Quantity", 1 },
                { "Note", 7 },
                { "Id", 2 },
                { "Name", 3 },
                { "Type", 4 },
                { "Unit", 5 },
                { "Price", 6 },
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

            dgvMain.Columns["Type"].Width = 70;

            // make columns readonly
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.ReadOnly = true;
            }
            // make columns editable
            foreach (var column in Protection_TC.GetChangeablePropertiesNames)
            {
                dgvMain.Columns[column].ReadOnly = false;
            }

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AddNewObjects(List<Protection> newObjs)
        {
            int i = 1;
            int rowCount = dgvMain.Rows.Count;
            foreach (Protection obj in newObjs)
            {
                var sttc = CreateNewObject(obj, rowCount + i);
                newItems.Add(sttc);
                i++;
            }

            AddNewRowsToDGV(newItems, dgvMain);
        }
        public async Task SaveChanges()
        {
            dgvMain.CurrentCell = null;// stops editing in dgvMain and save changes

            // check if all rows have unique combination of childId and Symbol fields
            if (!CheckUniqueColumnsValues(dgvMain, new List<string> { "Id" }))
            {
                MessageBox.Show("Не все строки имеют уникальное поле ID");
                return;
            }

            if (!DetectChanges())
            { MessageBox.Show("no changes in Protection"); return; }// check if rows in dgvMain have difference values in copy columns and add them to changedItems (not newItems, deletedItems)

            // Delete deleted rows from db
            if (deletedItems.Count > 0)
                dbCon.Delete(deletedItems); deletedItems.Clear();
            // save new rows in db
            if (newItems.Count > 0)
                dbCon.Add<Protection_TC, Protection>(newItems); newItems.Clear();
            // save changes in db
            if (changedItems.Count > 0)
                dbCon.Update(changedItems); changedItems.Clear();

            // change values of copy columns to original
            SetCopyColumnsValues(dgvMain, Protection_TC.GetChangeablePropertiesNames);
        }
        private bool DetectChanges()
        {
            bool result = false;
            // check if rows in dgvMain have difference values in copy columns
            foreach (var row in dgvMain.Rows.Cast<DataGridViewRow>())
            {
                if (CheckIfCellValueChanged(row, Protection_TC.GetChangeablePropertiesNames, () =>
                {
                    var newObj = CreateNewObject(row);
                    changedItems.Add(newObj);
                }))
                    result = true;
            }
            return result;
        }

        ///////////////////////////////////////////////////// * Events handlers * /////////////////////////////////////////////////////////////////////////////////
        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            var newForm = new Win7_7_Protection();
            newForm.SetAsAddingForm();
            newForm.ShowDialog();
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count > 0)
            {
                List<DataGridViewRow> rowsToDelete = GetSelectedRows(dgvMain);

                string message;
                if (rowsToDelete.Count == 1)
                    message = "Вы действительно хотите удалить выбранный объект?\n";
                else
                    message = "Вы действительно хотите удалить выбранные объекты?\n";

                DialogResult result = MessageSender.SendQuestionDeliteObjects(message, rowsToDelete, "Id");

                if (result == DialogResult.Yes)
                {
                    foreach (var row in rowsToDelete)
                    {
                        var objToDelite = CreateNewObject(row);
                        deletedItems.Add(objToDelite);
                    }
                    DeleteRows(rowsToDelete, dgvMain);
                }
                dgvMain.Refresh();
            }
        }

        private void dgvMain_CellEndEdit(object sender, DataGridViewCellEventArgs e) // todo - fix problem with selection replacing row (error while remove it)
        {
            var row = dgvMain.Rows[e.RowIndex];
            dgvMain.Enabled = false;
            // if changed cell is in Order column then reorder dgv
            if (e.ColumnIndex == dgvMain.Columns["Order"].Index)
            {
                // check if new cell value is a number
                if (int.TryParse(row.Cells["Order"].Value.ToString(), out int orderValue))
                {
                    if (orderValue == e.RowIndex + 1) { dgvMain.Enabled = true; return; }
                    if (orderValue > 0 && orderValue <= dgvMain.Rows.Count)
                    { ReorderRows(row, orderValue, dgvMain); }
                    else
                    { ReorderRows(row, dgvMain.Rows.Count, dgvMain); } // insert row to the end of dgv

                }
                else
                {
                    MessageBox.Show("Ошибка ввода в поле \"№\"");
                    row.Cells["Order"].Value = e.RowIndex + 1;
                }
            }
            // here we can add other columns to check cell's value change with some conditions
            dgvMain.Enabled = true;
        }
        private Protection_TC CreateNewObject(DataGridViewRow row) 
        {
            return new Protection_TC
            {
                ParentId = _tcId,
                ChildId = int.Parse(row.Cells["Id"].Value.ToString()),

                Order = int.Parse(row.Cells["Order"].Value.ToString()),
                Quantity = double.Parse(row.Cells["Quantity"].Value.ToString()),
                Note = row.Cells["Note"].Value?.ToString(),
            };
        }
        private Protection_TC CreateNewObject(Protection obj, int oreder)
        {
            return new Protection_TC
            {
                ParentId = _tcId,
                ChildId = obj.Id,
                Child = obj,

                Order = oreder,
                Quantity = 0,
            };
        }

    }

}
