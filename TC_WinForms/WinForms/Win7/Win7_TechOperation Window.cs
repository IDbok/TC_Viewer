using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TcModels.Models.TcContent;
using System.Windows.Forms.VisualStyles;
using TcModels.Models.TcContent.Work;
using TC_WinForms.DataProcessing;
using Microsoft.EntityFrameworkCore;

namespace TC_WinForms.WinForms
{
    public partial class Win7_TechOperation_Window : Form
    {
        MyDbContext context;

        TechOperation techOperation;


        public Win7_TechOperation_Window(int? _techOperationint = null)
        {
            InitializeComponent();

            context = new MyDbContext();

            if(_techOperationint!=null)
            {
                techOperation = context.TechOperations
                    .Include(t => t.techTransitionTypicals)
                    .Single(s => s.Id == _techOperationint);

                textBox1.Text = techOperation.Name;

                if(techOperation.Category!=null)
                {
                    checkBox1.Checked = true;
                }
                else
                {
                    checkBox1.Checked = false;
                }
            }

            dataGridViewTPAll.CellClick += DataGridViewTPAll_CellClick;
            dataGridViewTPLocal.CellClick += DataGridViewTPLocal_CellClick;

            dataGridViewTPLocal.CellEndEdit += DataGridViewTPLocal_CellEndEdit;

            if (techOperation == null)
            {
                techOperation = new TechOperation();
                context.TechOperations.Add(techOperation);
            }

            UpdateGridAllTP();
            UpdateGridLocalTP();


            //  var bb = context.ChangeTracker.Entries().Count();

        }

        private void DataGridViewTPLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex<0) return;

            if (e.ColumnIndex == 4)
            {
                var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var idd = (TechTransitionTypical)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

                idd.Etap = gg;

            }

            if (e.ColumnIndex == 5)
            {
                var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var idd = (TechTransitionTypical)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

                idd.Posled = gg;

            }

        }

        private void DataGridViewTPAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var Idd = (TechTransition)dataGridViewTPAll.Rows[e.RowIndex].Cells[0].Value;
                techOperation.techTransitionTypicals.Add(new TechTransitionTypical { techTransition = Idd });
                UpdateGridLocalTP();
            }
        }

        private void DataGridViewTPLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var IddGuid = (TechTransitionTypical)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

                techOperation.techTransitionTypicals.Remove(IddGuid);
                UpdateGridLocalTP();
            }
        }

        public void UpdateGridLocalTP()
        {
            var offScroll = dataGridViewTPLocal.FirstDisplayedScrollingRowIndex;
            dataGridViewTPLocal.Rows.Clear();

            var LocalTP = techOperation.techTransitionTypicals.ToList();

            foreach (TechTransitionTypical techTransitionTypical in LocalTP)
            {
                List<object> listItem = new List<object>();
                listItem.Add(techTransitionTypical);
                listItem.Add("Удалить");

                listItem.Add(techTransitionTypical.techTransition?.Name);
                listItem.Add(techTransitionTypical.techTransition?.TimeExecution);

                listItem.Add(techTransitionTypical.Etap);
                listItem.Add(techTransitionTypical.Posled);


                dataGridViewTPLocal.Rows.Add(listItem.ToArray());
            }


            try
            {
                dataGridViewTPLocal.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }



        public void UpdateGridAllTP()
        {
            var offScroll = dataGridViewTPAll.FirstDisplayedScrollingRowIndex;
            dataGridViewTPAll.Rows.Clear();

            //bool AddCategor = false;
            //if (comboBoxTPCategoriya.Items.Count == 0)
            //{
            //    AddCategor = true;
            //    comboBoxTPCategoriya.Items.Add("Все");
            //}



            var allTP = context.TechTransitions.ToList();
            // var list = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.ToList();


            foreach (TechTransition techTransition in allTP)
            {
                //if (textBoxPoiskTP.Text != "" &&
                //    techTransition.Name.ToLower().IndexOf(textBoxPoiskTP.Text.ToLower()) == -1)
                //{
                //    continue;
                //}

                //if (AddCategor)
                //{
                //    if (!string.IsNullOrEmpty(techTransition.Category))
                //    {
                //        if (comboBoxTPCategoriya.Items.Contains(techTransition.Category) == false)
                //        {
                //            comboBoxTPCategoriya.Items.Add(techTransition.Category);
                //        }
                //    }
                //}

                //if (comboBoxTPCategoriya.SelectedIndex > 0)
                //{
                //    if ((string)comboBoxTPCategoriya.SelectedItem != techTransition.Category)
                //    {
                //        continue;
                //    }
                //}


                List<object> listItem = new List<object>();

                listItem.Add(techTransition);
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

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                context.SaveChanges();
                StaticWinForms.Win7_new.UpdateTO();
                MessageBox.Show("Данные сохранены");
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }

        }

        private void Win7_TechOperation_Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            var Track = context.ChangeTracker.Entries().Where(w => w.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged).Count(); 

            if (Track > 0)
            {
                if (MessageBox.Show("Закрыть без сохранения?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            techOperation.Name = textBox1.Text;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                techOperation.Category = "Типовой";
            }
            else
            {
                techOperation.Category = null;
            }
            
        }
    }
}
