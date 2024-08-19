using ExcelParsing.DataProcessing;
using Microsoft.EntityFrameworkCore;
//using NCalc;
using System.CodeDom;
using System.Data;
using System.Windows.Forms;
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
        private List<Staff> AllStaff;
        private List<ExecutionWork> listExecutionWork;

        private TechOperationWork SelectedTO => (TechOperationWork)comboBoxTO.SelectedItem;

        public AddEditTechOperationForm()
        {
            InitializeComponent();
        }

        public AddEditTechOperationForm(TechOperationForm techOperationForm)
        {
            InitializeComponent();
            TechOperationForm = techOperationForm;

            this.Text = $"{TechOperationForm.TehCarta.Name} ({TechOperationForm.TehCarta.Article}) - Редактор хода работ";

            //var context = techOperationForm.context;

            // dataGridViewAllTO.CellContentClick += DataGridViewAllTO_CellContentClick;
            dataGridViewAllTO.CellClick += DataGridViewAllTO_CellClick;

            dataGridViewTO.CellClick += DataGridViewTO_CellClick;

            //dataGridViewTPAll.CellContentClick += DataGridViewTPAll_CellContentClick;
            dataGridViewTPAll.CellClick += DataGridViewTPAll_CellClick;
            dataGridViewTPAll.SelectionChanged += DataGridViewTPAll_SelectionChanged;

            dataGridViewTPLocal.CellClick += DataGridViewTPLocal_CellClick;
            dataGridViewTPLocal.CellEndEdit += DataGridViewTPLocal_CellEndEdit;
            dataGridViewTPLocal.CellFormatting += DataGridViewTPLocal_CellFormatting; 
            dataGridViewTPLocal.SelectionChanged += DataGridViewTPLocal_SelectionChanged;

            dataGridViewStaff.CellContentClick += DataGridViewStaff_CellContentClick;
            dataGridViewStaff.CellClick += DataGridViewStaff_CellClick;
            dataGridViewStaff.CellEndEdit += DataGridViewStaff_CellEndEdit;
            dataGridViewStaff.CellBeginEdit += DataGridViewStaff_CellBeginEdit;


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


            dataGridViewPovtor.CellContentClick += DataGridViewPovtor_CellContentClick;
            dataGridViewPovtor.CellValueChanged += DataGridViewPovtor_CellValueChanged;
            dataGridViewPovtor.CellFormatting += dataGridViewPovtor_CellFormatting;
            dataGridViewPovtor.SelectionChanged += DataGridViewPovtor_SelectionChanged;


            comboBoxFiltrCategor.SelectedIndexChanged += ComboBoxFiltrCategor_SelectedIndexChanged;
            comboBoxFilterComponent.SelectedIndexChanged += ComboBoxFilterComponent_SelectedIndexChanged;

            textBoxPoiskTo.TextChanged += TextBoxPoiskTo_TextChanged;
            textBoxPoiskComponent.TextChanged += TextBoxPoiskComponent_TextChanged;
            textBoxPoiskTP.TextChanged += TextBoxPoiskTP_TextChanged;
            textBoxPoiskSZ.TextChanged += TextBoxPoiskSZ_TextChanged;
            textBoxPoiskMach.TextChanged += TextBoxPoiskMach_TextChanged;
            PoiskPersonal.TextChanged += TextBoxPersonalPoisk_TextChanged;

            comboBoxTPCategoriya.SelectedIndexChanged += ComboBoxTPCategoriya_SelectedIndexChanged;

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


        private void TextBoxPoiskTo_TextChanged(object? sender, EventArgs e)
        {
            UpdateTO();
        }


        private void DataGridViewAllTO_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var IddGuid = (TechOperation)dataGridViewAllTO.Rows[e.RowIndex].Cells[0].Value;

                AddTOWToGridLocalTO(IddGuid);
            }
        }

        private void AddTOWToGridLocalTO(TechOperation techOperationWork, bool Update = false)
        {
            if (Update)
            {
                var temp = TechOperationForm.context.TechOperations.Single(s => s.Id == techOperationWork.Id);
                techOperationWork = temp;
            }

            TechOperationForm.AddTechOperation(techOperationWork);
            UpdateLocalTO();
            TechOperationForm.UpdateGrid();
        }

        private void DataGridViewTO_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить ТО?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var IddGuid = (TechOperationWork)dataGridViewTO.Rows[e.RowIndex].Cells[0].Value;

                    TechOperationForm.DeleteTechOperation(IddGuid);
                    UpdateLocalTO();
                    TechOperationForm.UpdateGrid();
                }
            }
        }

        public void UpdateTO()
        {
            var offScroll = dataGridViewAllTO.FirstDisplayedScrollingRowIndex;

            //DataGridViewRow currentRow = dataGridViewAllTO.CurrentRow;
            int currentRowIndex = dataGridViewAllTO.CurrentRow?.Index ?? 0;

            dataGridViewAllTO.Rows.Clear();

            var context = TechOperationForm.context;
            allTO = context
                .TechOperations
                .Include(t => t.techTransitionTypicals)
                .ToList();

            var filteredOperations = FilterTechOperations(textBoxPoiskTo.Text);
            foreach (var operation in filteredOperations)
            {
                AddTechOperationToGridAllTO(operation);
            }

            if (offScroll > 0 && offScroll < dataGridViewAllTO.Rows.Count)
                dataGridViewAllTO.FirstDisplayedScrollingRowIndex = offScroll;
        }
        private IEnumerable<TechOperation> FilterTechOperations(string searchText)
        {
            return allTO.Where(to => (string.IsNullOrEmpty(searchText) || to.Name.ToLower().Contains(searchText.ToLower()))
                && (to.IsReleased == true || to.CreatedTCId == TechOperationForm.tcId));
        }
        private void AddTechOperationToGridAllTO(TechOperation techOperation)
        {
            var row = new List<object>
            {
                techOperation,
                "Добавить",
                techOperation.Name,
                techOperation.Category == "Типовая ТО"
            };
            dataGridViewAllTO.Rows.Add(row.ToArray());
        }

        public void UpdateLocalTO()
        {
            var offScroll = dataGridViewTO.FirstDisplayedScrollingRowIndex;
            dataGridViewTO.Rows.Clear();

            List<TechOperationWork> list = TechOperationForm.TechOperationWorksList
                .Where(w => w.Delete == false)
                .OrderBy(o => o.Order)
                .ToList();

            foreach (TechOperationWork techOperationWork in list)
            {
                AddTechOperationToGridLocalTO(techOperationWork);
            }

            List<TechOperationWork> list2 = new List<TechOperationWork>(list);
            comboBoxTO.DataSource = list2;
            comboBoxTO2.DataSource = list2;
            comboBoxTO3.DataSource = list2;

            if (offScroll > 0 && offScroll < dataGridViewTO.Rows.Count)
                dataGridViewTO.FirstDisplayedScrollingRowIndex = offScroll;

        }
        private void AddTechOperationToGridLocalTO(TechOperationWork techOperationWork)
        {
            var row = new List<object>
            {
                techOperationWork,
                "Удалить",
                techOperationWork.techOperation.Name,
                techOperationWork.techOperation.Category == "Типовая ТО"
            };
            dataGridViewTO.Rows.Add(row.ToArray());
        }

        private void comboBoxTO_SelectedIndexChanged(object sender, EventArgs e)
        {
            //var work = (TechOperationWork)comboBoxTO.SelectedItem;
            UpdateGridAllTP();
            UpdateGridLocalTP();
        }


        #endregion

        #region TP


        private void DataGridViewTPLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6)
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
            else if (e.ColumnIndex == 4)
            {
                var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var idd = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
                var work = (TechOperationWork)comboBoxTO.SelectedItem;



                var wor = work.executionWorks.SingleOrDefault(s => s.IdGuid == idd);
                if (wor != null)
                {

                    var oldValue = wor.Coefficient;

                    if (oldValue == gg)
                    {
                        return;
                    }
                    try
                    {
                        wor.Coefficient = gg ?? "";
                        try
                        {
                            var bn = WorkParser.EvaluateExpression(wor.techTransition?.TimeExecution.ToString().Replace(',', '.') + "*" + wor.Coefficient.Replace(',', '.')); //ee.Evaluate();
                            wor.Value = Math.Round(bn, 2);//double.Parse(bn.ToString()), 2);
                        }
                        catch (Exception)
                        {
                            wor.Value = -1;
                        }


                        // todo: реализовать обновление только ячейки времени выполнения, а не всей таблицы

                        //var index = dataGridViewTPLocal.Columns["Value"].Index;

                        //if (index == -1)
                        //{
                        //    return;
                        //}

                        //dataGridViewTPLocal.Rows[e.RowIndex].Cells[index].Value = wor.Value == -1 ? "Ошибка" : wor.Value;

                        BeginInvoke(new Action(() =>
                        {
                            UpdateGridLocalTP();
                        }));

                        TechOperationForm.UpdateGrid();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else if (e.ColumnIndex == 7)
            {
                var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                var idd = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
                var work = (TechOperationWork)comboBoxTO.SelectedItem;
                // TechOperationForm.TechOperationWorksList.Single(s => s.Id == work.Id).executionWorks.Single(s => s.IdGuid == idd).
                var wor = work.executionWorks.SingleOrDefault(s => s.IdGuid == idd);
                if (wor != null)
                {
                    wor.PictureName = gg;
                    TechOperationForm.UpdateGrid();
                }
            }

        }

        private void DataGridViewTPLocal_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var work = (TechOperationWork)comboBoxTO.SelectedItem;
                var IddGuid = (Guid)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
                //var tech = allTP.Single(s => s.Id == Idd);
                //TechOperationForm.AddTechTransition(tech, work);

                // запрос на подтверждение удаления
                var result = MessageBox.Show("Вы уверены, что хотите удалить ТП?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {

                    TechOperationForm.DeleteTechTransit(IddGuid, work);
                    UpdateGridLocalTP();
                    TechOperationForm.UpdateGrid();
                }
            }
        }

        private void DataGridViewTPLocal_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dataGridViewTPLocal.Columns["PictureName"].Index
                || e.ColumnIndex == dataGridViewTPLocal.Columns["Comment"].Index)// Индекс столбца с checkBox
            {
                TechOperationForm.CellChangeReadOnly(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex], false);

            }
            else if (e.ColumnIndex == dataGridViewTPLocal.Columns["Coefficient"].Index)
            {
                var nameIndex = dataGridViewTPLocal.Columns["dataGridViewTextBoxColumn8"]?.Index ?? 0;
                string rowName = dataGridViewTPLocal.Rows[e.RowIndex].Cells[nameIndex].Value.ToString() ?? "";

                if (rowName == "Повторить")
                {
                    TechOperationForm.CellChangeReadOnly(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex], true);
                }
                else
                {
                    TechOperationForm.CellChangeReadOnly(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex], false);
                }
            }
            else
            {
                TechOperationForm.CellChangeReadOnly(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex], true);
            }
        }
        private void DataGridViewTPLocal_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewTPLocal.SelectedRows.Count > 0)
            {
                // Get the ExecutionWork object corresponding to the selected row
                var selectedRow = dataGridViewTPLocal.SelectedRows[0];
                var id = (Guid)selectedRow.Cells[0].Value;
                var executionWork = FindExecutionWorkById(id);

                if (executionWork != null)
                {
                    TechOperationForm.HighlightExecutionWorkRow(executionWork, false);
                }
            }
        }

        // Method to find ExecutionWork by its ID
        private ExecutionWork? FindExecutionWorkById(Guid id)
        {
            foreach (var techOperationWork in TechOperationForm.TechOperationWorksList)
            {
                var executionWork = techOperationWork.executionWorks.SingleOrDefault(ew => ew.IdGuid == id);
                if (executionWork != null)
                {
                    return executionWork;
                }
            }
            return null;
        }

        private void DataGridViewTPAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var work = (TechOperationWork)comboBoxTO.SelectedItem;
                var Idd = (TechTransition)dataGridViewTPAll.Rows[e.RowIndex].Cells[0].Value;


                if (Idd.Name != "Повторить п.")
                {
                    CoefficientForm coefficient = new CoefficientForm(Idd);

                    if (coefficient.ShowDialog() == DialogResult.OK)
                    {
                        TechOperationForm.AddTechTransition(Idd, work, null, coefficient);
                        UpdateGridLocalTP();
                        TechOperationForm.UpdateGrid();
                    }
                }
                else
                {
                    TechOperationForm.AddTechTransition(Idd, work, null, null);
                    UpdateGridLocalTP();
                    TechOperationForm.UpdateGrid();
                }
            }
        }

        private void AddNewTP(TechTransition TP, TechOperationWork TOW)
        {
            TechOperationForm.AddTechTransition(TP, TOW);
            UpdateGridLocalTP();
            TechOperationForm.UpdateGrid();
        }

        private void DataGridViewTPAll_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridViewTPAll.SelectedRows.Count > 0)
            {
                var dc = (TechTransition)dataGridViewTPAll.SelectedRows[0].Cells[0].Value;

                labelComName.Text = dc.CommentName;
                labelComTime.Text = dc.CommentTimeExecution;
            }
        }

        private void TextBoxPoiskTP_TextChanged(object? sender, EventArgs e)
        {
            UpdateGridAllTP();
        }
        private IEnumerable<TechTransition> FilterTechTransitions(string searchText)
        {
            return allTP.Where(to =>
            (string.IsNullOrEmpty(searchText) || to.Name.ToLower().Contains(searchText.ToLower()))

                && ((comboBoxTPCategoriya.SelectedIndex == 0 || string.IsNullOrEmpty((string)comboBoxTPCategoriya.SelectedItem))
                    ||
                    to.Category == (string)comboBoxTPCategoriya.SelectedItem)
                && (to.IsReleased == true || to.CreatedTCId == TechOperationForm.tcId)
                )
            ;
        }
        public void UpdateGridAllTP()
        {
            var offScroll = dataGridViewTPAll.FirstDisplayedScrollingRowIndex;
            dataGridViewTPAll.Rows.Clear();

            var work = (TechOperationWork)comboBoxTO.SelectedItem;

            if (work == null)
            {
                return;
            }

            var context = TechOperationForm.context;

            allTP = context.TechTransitions.ToList(); // todo: добавить фильтрацию по выпуску и номеру карты
            //var list = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.ToList();

            bool AddCategor = false;
            if (comboBoxTPCategoriya.Items.Count == 0)
            {
                AddCategor = true;
                comboBoxTPCategoriya.Items.Add("Все");

                var allCategories = allTP.Select(tp => tp.Category).Distinct();
                foreach (var category in allCategories)
                {
                    if (string.IsNullOrEmpty(category))
                    {
                        continue;
                    }
                    comboBoxTPCategoriya.Items.Add(category);
                }
            }

            //TechTransition povtor = new TechTransition();
            //povtor.Name = "Повторить";
            //List<object> listItem1 = new List<object>
            //{
            //    povtor,
            //    "Добавить",
            //    povtor.Name
            //};
            //dataGridViewTPAll.Rows.Add(listItem1.ToArray());

            

            var filteredTransitions = FilterTechTransitions(textBoxPoiskTP.Text);

            // находим ТП "Повторить п." и добавляем его первым в список
            var repeatTechTransition = allTP.SingleOrDefault(tp => tp.Name == "Повторить п.");
            if (repeatTechTransition != null)
            {
                List<object> listItem = new()
                {
                    repeatTechTransition,
                    "Добавить",
                    repeatTechTransition.Name,
                    repeatTechTransition.TimeExecution
                };
                dataGridViewTPAll.Rows.Add(listItem.ToArray());
            }

            if (work?.techOperation.Category == "Типовая ТО")
            {
                return;
            }

            foreach (TechTransition techTransition in filteredTransitions)// allTP)
            {
                if (techTransition.Name == "Повторить п.")
                {
                    continue;
                }

                List<object> listItem = new()
                {
                    techTransition,
                    "Добавить",
                    techTransition.Name,
                    techTransition.TimeExecution
                };

                dataGridViewTPAll.Rows.Add(listItem.ToArray());
            }

            try
            {
                if (offScroll > 0 && offScroll < dataGridViewTPAll.Rows.Count)
                    dataGridViewTPAll.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {

            }

        }



        private void ComboBoxTPCategoriya_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateGridAllTP();
        }

        public void UpdateGridLocalTP()
        {
            var offScroll = dataGridViewTPLocal.FirstDisplayedScrollingRowIndex;

            dataGridViewTPLocal.Rows.Clear();
            var work = (TechOperationWork)comboBoxTO.SelectedItem;

            if (work == null)
            {
                return;
            }

            var LocalTPs = TechOperationForm.TechOperationWorksList.Single(s => s == work)
                .executionWorks.Where(w => w.Delete == false)
                .OrderBy(o => o.Order).ToList();

            foreach (ExecutionWork executionWork in LocalTPs)
            {
                List<object> listItem = new List<object>
                {
                    executionWork.IdGuid
                };

                // добавляем кнопку "Удалить" только для не типовых ТО
                if (work.techOperation.Category == "Типовая ТО" && executionWork.Repeat == false)
                {
                    listItem.Add("");
                }
                else
                {
                    listItem.Add("Удалить");
                }

                if (executionWork.Repeat)
                {
                    listItem.Add("Повторить");
                    listItem.Add("");

                    listItem.Add("");
                    //listItem.Add("");
                }
                else
                {
                    listItem.Add(executionWork.techTransition?.Name);
                    listItem.Add(executionWork.techTransition?.TimeExecution);

                    listItem.Add(executionWork.Coefficient);

                    //if(executionWork.Value==-1)
                    //{
                    //    listItem.Add("Ошибка");
                    //}
                    //else
                    //{
                    //    listItem.Add(executionWork.Value);
                    //}
                }

                //////////////////////////////////////////////26.06.2024
                //listItem.Add(executionWork.techTransition?.TimeExecution);

                //listItem.Add(executionWork.Coefficient);


                if (executionWork.Value == -1)
                {
                    listItem.Add("Ошибка");
                }
                else
                {
                    listItem.Add(executionWork.Value);
                }
                ////////////////////////////////////////////////////////

                listItem.Add(executionWork.Comments);

                listItem.Add(executionWork.PictureName);

                dataGridViewTPLocal.Rows.Add(listItem.ToArray());
            }

            listExecutionWork = new List<ExecutionWork>(LocalTPs);

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
                if (offScroll > 0 && offScroll < dataGridViewTPLocal.Rows.Count)
                    dataGridViewTPLocal.FirstDisplayedScrollingRowIndex = offScroll;

            }
            catch (Exception e)
            {
            }

        }
        #endregion

        #region Staff
        private void DataGridViewStaff_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewStaff.CommitEdit(DataGridViewDataErrorContexts.Commit);
            ClickDataGridViewStaff();
        }

        private void TextBoxPersonalPoisk_TextChanged(object? sender, EventArgs e)
        {
            UpdateStaffAll();
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
        private void DataGridViewStaff_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)  // Проверка, что это столбец с чекбоксами
            {
                var staff_TC = (Staff_TC)dataGridViewStaff.Rows[e.RowIndex].Cells[0].Value;
                var symbol = staff_TC.Symbol;
                var EW = (ExecutionWork)comboBoxSZ.SelectedItem;
                if (EW != null)
                {
                    var staffs = EW.Staffs.Where(w => w.Symbol == symbol).ToList();

                    // Проверяем, есть ли уже такой символ среди выбранных
                    if (staffs.Count >= 1 && (bool)dataGridViewStaff.Rows[e.RowIndex].Cells[2].Value == false)
                    {
                        MessageBox.Show("Роль с таким обозначением уже добавлена в переход");
                        e.Cancel = true;
                    }
                }
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

                Task.Run(() =>
                {
                    this.BeginInvoke((Action)(() => UpdateGridStaff()));
                });
            }
        }

        private void DataGridViewStaffAll_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
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
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var idd = (Staff_TC)dataGridViewStaff.Rows[e.RowIndex].Cells[0].Value;

                if (idd != null)
                {
                    if (MessageBox.Show("Вы действительно хотите полностью удалить данную роль из техкарты?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                    {
                        var vv = TechOperationForm.TehCarta.Staff_TCs;
                        vv.Remove(idd);

                        UpdateGridStaff();
                    }
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

            TechOperationForm.HighlightExecutionWorkRow(ExecutionWorkBox, true);

            var work = (TechOperationWork)comboBoxTO.SelectedItem;
            var LocalTP = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.Single(s => s.IdGuid == ExecutionWorkBox.IdGuid);

            var AllStaff = TechOperationForm.TehCarta.Staff_TCs.OrderBy(x => x.Symbol);


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
                if (offScroll > 0 && offScroll < dataGridViewStaff.Rows.Count)
                    dataGridViewStaff.FirstDisplayedScrollingRowIndex = offScroll;

            }
            catch (Exception e)
            {
            }

        }

        public void UpdateStaffAll()
        {
            var offScroll = dataGridViewStaffAll.FirstDisplayedScrollingRowIndex;

            var context = TechOperationForm.context;
            AllStaff = context.Staffs.ToList();

            dataGridViewStaffAll.Rows.Clear();

            var filteredPersonal = FilterStaff(PoiskPersonal.Text);
            foreach (Staff staff in filteredPersonal)
            {
                AddStuffToGridAllStaff(staff);
            }

            if (offScroll > 0 && offScroll < dataGridViewStaffAll.Rows.Count)
                dataGridViewStaffAll.FirstDisplayedScrollingRowIndex = offScroll;
        }

        public void UpdateGridStaffAll()
        {
            var ExecutionWorkBox = (ExecutionWork)comboBoxStaff.SelectedItem;

            var context = TechOperationForm.context;
            AllStaff = context.Staffs.ToList();

            dataGridViewStaffAll.Rows.Clear();
            if (ExecutionWorkBox == null)
            {
                return;
            }

            var filteredPersonal = FilterStaff(PoiskPersonal.Text);
            foreach (Staff staff in filteredPersonal)
            {
                AddStuffToGridAllStaff(staff);
            }
        }

        private void AddStuffToGridAllStaff(Staff staff)
        {
            List<object> staffRow = new List<object>
                {
                    staff,
                    "Добавить",
                    staff.Name,
                    staff.Type,
                    staff.Functions,
                    staff.CombineResponsibility ?? "",
                    staff.Qualification,
                    staff.Comment ?? ""
                };
            dataGridViewStaffAll.Rows.Add(staffRow.ToArray());
        }

        private IEnumerable<Staff> FilterStaff(string searchText)
        {
            return AllStaff.Where(stf => string.IsNullOrEmpty(searchText) || stf.Name.ToLower().Contains(searchText.ToLower()));
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

            TechOperationForm.HighlightExecutionWorkRow(work, true);

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


                List<object> listItem = new List<object>
                {
                    prot.Child,
                    "Добавить",
                    prot.Child.Name,
                    prot.Child.Type,
                    prot.Child.Unit,
                    prot.Quantity
                };

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

                List<object> listItem = new List<object>
                {
                    prot,
                    "Добавить",
                    prot.Name,
                    prot.Type,
                    prot.Unit,
                    ""
                };
                dataGridViewAllSZ.Rows.Add(listItem.ToArray());
            }

            try
            {
                if (offScroll > 0 && offScroll < dataGridViewAllSZ.Rows.Count)
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
                List<object> listItem = new List<object>
                {
                    sz,
                    "Удалить",
                    sz.Child.Name,
                    sz.Child.Type,
                    sz.Child.Unit,
                    sz.Quantity
                };
                dataGridViewLocalSZ.Rows.Add(listItem.ToArray());
            }

            try
            {
                if (offScroll > 0 && offScroll < dataGridViewLocalSZ.Rows.Count)
                    dataGridViewLocalSZ.FirstDisplayedScrollingRowIndex = offScroll;
            }
            catch (Exception e)
            {
            }

        }

        private void DataGridViewAllSZ_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
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
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
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

            if (work == null)
            {
                return;
            }

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


                List<object> listItem = new List<object>
                {
                    componentTc.Child,
                    "Добавить",
                    componentTc.Child.Name,
                    componentTc.Child.Type,
                    componentTc.Child.Unit,
                    componentTc.Quantity
                };
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


                List<object> listItem = new List<object>
                {
                    component,
                    "Добавить",
                    component.Name,
                    component.Type,
                    component.Unit,
                    ""
                };
                dataGridViewComponentAll.Rows.Add(listItem.ToArray());
            }


            if (UpdFilt)
            {
                comboBoxFilterComponent.DataSource = AllFilterComponent;
                comboBoxFilterComponent.SelectedIndex = 0;
            }


            try
            {
                if (offScroll > 0 && offScroll < dataGridViewComponentAll.Rows.Count)
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
            if (work == null)
            {
                return;
            }


            var LocalComponent = work.ComponentWorks.ToList();

            foreach (ComponentWork componentWork in LocalComponent)
            {
                List<object> listItem = new List<object>
                {
                    componentWork,
                    "Удалить",
                    componentWork.component.Name,
                    componentWork.component.Type,
                    componentWork.component.Unit,
                    componentWork.Quantity.ToString(),
                    componentWork.Comments
                };
                dataGridViewComponentLocal.Rows.Add(listItem.ToArray());
            }

            try
            {
                if (offScroll > 0 && offScroll < dataGridViewComponentLocal.Rows.Count)
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
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
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
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
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

            if (e.ColumnIndex == 6)
            {
                var work = (TechOperationWork)comboBoxTO2.SelectedItem;
                var Idd = (ComponentWork)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[0].Value;
                var value = (string)dataGridViewComponentLocal.Rows[e.RowIndex].Cells[6].Value;

                Idd.Comments = value ?? "";
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

            if (work == null)
            {
                return;
            }

            var LocalInstrument = work.ToolWorks.ToList();

            foreach (var InstrumentWork in LocalInstrument)
            {
                List<object> listItem = new List<object>
                {
                    InstrumentWork,
                    "Удалить",
                    InstrumentWork.tool.Name,
                    InstrumentWork.tool.Type,
                    InstrumentWork.tool.Unit,
                    InstrumentWork.Quantity,
                    InstrumentWork.Comments
                };
                dataGridViewInstrumentLocal.Rows.Add(listItem.ToArray());
            }

            try
            {
                if (offScroll > 0 && offScroll < dataGridViewInstrumentLocal.Rows.Count)
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

            if (work == null)
            {
                return;
            }

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

                if (comboBoxFiltrCategor.SelectedIndex > 0)
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

                if (AllMyInstr.Any(a => a.Child == component))
                {
                    continue;
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
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var work = (TechOperationWork)comboBoxTO3.SelectedItem;

                if (work == null)
                {
                    return;
                }

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
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var work = (TechOperationWork)comboBoxTO3.SelectedItem;
                if (work == null)
                {
                    return;
                }
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
                if (work == null)
                {
                    return;
                }
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

            if (e.ColumnIndex == 6)
            {
                var work = (TechOperationWork)comboBoxTO3.SelectedItem;
                if (work == null)
                {
                    return;
                }
                var Idd = (ToolWork)dataGridViewInstrumentLocal.Rows[e.RowIndex].Cells[0].Value;
                var value = (string)dataGridViewInstrumentLocal.Rows[e.RowIndex].Cells[6].Value;

                Idd.Comments = value;
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

            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
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

        private ExecutionWork? executionWorkPovtor;
        //private void DataGridViewPovtor_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        //{
        //    dataGridViewPovtor.CommitEdit(DataGridViewDataErrorContexts.Commit);

        //    if (e.ColumnIndex == 1 && e.RowIndex >= 0)
        //    {
        //        var currentEW = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
        //        var isSelected = (bool)dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

        //        if (executionWorkPovtor != null)
        //        {
        //            if (isSelected)
        //            {
        //                if (!executionWorkPovtor.ListexecutionWorkRepeat2.Contains(currentEW))
        //                {
        //                    executionWorkPovtor.ListexecutionWorkRepeat2.Add(currentEW);
        //                    TechOperationForm.UpdateGrid();
        //                }
        //            }
        //            else
        //            {
        //                if (executionWorkPovtor.ListexecutionWorkRepeat2.Contains(currentEW))
        //                {
        //                    executionWorkPovtor.ListexecutionWorkRepeat2.Remove(currentEW);
        //                    TechOperationForm.UpdateGrid();
        //                }
        //            }
        //        }
        //    }
        //}
        private void DataGridViewPovtor_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewPovtor.SelectedRows.Count > 0)
            {
                // Получаем выделенную строку
                var selectedRow = dataGridViewPovtor.SelectedRows[0];
                var executionWork = (ExecutionWork)selectedRow.Cells[0].Value;

                // Вызываем метод для выделения строки
                TechOperationForm.HighlightExecutionWorkRow(executionWork, false);
            }
        }

        private void dataGridViewPovtor_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (executionWorkPovtor == null) return;

            var executionWork = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
            // позиция для executionWorkPovtor в таблице dataGridViewPovtor
            var powtorIndex = dataGridViewPovtor.Rows.Cast<DataGridViewRow>().ToList().FindIndex(x => x.Cells[0].Value == executionWorkPovtor);
            bool isReadOnlyRow = powtorIndex < e.RowIndex;

            if (e.ColumnIndex == 1) // Индекс столбца с checkBox
            {
                if (executionWork == executionWorkPovtor)
                {
                    SetReadOnlyAndColor(e.ColumnIndex, Color.DarkSeaGreen);
                }
                else if (isReadOnlyRow)
                {
                    SetReadOnlyAndColor(e.ColumnIndex, Color.DarkSalmon);
                }
            }
            else if (e.ColumnIndex == 5 || e.ColumnIndex == 6 || e.ColumnIndex == 7) // Индекс столбца с checkBox
            {
                if (executionWork == executionWorkPovtor || isReadOnlyRow)
                {
                    // Делаем ячейку недоступной
                    dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
                }
                else // todo: снимать повтор, если объект перемещают "ниже" повтора
                {
                    var existingRepeat = executionWorkPovtor.ExecutionWorkRepeats
                        .SingleOrDefault(x => x.ChildExecutionWork == executionWork);

                    dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = existingRepeat != null ? Color.LightGray : Color.White;
                    dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = existingRepeat == null;
                }
            }
            void SetReadOnlyAndColor(int columnIndex, Color color, bool readOnly = true)
            {
                dataGridViewPovtor.Rows[e.RowIndex].Cells[columnIndex].ReadOnly = readOnly;
                dataGridViewPovtor.Rows[e.RowIndex].DefaultCellStyle.BackColor = color;
            }
        }

        private void DataGridViewPovtor_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            dataGridViewPovtor.CommitEdit(DataGridViewDataErrorContexts.Commit);


            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var currentEW = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
                var isSelected = (bool)dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                // позиция для executionWorkPovtor в таблице dataGridViewPovtor
                var powtorIndex = dataGridViewPovtor.Rows.Cast<DataGridViewRow>().ToList().FindIndex(x => x.Cells[0].Value == executionWorkPovtor);

                if (executionWorkPovtor != null && powtorIndex > e.RowIndex)//currentEW.Order < executionWorkPovtor.Order)
                {
                    var existingRepeat = executionWorkPovtor.ExecutionWorkRepeats
                        .SingleOrDefault(x => x.ChildExecutionWork == currentEW);

                    if (isSelected)
                    {
                        if (existingRepeat == null)
                        {
                            var newRepeat = new ExecutionWorkRepeat
                            {
                                ParentExecutionWork = executionWorkPovtor,
                                ParentExecutionWorkId = executionWorkPovtor.Id,
                                ChildExecutionWork = currentEW,
                                ChildExecutionWorkId = currentEW.Id,
                                NewCoefficient = "*1"
                            };
                            executionWorkPovtor.ExecutionWorkRepeats.Add(newRepeat);
                            TechOperationForm.UpdateGrid();
                        }
                    }
                    else
                    {
                        if (existingRepeat != null)
                        {
                            dataGridViewPovtor.Rows[e.RowIndex].Cells[5].Value = "";

                            executionWorkPovtor.ExecutionWorkRepeats.Remove(existingRepeat);
                            TechOperationForm.UpdateGrid();
                        }
                    }


                    RecalculateExecutionWorkPovtorValue(executionWorkPovtor);

                    // Перерисовать таблицу
                    dataGridViewPovtor.Invalidate();
                    //dataGridViewTPLocal.Invalidate();

                    TechOperationForm.UpdateGrid(); // todo: реализовать метод по изменению значения только для одной строки в таблице TechOperationForm
                }
            }
        }

        private void DataGridViewPovtor_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 5 || e.ColumnIndex == 6 || e.ColumnIndex == 7) && e.RowIndex >= 0)
            {
                var currentEW = (ExecutionWork)dataGridViewPovtor.Rows[e.RowIndex].Cells[0].Value;
                var isSelected = (bool)dataGridViewPovtor.Rows[e.RowIndex].Cells[1].Value;

                if (executionWorkPovtor != null)
                {
                    var existingRepeat = executionWorkPovtor.ExecutionWorkRepeats
                        .SingleOrDefault(x => x.ChildExecutionWork == currentEW);

                    if (isSelected)
                    {
                        if (existingRepeat != null)
                        {
                            if (e.ColumnIndex == 5)
                            {
                                existingRepeat.NewCoefficient = (string)dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                            }
                            else if (e.ColumnIndex == 6)
                            {
                                existingRepeat.NewEtap = (string)dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                            }
                            else if (e.ColumnIndex == 7)
                            {
                                existingRepeat.NewPosled = (string)dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                            }

                            RecalculateExecutionWorkPovtorValue(executionWorkPovtor);
                            TechOperationForm.UpdateGrid();
                        }
                    }
                    else
                    {
                        // отмена изменений
                        dataGridViewPovtor.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = null;

                    }

                    UpdateGridLocalTP();
                }
            }
        }
        public void UpdatePovtor()
        {
            dataGridViewPovtor.Rows.Clear();
            executionWorkPovtor = null;

            var select = dataGridViewTPLocal.SelectedRows;
            if (select.Count > 0)
            {
                var id = (Guid)select[0].Cells[0].Value;

                var al = TechOperationForm.TechOperationWorksList.Where(w => w.Delete == false).OrderBy(o => o.Order);

                ExecutionWork? exeWork = null;
                // Поиск ТП с заданным Guid
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
                    //var selectedEW = exeWork.ListexecutionWorkRepeat2.ToList();
                    var selectedEW = exeWork.ExecutionWorkRepeats.Select(ewr => ewr.ChildExecutionWork).ToList();
                    var selectedEWR = exeWork.ExecutionWorkRepeats.ToList();
                    foreach (TechOperationWork techOperationWork in al)
                    {
                        var allEwInTo = techOperationWork.executionWorks.Where(w => w.Delete == false /*&& w.Repeat == false*/).OrderBy(o => o.Order); ////// 26/06/2024 - добавил повторы в выборку. Т.к. в картах такие объекты тоже входят в повторы

                        foreach (ExecutionWork executionWork in allEwInTo)
                        {
                            var isSelected = selectedEW.SingleOrDefault(s => s == executionWork) != null;

                            List<object> listItem = new List<object>
                            {
                                executionWork
                            };
                            if (isSelected)
                            {
                                listItem.Add(true);
                            }
                            else
                            {
                                listItem.Add(false);
                            }
                            listItem.Add(techOperationWork.techOperation.Name);
                            listItem.Add(executionWork.techTransition?.Name ?? ""); // todo - проверить, может ли имя быть null у EW

                            listItem.Add(executionWork.Coefficient ?? "");

                            if (isSelected)
                            {
                                ExecutionWorkRepeat? techOperationWorkRepeat = selectedEWR.SingleOrDefault(s => s.ChildExecutionWork == executionWork);

                                if (techOperationWorkRepeat != null)
                                {
                                    listItem.Add(techOperationWorkRepeat.NewCoefficient);

                                    listItem.Add(techOperationWorkRepeat.NewEtap);
                                    listItem.Add(techOperationWorkRepeat.NewPosled);
                                }
                            }

                            dataGridViewPovtor.Rows.Add(listItem.ToArray());
                        }
                    }
                }

            }

        }

        private void RecalculateExecutionWorkPovtorValue(ExecutionWork executionWorkPovtor)
        {
            double totalValue = 0;

            foreach (var repeat in executionWorkPovtor.ExecutionWorkRepeats)
            {
                double coefficient = 1;
                if (!string.IsNullOrEmpty(repeat.NewCoefficient))
                {
                    try
                    {
                        coefficient = WorkParser.EvaluateExpression(repeat.NewCoefficient);
                    }
                    catch
                    {
                        coefficient = 1; // значение по умолчанию, если выражение не удаётся вычислить
                    }
                }

                totalValue += repeat.ChildExecutionWork.Value * coefficient;
            }

            executionWorkPovtor.Value = totalValue;
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

        private void btnCreateNewTP_Click(object sender, EventArgs e)
        {
            // 1. Открытие формы по добавлению нового ТП с передачей номера ТК
            var AddingForm = new Win7_TechTransitionEditor(new TechTransition() { CreatedTCId = TechOperationForm.tcId }, isNewObject: true);
            AddingForm.AfterSave = async (createdObj) => AddNewTP(createdObj, SelectedTO);//AddNewObjectInDataGridView(createdObj);
            AddingForm.ShowDialog();

        }

        private void btnAddNewTO_Click(object sender, EventArgs e)
        {
            var AddingForm = new Win7_TechOperation_Window(isTcEditingForm: true, createdTcId: TechOperationForm.tcId);
            AddingForm.AfterSave = async (createdObj) => AddTOWToGridLocalTO(createdObj, true);
            AddingForm.ShowDialog();

            UpdateTO();
        }

        private void btnToChange_Click(object sender, EventArgs e)
        {
            if (dataGridViewTO.SelectedRows.Count == 1 && dataGridViewAllTO.SelectedRows.Count == 1)
            {
                // Получаем выделенную строку из dataGridViewTO
                var selectedRowTO = dataGridViewTO.SelectedRows[0];
                var techOperationWork = (TechOperationWork)selectedRowTO.Cells[0].Value;

                // Получаем выделенную строку из dataGridViewAllTO
                var selectedRowAllTO = dataGridViewAllTO.SelectedRows[0];
                var newTechOperation = (TechOperation)selectedRowAllTO.Cells[0].Value;

                if (techOperationWork.techOperationId == newTechOperation.Id)
                {
                    MessageBox.Show("Вы выбрали одинаковые операции.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите заменить {techOperationWork.techOperation.Name} на {newTechOperation.Name}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }

                if (newTechOperation.Category == "Типовая ТО")
                {
                    MessageBox.Show("Замена на типовую ТО недопустима.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Обновляем TechOperation в выделенной строке dataGridViewTO
                techOperationWork.techOperation = newTechOperation;
                techOperationWork.techOperationId = newTechOperation.Id;

                // Обновляем отображение в dataGridViewTO
                UpdateLocalTO();
                TechOperationForm.UpdateGrid();
            }
            else
            {
                MessageBox.Show("Выберите по одной строке в обеих таблицах для замены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
