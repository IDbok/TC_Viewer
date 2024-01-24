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
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    public partial class Win7_3_Staff : Form
    {
        private DbConnector dbCon = new DbConnector();

        private List<Staff> objList = new List<Staff>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<Staff> changedObjs = new List<Staff>();
        private Staff newObj;
        public Win7_3_Staff(int accessLevel)
        {
            InitializeComponent();
            AccessInitialization(accessLevel);
        }


        private void Win7_3_Staff_Load(object sender, EventArgs e)
        {
            dgvMain.Columns.Clear();

            objList = dbCon.GetObjectList<Staff>();

            var bindingList = new BindingList<Staff>(objList);
            dgvMain.DataSource = bindingList;

            // set columns names
            WinProcessing.SetTableHeadersNames(Staff.GetPropertiesNames(), dgvMain);

            // set columns order and visibility
            WinProcessing.SetTableColumnsOrder(Staff.GetPropertiesOrder(), dgvMain);
        }

        private void AccessInitialization(int accessLevel)
        {
            // todo - reverse accessibility
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

        private void Win7_3_Staff_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (newObj != null)
            {
                if (MessageBox.Show("Сохранить новую запись?", "Сохранение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    //dbCon.AddObject(newObj);
                }
            }
        }

        private void btnAddNewObj_Click(object sender, EventArgs e)
        {

            // create new object and add it to newCard if newCard is null
            if (newObj != null) // todo - add check for all required fields (ex. Type can be only as "Ремонтная", "Монтажная", "ТТ")
                if (newObj.GetType().GetProperty("Name").GetValue(newObj) == ""
                  || newObj.GetType().GetProperty("Type").GetValue(newObj) == ""
                  || newObj.GetType().GetProperty("Functions").GetValue(newObj) == ""
                  || newObj.GetType().GetProperty("Qualification").GetValue(newObj) == "")
                {
                    MessageBox.Show($"Внесите обязательные для объекта с id({newObj.Id}) поля (Артикул, Тип карты, Сеть, кВ)");
                    ColorizeEmptyRequiredCells();
                    return;
                };
            newObj = DataProcessing.DataProcessing.addNewStaff();

            objList.Insert(0, newObj);

            // scroll to new row and refresh dgvMain
            dgvMain.FirstDisplayedScrollingRowIndex = 0;
            dgvMain.Refresh();
        }

        private void ColorizeEmptyRequiredCells() // todo - change call collore after value changed to non empty
        {
            DataGridViewRow row = dgvMain.Rows[0];
            var colNames = Staff.GetPropertiesRequired();
            foreach (var colName in colNames)
            {
                // get collumn index by name
                var colIndex = dgvMain.Columns[colName].Index;

                DataGridProcessing.ColorizeCell(dgvMain, colIndex, row.Index, "Red");
            }

            
        }
    }
}
