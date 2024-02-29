using System;
using System.CodeDom.Compiler;
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
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Work
{
    public partial class TechOperationForm : Form
    {
        public MyDbContext context;
        public int tcId;
        private BindingSource binding;

        public List<TechOperationDataGridItem> list = null;
        public List<TechOperationWork> TechOperationWorksList;
        public TechnologicalCard TehCarta;

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

            DateTime t1 = DateTime.Now;

            TehCarta = context.TechnologicalCards.Where(w=>w.Id== tcId)
                .Include(t => t.Machines).Include(t => t.Machine_TCs)
            .Include(t => t.Protection_TCs)
            //.Include(t => t.Protections)
            .Include(t => t.Tool_TCs)
            .Include(t => t.Component_TCs)
           // .Include(t => t.Components)
            .Single(s => s.Id == tcId);

            DateTime t2 = DateTime.Now;

            TechOperationWorksList =
               context.TechOperationWorks.Where(w => w.TechnologicalCardId == tcId)
                    //.Include(i=>i.technologicalCard).ThenInclude(t=>t.Machine_TCs)
                    //.Include(i => i.technologicalCard).ThenInclude(t => t.Machines)
                   .Include(i => i.techOperation)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Protection_TCs)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Protections)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Tool_TCs)
                   //.Include(i => i.technologicalCard).ThenInclude(t => t.Component_TCs)
                   .Include(i => i.ComponentWorks).ThenInclude(t=>t.component)
                  
                   .Include(r => r.executionWorks).ThenInclude(t => t.techTransition)
                   .Include(r => r.executionWorks).ThenInclude(t => t.Machines)
                   .Include(r => r.executionWorks).ThenInclude(t => t.Staffs)
                   .Include(r=>r.ToolWorks).ThenInclude(r=>r.tool).ToList();

            DateTime t3 = DateTime.Now;

            UpdateGrid();

            DateTime t4 = DateTime.Now;
        }

        public void UpdateGrid()
        {
            dgvMain.Rows.Clear();
            list.Clear();

            while (dgvMain.Columns.Count > 0)
            {
                dgvMain.Columns.RemoveAt(0);
            }

            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");
            dgvMain.Columns.Add("", "");


            foreach (Machine_TC tehCartaMachineTC in TehCarta.Machine_TCs)
            {
                dgvMain.Columns.Add("", "Время " + tehCartaMachineTC.Child.Name + ", мин.");
            }

            dgvMain.Columns.Add("", "№ СЗ");

            dgvMain.Columns[0].HeaderText = "№";
            dgvMain.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvMain.Columns[0].Width = 30;

            dgvMain.Columns[1].HeaderText = "Технологические операции";
            dgvMain.Columns[2].HeaderText = "Исполнитель";
            dgvMain.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvMain.Columns[2].Width = 120;
            dgvMain.Columns[3].HeaderText = "Технологические переходы";
            dgvMain.Columns[4].HeaderText = "Время действ., мин.";

            dgvMain.Columns[5].HeaderText = "Время этапа, мин.";


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




            var TechOperationWorksListLocal = TechOperationWorksList.Where(w => w.Delete == false).OrderBy(o => o.Order).ToList();



            int nomer = 1;
            foreach (TechOperationWork techOperationWork in TechOperationWorksListLocal)
            {
                List<ExecutionWork> bb = techOperationWork.executionWorks.Where(w => w.Delete == false).OrderBy(o => o.Order).ToList();

                foreach (ExecutionWork executionWork in bb)
                {
                    if (executionWork.IdGuid == new Guid())
                    {
                        executionWork.IdGuid = Guid.NewGuid();
                    }

                    string StaffStr = "";

                    foreach (Staff_TC executionWorkStaff in executionWork.Staffs)
                    {
                        if (StaffStr != "")
                        {
                            StaffStr += ",";
                        }

                        StaffStr += executionWorkStaff.Symbol;
                    }



                    string ProtectStr = "";
                    foreach (Protection_TC executionWorkProtection in executionWork.Protections)
                    {
                        if (ProtectStr != "")
                        {
                            ProtectStr += ",";
                        }

                        ProtectStr += executionWorkProtection.Order;
                    }

                    List<bool> mach = new List<bool>();

                    foreach (Machine_TC tehCartaMachineTC in TehCarta.Machine_TCs)
                    {
                        var sing = executionWork.Machines.SingleOrDefault(s => s == tehCartaMachineTC);


                        if (sing == null)
                        {
                            mach.Add(false);

                        }
                        else
                        {
                            mach.Add(true);
                        }
                    }




                    list.Add(new TechOperationDataGridItem
                    {
                        Nomer = nomer,
                        Staff = StaffStr,
                        TechOperation = techOperationWork.techOperation.Name,
                        TechTransition = executionWork.techTransition?.Name,
                        TechTransitionValue = executionWork.Value.ToString(),
                        Protections = ProtectStr,
                        Etap = executionWork.Etap,
                        Posled = executionWork.Posled,
                        Work = true,
                        techWork = executionWork,
                        listMach = mach
                    });


                    nomer++;
                }


                foreach (ToolWork toolWork in techOperationWork.ToolWorks)
                {
                    string strComp =
                        $"{toolWork.tool.Name}   {toolWork.tool.Type}    {toolWork.tool.Unit}";
                    list.Add(new TechOperationDataGridItem
                    {
                        Nomer = nomer,
                        Staff = "",
                        TechOperation = techOperationWork.techOperation.Name,
                        TechTransition = strComp,
                        TechTransitionValue = toolWork.Quantity.ToString(),
                        ItsTool = true
                    });
                    nomer++;
                }

                foreach (ComponentWork componentWork in techOperationWork.ComponentWorks)
                {
                    string strComp =
                        $"{componentWork.component.Name}   {componentWork.component.Type}    {componentWork.component.Unit}";

                    list.Add(new TechOperationDataGridItem
                    {
                        Nomer = nomer,
                        Staff = "",
                        TechOperation = techOperationWork.techOperation.Name,
                        TechTransition = strComp,
                        TechTransitionValue = componentWork.Quantity.ToString(),
                        ItsComponent = true
                    });
                    nomer++;
                }

            }


            for (var index = 0; index < list.Count; index++)
            {
                TechOperationDataGridItem techOperationDataGridItem = list[index];

                List<ExecutionWork> podchet = new List<ExecutionWork>();
                List<TechOperationDataGridItem> podchet2 = new List<TechOperationDataGridItem>();
                List<TechOperationDataGridItem> TampPodchet = new List<TechOperationDataGridItem>();

                if (techOperationDataGridItem.Work)
                {
                    podchet.Add(techOperationDataGridItem.techWork);
                    podchet2.Add(techOperationDataGridItem);
                    if (techOperationDataGridItem.Etap != "" && techOperationDataGridItem.Etap != "0")
                    {
                        int plusIndex = index;
                        while (true)
                        {
                            plusIndex = plusIndex + 1;
                            if (plusIndex < list.Count)
                            {
                                TechOperationDataGridItem tech2 = list[plusIndex];
                                if (tech2.Work == false)
                                {
                                    TampPodchet.Add(tech2);
                                    continue;
                                }

                                if (techOperationDataGridItem.Etap == tech2.Etap)
                                {
                                    index = plusIndex;
                                    podchet.Add(tech2.techWork);
                                    podchet2.Add(tech2);
                                    tech2.TimeEtap = "-1";
                                    TampPodchet.ForEach(f => f.TimeEtap = "-1");
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    double Paral = 0;

                    foreach (ExecutionWork executionWork in podchet)
                    {
                        if (executionWork.Posled != "" && executionWork.Posled != "0")
                        {
                            var allSum = podchet.Where(w => w.Posled == executionWork.Posled)
                                .Sum(s => s.Value);
                            executionWork.TempTimeExecution = allSum;
                        }
                        else
                        {
                            executionWork.TempTimeExecution = executionWork.Value;
                        }
                    }


                    var col = podchet2[0].listMach.Count;
                    if (podchet2.Count > 1)
                    {

                        for (int i = 0; i < col; i++)
                        {
                            bool tr = false;
                            foreach (TechOperationDataGridItem operationDataGridItem in podchet2)
                            {
                                if (operationDataGridItem.listMach[i] == true)
                                {
                                    tr = true;
                                }
                            }

                            if (tr)
                            {
                                foreach (TechOperationDataGridItem operationDataGridItem in podchet2)
                                {
                                    operationDataGridItem.listMach[i] = true;
                                }
                            }
                        }
                    }



                    Paral = podchet.Max(m => m.TempTimeExecution);
                    techOperationDataGridItem.TimeEtap = Paral.ToString();
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
                str.Add(techOperationDataGridItem.TimeEtap);

                techOperationDataGridItem.listMachStr = new List<string>();

                if (techOperationDataGridItem.listMachStr.Count == 0 && techOperationDataGridItem.listMach.Count>0)
                {
                    for (var index = 0; index < TehCarta.Machine_TCs.Count; index++)
                    {
                        bool b = techOperationDataGridItem.listMach[index];
                        if (b)
                        {
                            str.Add(techOperationDataGridItem.TimeEtap);
                        }
                        else
                        {
                            if (techOperationDataGridItem.TimeEtap == "-1")
                            {
                                str.Add("-1");
                            }
                            else
                            {
                                str.Add("");
                            }
                        }
                    }
                }

                str.Add(techOperationDataGridItem.Protections);
                dgvMain.Rows.Add(str.ToArray());

                if (techOperationDataGridItem.ItsComponent)
                {
                    dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[3].Style.BackColor = Color.Salmon;
                    dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[4].Style.BackColor = Color.Salmon;
                }

                if (techOperationDataGridItem.ItsTool)
                {
                    dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[3].Style.BackColor = Color.Aquamarine;
                    dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[4].Style.BackColor = Color.Aquamarine;
                }

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

            if (e.ColumnIndex >= 5)
            {
                var bb = (string)e.Value;
                if (bb == "-1")
                {
                    e.Value = string.Empty;
                    e.FormattingApplied = true;
                }
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

            if (e.ColumnIndex >= 5)
            {
                var bb = (string)e.Value;
                if (bb == "-1")
                {
                    e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
                }
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
            var exec = TOWork.executionWorks.Where(w => w.techTransition == tech && w.Delete == true).ToList();

            if (exec.Count > 0)
            {
                var one = exec[0];
                one.Delete = false;
            }
            else
            {
                ExecutionWork techOpeWork = new ExecutionWork();
                techOpeWork.IdGuid = Guid.NewGuid();
                techOpeWork.techOperationWork = TOWork;
                techOpeWork.NewItem = true;
                techOpeWork.techTransition = tech;
                techOpeWork.Value = tech.TimeExecution;
                TOWork.executionWorks.Add(techOpeWork);
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
        public void DeleteTechTransit(Guid IdGuid, TechOperationWork techOperationWork)
        {
            TechOperationWork TOWork = TechOperationWorksList.Single(s => s.Id == techOperationWork.Id);
            var vb = TOWork.executionWorks.SingleOrDefault(s => s.IdGuid == IdGuid);
            if (vb != null)
            {
                vb.Delete = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            using (var context = new MyDbContext())
            {
                var TC = context.TechnologicalCards.Single(s => s.Id == 1);

                var ff = context.Protections.Take(5).ToList();


                foreach (Protection protection in ff)
                {
                    Protection_TC tt = new Protection_TC();
                    tt.Child = protection;
                    tt.Quantity = 5;
                    TC.Protection_TCs.Add(tt);
                }

                context.SaveChanges();
            }

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

        private void button1_Click_1(object sender, EventArgs e)
        {
            //context

            List<TechOperationWork> AllDele = TechOperationWorksList.Where(w => w.Delete == true).ToList();
            foreach (TechOperationWork techOperationWork in AllDele)
            {
                TechOperationWorksList.Remove(techOperationWork);
                context.TechOperationWorks.Remove(techOperationWork);
            }

            foreach (TechOperationWork techOperationWork in TechOperationWorksList)
            {
                var to = context.TechOperationWorks.SingleOrDefault(s => techOperationWork.Id!=0 && s.Id == techOperationWork.Id);
                if (to == null)
                {
                    context.TechOperationWorks.Add(techOperationWork);
                }
                else
                {
                    to = techOperationWork;

                    //foreach (ExecutionWork executionWork in to.executionWorks)
                    //{
                    //    if (executionWork.Delete)
                    //    {
                    //        to.executionWorks.Remove(executionWork);
                    //        continue;
                    //    }

                    //    var exx = to.executionWorks.SingleOrDefault(s => s.Id != 0 && s.Id == executionWork.Id);
                    //    if (exx == null)
                    //    {
                    //        to.executionWorks.Add(executionWork);
                    //    }
                    //    else
                    //    {
                            
                    //    }

                    //}
                }
            }
            

            try
            {
                context.SaveChanges();
                MessageBox.Show("Успешно сохранено");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            


        }
    }
}
