using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
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

    public partial class Win6_Staff_3 : Form
    {
        private DbConnector dbCon = new DbConnector();

        private int _tcId;

        private List<Staff_TC> objList = new List<Staff_TC>();
        private List<Staff> subObjectList = new List<Staff>();
        private List<int> changedItemId = new List<int>();
        private List<int> deletedItemId = new List<int>();
        private List<Staff_TC> changedObjs = new List<Staff_TC>();
        private Staff_TC newObj;
        public Win6_Staff_3(int tcId)
        {
            InitializeComponent();
            this._tcId = tcId;
        }

        private void Win6_Staff_3_Load(object sender, EventArgs e)
        {
            dgvMain.Columns.Clear();

            objList = dbCon.GetIntermediateObjectList<Staff_TC, Staff>(_tcId);

            List<IntermediateUnit> intermediateUnits = new List<IntermediateUnit>();

            intermediateUnits = objList.Select(x => new IntermediateUnit { Staff_TC = x }).ToList();

            var bindingList = new BindingList<IntermediateUnit>(intermediateUnits);
            dgvMain.DataSource = bindingList;


            //WinProcessing.SetTableHeadersNames(Staff.GetPropertiesNames(), dgvSubElement);

            //Staff_TC.GetPropertiesNames(out var ownNames);
            //WinProcessing.SetTableHeadersNames(ownNames, dgvMain);

            //WinProcessing.SetTableColumnsOrder(Staff.GetPropertiesOrder(), dgvSubElement);

            //Staff_TC.GetPropertiesOrder(out var ownOrder);
            //WinProcessing.SetTableColumnsOrder(ownOrder, dgvMain);

            ////set column head height
            //dgvMain.ColumnHeadersHeight = 550;
            //dgvSubElement.ColumnHeadersHeight = 150;

            //// make row size of dgvMain and dgvSubElement conected
            //dgvMain.RowTemplate.Height = 50;
            //dgvSubElement.RowTemplate.Height = 50;

            // make invisible row selector column
            //dgvMain.RowHeadersVisible = false;

            // авторененос в ячейке и авторасширение строки
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;


            dgvMain.Refresh();
        }
        private void Win6_Staff_3_FormClosing(object sender, FormClosingEventArgs e)
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
    public class IntermediateUnit
    {
        public Staff_TC Staff_TC { get; set; }
        public int Order => Staff_TC.Order;
        public string Symbol => Staff_TC.Symbol;
        public int ParentId => Staff_TC.ParentId;
        public int ChildId => Staff_TC.ChildId;
        public string Name => Staff_TC.Child.Name;
        public string Type => Staff_TC.Child.Type;
        public string Functions => Staff_TC.Child.Functions;
        public string? CombineResponsibility => Staff_TC.Child.CombineResponsibility;
        public string Qualification => Staff_TC.Child.Qualification;
        public string? Comment => Staff_TC.Child.Comment;



    }
}
