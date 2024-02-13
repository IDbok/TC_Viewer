using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.DataProcessing;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms
{
    
    public partial class Win6_Staff_2 : Form
    {
        private DbConnector dbCon = new DbConnector();

        private int _tcId;

        private List<Staff_TC> objList = new List<Staff_TC>();
        private List<Staff> subObjectList = new List<Staff>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<Staff_TC> changedObjs = new List<Staff_TC>();
        private Staff_TC newObj;
        public Win6_Staff_2(int tcId)
        {
            InitializeComponent();
            this._tcId = tcId;
        }

        private void Win6_Staff_2_Load(object sender, EventArgs e)
        {
            dgvMain.Columns.Clear();

            objList = dbCon.GetIntermediateObjectList<Staff_TC, Staff>(_tcId);
            subObjectList = objList.Select(x => x.Child).ToList();

            var bindingList = new BindingList<Staff_TC>(objList);
            dgvMain.DataSource = bindingList;

            var bindingList_sub = new BindingList<Staff>(subObjectList);
            dgvSubElement.DataSource = bindingList_sub;

            WinProcessing.SetTableHeadersNames(Staff.GetPropertiesNames, dgvSubElement);

            //Staff_TC.GetPropertiesNames_old(out var ownNames);
            //WinProcessing.SetTableHeadersNames(ownNames, dgvMain);

            //WinProcessing.SetTableColumnsOrder(Staff.GetPropertiesOrder(), dgvSubElement);

            //Staff_TC.GetPropertiesOrder_old(out var ownOrder);
            //WinProcessing.SetTableColumnsOrder(ownOrder, dgvMain);

            ////set column head height
            //dgvMain.ColumnHeadersHeight = 550;
            //dgvSubElement.ColumnHeadersHeight = 150;

            //// make row size of dgvMain and dgvSubElement conected
            //dgvMain.RowTemplate.Height = 50;
            //dgvSubElement.RowTemplate.Height = 50;

            // make invisible row selector column
            dgvSubElement.RowHeadersVisible = false;

            // авторененос в ячейке и авторасширение строки
            dgvSubElement.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dgvSubElement.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;


            dgvMain.Refresh();
            dgvSubElement.Refresh();
        }
        private void Win6_Staff_2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (newObj != null)
            {
                if (MessageBox.Show("Сохранить новую запись?", "Сохранение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Сохраняю всё что нужно", "Сохранение");
                    //dbCon.AddObject(newObj);
                }
            }
        }

        private void btnAddNewObj_Click(object sender, EventArgs e)
        {
            // new form where loads from db all staff objects
            // user selects one ore more of them by check boxes 
            // and then press "add" button
            // then new rows are added to dgvMain
            // in all new rows symbol are colored in red

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
                    DGVProcessing.DeleteRowsStaff_TC(rowsToDelete, dgvMain, dbCon);
                }

                dgvMain.Refresh();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    }

}
