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
using Component = TcModels.Models.TcContent.Component;

namespace TC_WinForms.WinForms
{
    public partial class Win7_4_Component : Form
    {
        private bool isAddingForm = false;
        public void SetAsAddingForm()
        {
            isAddingForm = true;
        }
        private DbConnector dbCon = new DbConnector();

        private List<Component> objList = new List<Component>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<Component> changedObjs = new List<Component>();
        private Component newObj;
        private List<string> requiredCols = new List<string>();
        public Win7_4_Component(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }
        public Win7_4_Component()
        {
            InitializeComponent();
        }

        private void Win7_4_Component_Load(object sender, EventArgs e)
        {
            dgvMain.Columns.Clear();

            objList = dbCon.GetObjectList<Component>();

            var bindingList = new BindingList<Component>(objList);
            dgvMain.DataSource = bindingList;

            // set columns names
            WinProcessing.SetTableHeadersNames(Component.GetPropertiesNames, dgvMain);
            // set columns order and visibility
            WinProcessing.SetTableColumnsOrder(Component.GetPropertiesOrder, dgvMain);

            requiredCols = Component.GetPropertiesRequired;

            if (isAddingForm)
            {
                isAddingFormSetControls();
            }
        }
        private void Win7_4_Component_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (newObj != null)
            {
                if (MessageBox.Show("Сохранить новую запись?", "Сохранение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Сохраняю всё что нужно в Component ", "Сохранение");
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

                if (newObj.GetType().GetProperty(requiredCols[0]).GetValue(newObj) == ""
                  || newObj.GetType().GetProperty(requiredCols[1]).GetValue(newObj) == ""
                  || newObj.GetType().GetProperty(requiredCols[2]).GetValue(newObj) == ""
                  || newObj.GetType().GetProperty(requiredCols[3]).GetValue(newObj) == "")
                {
                    MessageBox.Show($"Внесите обязательные для объекта с id({newObj.Id}) поля (Артикул, Тип карты, Сеть, кВ)");
                    ColorizeEmptyRequiredCells();
                    return;
                };

            newObj = DataProcessing.DataProcessing.addNewObject<Component>();

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
                    DGVProcessing.DeleteRowsById<Component>(rowsToDelete, dgvMain, dbCon);
                }

                dgvMain.Refresh();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void ColorizeEmptyRequiredCells() // todo - change call collore after value changed to non empty
        {
            DataGridViewRow row = dgvMain.Rows[0];
            var colNames = Component.GetPropertiesRequired;
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

        private void isAddingFormSetControls()
        {
            // make all collumns readonly
            foreach (DataGridViewColumn c in dgvMain.Columns)
            {
                c.ReadOnly = true;
            }
            // add checkbox in row header
            var col = new DataGridViewCheckBoxColumn();
            col.Name = "Selected";
            col.HeaderText = "";
            col.Width = 30;
            col.ReadOnly = false;
            dgvMain.Columns.Insert(0, col);

            dgvMain.RowHeadersVisible = false;

            // hide buttons for adding, updating and deleting
            foreach (var btn in pnlControlBtns.Controls)
            {
                if (btn is Button)
                {
                    (btn as Button).Visible = false;
                }
            }

            // add button for adding selected rows to TC
            var btnAddSelected = new Button();
            btnAddSelected.Text = "Добавить выбранные";
            btnAddSelected.Click += BtnAddSelected_Click;
            btnAddSelected.Dock = DockStyle.Right;
            btnAddSelected.Width = 150;
            pnlControlBtns.Controls.Add(btnAddSelected);

            // add button Cancel
            var btnCancel = new Button();
            btnCancel.Text = "Отмена";
            // btnCancel.Click += BtnCancel_Click;
            btnCancel.Dock = DockStyle.Right;
            btnCancel.Width = 150;
            pnlControlBtns.Controls.Add(btnCancel);
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
            var selectedObjs = selectedRows.Select(r => r.DataBoundItem as Staff).ToList();
            // find opened form
            var tcEditor = Application.OpenForms.OfType<Win6_Staff>().FirstOrDefault();

            tcEditor.AddNewObjects(selectedObjs);

            // close form
            this.Close();
        }
    }
}
