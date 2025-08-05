using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Work;
using TcDbConnector;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace TC_WinForms.WinForms;

public partial class Win7_TechOperation_Window : Form
{
    MyDbContext context;

    TechOperation techOperation;
    private ConcurrencyBlockService<TechOperation> techOperationBlockService;

    public delegate Task PostSaveActionTO(TechOperation modelObject);
    public PostSaveActionTO? AfterSave { get; set; }

    private readonly bool _isTcEditingForm;


    public Win7_TechOperation_Window(int? _techOperationId = null, bool isTcEditingForm = false, int? createdTcId = null)
    {
        _isTcEditingForm = isTcEditingForm;
        InitializeComponent();

        context = new MyDbContext();

        if (_techOperationId != null)
        {
            techOperation = context.TechOperations
                .Include(t => t.techTransitionTypicals)
                .Single(s => s.Id == _techOperationId);

            textBox1.Text = techOperation.Name;
            cbxIsReleased.Checked = techOperation.IsReleased;

            if (techOperation.IsTypical)
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }

            var timerInterval = 1000 * 60 * 25;

            techOperationBlockService = new ConcurrencyBlockService<TechOperation>(techOperation, timerInterval);
            techOperationBlockService.BlockObject();
        }

        var formName = techOperation == null ? "Создание ТО" : $"Редактор ТО {techOperation.Name}";

        this.Text = formName;

        dataGridViewTPAll.CellClick += DataGridViewTPAll_CellClick;
        dataGridViewTPLocal.CellClick += DataGridViewTPLocal_CellClick;

        dataGridViewTPLocal.CellEndEdit += DataGridViewTPLocal_CellEndEdit;

        dataGridViewTPLocal.Columns["OrderColumn"].DefaultCellStyle.BackColor = Color.LightGray;
        dataGridViewTPLocal.Columns["Comment"].DefaultCellStyle.BackColor = Color.LightGray;
        dataGridViewTPLocal.Columns["EtapColumn"].DefaultCellStyle.BackColor = Color.LightGray;
        dataGridViewTPLocal.Columns["PosledColumn"].DefaultCellStyle.BackColor = Color.LightGray;
        dataGridViewTPLocal.Columns["Coefficient"].DefaultCellStyle.BackColor = Color.LightGray;

        comboBoxTPCategoriya.SelectedIndexChanged += ComboBoxTPCategoriya_SelectedIndexChanged;
        textBoxPoiskTP.TextChanged += TextBoxPoiskTP_TextChanged;

        if (techOperation == null)
        {
            techOperation = new TechOperation();
            techOperation.CreatedTCId = createdTcId;
            context.TechOperations.Add(techOperation);
        }

        UpdateGridAllTP();
        UpdateGridLocalTP();

        if (_isTcEditingForm)
        {
            button1.Text = "Добавить";
        }

        //  var bb = context.ChangeTracker.Entries().Count();

    }

    private void ComboBoxTPCategoriya_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateGridAllTP();
    }


    private void TextBoxPoiskTP_TextChanged(object? sender, EventArgs e)
    {
        UpdateGridAllTP();
    }

    private void DataGridViewTPLocal_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        if (e.ColumnIndex == dataGridViewTPLocal.Columns["EtapColumn"].Index)
        {
            var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            var idd = (TechTransitionTypical)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

            idd.Etap = gg;

        }
        else if (e.ColumnIndex == dataGridViewTPLocal.Columns["PosledColumn"].Index)
        {
            var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            var idd = (TechTransitionTypical)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

            idd.Posled = gg;

        }
        else if (e.ColumnIndex == dataGridViewTPLocal.Columns["Comment"].Index)
        {
            var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            var idd = (TechTransitionTypical)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

            idd.Comments = gg;

        }
        else if (e.ColumnIndex == dataGridViewTPLocal.Columns["Coefficient"].Index)
        {
            var gg = (string)dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            var idd = (TechTransitionTypical)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;

            idd.Coefficient = gg;

        }
        else if (e.ColumnIndex == dataGridViewTPLocal.Columns["OrderColumn"].Index)
        {
            var techTransition = (TechTransitionTypical)dataGridViewTPLocal.Rows[e.RowIndex].Cells[0].Value;
            int newOrder = 1;
            try
            {
                newOrder = Convert.ToInt32(dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Введите числовое значение!", "Ошибка формата!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridViewTPLocal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = techTransition.Order;
                return;
            }

            if(techTransition != null)
            {
                newOrder = newOrder <= 0 ? 1 : newOrder;
                newOrder = newOrder > dataGridViewTPLocal.RowCount ? dataGridViewTPLocal.RowCount : newOrder;

                this.BeginInvoke(new MethodInvoker(() =>//используется для обхода рекурсивного вызова перемещения строк
                {
                    var currentOrder = techTransition.Order;
                    if (currentOrder == newOrder)
                        return;

                    DGVProcessing.ReorderRows(dataGridViewTPLocal.Rows[e.RowIndex], newOrder, dataGridViewTPLocal);

                    foreach (DataGridViewRow dataGridViewRow in dataGridViewTPLocal.Rows)
                    {
                        // кажется так лаконичнее, но перед релизом не решаюсь внедрять. Предварительно работает
                        var techOperationWorkRow = (TechTransitionTypical)dataGridViewRow.Cells[0].Value;
                        techOperationWorkRow.Order = dataGridViewRow.Index + 1; // Обновляем свойство Order
                    }
                    UpdateGridLocalTP();
                }));
            }
        }

    }

    private void DataGridViewTPAll_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 1 && e.RowIndex >= 0)
        {
            var Idd = (TechTransition)dataGridViewTPAll.Rows[e.RowIndex].Cells[0].Value;
            techOperation.techTransitionTypicals.Add(new TechTransitionTypical { TechTransition = Idd, Order = techOperation.techTransitionTypicals.Count + 1});
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

        foreach (TechTransitionTypical techTransitionTypical in LocalTP.OrderBy(o => o.Order))
        {
            List<object> listItem = new List<object>
            {
                techTransitionTypical,
                "Удалить",
                techTransitionTypical.TechTransition?.Name!,
                techTransitionTypical.TechTransition?.TimeExecution!,
                techTransitionTypical.Order,
                techTransitionTypical.Coefficient ?? "",
                techTransitionTypical.Etap,
                techTransitionTypical.Posled,
                techTransitionTypical.Comments ?? ""

            };


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

        bool AddCategor = false;
        if (comboBoxTPCategoriya.Items.Count == 0)
        {
            AddCategor = true;
            comboBoxTPCategoriya.Items.Add("Все");
        }

        var allTP = context.TechTransitions.ToList();
        // var list = TechOperationForm.TechOperationWorksList.Single(s => s == work).executionWorks.ToList();

        foreach (TechTransition techTransition in allTP)
        {
            if (textBoxPoiskTP.Text != "" &&
                techTransition.Name.ToLower().IndexOf(textBoxPoiskTP.Text.ToLower()) == -1)
            {
                continue;
            }

            if (AddCategor)
            {
                if (!string.IsNullOrEmpty(techTransition.Category))
                {
                    if (comboBoxTPCategoriya.Items.Contains(techTransition.Category) == false)
                    {
                        comboBoxTPCategoriya.Items.Add(techTransition.Category);
                    }
                }
            }

            if (comboBoxTPCategoriya.SelectedIndex > 0)
            {
                if ((string)comboBoxTPCategoriya.SelectedItem != techTransition.Category)
                {
                    continue;
                }
            }


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

    private async void button1_Click(object sender, EventArgs e)
    {
        try
        {
            if (techOperationBlockService != null)
                techOperationBlockService.CleanBlockData();

            // проверка полей на уникальность
            if (!await UniqueFieldChecker<TechOperation>.IsPropertiesUnique(techOperation))
                return;

            context.SaveChanges();

            if (_isTcEditingForm)
            {
                AfterSave?.Invoke(techOperation);
                Close();
            }
            else
            {
                StaticWinForms.Win7_new.UpdateTO();
                MessageBox.Show("Данные сохранены");

            }

        }
        catch (Exception ee)
        {
            MessageBox.Show(ee.Message);
        }

    }

    private void Win7_TechOperation_Window_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (techOperationBlockService != null)
            techOperationBlockService.CleanBlockData();

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
        if (checkBox1.Checked)
        {
            techOperation.Category = "Типовая ТО";
        }
        else
        {
            techOperation.Category = "ТО";
        }

    }

    private void cbxIsReleased_CheckedChanged(object sender, EventArgs e)
    {
        techOperation.IsReleased = cbxIsReleased.Checked;
    }
}
