using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Work
{
    public partial class TechOperationForm : Form
    {
        public MyDbContext context;
        private int tcId;
        private BindingSource binding;

        public List<TechOperationDataGridItem> list = null;
        public List<TechOperationWork> TechOperationWorksList;
        private TechnologicalCard TehCarta;

        public TechOperationForm()
        {
            InitializeComponent();
        }

        public TechOperationForm(int tcId)
        {
            InitializeComponent();

            dgvMain.CellPainting += DgvMain_CellPainting;
            dgvMain.CellFormatting += DgvMain_CellFormatting;

            this.tcId = tcId;


            list = new List<TechOperationDataGridItem>();

            // binding = new BindingSource();
            // binding.DataSource = list;
            // dgvMain.DataSource = list;

            context = new MyDbContext();

            TehCarta = context.TechnologicalCards.Single(s => s.Id == tcId);

                 TechOperationWorksList =
                    context.TechOperationWorks.Where(w => w.TechnologicalCardId == tcId)
                        .Include(i => i.techOperation)
                        .Include(r => r.executionWorks).ThenInclude(t => t.techTransition).ToList();
            
            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");
            
            dgvMain.Columns[0].HeaderText = "#";
            dgvMain.Columns[0].Width = 50;

            dgvMain.Columns[1].HeaderText = "Технологические операции";
            dgvMain.Columns[2].HeaderText = "Исполнитель";
            dgvMain.Columns[3].HeaderText = "Технологические переходы";
            dgvMain.Columns[4].HeaderText = "Время действ., мин.";



            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // автоподбор ширины столбцов под ширину таблицы
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            //dgvMain.RowHeadersWidth = 25;

            // автоперенос в ячейках
            dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;


            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                column.ReadOnly = true;
                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }

            UpdateGrid();
        }

        public void UpdateGrid()
        {
            dgvMain.Rows.Clear();
            list.Clear();

            var TechOperationWorksListLocal = TechOperationWorksList.Where(w => w.Delete == false).ToList();

            int nomer = 1;
            foreach (TechOperationWork techOperationWork in TechOperationWorksListLocal)
            {
                //List<ExecutionWork> bb = context.ExecutionWorks.Where(w => w.techOperationWork == techOperationWork).ToList();

                List<ExecutionWork> bb = techOperationWork.executionWorks.Where(w=>w.Delete==false).ToList();

                foreach (ExecutionWork executionWork in bb)
                {
                    string techOperationName = "";
                    //if (HideTwo == false)
                    //{
                    //    techOperationName = techOperationWork.techOperation.Name;
                    //}


                    list.Add(new TechOperationDataGridItem
                    {
                        Nomer = nomer,
                        Staff = "",
                        TechOperation = techOperationWork.techOperation.Name,
                        TechTransition = executionWork.techTransition?.Name,
                        TechTransitionValue = executionWork.techTransition?.TimeExecution.ToString()
                    });
                    nomer++;
                }

            }

            foreach (TechOperationDataGridItem techOperationDataGridItem in list)
            {
                List<string> str = new List<string>();

                str.Add(techOperationDataGridItem.Nomer.ToString());
                str.Add(techOperationDataGridItem.TechOperation);
                str.Add(techOperationDataGridItem.Staff);
                str.Add(techOperationDataGridItem.TechTransition);
                str.Add(techOperationDataGridItem.TechTransitionValue);
                dgvMain.Rows.Add(str.ToArray());
            }
        }



        private void DgvMain_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // Первую строку всегда показывать
            if (e.RowIndex == 0)
                return;


            if (IsTheSameCellValue(e.ColumnIndex, e.RowIndex) && e.ColumnIndex == 1)
            {
                e.Value = string.Empty;
                e.FormattingApplied = true;
            }
        }

        private void DgvMain_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;

            // Пропуск заголовков колонок и строк, и первой строки
            if (e.RowIndex < 1 || e.ColumnIndex < 0)
            {
                e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.Single;
                return;
            }

            if (IsTheSameCellValue(e.ColumnIndex, e.RowIndex) && e.ColumnIndex == 1)
            {
                e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            }
            else
            {
                e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.Single;
            }

        }

        bool IsTheSameCellValue(int column, int row)
        {
            DataGridViewCell cell1 = dgvMain[column, row];
            DataGridViewCell cell2 = dgvMain[column, row - 1];
            if (cell1.Value == null || cell2.Value == null)
            {
                return false;
            }
            return cell1.Value.ToString() == cell2.Value.ToString();
        }

        private bool IsRepeatedCellValue(int rowIndex, int colIndex)
        {
            DataGridViewCell currCell = dgvMain.Rows[rowIndex].Cells[colIndex];
            DataGridViewCell prevCell = dgvMain.Rows[rowIndex - 1].Cells[colIndex];

            if (dgvMain.Rows[rowIndex].Cells[1].Value.Equals(dgvMain.Rows[rowIndex - 1].Cells[1].Value)
                && dgvMain.Rows[rowIndex].Cells[2].Value.Equals(dgvMain.Rows[rowIndex - 1].Cells[2].Value)
                && dgvMain.Rows[rowIndex].Cells[3].Value.Equals(dgvMain.Rows[rowIndex - 1].Cells[3].Value)
               )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddTechOperation(TechOperation TechOperat)
        {
            var vb = TechOperationWorksList.SingleOrDefault(s => s.techOperation == TechOperat);
            if (vb == null)
            {
                TechOperationWork techOperationWork = new TechOperationWork();
                techOperationWork.techOperation = TechOperat;
                techOperationWork.technologicalCard = TehCarta;
                techOperationWork.NewItem = true;

                TechOperationWorksList.Add(techOperationWork);
            }
            else
            {
                vb.Delete = false;
            }
        }


        public void AddTechTransition(TechTransition tech, TechOperationWork techOperationWork)
        {
            TechOperationWork TOWork = TechOperationWorksList.Single(s => s.Id == techOperationWork.Id);
            ExecutionWork exec = TOWork.executionWorks.SingleOrDefault(s => s.techTransition!=null && s.techTransition?.Id == tech.Id);
            if (exec == null)
            {
                ExecutionWork techOpeWork = new ExecutionWork();
                techOpeWork.techOperationWork = TOWork;
                techOpeWork.NewItem = true;
                techOpeWork.techTransition = tech;
                TOWork.executionWorks.Add(techOpeWork);
            }
            else
            {
                exec.Delete = false;
            }

        }


        public void DeleteTechOperation(TechOperationWork TechOperat)
        {
            var vb = TechOperationWorksList.SingleOrDefault(s => s == TechOperat);
            if (vb != null)
            {
                vb.Delete = true;
            }
        }
        public void DeleteTechTransit(ExecutionWork techTransit, TechOperationWork techOperationWork)
        {
            TechOperationWork TOWork = TechOperationWorksList.Single(s => s.Id == techOperationWork.Id);
            var vb = TOWork.executionWorks.SingleOrDefault(s => s == techTransit);
            if (vb != null)
            {
                vb.Delete = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            return;

            using (var context = new MyDbContext())
            {
                var TC = context.TechnologicalCards.Single(s => s.Id == 1);

                TechOperation techOperation = new TechOperation();
                techOperation.Name = "Установка автовышки";
                context.TechOperations.Add(techOperation);

                TechOperationWork techOperationWork = new TechOperationWork();
                techOperationWork.techOperation = techOperation;
                techOperationWork.technologicalCard = TC;

                var techOperation2 = new TechOperation();
                techOperation2.Name = "Подготовка к работе с люльки";
                context.TechOperations.Add(techOperation2);

                TechOperationWork techOperationWork2 = new TechOperationWork();
                techOperationWork2.techOperation = techOperation2;
                techOperationWork2.technologicalCard = TC;




                TechTransition techTransition = new TechTransition();
                techTransition.Name = "Определить место установки техники";
                techTransition.TimeExecution = 3;
                context.TechTransitions.Add(techTransition);

                ExecutionWork executionWork = new ExecutionWork();
                executionWork.techTransition = techTransition;
                executionWork.techOperationWork = techOperationWork;
                context.ExecutionWorks.Add(executionWork);

                techTransition = new TechTransition();
                techTransition.Name = "Установить автовышку";
                techTransition.TimeExecution = 5.5;
                context.TechTransitions.Add(techTransition);

                executionWork = new ExecutionWork();
                executionWork.techTransition = techTransition;
                executionWork.techOperationWork = techOperationWork;
                context.ExecutionWorks.Add(executionWork);

                techTransition = new TechTransition();
                techTransition.Name = "Установить ограждение рабочей зоны";
                techTransition.TimeExecution = 5;
                context.TechTransitions.Add(techTransition);

                executionWork = new ExecutionWork();
                executionWork.techTransition = techTransition;
                executionWork.techOperationWork = techOperationWork;
                context.ExecutionWorks.Add(executionWork);

                techTransition = new TechTransition();
                techTransition.Name = "Подготовить инструменты и материалы";
                techTransition.TimeExecution = 4;
                context.TechTransitions.Add(techTransition);

                executionWork = new ExecutionWork();
                executionWork.techTransition = techTransition;
                executionWork.techOperationWork = techOperationWork;
                context.ExecutionWorks.Add(executionWork);

                techTransition = new TechTransition();
                techTransition.Name = "Загрузить в люльку инструменты и материалы";
                techTransition.TimeExecution = 3;
                context.TechTransitions.Add(techTransition);

                executionWork = new ExecutionWork();
                executionWork.techTransition = techTransition;
                executionWork.techOperationWork = techOperationWork2;
                context.ExecutionWorks.Add(executionWork);

                techTransition = new TechTransition();
                techTransition.Name = "Надеть страховочную привязь";
                techTransition.TimeExecution = 1.5;
                context.TechTransitions.Add(techTransition);

                executionWork = new ExecutionWork();
                executionWork.techTransition = techTransition;
                executionWork.techOperationWork = techOperationWork2;
                context.ExecutionWorks.Add(executionWork);

                techTransition = new TechTransition();
                techTransition.Name = "Войти в люльку, закрепиться удерживающим стропом, закрыть дверь на запорное устройство";
                techTransition.TimeExecution = 1;
                context.TechTransitions.Add(techTransition);

                executionWork = new ExecutionWork();
                executionWork.techTransition = techTransition;
                executionWork.techOperationWork = techOperationWork2;
                context.ExecutionWorks.Add(executionWork);


                context.SaveChanges();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AddEditTechOperationForm addEditTechOperationForm = new AddEditTechOperationForm(this);
            addEditTechOperationForm.Show();
        }


        
    }
}
