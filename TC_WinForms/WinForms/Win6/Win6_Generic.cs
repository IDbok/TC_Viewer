using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win6_Generic<T,C> : Form 
        where T : class, IIntermediateTable<TechnologicalCard,C>
        where C : class, IDGViewable
    {
        private DbConnector dbCon = new DbConnector();

        private int _tcId;

        private List<T> objList = new List<T>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<T> changedObjs = new List<T>();
        private T newObj;
        private List<string> requiredCols = new List<string>();
        public Win6_Generic(int tcId)
        {
            InitializeComponent();
            this._tcId = tcId;
        }

        private void Win6_Generic_Load(object sender, EventArgs e)
        {
            dgvMain.Columns.Clear();

            objList = dbCon.GetIntermediateObjectList<T,C>(_tcId);

            //if (typeof(C) == typeof(Staff))
            //    objList = dbCon.GetIntermediateObjectList<T, Staff>(_tcId);
            //var displayList = objList.Select(i => new { i.id, ChildId = i.Child.id, ChildName = i.Child.Name, i.ParentId }).ToList();


            var bindingList = new BindingList<T>(objList);
            dgvMain.DataSource = objList;//bindingList;

            dgvMain.AutoGenerateColumns = true;

            dgvMain.Refresh();
            // set columns names
            //WinProcessing.SetTableHeadersNames(
            //    (Dictionary<string, string>)typeof(C).GetMethod("GetPropertiesNames", BindingFlags.Static | BindingFlags.Public).Invoke(null, null), dgvMain);
            //// set columns order and visibility
            //WinProcessing.SetTableColumnsOrder(
            //    (Dictionary<string, int>)typeof(C).GetMethod("GetPropertiesOrder", BindingFlags.Static | BindingFlags.Public).Invoke(null,null), dgvMain);

            //requiredCols = Machine.GetPropertiesRequired();
        }
        private void Win6_Generic_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (newObj != null)
            {
                if (MessageBox.Show("Сохранить новую запись?", "Сохранение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Сохраняю всё что нужно в Machine ", "Сохранение");
                    //dbCon.AddObject(newObj);
                }
            }
        }
        private void AccessInitialization(int accessLevel)
        {
            // todo - make accessibility for all buttons
            if (accessLevel == 0) // viewer
            {
                // hide all buttons
                pnlControlBtns.Visible = false;
                // pnlNavigationBtns.Visible = false;
                btnAddNewObj.Enabled = false;
                btnUpdateObj.Enabled = false;
                btnDeleteObj.Enabled = false;
            }
            else if (accessLevel == 1) // TC editor
            {

            }
            else if (accessLevel == 2) // Progect editor
            {
            }
        }

        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            //// create new object and add it to newCard if newCard is null
            //if (newObj != null) // todo - add check for all required fields (ex. Type can be only as "Ремонтная", "Монтажная", "ТТ")

            //    if (!CheckAllRequeredFields(newObj, requiredCols))
            //    {
            //        MessageBox.Show($"Внесите обязательные для объекта с id({newObj.Id}) поля (Артикул, Тип карты, Сеть, кВ)");
            //        ColorizeEmptyRequiredCells();
            //        return;
            //    };

            //newObj = DataProcessing.DataProcessing.addNewObject<T>();

            //objList.Insert(0, newObj);

            //// scroll to new row and refresh dgvMain
            //dgvMain.FirstDisplayedScrollingRowIndex = 0;
            //dgvMain.Refresh();
        }

        private void btnDeleteObj_Click(object sender, EventArgs e)
        {
            if (dgvMain.SelectedRows.Count > 0)
            {
                List<DataGridViewRow> rowsToDelete = DGVProcessing.GetSelectedRows(dgvMain);

                string message = null;
                if (rowsToDelete.Count == 1)
                    message = "Вы действительно хотите удалить выбранные объект?\n"; 
                else
                    message = "Вы действительно хотите удалить выбранные объекты?\n";

                DialogResult result = MessageSender.SendQuestionDeliteObjects(message, rowsToDelete, "Id");


                if (result == DialogResult.Yes)
                {
                    //DataGridProcessing.DeleteRowsById(rowsToDelete, dgvMain, dbCon);
                }

                dgvMain.Refresh();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void ColorizeEmptyRequiredCells() // todo - change call collore after value changed to non empty
        {
            DataGridViewRow row = dgvMain.Rows[0];
            var colNames = Machine.GetPropertiesRequired();
            foreach (var colName in colNames)
            {
                // get collumn index by name
                var colIndex = dgvMain.Columns[colName].Index;

                DGVProcessing.ColorizeCell(dgvMain, colIndex, row.Index, "Red");
            }


        }

        private bool CheckAllRequeredFields<T>(T obj, List<string> reqCols)
        {
            foreach (var colName in reqCols)
            {
                if (obj.GetType().GetProperty(colName).GetValue(obj) == "")
                    return false;
            }
            return true;
        }
    }
}
