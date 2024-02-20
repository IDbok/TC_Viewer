using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcDbConnector;
using TcModels.Models.TcContent;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TC_WinForms.WinForms.Work
{
    public partial class AddEditTechOperationForm : Form
    {
        public TechOperationForm TechOperationForm { get; }
        private List<TechOperation> allTO;
        private List<TechTransition> allTP;
        public AddEditTechOperationForm()
        {
            InitializeComponent();
        }

        public AddEditTechOperationForm(TechOperationForm techOperationForm)
        {
            InitializeComponent();
            TechOperationForm = techOperationForm;

            dataGridViewAllTO.CellContentClick += DataGridViewAllTO_CellContentClick;
            dataGridViewTPAll.CellContentClick += DataGridViewTPAll_CellContentClick;

            UpdateTO();
            UpdateLocalTO();
        }

        private void DataGridViewTPAll_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewTPAll.CommitEdit(DataGridViewDataErrorContexts.Commit);
            ClickBoxTPAll();
        }

        private void DataGridViewAllTO_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewAllTO.CommitEdit(DataGridViewDataErrorContexts.Commit);
            ClickBoxAll();
        }

        public void ClickBoxTPAll()
        {
            bool updateTP = false;
            var work = (TechOperationWork)comboBoxTO.SelectedItem;
            var listWork = TechOperationForm.TechOperationWorksList.Single(s => s.Id == work.Id).executionWorks
                .ToList();

            foreach (DataGridViewRow row in dataGridViewTPAll.Rows)
            {
                var idd = (int)row.Cells[0].Value;
                var chech = (bool)row.Cells[1].Value;

                var techTransit = listWork.SingleOrDefault(s => s.techTransition?.Id == idd);
                if (chech)
                {
                    if (techTransit == null)
                    {
                        var tech = allTP.Single(s => s.Id == idd);
                        TechOperationForm.AddTechTransition(tech, work);
                        updateTP = true;
                    }
                    else
                    {
                        if (techTransit.Delete == true)
                        {
                            techTransit.Delete = false;
                            updateTP = true;
                        }
                    }
                }
                else
                {
                    if (techTransit != null && techTransit.Delete == false)
                    {
                        TechOperationForm.DeleteTechTransit(techTransit, work);
                        updateTP = true;
                    }
                }
            }

            if (updateTP)
            {
                UpdateGridLocalTP();
                TechOperationForm.UpdateGrid();
            }
        }

        public void ClickBoxAll()
        {
            bool updateTO = false;
            foreach (DataGridViewRow? row in dataGridViewAllTO.Rows)
            {
                var idd = (int)row.Cells[0].Value;
                var chech = (bool)row.Cells[1].Value;

                TechOperationWork TechOperat = TechOperationForm.TechOperationWorksList.SingleOrDefault(s => s.techOperation.Id == idd);
                if (chech)
                {
                    if (TechOperat == null)
                    {
                        TechOperation sil = allTO.Single(s => s.Id == idd);
                        TechOperationForm.AddTechOperation(sil);
                        updateTO = true;
                    }
                    else
                    {
                        if (TechOperat.Delete == true)
                        {
                            TechOperat.Delete = false;
                            updateTO = true;
                        }
                    }
                }
                else
                {
                    if (TechOperat != null && TechOperat.Delete == false)
                    {
                        TechOperationForm.DeleteTechOperation(TechOperat);
                        updateTO = true;
                    }
                }
            }

            if (updateTO)
            {
                UpdateLocalTO();
                TechOperationForm.UpdateGrid();
            }

        }

        public void UpdateTO()
        {
            dataGridViewAllTO.Rows.Clear();

            var context = TechOperationForm.context;
            allTO = context.TechOperations.ToList();
            var list = TechOperationForm.TechOperationWorksList;

            foreach (TechOperation techOperation in allTO)
            {
                List<object> listItem = new List<object>();

                listItem.Add(techOperation.Id);

                if (list.SingleOrDefault(s => s.Id == techOperation.Id) != null)
                {
                    listItem.Add(true);
                }
                else
                {
                    listItem.Add(false);
                }

                listItem.Add(techOperation.Name);

                dataGridViewAllTO.Rows.Add(listItem.ToArray());
            }


        }


        public void UpdateLocalTO()
        {
            dataGridViewTO.Rows.Clear();

            List<TechOperationWork> list = TechOperationForm.TechOperationWorksList.Where(w => w.Delete == false).ToList();
            foreach (TechOperationWork techOperationWork in list)
            {
                List<object> listItem = new List<object>();
                listItem.Add(techOperationWork.Id);
                listItem.Add(techOperationWork.techOperation.Name);
                dataGridViewTO.Rows.Add(listItem.ToArray());
            }

            List<TechOperationWork> list2 = new List<TechOperationWork>(list);
            comboBoxTO.DataSource = list2;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                var select = dataGridViewTO.SelectedRows;
                if (select.Count > 0)
                {
                    var id = (int)select[0].Cells[0].Value;

                    foreach (TechOperationWork item in comboBoxTO.Items)
                    {
                        if (item.Id == id)
                        {
                            comboBoxTO.SelectedItem = item;
                        }
                    }
                }
            }
        }

        private void comboBoxTO_SelectedIndexChanged(object sender, EventArgs e)
        {
            var work = (TechOperationWork)comboBoxTO.SelectedItem;


            UpdateGridAllTP();
            UpdateGridLocalTP();
        }


        public void UpdateGridAllTP()
        {
            dataGridViewTPAll.Rows.Clear();

            var work = (TechOperationWork)comboBoxTO.SelectedItem;

            var context = TechOperationForm.context;

            allTP = context.TechTransitions.ToList();
            var list = TechOperationForm.TechOperationWorksList.Single(s=>s.Id==work.Id).executionWorks.ToList();

            foreach (TechTransition techTransition in allTP)
            {
                List<object> listItem = new List<object>();

                listItem.Add(techTransition.Id);
                if (list.SingleOrDefault(s => s.techTransitionId == techTransition.Id) != null)
                {
                    listItem.Add(true);
                }
                else
                {
                    listItem.Add(false);
                }


                listItem.Add(techTransition.Name);
                listItem.Add(techTransition.TimeExecution);

                dataGridViewTPAll.Rows.Add(listItem.ToArray());
            }
        }

        public void UpdateGridLocalTP()
        {
            dataGridViewTPLocal.Rows.Clear();
            var work = (TechOperationWork)comboBoxTO.SelectedItem;
          var   LocalTP = TechOperationForm.TechOperationWorksList.Single(s => s.Id == work.Id).executionWorks.ToList();

            foreach (ExecutionWork executionWork in LocalTP)
            {
                List<object> listItem = new List<object>();
                listItem.Add(executionWork.Id);
                listItem.Add(executionWork.techTransition?.Name);
                listItem.Add(executionWork.techTransition?.TimeExecution);
                dataGridViewTPLocal.Rows.Add(listItem.ToArray());
            }


            //dataGridViewTO.Rows.Clear();

            //List<TechOperationWork> list = TechOperationForm.TechOperationWorksList.Where(w => w.Delete == false).ToList();
            //foreach (TechOperationWork techOperationWork in list)
            //{
            //    List<object> listItem = new List<object>();
            //    listItem.Add(techOperationWork.Id);
            //    listItem.Add(techOperationWork.techOperation.Name);
            //    dataGridViewTO.Rows.Add(listItem.ToArray());
            //}

            //List<TechOperationWork> list2 = new List<TechOperationWork>(list);
            //comboBoxTO.DataSource = list2;
        }



    }
}
