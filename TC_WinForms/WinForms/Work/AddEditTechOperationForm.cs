﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using TC_WinForms.DataProcessing;
using TcDbConnector;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using Component = TcModels.Models.TcContent.Component;
using Machine = TcModels.Models.TcContent.Machine;

namespace TC_WinForms.WinForms.Work
{
    public partial class AddEditTechOperationForm : Form
    {
        public TechOperationForm TechOperationForm { get; }
        private List<TechOperation> allTO;
        private List<TechTransition> allTP;
        //private List<Staff_TC> AllStaff;
        private List<ExecutionWork> listExecutionWork;

        public AddEditTechOperationForm()
        {
            InitializeComponent();
        }

        public AddEditTechOperationForm(TechOperationForm techOperationForm)
        {
            InitializeComponent();
            TechOperationForm = techOperationForm;

            // dataGridViewAllTO.CellContentClick += DataGridViewAllTO_CellContentClick;
            dataGridViewAllTO.CellClick += DataGridViewAllTO_CellClick;

            dataGridViewTO.CellClick += DataGridViewTO_CellClick;

            dataGridViewTPAll.CellContentClick += DataGridViewTPAll_CellContentClick;
            dataGridViewTPAll.CellClick += DataGridViewTPAll_CellClick;
            dataGridViewTPLocal.CellClick += DataGridViewTPLocal_CellClick;
            dataGridViewTPLocal.CellEndEdit += DataGridViewTPLocal_CellEndEdit;

            dataGridViewStaff.CellContentClick += DataGridViewStaff_CellContentClick;
            dataGridViewStaff.CellClick += DataGridViewStaff_CellClick;
            dataGridViewStaff.CellEndEdit += DataGridViewStaff_CellEndEdit;

            dataGridViewStaffAll.CellClick += DataGridViewStaffAll_CellClick;

            dataGridViewComponentAll.CellClick += DataGridViewComponentAll_CellClick;
            dataGridViewComponentLocal.CellClick += DataGridViewComponentLocal_CellClick;
            dataGridViewComponentLocal.CellEndEdit += DataGridViewComponentLocal_CellEndEdit;

            dataGridViewInstumentAll.CellClick += DataGridViewInstumentAll_CellClick;
            dataGridViewInstrumentLocal.CellClick += DataGridViewInstrumentLocal_CellClick;
            dataGridViewInstrumentLocal.CellEndEdit += DataGridViewInstrumentLocal_CellEndEdit;

            dataGridViewAllSZ.CellClick += DataGridViewAllSZ_CellClick;
            dataGridViewLocalSZ.CellClick += DataGridViewLocalSZ_CellClick;


            dataGridViewEtap.CellEndEdit += DataGridViewEtap_CellEndEdit;
            dataGridViewEtap.CellContentClick += DataGridViewEtap_CellContentClick;

            dataGridViewMeha.CellContentClick += DataGridViewMeha_CellContentClick;

            var context = techOperationForm.context;

            dataGridViewPovtor.CellContentClick += DataGridViewPovtor_CellContentClick;


            comboBoxFiltrCategor.SelectedIndexChanged += ComboBoxFiltrCategor_SelectedIndexChanged;
            comboBoxFilterComponent.SelectedIndexChanged += ComboBoxFilterComponent_SelectedIndexChanged;

            textBoxPoiskTo.TextChanged += TextBoxPoiskTo_TextChanged;
            textBoxPoiskComponent.TextChanged += TextBoxPoiskComponent_TextChanged;
            textBoxPoiskTP.TextChanged += TextBoxPoiskTP_TextChanged;
            textBoxPoiskSZ.TextChanged += TextBoxPoiskSZ_TextChanged;
            textBoxPoiskMach.TextChanged += TextBoxPoiskMach_TextChanged;

            var Even = new DGVEvents();
            Even.EventsObj = this;
            Even.Table = 1;
            Even.AddGragDropEvents(dataGridViewTO);

            Even = new DGVEvents();
            Even.EventsObj = this;
            Even.Table = 2;
            Even.AddGragDropEvents(dataGridViewTPLocal);



            UpdateTO();
            UpdateLocalTO();
        }
        


        #region TO



        public void ClickBoxAll()
        {
            //bool updateTO = false;
            //foreach (DataGridViewRow? row in dataGridViewAllTO.Rows)
            //{
            //    var idd = (int)row.Cells[0].Value;
            //    var chech = (bool)row.Cells[1].Value;

            //    TechOperationWork TechOperat =
            //        TechOperationForm.TechOperationWorksList.SingleOrDefault(s => s.techOperation.Id == idd);
            //    if (chech)
            //    {
            //        if (TechOperat == null)
            //        {
            //            TechOperation sil = allTO.Single(s => s.Id == idd);
            //            TechOperationForm.AddTechOperation(sil);
            //            updateTO = true;
            //        }
            //        else
            //        {
            //            if (TechOperat.Delete == true)
            //            {
            //                TechOperat.Delete = false;
            //                updateTO = true;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (TechOperat != null && TechOperat.Delete == false)
            //        {
            //            TechOperationForm.DeleteTechOperation(TechOperat);
            //            updateTO = true;
            //        }
            //    }
            //}

            //if (updateTO)
            //{
            //    UpdateLocalTO();
            //    TechOperationForm.UpdateGrid();
            //}

        }


        private void DataGridViewAllTO_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewAllTO.CommitEdit(DataGridViewDataErrorContexts.Commit);
            ClickBoxAll();
        }

        private void TextBoxPoiskTo_TextChanged(object? sender, EventArgs e)
        {
            UpdateTO();
        }


        private void DataGridViewAllTO_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var IddGuid = (TechOperation)dataGridViewAllTO.Rows[e.RowIndex].Cells[0].Value;

                TechOperationForm.AddTechOperation(IddGuid);
                UpdateLocalTO();
                TechOperationForm.UpdateGrid();
            }
        }


        private void DataGridViewTO_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var IddGuid = (TechOperationWork)dataGridViewTO.Rows[e.RowIndex].Cells[0].Value;

                TechOperationForm.DeleteTechOperation(IddGuid);
                UpdateLocalTO();
                TechOperationForm.UpdateGrid();
            }
        }

        public void UpdateTO()
        {
            var offScroll = dataGridViewAllTO.FirstDisplayedScrollingRowIndex;
            dataGridViewAllTO.Rows.Clear();

            var context = TechOperationForm.context;
            allTO = context.TechOperations.ToList();
            var list = TechOperationForm.TechOperationWorksList;


            foreach (TechOperation techOperation in allTO)
            {
                if (textBoxPoiskTo.Text != "" &&
                    techOperation.Name.ToLower().IndexOf(textBoxPoiskTo.Text.ToLower()) == -1)
                {
                    continue;
                }


                List<object> listItem = new List<object>();

                listItem.Add(techOperation);

                listItem.Add("Добавить");

                //if (list.SingleOrDefault(s => s.Id == techOperation.Id) != null)
                //{
                //    listItem.Add(true);
                //}
                //else
                //{
                //    listItem.Add(false);
                //}

                listItem.Add(techOperation.Name);

                dataGridViewAllTO.Rows.Add(listItem.ToArray());
            }

            try
            {
                dataGridViewAllTO.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }


        }


        public void UpdateLocalTO()
        {
            var offScroll = dataGridViewTO.FirstDisplayedScrollingRowIndex;
            dataGridViewTO.Rows.Clear();

            List<TechOperationWork> list = TechOperationForm.TechOperationWorksList.Where(w => w.Delete == false)
                .ToList();
            foreach (TechOperationWork techOperationWork in list)
            {
                List<object> listItem = new List<object>();
                listItem.Add(techOperationWork);
                listItem.Add("Удалить");
                listItem.Add(techOperationWork.techOperation.Name);
                dataGridViewTO.Rows.Add(listItem.ToArray());
            }

            List<TechOperationWork> list2 = new List<TechOperationWork>(list);
            comboBoxTO.DataSource = list2;
            comboBoxTO2.DataSource = list2;
            comboBoxTO3.DataSource = list2;

            try
            {
                dataGridViewTO.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }


        private void comboBoxTO_SelectedIndexChanged(object sender, EventArgs e)
        {
            var work = (TechOperationWork)comboBoxTO.SelectedItem;


            UpdateGridAllTP();
            UpdateGridLocalTP();
        }


        #endregion

        #region TP

        private void DataGridViewTPLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var idd = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
                var work = (TechOperationWork)comboBoxTO.SelectedItem;
                // TechOperationForm.TechOperationWorksList.Single(s => s.Id == work.Id).executionWorks.Single(s => s.IdGuid == idd).
                var wor = work.executionWorks.SingleOrDefault(s => s.IdGuid == idd);
                if (wor != null)
                {
                    var result = new double();
                    if (double.TryParse(gg, out result))
                    {
                        wor.Value = result;
                        TechOperationForm.UpdateGrid();
                    }
                }
            }

            if (e.ColumnIndex == 4)
            {
                var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var idd = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
                var work = (TechOperationWork)comboBoxTO.SelectedItem;
                // TechOperationForm.TechOperationWorksList.Single(s => s.Id == work.Id).executionWorks.Single(s => s.IdGuid == idd).
                var wor = work.executionWorks.SingleOrDefault(s => s.IdGuid == idd);
                if (wor != null)
                {
                    wor.Comments = gg;
                    TechOperationForm.UpdateGrid();
                }
            }
        }


        private void DataGridViewTPAll_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            //dataGridViewTPAll.CommitEdit(DataGridViewDataErrorContexts.Commit);
            //ClickBoxTPAll();
        }

        private void DataGridViewTPLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var work = (TechOperationWork)comboBoxTO.SelectedItem;
                var IddGuid = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
                //var tech = allTP.Single(s => s.Id == Idd);
                //TechOperationForm.AddTechTransition(tech, work);


                TechOperationForm.DeleteTechTransit(IddGuid, work);
                UpdateGridLocalTP();
                TechOperationForm.UpdateGrid();
            }
        }

        private void DataGridViewTPAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var work = (TechOperationWork)comboBoxTO.SelectedItem;
                var Idd = (TechTransition)dataGridViewTPAll.Rows[e.RowIndex].Cells[0].Value;
                TechOperationForm.AddTechTransition(Idd, work);
                UpdateGridLocalTP();
                TechOperationForm.UpdateGrid();
            }
        }


        private void TextBoxPoiskTP_TextChanged(object? sender, EventArgs e)
        {
            UpdateGridAllTP();
        }
        public void UpdateGridAllTP()
        {
            var offScroll = dataGridViewTPAll.FirstDisplayedScrollingRowIndex;
            dataGridViewTPAll.Rows.Clear();

            var work = (TechOperationWork)comboBoxTO.SelectedItem;

            var context = TechOperationForm.context;

            allTP = context.TechTransitions.ToList();
            var list = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.ToList();


            TechTransition povtor = new TechTransition();
            povtor.Name = "Повторить";
            List<object> listItem1 = new List<object>();
            listItem1.Add(povtor);
            listItem1.Add("Добавить");
            listItem1.Add(povtor.Name);
            dataGridViewTPAll.Rows.Add(listItem1.ToArray());

            foreach (TechTransition techTransition in allTP)
            {
                if (textBoxPoiskTP.Text != "" &&
                    techTransition.Name.ToLower().IndexOf(textBoxPoiskTP.Text.ToLower()) == -1)
                {
                    continue;
                }

                List<object> listItem = new List<object>();

                listItem.Add(techTransition);
                //if (list.SingleOrDefault(s => s.techTransitionId == techTransition.Id) != null)
                //{
                //    listItem.Add(true);
                //}
                //else
                //{
                //    listItem.Add(false);
                //}
                listItem.Add("Добавить");


                listItem.Add(techTransition.Name);
                listItem.Add(techTransition.TimeExecution);

                dataGridViewTPAll.Rows.Add(listItem.ToArray());
            }

            try
            {
                dataGridViewTPAll.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }



        public void UpdateGridLocalTP()
        {
            var offScroll = dataGridViewTPLocal.FirstDisplayedScrollingRowIndex;
            dataGridViewTPLocal.Rows.Clear();
            var work = (TechOperationWork)comboBoxTO.SelectedItem;
            var LocalTP = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.Where(w => w.Delete == false).ToList();

            foreach (ExecutionWork executionWork in LocalTP)
            {
                List<object> listItem = new List<object>();
                listItem.Add(executionWork.IdGuid);
                listItem.Add("Удалить");
                if (executionWork.Repeat)
                {
                    listItem.Add("Повторить");
                    listItem.Add("");
                }
                else
                {
                    listItem.Add(executionWork.techTransition?.Name);
                    listItem.Add(executionWork.techTransition?.TimeExecution);
                }

                listItem.Add(executionWork.Comments);

                dataGridViewTPLocal.Rows.Add(listItem.ToArray());
            }

            listExecutionWork = new List<ExecutionWork>(LocalTP);

            if (listExecutionWork.Count == 0)
            {
                comboBoxStaff.DataSource = null;
                comboBoxSZ.DataSource = null;
            }
            else
            {
                comboBoxStaff.DataSource = listExecutionWork;
                comboBoxSZ.DataSource = listExecutionWork;
            }

            try
            {
                dataGridViewTPLocal.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
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




        #endregion


        #region Staff
        private void DataGridViewStaff_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewStaff.CommitEdit(DataGridViewDataErrorContexts.Commit);
            ClickDataGridViewStaff();
        }


        public void ClickDataGridViewStaff()
        {
            bool updateTO = false;


            var AllStaff = TechOperationForm.TehCarta.Staff_TCs.ToList();
            var ExecutionWorkBox = (ExecutionWork)comboBoxStaff.SelectedItem;
            var work = (TechOperationWork)comboBoxTO.SelectedItem;

            var LocalTP = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.Single(s => s.IdGuid == ExecutionWorkBox.IdGuid);

            foreach (DataGridViewRow? row in dataGridViewStaff.Rows)
            {
                var idd = (Staff_TC)row.Cells[0].Value;
                var chech = (bool)row.Cells[2].Value;

                var staf = LocalTP.Staffs.SingleOrDefault(s => s == idd);
                if (chech)
                {
                    if (staf == null)
                    {
                        var sta = AllStaff.SingleOrDefault(s => s == idd);
                        LocalTP.Staffs.Add(sta);
                        updateTO = true;
                    }
                }
                else
                {
                    if (staf != null)
                    {
                        var sta = LocalTP.Staffs.Remove(staf);
                        updateTO = true;
                    }
                }
            }

            if (updateTO)
            {
                TechOperationForm.UpdateGrid();
            }

        }

        private void DataGridViewStaff_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                var vv = TechOperationForm.TehCarta.Staff_TCs;
                var Idd = (Staff_TC)dataGridViewStaff.Rows[e.RowIndex].Cells[0].Value;
                var value = (string)dataGridViewStaff.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                Idd.Symbol = value;

                //Staff_TC staffTc = new Staff_TC();
                //staffTc.Child = Idd.Child;
                //staffTc.Symbol = value;

                //foreach (ExecutionWork ex in Idd.ExecutionWorks)
                //{
                //    staffTc.ExecutionWorks.Add(ex);
                //}
                //vv.Remove(Idd);
                //vv.Add(staffTc);

                Task.Run(() =>
                {
                    this.BeginInvoke((Action)(() => UpdateGridStaff()));
                });
            }
        }

        private void DataGridViewStaffAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var idd = (Staff)dataGridViewStaffAll.Rows[e.RowIndex].Cells[0].Value;
                var vv = TechOperationForm.TehCarta.Staff_TCs;

                if (idd != null)
                {
                    Staff_TC staffTc = new Staff_TC();
                    staffTc.Child = idd;
                    staffTc.Symbol = " ";
                    vv.Add(staffTc);
                    // TechOperationForm.TehCarta.Staff_TCs.Add(staffTc);
                    Task.Run(() =>
                    {
                        this.BeginInvoke((Action)(() => UpdateGridStaff()));
                    });
                }

            }
        }


        private void DataGridViewStaff_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var idd = (Staff_TC)dataGridViewStaff.Rows[e.RowIndex].Cells[0].Value;

                if (idd != null)
                {
                    var vv = TechOperationForm.TehCarta.Staff_TCs;
                    vv.Remove(idd);
                    //TechOperationForm.TehCarta.Staff_TCs.Remove(idd);

                    UpdateGridStaff();
                }

            }
        }



        private void comboBoxStaff_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGridStaff();
            UpdateGridStaffAll();
        }



        public void UpdateGridStaff()
        {
            var offScroll = dataGridViewStaff.FirstDisplayedScrollingRowIndex;
            var ExecutionWorkBox = (ExecutionWork)comboBoxStaff.SelectedItem;
            dataGridViewStaff.Rows.Clear();
            if (ExecutionWorkBox == null)
            {
                return;
            }

            var work = (TechOperationWork)comboBoxTO.SelectedItem;
            var LocalTP = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.Single(s => s.IdGuid == ExecutionWorkBox.IdGuid);

            var AllStaff = TechOperationForm.TehCarta.Staff_TCs;


            foreach (Staff_TC staffTc in AllStaff)
            {
                List<object> listItem = new List<object>();
                listItem.Add(staffTc);
                listItem.Add("Удалить");

                var vs = LocalTP.Staffs.SingleOrDefault(s => s == staffTc);
                if (vs != null)
                {
                    listItem.Add(true);
                }
                else
                {
                    listItem.Add(false);
                }

                listItem.Add(staffTc.Symbol);
                listItem.Add(staffTc.Child.Name);
                listItem.Add(staffTc.Child.Type);
                listItem.Add(staffTc.Child.Functions);
                listItem.Add(staffTc.Child.CombineResponsibility ?? "");
                listItem.Add(staffTc.Child.Qualification);
                listItem.Add(staffTc.Child.Comment ?? "");
                dataGridViewStaff.Rows.Add(listItem.ToArray());
            }

            try
            {
                dataGridViewStaff.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }

        public void UpdateGridStaffAll()
        {
            var ExecutionWorkBox = (ExecutionWork)comboBoxStaff.SelectedItem;
            dataGridViewStaffAll.Rows.Clear();
            if (ExecutionWorkBox == null)
            {
                return;
            }

            var allStaff = TechOperationForm.context.Staffs.ToList();
            foreach (Staff staff in allStaff)
            {
                if (PoiskPersonal.Text != "" &&
                    staff.Name.ToLower().IndexOf(PoiskPersonal.Text.ToLower()) == -1)
                {
                    continue;
                }


                List<object> listItem = new List<object>();
                listItem.Add(staff);
                listItem.Add("Добавить");
                listItem.Add(staff.Name);
                listItem.Add(staff.Type);
                listItem.Add(staff.Functions);
                listItem.Add(staff.CombineResponsibility ?? "");
                listItem.Add(staff.Qualification);
                listItem.Add(staff.Comment ?? "");
                dataGridViewStaffAll.Rows.Add(listItem.ToArray());
            }

        }


        #endregion



        #region средства защиты

        private void comboBoxSZ_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGridAllSZ();
            UpdateGridLocalSZ();
        }


        private void TextBoxPoiskSZ_TextChanged(object? sender, EventArgs e)
        {
            UpdateGridAllSZ();
        }
        public void UpdateGridAllSZ()
        {
            var offScroll = dataGridViewAllSZ.FirstDisplayedScrollingRowIndex;
            dataGridViewAllSZ.Rows.Clear();

            var work = (ExecutionWork)comboBoxSZ.SelectedItem;

            if (work == null)
            {
                return;
            }

            var context = TechOperationForm.context;

            var protection = context.Protections.ToList();
            var LocalTP = work.Protections.ToList();

            var Allsz = context.Protection_TCs.Where(w => w.ParentId == TechOperationForm.tcId).ToList();

            foreach (Protection_TC prot in Allsz)
            {
                if (textBoxPoiskSZ.Text != "" &&
                    prot.Child.Name.ToLower().IndexOf(textBoxPoiskSZ.Text.ToLower()) == -1)
                {
                    continue;
                }


                if (LocalTP.SingleOrDefault(s => s.Child == prot.Child) != null)
                {
                    continue;
                }


                List<object> listItem = new List<object>();

                listItem.Add(prot.Child);
                listItem.Add("Добавить");

                listItem.Add(prot.Child.Name);
                listItem.Add(prot.Child.Type);
                listItem.Add(prot.Child.Unit);
                listItem.Add(prot.Quantity);

                dataGridViewAllSZ.Rows.Add(listItem.ToArray());
            }


            foreach (Protection prot in protection)
            {
                if (textBoxPoiskSZ.Text != "" &&
                    prot.Name.ToLower().IndexOf(textBoxPoiskSZ.Text.ToLower()) == -1)
                {
                    continue;
                }

                if (LocalTP.SingleOrDefault(s => s.Child == prot) != null)
                {
                    continue;
                }

                if (Allsz.SingleOrDefault(s => s.Child == prot) != null)
                {
                    continue;
                }

                List<object> listItem = new List<object>();
                listItem.Add(prot);

                listItem.Add("Добавить");

                listItem.Add(prot.Name);
                listItem.Add(prot.Type);
                listItem.Add(prot.Unit);
                listItem.Add("");
                dataGridViewAllSZ.Rows.Add(listItem.ToArray());
            }

            try
            {
                dataGridViewAllSZ.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }


        }

        public void UpdateGridLocalSZ()
        {
            var offScroll = dataGridViewLocalSZ.FirstDisplayedScrollingRowIndex;
            dataGridViewLocalSZ.Rows.Clear();
            var work = (ExecutionWork)comboBoxSZ.SelectedItem;

            if (work == null)
            {
                return;
            }

            var LocalTP = work.Protections.ToList();

            foreach (Protection_TC sz in LocalTP)
            {
                List<object> listItem = new List<object>();
                listItem.Add(sz);

                listItem.Add("Удалить");

                listItem.Add(sz.Child.Name);
                listItem.Add(sz.Child.Type);
                listItem.Add(sz.Child.Unit);
                listItem.Add(sz.Quantity);
                dataGridViewLocalSZ.Rows.Add(listItem.ToArray());
            }

            try
            {
                dataGridViewLocalSZ.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }

        private void DataGridViewAllSZ_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var work = (ExecutionWork)comboBoxSZ.SelectedItem;
                var Idd = (Protection)dataGridViewAllSZ.Rows[e.RowIndex].Cells[0].Value;

                var context = TechOperationForm.context;

                var orderMax = 0;

                var tc = work.techOperationWork.technologicalCard;

                var list = tc.Protection_TCs.ToList();

                var proty = list.SingleOrDefault(s => s.Child == Idd);
                if (proty != null)
                {
                    work.Protections.Add(proty);
                }
                else
                {
                    if (list.Count > 0)
                    {
                        orderMax = list.Max(m => m.Order);
                    }

                    Protection_TC protectionTc = new Protection_TC();
                    protectionTc.Child = Idd;
                    protectionTc.ParentId = TechOperationForm.tcId;
                    protectionTc.Parent = tc;
                    protectionTc.Quantity = 1;
                    protectionTc.Order = orderMax + 1;

                    context.Protection_TCs.Add(protectionTc);
                    work.Protections.Add(protectionTc);
                }

                UpdateGridAllSZ();
                UpdateGridLocalSZ();
                TechOperationForm.UpdateGrid();
            }
        }

        private void DataGridViewLocalSZ_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var work = (ExecutionWork)comboBoxSZ.SelectedItem;
                var Idd = (Protection_TC)dataGridViewLocalSZ.Rows[e.RowIndex].Cells[0].Value;

                work.Protections.Remove(Idd);
                UpdateGridAllSZ();
                UpdateGridLocalSZ();
                TechOperationForm.UpdateGrid();
            }
        }

        #endregion




        #region Component


        private void TextBoxPoiskComponent_TextChanged(object? sender, EventArgs e)
        {
            UpdateComponentAll();
        }



        private void ComboBoxFilterComponent_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateComponentAll();
        }

        private List<string> AllFilterComponent;

        public void UpdateComponentAll()
        {
            var offScroll = dataGridViewComponentAll.FirstDisplayedScrollingRowIndex;
            dataGridViewComponentAll.Rows.Clear();

            var work = (TechOperationWork)comboBoxTO2.SelectedItem;

            var context = TechOperationForm.context;

            var AllMyComponent = TechOperationForm.TehCarta.Component_TCs.Where(w => w.ParentId == TechOperationForm.tcId).ToList();

            var AllComponent = context.Components.ToList();

            var LocalComponent = work.ComponentWorks.ToList();


            bool UpdFilt = false;

            if (AllFilterComponent == null)
            {
                UpdFilt = true;
                AllFilterComponent = new List<string>();
                AllFilterComponent.Add("Все");
            }


            foreach (Component_TC componentTc in AllMyComponent)
            {
                if (textBoxPoiskComponent.Text != "" &&
                    componentTc.Child.Name.ToLower().IndexOf(textBoxPoiskComponent.Text.ToLower()) == -1)
                {
                    continue;
                }

                if (LocalComponent.SingleOrDefault(s => s.component == componentTc.Child) != null)
                {
                    continue;
                }

                if (comboBoxFilterComponent.SelectedIndex > 0)
                {
                    var selCateg = (string)comboBoxFilterComponent.SelectedItem;

                    if (selCateg != componentTc.Child.Categoty)
                    {
                        continue;
                    }

                }


                List<object> listItem = new List<object>();
                listItem.Add(componentTc.Child);

                listItem.Add("Добавить");

                listItem.Add(componentTc.Child.Name);
                listItem.Add(componentTc.Child.Type);
                listItem.Add(componentTc.Child.Unit);
                listItem.Add(componentTc.Quantity);
                dataGridViewComponentAll.Rows.Add(listItem.ToArray());
            }

            foreach (Component component in AllComponent)
            {
                if (UpdFilt)
                {
                    if (!AllFilterComponent.Contains(component.Categoty))
                    {
                        AllFilterComponent.Add(component.Categoty);
                    }
                }


                if (textBoxPoiskComponent.Text != "" &&
                    component.Name.ToLower().IndexOf(textBoxPoiskComponent.Text.ToLower()) == -1)
                {
                    continue;
                }

                if (LocalComponent.SingleOrDefault(s => s.component == component) != null)
                {
                    continue;
                }

                if (AllMyComponent.SingleOrDefault(s => s.Child == component) != null)
                {
                    continue;
                }


                if (comboBoxFilterComponent.SelectedIndex > 0)
                {
                    var selCateg = (string)comboBoxFilterComponent.SelectedItem;

                    if (selCateg != component.Categoty)
                    {
                        continue;
                    }

                }


                List<object> listItem = new List<object>();
                listItem.Add(component);

                listItem.Add("Добавить");

                listItem.Add(component.Name);
                listItem.Add(component.Type);
                listItem.Add(component.Unit);
                listItem.Add("");
                dataGridViewComponentAll.Rows.Add(listItem.ToArray());
            }


            if (UpdFilt)
            {
                comboBoxFilterComponent.DataSource = AllFilterComponent;
                comboBoxFilterComponent.SelectedIndex = 0;
            }


            try
            {
                dataGridViewComponentAll.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }

        public void UpdateComponentLocal()
        {
            var offScroll = dataGridViewComponentLocal.FirstDisplayedScrollingRowIndex;
            dataGridViewComponentLocal.Rows.Clear();
            var work = (TechOperationWork)comboBoxTO2.SelectedItem;
            var LocalComponent = work.ComponentWorks.ToList();

            foreach (ComponentWork componentWork in LocalComponent)
            {
                List<object> listItem = new List<object>();
                listItem.Add(componentWork);

                listItem.Add("Удалить");

                listItem.Add(componentWork.component.Name);
                listItem.Add(componentWork.component.Type);
                listItem.Add(componentWork.component.Unit);
                listItem.Add(componentWork.Quantity.ToString());
                dataGridViewComponentLocal.Rows.Add(listItem.ToArray());
            }

            try
            {
                dataGridViewComponentLocal.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }


        private void comboBoxTO2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComponentAll();
            UpdateComponentLocal();
        }


        private void DataGridViewComponentAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var work = (TechOperationWork)comboBoxTO2.SelectedItem;
                var Idd = (Component)dataGridViewComponentAll.Rows[e.RowIndex].Cells[0].Value;

                ComponentWork componentWork = new ComponentWork();
                componentWork.component = Idd;
                componentWork.Quantity = 1;
                work.ComponentWorks.Add(componentWork);

                UpdateComponentAll();
                UpdateComponentLocal();
                TechOperationForm.UpdateGrid();
            }
        }

        private void DataGridViewComponentLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var work = (TechOperationWork)comboBoxTO2.SelectedItem;
                var Idd = (ComponentWork)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[0].Value;

                work.ComponentWorks.Remove(Idd);
                UpdateComponentAll();
                UpdateComponentLocal();
                TechOperationForm.UpdateGrid();
            }
        }


        private void DataGridViewComponentLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 5)
            {
                var work = (TechOperationWork)comboBoxTO2.SelectedItem;
                var Idd = (ComponentWork)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[0].Value;
                var value = (object)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[5].Value;

                double don = 0;
                if (double.TryParse((string)value, out don))
                {
                    Idd.Quantity = don;
                }
                else
                {
                    Idd.Quantity = 0;
                }
                //UpdateComponentAll();
                //UpdateComponentLocal();
                TechOperationForm.UpdateGrid();
            }
        }

        #endregion


        #region Instument


        public void UpdateInstrumentLocal()
        {
            var offScroll = dataGridViewInstrumentLocal.FirstDisplayedScrollingRowIndex;
            dataGridViewInstrumentLocal.Rows.Clear();
            var work = (TechOperationWork)comboBoxTO3.SelectedItem;
            var LocalInstrument = work.ToolWorks.ToList();

            foreach (var InstrumentWork in LocalInstrument)
            {
                List<object> listItem = new List<object>();
                listItem.Add(InstrumentWork);

                listItem.Add("Удалить");

                listItem.Add(InstrumentWork.tool.Name);
                listItem.Add(InstrumentWork.tool.Type);
                listItem.Add(InstrumentWork.tool.Unit);
                listItem.Add(InstrumentWork.Quantity);
                dataGridViewInstrumentLocal.Rows.Add(listItem.ToArray());
            }

            try
            {
                dataGridViewInstrumentLocal.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }


        private void ComboBoxFiltrCategor_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateInstrumentAll();
        }

        private List<string> AllFilterInstrument;

        public void UpdateInstrumentAll()
        {
            var offScroll = dataGridViewInstumentAll.FirstDisplayedScrollingRowIndex;
            dataGridViewInstumentAll.Rows.Clear();

            var work = (TechOperationWork)comboBoxTO3.SelectedItem;

            var context = TechOperationForm.context;

            var AllMyInstr = TechOperationForm.TehCarta.Tool_TCs.ToList();

            var AllInstr = context.Tools.ToList();

            var LocalComponent = work.ToolWorks.ToList();

            bool UpdFilt = false;

            if (AllFilterInstrument == null)
            {
                UpdFilt = true;
                AllFilterInstrument = new List<string>();
                AllFilterInstrument.Add("Все");
            }

            foreach (var componentTc in AllMyInstr)
            {
                if (LocalComponent.SingleOrDefault(s => s.tool == componentTc.Child) != null)
                {
                    continue;
                }

                if (textBoxPoiskInstrument.Text != "" &&
                    componentTc.Child.Name.ToLower().IndexOf(textBoxPoiskInstrument.Text.ToLower()) == -1)
                {
                    continue;
                }

                if (comboBoxFiltrCategor.SelectedIndex > 0 )
                {
                    var selCateg = (string)comboBoxFiltrCategor.SelectedItem;

                    if (selCateg != componentTc.Child.Categoty)
                    {
                        continue;
                    }

                }


                List<object> listItem = new List<object>();
                listItem.Add(componentTc.Child);

                listItem.Add("Добавить");

                listItem.Add(componentTc.Child.Name);
                listItem.Add(componentTc.Child.Type);
                listItem.Add(componentTc.Child.Unit);
                listItem.Add(componentTc.Quantity.ToString());
                dataGridViewInstumentAll.Rows.Add(listItem.ToArray());
            }

            foreach (var component in AllInstr)
            {
                if (UpdFilt)
                {
                    if (!AllFilterInstrument.Contains(component.Categoty))
                    {
                        AllFilterInstrument.Add(component.Categoty);
                    }
                }


                if (LocalComponent.SingleOrDefault(s => s.tool == component) != null)
                {
                    continue;
                }

                if (textBoxPoiskInstrument.Text != "" &&
                    component.Name.ToLower().IndexOf(textBoxPoiskInstrument.Text.ToLower()) == -1)
                {
                    continue;
                }



                if (comboBoxFiltrCategor.SelectedIndex > 0)
                {
                    var selCateg = (string)comboBoxFiltrCategor.SelectedItem;

                    if (selCateg != component.Categoty)
                    {
                        continue;
                    }

                }


                List<object> listItem = new List<object>();
                listItem.Add(component);

                listItem.Add("Добавить");

                listItem.Add(component.Name);
                listItem.Add(component.Type);
                listItem.Add(component.Unit);
                listItem.Add("");
                dataGridViewInstumentAll.Rows.Add(listItem.ToArray());
            }

            if (UpdFilt)
            {
                comboBoxFiltrCategor.DataSource = AllFilterInstrument;
                comboBoxFiltrCategor.SelectedIndex = 0;
            }


            try
            {
                dataGridViewInstumentAll.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }


        private void comboBoxTO3_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInstrumentAll();
            UpdateInstrumentLocal();
        }


        private void DataGridViewInstumentAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var work = (TechOperationWork)comboBoxTO3.SelectedItem;
                var Idd = (Tool)dataGridViewInstumentAll.Rows[e.RowIndex].Cells[0].Value;

                ToolWork toolWork = new ToolWork();
                toolWork.tool = Idd;
                toolWork.Quantity = 1;
                work.ToolWorks.Add(toolWork);

                UpdateInstrumentAll();
                UpdateInstrumentLocal();
                TechOperationForm.UpdateGrid();
            }
        }


        private void DataGridViewInstrumentLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                var work = (TechOperationWork)comboBoxTO3.SelectedItem;
                var Idd = (ToolWork)dataGridViewInstrumentLocal.Rows[e.RowIndex].Cells[0].Value;

                work.ToolWorks.Remove(Idd);
                UpdateInstrumentAll();
                UpdateInstrumentLocal();
                TechOperationForm.UpdateGrid();
            }
        }


        private void DataGridViewInstrumentLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 5)
            {
                var work = (TechOperationWork)comboBoxTO3.SelectedItem;
                var Idd = (ToolWork)dataGridViewInstrumentLocal.Rows[e.RowIndex].Cells[0].Value;
                var value = (object)dataGridViewInstrumentLocal.Rows[e.RowIndex].Cells[5].Value;

                double don = 0;
                if (double.TryParse((string)value, out don))
                {
                    Idd.Quantity = don;
                }
                else
                {
                    Idd.Quantity = 0;
                }
                //UpdateComponentAll();
                //UpdateComponentLocal();
                TechOperationForm.UpdateGrid();
            }
        }



        private void textBoxPoiskInstrument_TextChanged(object sender, EventArgs e)
        {
            UpdateInstrumentAll();
        }

        #endregion



        #region этапы



        private void TextBoxPoiskMach_TextChanged(object? sender, EventArgs e)
        {
            dataGridViewMehaUpdate();
        }

        public void dataGridViewMehaUpdate()
        {
            var offScroll = dataGridViewMeha.FirstDisplayedScrollingRowIndex;
            dataGridViewMeha.Rows.Clear();

            var Msch = TechOperationForm.TehCarta.Machine_TCs.ToList();
            var context = TechOperationForm.context;
            var all = context.Machines.ToList();

            foreach (var machine in Msch)
            {
                if (textBoxPoiskMach.Text != "" &&
                    machine.Child.Name.ToLower().IndexOf(textBoxPoiskMach.Text.ToLower()) == -1)
                {
                    continue;
                }

                List<object> listItem = new List<object>();
                listItem.Add(machine.Child);
                listItem.Add(true);
                listItem.Add(machine.Child.Name);
                listItem.Add(machine.Child.Type);
                listItem.Add(machine.Child.Unit);
                listItem.Add(machine.Quantity);
                dataGridViewMeha.Rows.Add(listItem.ToArray());
            }


            foreach (Machine machine in all)
            {
                if (textBoxPoiskMach.Text != "" &&
                    machine.Name.ToLower().IndexOf(textBoxPoiskMach.Text.ToLower()) == -1)
                {
                    continue;
                }

                var sin = Msch.SingleOrDefault(s => s.Child == machine);
                if (sin == null)
                {
                    List<object> listItem = new List<object>();
                    listItem.Add(machine);
                    listItem.Add(false);
                    listItem.Add(machine.Name);
                    listItem.Add(machine.Type);
                    listItem.Add(machine.Unit);
                    listItem.Add("");
                    dataGridViewMeha.Rows.Add(listItem.ToArray());
                }
            }

            try
            {
                dataGridViewMeha.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }


        }

        public void dataGridViewEtapUpdate()
        {
            var offScroll = dataGridViewEtap.FirstDisplayedScrollingRowIndex;
            dataGridViewEtap.Rows.Clear();

            var al = TechOperationForm.TechOperationWorksList.Where(w => w.Delete == false).OrderBy(o => o.Order);

            var allMsch = TechOperationForm.TehCarta.Machine_TCs.ToList();

            while (dataGridViewEtap.Columns.Count > 5)
            {
                dataGridViewEtap.Columns.RemoveAt(5);
            }

            foreach (var machine in allMsch)
            {
                var chach = new DataGridViewCheckBoxColumn();
                chach.HeaderText = machine.Child.Name;

                dataGridViewEtap.Columns.Add(chach);
            }


            foreach (TechOperationWork techOperationWork in al)
            {
                var lli = techOperationWork.executionWorks.Where(w => w.Delete == false).OrderBy(o => o.Order);
                foreach (ExecutionWork Wor in lli)
                {
                    List<object> listItem = new List<object>();
                    listItem.Add(Wor);

                    listItem.Add(techOperationWork.techOperation.Name);
                    if (Wor.Repeat)
                    {
                        listItem.Add("Повторить");
                    }
                    else
                    {
                        listItem.Add(Wor.techTransition.Name);
                    }

                    if (Wor.Etap == "")
                    {
                        listItem.Add("0");
                    }
                    else
                    {
                        listItem.Add(Wor.Etap);
                    }

                    if (Wor.Posled == "")
                    {
                        listItem.Add("0");
                    }
                    else
                    {
                        listItem.Add(Wor.Posled);
                    }

                    foreach (var machine in allMsch)
                    {
                        var fv = Wor.Machines.SingleOrDefault(w => w.Child == machine.Child);
                        if (fv == null)
                        {
                            listItem.Add(false);
                        }
                        else
                        {
                            listItem.Add(true);
                        }
                    }

                    dataGridViewEtap.Rows.Add(listItem.ToArray());
                }
            }
            try
            {
                dataGridViewEtap.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }


        private void DataGridViewEtap_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                ExecutionWork vb = (ExecutionWork)dataGridViewEtap.Rows[e.RowIndex].Cells[0].Value;
                string strEtap = (string)dataGridViewEtap.Rows[e.RowIndex].Cells[3].Value;

                vb.Etap = strEtap;
                TechOperationForm.UpdateGrid();
            }


            if (e.ColumnIndex == 4)
            {
                ExecutionWork vb = (ExecutionWork)dataGridViewEtap.Rows[e.RowIndex].Cells[0].Value;
                string strEtap = (string)dataGridViewEtap.Rows[e.RowIndex].Cells[4].Value;

                vb.Posled = strEtap;
                TechOperationForm.UpdateGrid();
            }

        }

        private void DataGridViewEtap_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 4)
            {
                dataGridViewEtap.CommitEdit(DataGridViewDataErrorContexts.Commit);
                var che = (bool)dataGridViewEtap.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var id = (ExecutionWork)dataGridViewEtap.Rows[e.RowIndex].Cells[0].Value;

                var allMsch = TechOperationForm.TehCarta.Machine_TCs.ToList();
                var mach = allMsch[e.ColumnIndex - 5];

                var bn = id.Machines.SingleOrDefault(s => s == mach);
                if (che)
                {
                    if (bn == null)
                    {
                        id.Machines.Add(mach);
                        TechOperationForm.UpdateGrid();
                    }
                }
                else
                {
                    if (bn != null)
                    {
                        id.Machines.Remove(bn);
                        TechOperationForm.UpdateGrid();
                    }
                }
            }
        }



        private void DataGridViewMeha_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {

            if (e.ColumnIndex == 1)
            {
                dataGridViewMeha.CommitEdit(DataGridViewDataErrorContexts.Commit);
                var id = (Machine)dataGridViewMeha.Rows[e.RowIndex].Cells[0].Value;
                var che = (bool)dataGridViewMeha.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                var allMsch = TechOperationForm.TehCarta.Machines.ToList();

                if (che)
                {
                    var bn = TechOperationForm.TehCarta.Machine_TCs.SingleOrDefault(s => s.Child == id);
                    if (bn == null)
                    {
                        bn = new Machine_TC();
                        bn.Child = id;
                        bn.Quantity = 1;
                        bn.Parent = TechOperationForm.TehCarta;
                        TechOperationForm.TehCarta.Machine_TCs.Add(bn);
                        dataGridViewEtapUpdate();
                        dataGridViewMehaUpdate();
                        TechOperationForm.UpdateGrid();
                    }
                }
                else
                {
                    var bn = TechOperationForm.TehCarta.Machine_TCs.SingleOrDefault(s => s.Child == id);
                    if (bn != null)
                    {
                        TechOperationForm.TehCarta.Machine_TCs.Remove(bn);
                        dataGridViewEtapUpdate();
                        dataGridViewMehaUpdate();
                        TechOperationForm.UpdateGrid();
                    }
                }

            }
        }


        #endregion


        #region Повторить


        private void DataGridViewPovtor_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewPovtor.CommitEdit(DataGridViewDataErrorContexts.Commit);

            if (e.ColumnIndex == 1)
            {
                var id = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
                var che = (bool)dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                if (executionWorkPovtor != null)
                {
                    if (che)
                    {
                        if (!executionWorkPovtor.ListexecutionWorkRepeat2.Contains(id))
                        {
                            executionWorkPovtor.ListexecutionWorkRepeat2.Add(id);
                            TechOperationForm.UpdateGrid();
                        }
                    }
                    else
                    {
                        if (executionWorkPovtor.ListexecutionWorkRepeat2.Contains(id))
                        {
                            executionWorkPovtor.ListexecutionWorkRepeat2.Remove(id);
                            TechOperationForm.UpdateGrid();
                        }
                    }
                }

            }

        }

        private ExecutionWork executionWorkPovtor;

        public void UpdatePovtor()
        {
            dataGridViewPovtor.Rows.Clear();
            executionWorkPovtor = null;
            var select = dataGridViewTPLocal.SelectedRows;
            if (select.Count > 0)
            {
                var id = (Guid)select[0].Cells[0].Value;

                var al = TechOperationForm.TechOperationWorksList.Where(w => w.Delete == false).OrderBy(o => o.Order);

                ExecutionWork exeWork = null;

                foreach (TechOperationWork techOperationWork in al)
                {
                    if (exeWork != null)
                    {
                        break;
                    }
                    foreach (ExecutionWork executionWork in techOperationWork.executionWorks)
                    {
                        if (executionWork.IdGuid == id)
                        {
                            exeWork = executionWork;
                            break;
                        }
                    }
                }

                if (exeWork != null && exeWork.Repeat)
                {
                    executionWorkPovtor = exeWork;
                    var allRep = exeWork.ListexecutionWorkRepeat2.ToList();

                    foreach (TechOperationWork techOperationWork in al)
                    {
                        var lli = techOperationWork.executionWorks.Where(w => w.Delete == false && w.Repeat == false).OrderBy(o => o.Order);


                        foreach (ExecutionWork executionWork in lli)
                        {
                            List<object> listItem = new List<object>();
                            listItem.Add(executionWork);
                            if (allRep.SingleOrDefault(s => s == executionWork) != null)
                            {
                                listItem.Add(true);
                            }
                            else
                            {
                                listItem.Add(false);
                            }
                            listItem.Add(techOperationWork.techOperation.Name);
                            listItem.Add(executionWork.techTransition?.Name);
                            dataGridViewPovtor.Rows.Add(listItem.ToArray());
                        }
                    }
                }

            }

        }




        #endregion

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Name == "tabPage2")
            {
                var select = dataGridViewTO.SelectedRows;
                if (select.Count > 0)
                {
                    var id = (TechOperationWork)select[0].Cells[0].Value;

                    foreach (TechOperationWork item in comboBoxTO.Items)
                    {
                        if (item == id)
                        {
                            comboBoxTO.SelectedItem = item;
                        }
                    }
                }
            }

            if (tabControl1.SelectedTab.Name == "tabPage5")
            {
                var select = dataGridViewTO.SelectedRows;
                if (select.Count > 0)
                {
                    var id = (TechOperationWork)select[0].Cells[0].Value;

                    foreach (TechOperationWork item in comboBoxTO2.Items)
                    {
                        if (item == id)
                        {
                            comboBoxTO2.SelectedItem = item;
                        }
                    }
                }
            }

            if (tabControl1.SelectedTab.Name == "tabPage6")
            {
                var select = dataGridViewTO.SelectedRows;
                if (select.Count > 0)
                {
                    var id = (TechOperationWork)select[0].Cells[0].Value;

                    foreach (TechOperationWork item in comboBoxTO3.Items)
                    {
                        if (item == id)
                        {
                            comboBoxTO3.SelectedItem = item;
                        }
                    }
                }
            }

            if (tabControl1.SelectedTab.Name == "tabPage4")
            {
                var select = dataGridViewTPLocal.SelectedRows;
                if (select.Count > 0)
                {
                    var id = (Guid)select[0].Cells[0].Value;

                    foreach (ExecutionWork item in comboBoxStaff.Items)
                    {
                        if (item.IdGuid == id)
                        {
                            comboBoxStaff.SelectedItem = item;
                        }
                    }
                }
            }

            if (tabControl1.SelectedTab.Name == "tabPage7")
            {
                var select = dataGridViewTPLocal.SelectedRows;
                if (select.Count > 0)
                {
                    var id = (Guid)select[0].Cells[0].Value;

                    foreach (ExecutionWork item in comboBoxSZ.Items)
                    {
                        if (item.IdGuid == id)
                        {
                            comboBoxSZ.SelectedItem = item;
                        }
                    }
                }
            }

            if (tabControl1.SelectedTab.Name == "tabPage8")
            {
                dataGridViewEtapUpdate();
                dataGridViewMehaUpdate();
            }

            if (tabControl1.SelectedTab.Name == "tabPage9")
            {
                UpdatePovtor();
            }


        }


        public void UpdateTable(int table)
        {
            if (table == 1)
            {
                var list = dataGridViewTO.Rows;
                foreach (DataGridViewRow dataGridViewRow in list)
                {
                    var idd = (TechOperationWork)dataGridViewRow.Cells[0].Value;
                    var ord = (int)dataGridViewRow.Cells["Order"].Value;
                    TechOperationWork TechOperat = TechOperationForm.TechOperationWorksList.SingleOrDefault(s => s == idd);

                    if (TechOperat != null)
                    {
                        TechOperat.Order = ord;
                    }

                }
                TechOperationForm.UpdateGrid();
            }

            if (table == 2)
            {
                var list = dataGridViewTPLocal.Rows;
                var work = (TechOperationWork)comboBoxTO.SelectedItem;

                foreach (DataGridViewRow dataGridViewRow in list)
                {
                    var IddGuid = (Guid)dataGridViewRow.Cells[0].Value;
                    var ord = (int)dataGridViewRow.Cells["Order1"].Value;

                    var bg = work.executionWorks.SingleOrDefault(s => s.IdGuid == IddGuid);
                    bg.Order = ord;

                }
                TechOperationForm.UpdateGrid();
            }

        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
