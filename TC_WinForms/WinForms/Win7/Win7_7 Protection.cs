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
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win7_7_Protection : Form
    {
        private bool isAddingForm = false;
        private Button btnAddSelected;
        private Button btnCancel;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }

        private DbConnector dbCon = new DbConnector();

        private List<Protection> objList = new List<Protection>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<Protection> changedObjs = new List<Protection>();
        private Protection newObj;
        private List<string> requiredCols = new List<string>();
        public Win7_7_Protection(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }
        public Win7_7_Protection()
        {
            InitializeComponent();
        }

        private void Win7_7_Protection_Load(object sender, EventArgs e)
        {
            dgvMain.Columns.Clear();

            objList = dbCon.GetObjectList<Protection>();

            var bindingList = new BindingList<Protection>(objList);
            dgvMain.DataSource = bindingList;

            // set columns names
            WinProcessing.SetTableHeadersNames(Protection.GetPropertiesNames(), dgvMain);
            // set columns order and visibility
            WinProcessing.SetTableColumnsOrder(Protection.GetPropertiesOrder(), dgvMain);

            requiredCols = Protection.GetPropertiesRequired();

            if (isAddingForm)
            {
                //isAddingFormSetControls();
                WinProcessing.SetAddingFormControls(pnlControlBtns, dgvMain, out btnAddSelected, out btnCancel);
                SetAddingFormEvents();
            }
        }
        private void Win7_7_Protection_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (newObj != null)
            {
                if (MessageBox.Show("Сохранить новую запись?", "Сохранение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Сохраняю всё что нужно в Protection ", "Сохранение");
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

            // create new object and add it to newCard if newCard is null
            if (newObj != null) // todo - add check for all required fields (ex. Type can be only as "Ремонтная", "Монтажная", "ТТ")

                if (!CheckAllRequeredFields(newObj, requiredCols))
                {
                    MessageBox.Show($"Внесите обязательные для объекта с id({newObj.Id}) поля (Артикул, Тип карты, Сеть, кВ)");
                    ColorizeEmptyRequiredCells();
                    return;
                };

            newObj = DataProcessing.DataProcessing.AddNewObject<Protection>();

            objList.Insert(0, newObj);

            // scroll to new row and refresh dgvMain
            dgvMain.FirstDisplayedScrollingRowIndex = 0;
            dgvMain.Refresh();
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
                    DGVProcessing.DeleteRowsById<Protection>(rowsToDelete, dgvMain, dbCon);
                }

                dgvMain.Refresh();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void ColorizeEmptyRequiredCells() // todo - change call collore after value changed to non empty
        {
            DataGridViewRow row = dgvMain.Rows[0];
            var colNames = Protection.GetPropertiesRequired();
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

        /////////////////////////////////////////// * isAddingFormMethods and Events * ///////////////////////////////////////////

        void SetAddingFormEvents()
        {
            btnAddSelected.Click += BtnAddSelected_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        void BtnAddSelected_Click(object sender, EventArgs e)
        {
            // get selected rows
            var selectedRows = dgvMain.Rows.Cast<DataGridViewRow>().Where(r => Convert.ToBoolean(r.Cells["Selected"].Value) == true).ToList();
            if (selectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строки для добавления");
                return;
            }
            // get selected objects
            var selectedObjs = selectedRows.Select(r => r.DataBoundItem as Protection).ToList();
            // find opened form
            var tcEditor = Application.OpenForms.OfType<Win6_Protection>().FirstOrDefault();

            tcEditor.AddNewObjects(selectedObjs);

            // close form
            this.Close();
        }
        void BtnCancel_Click(object sender, EventArgs e)
        {
            // close form
            this.Close();
        }
    }
}
