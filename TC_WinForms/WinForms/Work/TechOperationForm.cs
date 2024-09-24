using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using ExcelParsing.DataProcessing;
using Microsoft.EntityFrameworkCore;
using TC_WinForms.Interfaces;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace TC_WinForms.WinForms.Work;

public partial class TechOperationForm : Form, ISaveEventForm, IViewModeable
{
    private readonly TcViewState _tcViewState;

    private bool _isViewMode;
    private bool _isCommentViewMode;
    private bool _isMachineViewMode;

    public MyDbContext context { get; private set; } = new MyDbContext();

    public readonly int tcId;

    private List<TechOperationDataGridItem> list = new List<TechOperationDataGridItem>();
    private AddEditTechOperationForm? _editForm;

    public List<TechOperationWork> TechOperationWorksList = null!;
    public TechnologicalCard TehCarta = null!;
    private TCCache cache;
    public bool CloseFormsNoSave { get; set; } = false;

    //public TechOperationForm()
    //{
    //    InitializeComponent();

    //}

    public TechOperationForm(int tcId, TcViewState tcViewState, TCCache cache)//,  bool viewerMode = false)
    {
        this._tcViewState = tcViewState;
        this.cache = cache;
        this.tcId = tcId;
        //_isViewMode = viewerMode;
        InitializeComponent();
        dgvMain.CellPainting += DgvMain_CellPainting;
        dgvMain.CellFormatting += DgvMain_CellFormatting;
        dgvMain.CellEndEdit += DgvMain_CellEndEdit;
        dgvMain.CellMouseEnter += DgvMain_CellMouseEnter;

        this.KeyPreview = true;
        this.KeyDown += new KeyEventHandler(Form_KeyDown);

        _tcViewState.ViewModeChanged += OnViewModeChanged;
    }
    private async void TechOperationForm_Load(object sender, EventArgs e)
    {
        // Блокировка формы на время загрузки данных
        this.Enabled = false;

        try
        {
            await LoadDataAsync(tcId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        UpdateGrid();
        SetCommentViewMode();
        SetMachineViewMode();
        SetViewMode();

        this.Enabled = true;
    }
    private async Task LoadDataAsync(int tcId)
    {
        TehCarta = await cache.GetTechnologicalCardAsync(tcId);

        TechOperationWorksList = await cache.GetTechOperationsAsync(tcId);
    }

    public void SetCommentViewMode()
    {
        var isCommentViewMode = _tcViewState.IsCommentViewMode;

        dgvMain.Columns["RemarkColumn"].Visible = isCommentViewMode;
        dgvMain.Columns["ResponseColumn"].Visible = isCommentViewMode;
    }
  
    public void SetMachineViewMode(bool? isMachineViewMode = null)
    {
        if (isMachineViewMode != null)
            _isMachineViewMode = (bool)isMachineViewMode;
        else
            _isMachineViewMode = Win6_new.isMachineViewMode;

        foreach(DataGridViewColumn col in dgvMain.Columns)
        {
            if (col.Name.Contains("Machine"))
            {
                col.Visible = _isMachineViewMode;
            }
        }
    }
    public void SetViewMode(bool? isViewMode = null)
    {
        //if (isViewMode != null)
        //{
        //    _isViewMode = (bool)isViewMode;
        //}

        pnlControls.Visible = !_tcViewState.IsViewMode;
    }
    private void OnViewModeChanged()
    {
        UpdateGrid();
    }

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            PasteClipboardValue();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Delete)
        {
            DeleteCellValue();
            e.Handled = true;
        }
    }

    private void DeleteCellValue()
    {
        if (dgvMain.CurrentCell != null && !dgvMain.CurrentCell.ReadOnly)
        {
            dgvMain.CurrentCell.Value = string.Empty;

            // Вызов события CellEndEdit вручную
            var args = new DataGridViewCellEventArgs(dgvMain.CurrentCell.ColumnIndex, dgvMain.CurrentCell.RowIndex);
            DgvMain_CellEndEdit(dgvMain, args);
        }
    }

    private void PasteClipboardValue()
    {
        if (dgvMain.CurrentCell != null && !dgvMain.CurrentCell.ReadOnly)
        {
            var clipboardText = Clipboard.GetText();
            if (!string.IsNullOrEmpty(clipboardText))
            {
                dgvMain.CurrentCell.Value = clipboardText;

                // Вызов события CellEndEdit вручную
                var args = new DataGridViewCellEventArgs(dgvMain.CurrentCell.ColumnIndex, dgvMain.CurrentCell.RowIndex);
                DgvMain_CellEndEdit(dgvMain, args);
            }
        }
    }

    private void DgvMain_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        // todo: ненадёжный способ определения столбцов с комментариями
        if (e.ColumnIndex == dgvMain.Columns["ResponseColumn"].Index)//dgvMain.ColumnCount-1)
        {
            var idd = (ExecutionWork)dgvMain.Rows[e.RowIndex].Cells[0].Value;
            var gg = (string)dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (idd != null)
            {
                if (gg == null)
                {
                    gg = "";
                }
                idd.Otvet = gg;
                HasChanges = true;
            }

        }
        else if (e.ColumnIndex == dgvMain.Columns["RemarkColumn"].Index)// dgvMain.ColumnCount - 2)
        {
            var idd = (ExecutionWork)dgvMain.Rows[e.RowIndex].Cells[0].Value;
            var gg = (string)dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (idd != null)
            {
                if (gg == null)
                {
                    gg = "";
                }
                idd.Vopros = gg;
                HasChanges = true;
            }
        }
        else if (e.ColumnIndex == dgvMain.Columns["PictureNameColumn"].Index)
        {
            var idd = (ExecutionWork)dgvMain.Rows[e.RowIndex].Cells[0].Value;
            var gg = (string)dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (idd != null)
            {
                if(gg == idd.PictureName) return;

                idd.PictureName = gg;
                HasChanges = true;
                _editForm?.UpdateGridLocalTP();
            }
        }
        else if (e.ColumnIndex == dgvMain.Columns["CommentColumn"].Index)
        {
            var idd = (ExecutionWork)dgvMain.Rows[e.RowIndex].Cells[0].Value;
            var gg = (string)dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (idd != null)
            {
                if (gg == idd.Comments) return;

                idd.Comments = gg;
                HasChanges = true;
                _editForm?.UpdateGridLocalTP();
            }
        }
        //if (e.ColumnIndex> dgvMain.ColumnCount-3) 
        //{
            
        //}
    }

    

    
    public void UpdataBD()
    {
        var vb = context.ExecutionWorks.
            Include(t => t.techTransition)
            .ToList();

        foreach (var v in vb)
        {
            if (v.techTransition != null)
            {
                v.Value = v.techTransition.TimeExecution;
            }
        }
        context.SaveChanges();
    }


    public bool GetDontSaveData()
    {
        return HasChanges;
    }


    private void TechOperationForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        // Закрытие дочерней формы, если она была открыта
        _editForm?.Close();
    }

    #region Удалённый метод
    //public void UpdateGrid()
    //{
    //    dgvMain.Rows.Clear();
    //    list.Clear();

    //    while (dgvMain.Columns.Count > 0)
    //    {
    //        dgvMain.Columns.RemoveAt(0);
    //    }

    //    dgvMain.Columns.Add("", "");
    //    dgvMain.Columns.Add("", "");
    //    dgvMain.Columns.Add("", "");
    //    dgvMain.Columns.Add("", "");
    //    dgvMain.Columns.Add("", "");
    //    dgvMain.Columns.Add("", "");
    //    dgvMain.Columns.Add("", "");


    //    foreach (Machine_TC tehCartaMachineTC in TehCarta.Machine_TCs)
    //    {
    //        dgvMain.Columns.Add("", "Время " + tehCartaMachineTC.Child.Name + ", мин.");
    //    }

    //    dgvMain.Columns.Add("", "№ СЗ");
    //    dgvMain.Columns.Add("", "Примечания");

    //    dgvMain.Columns.Add("", "Замечание");
    //    dgvMain.Columns.Add("", "Ответ");

    //    int ii = 0;

    //    dgvMain.Columns[ii].Visible = false;
    //    ii++;

    //    dgvMain.Columns[ii].HeaderText = "№";
    //    dgvMain.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
    //    dgvMain.Columns[ii].Width = 30;
    //    ii++;
    //    dgvMain.Columns[ii].HeaderText = "Технологические операции";
    //    ii++;
    //    dgvMain.Columns[ii].HeaderText = "Исполнитель";
    //    dgvMain.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
    //    dgvMain.Columns[ii].Width = 120;
    //    ii++;
    //    dgvMain.Columns[ii].HeaderText = "Технологические переходы";
    //    ii++;
    //    dgvMain.Columns[ii].HeaderText = "Время действ., мин.";
    //    ii++;

    //    dgvMain.Columns[ii].HeaderText = "Время этапа, мин.";
    //    ii++;


    //    foreach (DataGridViewColumn column in dgvMain.Columns)
    //    {
    //        column.SortMode = DataGridViewColumnSortMode.NotSortable;
    //    }

    //    // автоподбор ширины столбцов под ширину таблицы
    //    dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    //    dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
    //    dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

    //    //dgvMain.RowHeadersWidth = 25;

    //    // автоперенос в ячейках
    //    dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;


    //    foreach (DataGridViewColumn column in dgvMain.Columns)
    //    {
    //        column.ReadOnly = true;
    //        column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
    //    }


    //    dgvMain.Columns[dgvMain.Columns.Count - 1].ReadOnly = false;

    //    if (TC_WinForms.DataProcessing.AuthorizationService.CurrentUser.UserRole() != DataProcessing.AuthorizationService.User.Role.User)
    //    {
    //        dgvMain.Columns[dgvMain.Columns.Count - 2].ReadOnly = false;
    //    }

    //    var TechOperationWorksListLocal = TechOperationWorksList.Where(w => w.Delete == false).OrderBy(o => o.Order).ToList();



    //    int nomer = 1;
    //    foreach (TechOperationWork techOperationWork in TechOperationWorksListLocal)
    //    {
    //        List<ExecutionWork> bb = techOperationWork.executionWorks.Where(w => w.Delete == false).OrderBy(o => o.Order).ToList();

    //        if (bb.Count == 0)
    //        {
    //            list.Add(new TechOperationDataGridItem
    //            {
    //                Nomer = -1,
    //                TechOperation = techOperationWork.techOperation.Name
    //            });
    //        }

    //        foreach (ExecutionWork executionWork in bb)
    //        {
    //            if (executionWork.IdGuid == new Guid())
    //            {
    //                executionWork.IdGuid = Guid.NewGuid();
    //            }

    //            string StaffStr = "";

    //            foreach (Staff_TC executionWorkStaff in executionWork.Staffs)
    //            {
    //                if (StaffStr != "")
    //                {
    //                    StaffStr += ",";
    //                }

    //                StaffStr += executionWorkStaff.Symbol;
    //            }


    //            var protectList = new List<int>();
    //            string ProtectStr = "";
    //            foreach (Protection_TC executionWorkProtection in executionWork.Protections)
    //            {
    //                protectList.Add(executionWorkProtection.Order);
    //                //if (ProtectStr != "")
    //                //{
    //                //    ProtectStr += ",";
    //                //}

    //                //ProtectStr += executionWorkProtection.Order;
    //            }

    //            ProtectStr = ConvertListToRangeString(protectList);


    //            List<bool> mach = new List<bool>();

    //            foreach (Machine_TC tehCartaMachineTC in TehCarta.Machine_TCs)
    //            {
    //                var sing = executionWork.Machines.SingleOrDefault(s => s == tehCartaMachineTC);


    //                if (sing == null)
    //                {
    //                    mach.Add(false);

    //                }
    //                else
    //                {
    //                    mach.Add(true);
    //                }
    //            }

    //            var itm = new TechOperationDataGridItem
    //            {
    //                Nomer = nomer,
    //                Staff = StaffStr,
    //                TechOperation = techOperationWork.techOperation.Name,
    //                TechTransition = executionWork.techTransition?.Name,
    //                TechTransitionValue = executionWork.Value.ToString(),
    //                Protections = ProtectStr,
    //                Etap = executionWork.Etap,
    //                Posled = executionWork.Posled,
    //                Work = true,
    //                techWork = executionWork,
    //                listMach = mach,
    //                Comments = executionWork.Comments,
    //                Vopros = executionWork.Vopros,
    //                Otvet = executionWork.Otvet,
    //                executionWorkItem = executionWork
    //            };

    //            if (itm.TechTransitionValue == "-1")
    //            {
    //                itm.TechTransitionValue = "Ошибка";
    //            }

    //            list.Add(itm);


    //            nomer++;
    //        }


    //        foreach (ToolWork toolWork in techOperationWork.ToolWorks)
    //        {
    //            string strComp =
    //                $"{toolWork.tool.Name}   {toolWork.tool.Type}    {toolWork.tool.Unit}";
    //            list.Add(new TechOperationDataGridItem
    //            {
    //                Nomer = nomer,
    //                Staff = "",
    //                TechOperation = techOperationWork.techOperation.Name,
    //                TechTransition = strComp,
    //                TechTransitionValue = toolWork.Quantity.ToString(),
    //                ItsTool = true,
    //                Comments = toolWork.Comments ?? ""

    //            });
    //            nomer++;
    //        }

    //        foreach (ComponentWork componentWork in techOperationWork.ComponentWorks)
    //        {
    //            string strComp =
    //                $"{componentWork.component.Name}   {componentWork.component.Type}    {componentWork.component.Unit}";

    //            list.Add(new TechOperationDataGridItem
    //            {
    //                Nomer = nomer,
    //                Staff = "",
    //                TechOperation = techOperationWork.techOperation.Name,
    //                TechTransition = strComp,
    //                TechTransitionValue = componentWork.Quantity.ToString(),
    //                ItsComponent = true,
    //                Comments = componentWork.Comments ?? ""
    //            });
    //            nomer++;
    //        }

    //    }


    //    for (var index = 0; index < list.Count; index++)
    //    {
    //        TechOperationDataGridItem techOperationDataGridItem = list[index];

    //        List<ExecutionWork> podchet = new List<ExecutionWork>();
    //        List<TechOperationDataGridItem> podchet2 = new List<TechOperationDataGridItem>();
    //        List<TechOperationDataGridItem> TampPodchet = new List<TechOperationDataGridItem>();

    //        if (techOperationDataGridItem.Work)
    //        {
    //            podchet.Add(techOperationDataGridItem.techWork);
    //            podchet2.Add(techOperationDataGridItem);
    //            if (techOperationDataGridItem.Etap != "" && techOperationDataGridItem.Etap != "0")
    //            {
    //                int plusIndex = index;
    //                while (true)
    //                {
    //                    plusIndex = plusIndex + 1;
    //                    if (plusIndex < list.Count)
    //                    {
    //                        TechOperationDataGridItem tech2 = list[plusIndex];
    //                        if (tech2.Work == false)
    //                        {
    //                            TampPodchet.Add(tech2);
    //                            continue;
    //                        }

    //                        if (techOperationDataGridItem.Etap == tech2.Etap)
    //                        {
    //                            index = plusIndex;
    //                            podchet.Add(tech2.techWork);
    //                            podchet2.Add(tech2);
    //                            tech2.TimeEtap = "-1";
    //                            TampPodchet.ForEach(f => f.TimeEtap = "-1");
    //                        }
    //                        else
    //                        {
    //                            break;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        break;
    //                    }
    //                }
    //            }

    //            double Paral = 0;

    //            foreach (ExecutionWork executionWork in podchet)
    //            {
    //                if (executionWork.Posled != "" && executionWork.Posled != "0")
    //                {
    //                    var allSum = podchet.Where(w => w.Posled == executionWork.Posled && w.Value != -1)
    //                        .Sum(s => s.Value);
    //                    executionWork.TempTimeExecution = allSum;
    //                }
    //                else
    //                {
    //                    executionWork.TempTimeExecution = executionWork.Value == -1 ? 0 : executionWork.Value;
    //                }
    //            }

    //            var col = podchet2[0].listMach.Count;
    //            if (podchet2.Count > 1)
    //            {

    //                for (int i = 0; i < col; i++)
    //                {
    //                    bool tr = false;
    //                    foreach (TechOperationDataGridItem operationDataGridItem in podchet2)
    //                    {
    //                        if (operationDataGridItem.listMach[i] == true)
    //                        {
    //                            tr = true;
    //                        }
    //                    }

    //                    if (tr)
    //                    {
    //                        foreach (TechOperationDataGridItem operationDataGridItem in podchet2)
    //                        {
    //                            operationDataGridItem.listMach[i] = true;
    //                        }
    //                    }
    //                }
    //            }



    //            Paral = podchet.Max(m => m.TempTimeExecution);
    //            techOperationDataGridItem.TimeEtap = Paral.ToString();
    //        }
    //    }

    //    foreach (TechOperationDataGridItem techOperationDataGridItem in list)
    //    {
    //        List<object> str = new List<object>();

    //        if (techOperationDataGridItem.techWork != null && techOperationDataGridItem.techWork.Repeat == true)
    //        {
    //            var repeatNumList = new List<int>();
    //            string strP = "";

    //            foreach (ExecutionWork executionWork in techOperationDataGridItem.techWork.ListexecutionWorkRepeat2)
    //            {
    //                var bn = list.SingleOrDefault(s => s.techWork == executionWork);
    //                if (bn != null)
    //                {
    //                    repeatNumList.Add(bn.Nomer);
    //                    //if (strP == "")
    //                    //{
    //                    //    strP = "п. " + bn.Nomer;
    //                    //}
    //                    //else
    //                    //{
    //                    //    strP += "," + bn.Nomer;
    //                    //}
    //                }
    //            }
    //            strP = ConvertListToRangeString(repeatNumList);

    //            str.Add(techOperationDataGridItem.executionWorkItem);

    //            str.Add(techOperationDataGridItem.Nomer.ToString());
    //            str.Add(techOperationDataGridItem.TechOperation);
    //            str.Add(techOperationDataGridItem.Staff);
    //            str.Add("Повторить п." + strP);
    //            str.Add(techOperationDataGridItem.TechTransitionValue);
    //            str.Add(techOperationDataGridItem.TimeEtap);

    //            techOperationDataGridItem.listMachStr = new List<string>();
    //            if (techOperationDataGridItem.listMachStr.Count == 0 && techOperationDataGridItem.listMach.Count > 0)
    //            {
    //                for (var index = 0; index < TehCarta.Machine_TCs.Count; index++)
    //                {
    //                    bool b = techOperationDataGridItem.listMach[index];
    //                    if (b)
    //                    {
    //                        str.Add(techOperationDataGridItem.TimeEtap);
    //                    }
    //                    else
    //                    {
    //                        if (techOperationDataGridItem.TimeEtap == "-1")
    //                        {
    //                            str.Add("-1");
    //                        }
    //                        else
    //                        {
    //                            str.Add("");
    //                        }
    //                    }
    //                }
    //            }

    //            str.Add(techOperationDataGridItem.Protections);

    //            if (techOperationDataGridItem.techWork != null)
    //            {
    //                str.Add(techOperationDataGridItem.techWork.Comments);
    //            }
    //            else
    //            {
    //                str.Add("");
    //            }

    //            str.Add(techOperationDataGridItem.Vopros);
    //            str.Add(techOperationDataGridItem.Otvet);


    //            dgvMain.Rows.Add(str.ToArray());

    //            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[3].Style.BackColor = Color.Yellow;
    //            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[4].Style.BackColor = Color.Yellow;

    //            continue;
    //        }

    //        str.Add(techOperationDataGridItem.executionWorkItem);

    //        if (techOperationDataGridItem.Nomer != -1)
    //        {
    //            str.Add(techOperationDataGridItem.Nomer.ToString());
    //        }
    //        else
    //        {
    //            str.Add("");
    //        }

    //        str.Add(techOperationDataGridItem.TechOperation);
    //        str.Add(techOperationDataGridItem.Staff);
    //        str.Add(techOperationDataGridItem.TechTransition);
    //        str.Add(techOperationDataGridItem.TechTransitionValue);
    //        str.Add(techOperationDataGridItem.TimeEtap);


    //        techOperationDataGridItem.listMachStr = new List<string>();


    //        for (var index = 0; index < TehCarta.Machine_TCs.Count; index++)
    //        {
    //            if (techOperationDataGridItem.listMachStr.Count == 0 && techOperationDataGridItem.listMach.Count > 0)
    //            {
    //                bool b = techOperationDataGridItem.listMach[index];
    //                if (b)
    //                {
    //                    str.Add(techOperationDataGridItem.TimeEtap);
    //                }
    //                else
    //                {
    //                    if (techOperationDataGridItem.TimeEtap == "-1")
    //                    {
    //                        str.Add("-1");
    //                    }
    //                    else
    //                    {
    //                        str.Add("");
    //                    }
    //                }

    //            }
    //            else
    //            {
    //                str.Add("");
    //            }
    //        }

    //        str.Add(techOperationDataGridItem.Protections);

    //        if (techOperationDataGridItem.techWork != null)
    //        {
    //            str.Add(techOperationDataGridItem.techWork.Comments);
    //        }
    //        else
    //        {
    //            str.Add(techOperationDataGridItem.Comments);
    //        }

    //        str.Add(techOperationDataGridItem.Vopros);
    //        str.Add(techOperationDataGridItem.Otvet);

    //        dgvMain.Rows.Add(str.ToArray());

    //        if (techOperationDataGridItem.ItsComponent)
    //        {
    //            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[3].Style.BackColor = Color.Salmon;
    //            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[4].Style.BackColor = Color.Salmon;
    //        }

    //        if (techOperationDataGridItem.ItsTool)
    //        {
    //            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[3].Style.BackColor = Color.Aquamarine;
    //            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[4].Style.BackColor = Color.Aquamarine;
    //        }

    //    }


    //    SetCommentViewMode();

    //}

    #endregion

    public void UpdateGrid()
    {
        var offScroll = dgvMain.FirstDisplayedScrollingRowIndex;

        ClearAndInitializeGrid();
        PopulateDataGrid();
        SetCommentViewMode();
        SetMachineViewMode();

        if (offScroll < dgvMain.Rows.Count && offScroll > 0) 
            dgvMain.FirstDisplayedScrollingRowIndex = offScroll;
    }

    /// <summary>
    /// Очищает существующие строки и колонки в DataGridView и инициализирует новые колонки.
    /// </summary>
    private void ClearAndInitializeGrid()
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
        dgvMain.Columns.Add("Staff", "Исполнитель");
        dgvMain.Columns.Add("TechTransitionName", "Технологические переходы");
        dgvMain.Columns.Add("", "");
        dgvMain.Columns.Add("", "");

        int i = 0;
        foreach (Machine_TC tehCartaMachineTC in TehCarta.Machine_TCs)
        {
            dgvMain.Columns.Add("Machine+{i}", "Время " + tehCartaMachineTC.Child.Name + ", мин.");
            i++;
        }

        dgvMain.Columns.Add("", "№ СЗ");
        dgvMain.Columns.Add("CommentColumn", "Примечание");
        dgvMain.Columns.Add("PictureNameColumn", "Рис.");
        dgvMain.Columns.Add("RemarkColumn", "Замечание");
        dgvMain.Columns.Add("ResponseColumn", "Ответ");

        dgvMain.Columns["RemarkColumn"].HeaderCell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Italic | FontStyle.Bold);
        dgvMain.Columns["RemarkColumn"].DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Italic);

        dgvMain.Columns["ResponseColumn"].HeaderCell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Italic | FontStyle.Bold);
        dgvMain.Columns["ResponseColumn"].DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Italic);

        int ii = 0;

        dgvMain.Columns[ii].Visible = false;
        ii++;
        dgvMain.Columns[ii].HeaderText = "№";
        dgvMain.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        dgvMain.Columns[ii].Width = 30;
        ii++;
        dgvMain.Columns[ii].HeaderText = "Технологические операции";
        ii++;
        //dgvMain.Columns[ii].HeaderText = "Исполнитель";
        dgvMain.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        dgvMain.Columns[ii].Width = 120;
        ii++;
        //dgvMain.Columns[ii].HeaderText = "Технологические переходы";
        ii++;
        dgvMain.Columns[ii].HeaderText = "Время действ., мин.";
        ii++;
        dgvMain.Columns[ii].HeaderText = "Время этапа, мин.";
        ii++;

        foreach (DataGridViewColumn column in dgvMain.Columns)
        {
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.ReadOnly = true;
            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }

        //dgvMain.Columns[dgvMain.Columns.Count - 1].ReadOnly = false; // todo - зачем это действие?
        // Устанавливаем форматирование для столбцов "Замечание" и "Ответ"
        

        //if (TC_WinForms.DataProcessing.AuthorizationService.CurrentUser.UserRole() != DataProcessing.AuthorizationService.User.Role.User)
        //{
        //    dgvMain.Columns[dgvMain.Columns.Count - 2].ReadOnly = false;
        //}

        dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
    }

    /// <summary>
    /// Заполняет DataGridView данными из списка технологических операций.
    /// </summary>
    private void PopulateDataGrid()
    {
        var techOperationWorksListLocal = 
            TechOperationWorksList.Where(w => !w.Delete).OrderBy(o => o.Order).ToList();
        int nomer = 1;

        foreach (var techOperationWork in techOperationWorksListLocal)
        {
            var executionWorks = techOperationWork.executionWorks.Where(w => !w.Delete).OrderBy(o => o.Order).ToList();

            if (executionWorks.Count == 0)
            {
                list.Add(new TechOperationDataGridItem
                {
                    Nomer = -1,
                    TechOperation = techOperationWork.techOperation.Name,
                    IdTO = techOperationWork.techOperation.Id
                });
            }

            foreach (var executionWork in executionWorks)
            {
                if (executionWork.IdGuid == Guid.Empty)
                {
                    executionWork.IdGuid = Guid.NewGuid();
                }

                var staffStr = string.Join(",", executionWork.Staffs.Select(s => s.Symbol));
                var protectList = executionWork.Protections.Select(p => p.Order).ToList();
                var protectStr = ConvertListToRangeString(protectList);
                var mach = TehCarta.Machine_TCs.Select(tc => executionWork.Machines.Contains(tc)).ToList();

                var itm = new TechOperationDataGridItem
                {
                    Nomer = nomer,
                    Staff = staffStr,
                    TechOperation = techOperationWork.techOperation.Name,
                    TechTransition = executionWork.techTransition?.Name ?? "",
                    TechTransitionValue = executionWork.Value.ToString(),
                    Protections = protectStr,
                    Etap = executionWork.Etap,
                    Posled = executionWork.Posled,
                    Work = true,
                    techWork = executionWork,
                    listMach = mach,
                    Comments = executionWork.Comments,
                    Vopros = executionWork.Vopros,
                    Otvet = executionWork.Otvet,
                    executionWorkItem = executionWork,

                    PictureName = executionWork.PictureName,
                    IdTO = techOperationWork.techOperation.Id
                };

                if (itm.TechTransitionValue == "-1")
                {
                    itm.TechTransitionValue = "Ошибка";
                }

                list.Add(itm);
                nomer++;
            }

            foreach (var toolWork in techOperationWork.ToolWorks)
            {
                list.Add(new TechOperationDataGridItem
                {
                    Nomer = nomer,
                    Staff = "",
                    TechOperation = techOperationWork.techOperation.Name,
                    TechTransition = $"{toolWork.tool.Name}   {toolWork.tool.Type}    {toolWork.tool.Unit}",
                    TechTransitionValue = toolWork.Quantity.ToString(),
                    ItsTool = true,
                    Comments = toolWork.Comments ?? ""
                });
                nomer++;
            }

            foreach (var componentWork in techOperationWork.ComponentWorks)
            {
                list.Add(new TechOperationDataGridItem
                {
                    Nomer = nomer,
                    Staff = "",
                    TechOperation = techOperationWork.techOperation.Name,
                    TechTransition = $"{componentWork.component.Name}   {componentWork.component.Type}    {componentWork.component.Unit}",
                    TechTransitionValue = componentWork.Quantity.ToString(),
                    ItsComponent = true,
                    Comments = componentWork.Comments ?? ""
                });
                nomer++;
            }
        }

        CalculateEtapTimes();
        AddRowsToGrid();
    }

    /// <summary>
    /// Рассчитывает время этапов для технологических операций, обрабатывая списки ExecutionWork и TechOperationDataGridItem.
    /// </summary>
    private void CalculateEtapTimes()
    {
        for (var index = 0; index < list.Count; index++)
        {
            var techOperationDataGridItem = list[index];

            if (techOperationDataGridItem.Work)
            {
                var podchet = new List<ExecutionWork> { techOperationDataGridItem.techWork };
                var podchet2 = new List<TechOperationDataGridItem> { techOperationDataGridItem };
                var tampPodchet = new List<TechOperationDataGridItem>();

                if (!string.IsNullOrEmpty(techOperationDataGridItem.Etap) && techOperationDataGridItem.Etap != "0")
                {
                    int plusIndex = index;
                    while (true)
                    {
                        plusIndex++;
                        if (plusIndex < list.Count)
                        {
                            var tech2 = list[plusIndex];
                            if (!tech2.Work)
                            {
                                tampPodchet.Add(tech2);
                                continue;
                            }

                            if (techOperationDataGridItem.Etap == tech2.Etap)
                            {
                                index = plusIndex;
                                podchet.Add(tech2.techWork);
                                podchet2.Add(tech2);
                                tech2.TimeEtap = "-1";
                                tampPodchet.ForEach(f => f.TimeEtap = "-1");
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

                foreach (var executionWork in podchet)
                {
                    if (!string.IsNullOrEmpty(executionWork.Posled) && executionWork.Posled != "0")
                    {
                        var allSum = podchet.Where(w => w.Posled == executionWork.Posled && w.Value != -1).Sum(s => s.Value);
                        executionWork.TempTimeExecution = allSum;
                    }
                    else
                    {
                        executionWork.TempTimeExecution = executionWork.Value == -1 ? 0 : executionWork.Value;
                    }
                }

                if (podchet2.Count > 1)
                {
                    var col = podchet2[0].listMach.Count;
                    for (int i = 0; i < col; i++)
                    {
                        bool tr = podchet2.Any(item => item.listMach[i]);
                        if (tr)
                        {
                            foreach (var item in podchet2)
                            {
                                item.listMach[i] = true;
                            }
                        }
                    }
                }

                double paral = podchet.Max(m => m.TempTimeExecution);
                techOperationDataGridItem.TimeEtap = paral.ToString();
            }
        }
    }

    /// <summary>
    /// Добавляет строки в DataGridView на основе подготовленного списка TechOperationDataGridItem.
    /// </summary>
    private void AddRowsToGrid()
    {
        foreach (var techOperationDataGridItem in list)
        {
            var str = new List<object>();
            var obj = context.Set<TechOperation>().Where(to => to.Id == techOperationDataGridItem.IdTO).FirstOrDefault();

            if(techOperationDataGridItem.Nomer == 18) // todo - убрать
            {
                Console.WriteLine();
            }

            if (techOperationDataGridItem.techWork != null && techOperationDataGridItem.techWork.Repeat)
            {
                //var repeatNumList = techOperationDataGridItem.techWork.ListexecutionWorkRepeat2
                //    .Select(executionWork => list.SingleOrDefault(s => s.techWork == executionWork))
                //    .Where(bn => bn != null)
                //    .Select(bn => bn.Nomer)
                //    .ToList();

                var repeatNumList = techOperationDataGridItem.techWork.ExecutionWorkRepeats
                    .Select(executionWorkRepeat => list.SingleOrDefault(s => s.techWork == executionWorkRepeat.ChildExecutionWork))
                    .Where(bn => bn != null)
                    .Select(bn => bn.Nomer)
                    .ToList();

                var strP = ConvertListToRangeString(repeatNumList);

                str.Add(techOperationDataGridItem.executionWorkItem);
                str.Add(techOperationDataGridItem.Nomer.ToString());
                str.Add(techOperationDataGridItem.TechOperation);
                str.Add(techOperationDataGridItem.Staff);
                str.Add("Повторить п." + strP);
                str.Add(techOperationDataGridItem.TechTransitionValue);
                str.Add(techOperationDataGridItem.TimeEtap);

                AddMachineColumns(techOperationDataGridItem, str);

                str.Add(techOperationDataGridItem.Protections);
                str.Add(techOperationDataGridItem.techWork?.Comments ?? "");

                str.Add(techOperationDataGridItem.PictureName);

                str.Add(techOperationDataGridItem.Vopros);
                str.Add(techOperationDataGridItem.Otvet);

                if (!obj.IsReleased)
                {
                    AddRowToGrid(str, Color.Yellow, Color.Yellow, Color.FromArgb(220, 218, 233));
                }
                else
                    AddRowToGrid(str, Color.Yellow, Color.Yellow);

                continue;
            }

            str.Add(techOperationDataGridItem.executionWorkItem);
            str.Add(techOperationDataGridItem.Nomer != -1 ? techOperationDataGridItem.Nomer.ToString() : "");
            str.Add(techOperationDataGridItem.TechOperation);
            str.Add(techOperationDataGridItem.Staff);
            str.Add(techOperationDataGridItem.TechTransition);
            str.Add(techOperationDataGridItem.TechTransitionValue);
            str.Add(techOperationDataGridItem.TimeEtap);

            AddMachineColumns(techOperationDataGridItem, str);

            str.Add(techOperationDataGridItem.Protections);
            str.Add(techOperationDataGridItem.techWork?.Comments ?? techOperationDataGridItem.Comments);

            str.Add(techOperationDataGridItem.PictureName);

            str.Add(techOperationDataGridItem.Vopros);
            str.Add(techOperationDataGridItem.Otvet);


            if (techOperationDataGridItem.ItsTool || techOperationDataGridItem.ItsComponent)
            {
                AddRowToGrid(str, techOperationDataGridItem.ItsComponent ? Color.Salmon : Color.Aquamarine, techOperationDataGridItem.ItsComponent ? Color.Salmon : Color.Aquamarine);
            }
            else if(!obj.IsReleased && techOperationDataGridItem.executionWorkItem != null && !techOperationDataGridItem.executionWorkItem.techTransition.IsReleased)
            {
                AddRowToGrid(str, Color.Empty, Color.FromArgb(220, 218, 233), Color.FromArgb(220, 218, 233));
            }
            else if(!obj.IsReleased)
            {
                AddRowToGrid(str, Color.Empty, Color.Empty, Color.FromArgb(220, 218, 233));
            }
            else if (techOperationDataGridItem.executionWorkItem != null && !techOperationDataGridItem.executionWorkItem.techTransition.IsReleased)
            {
                AddRowToGrid(str, Color.Empty, Color.FromArgb(220, 218, 233));
            }
            else
            {
                AddRowToGrid(str, Color.Empty, Color.Empty);
            }
        }
    }

    /// <summary>
    /// Добавляет данные о машинах в строки DataGridView на основе TechOperationDataGridItem.
    /// </summary>
    /// <param name="techOperationDataGridItem">Объект TechOperationDataGridItem, содержащий данные о текущей технологической операции.</param>
    /// <param name="str">Список объектов, представляющий строку данных для добавления в DataGridView.</param>
    private void AddMachineColumns(TechOperationDataGridItem techOperationDataGridItem, List<object> str)
    {
        techOperationDataGridItem.listMachStr = new List<string>();

        for (var index = 0; index < TehCarta.Machine_TCs.Count; index++)
        {
            if (techOperationDataGridItem.listMachStr.Count == 0 && techOperationDataGridItem.listMach.Count > 0)
            {
                bool b = techOperationDataGridItem.listMach[index];
                str.Add(b ? techOperationDataGridItem.TimeEtap : techOperationDataGridItem.TimeEtap == "-1" ? "-1" : "");
            }
            else
            {
                str.Add("");
            }
        }
    }

    /// <summary>
    /// Добавляет строку данных в DataGridView и устанавливает стиль ячеек.
    /// </summary>
    /// <param name="rowData">Список объектов, представляющий строку данных для добавления в DataGridView.</param>
    /// <param name="backColor1">Цвет фона первой ячейки.</param>
    /// <param name="backColor2">Цвет фона второй ячейки.</param>
    private void AddRowToGrid(List<object> rowData, Color? backColor1 = null, Color? backColor2 = null, Color? backColor3 = null)
    {
        dgvMain.Rows.Add(rowData.ToArray());

        if (backColor1.HasValue)
        {
            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[3].Style.BackColor = backColor1.Value;
        }
        if (backColor2.HasValue)
        {
            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[4].Style.BackColor = backColor2.Value;
        }
        if (backColor3.HasValue)
        {
            dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[2].Style.BackColor = backColor3.Value;
        }
        //dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[3].Style.BackColor = backColor1;
        //dgvMain.Rows[dgvMain.Rows.Count - 1].Cells[4].Style.BackColor = backColor2;
    }


    private void DgvMain_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            // Получение имени столбца по индексу
            string columnName = dgvMain.Columns[e.ColumnIndex].HeaderText;

            // Проверка имени столбца
            if (columnName == "Время действ., мин.")
            {
                var executionWork = dgvMain.Rows[e.RowIndex].Cells[0].Value as ExecutionWork;
                if (executionWork != null)
                {
                    if (!string.IsNullOrEmpty(executionWork.Coefficient) && executionWork.techTransition != null)
                        dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = executionWork.techTransition.TimeExecution + executionWork.Coefficient;
                }
            }
        }
    }


    private void DgvMain_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        // Первую строку всегда показывать
        //if (e.RowIndex < 0) // todo: Исправли с == 0  на  < 0 т.е мешает применению стиля к первой строке. Пока не поянятно, зачем была созданна.
        //    return;

        // Если значение ячейки совпадает с предыдущим значением в столбце с ТО (индекс 2),
        // то ячейка остается пустой.
        if (IsTheSameCellValue(e.ColumnIndex, e.RowIndex) && e.ColumnIndex == 2)
        {
            e.Value = string.Empty;
            e.FormattingApplied = true;
        }

        // Если это столбцов с временем этапа и механизмов (индекс >= 6), и значение ячейки равно "-1",
        // то ячейка остается пустой.
        if (e.ColumnIndex >= 6)
        {
            var cellValue = (string)e.Value;
            if (cellValue == "-1")
            {
                e.Value = string.Empty;
                e.FormattingApplied = true;
            }
        }

        // Если мы находимся в режиме редактирования ТК (_tcViewState.IsViewMode == false),
        // проверяем возможность редактирования ячейки.
        if (!_tcViewState.IsViewMode)
        {
            var executionWork = (ExecutionWork)dgvMain.Rows[e.RowIndex].Cells[0].Value;

            if (executionWork != null)
            {
                if ((e.ColumnIndex == dgvMain.Columns["RemarkColumn"].Index 
                    || e.ColumnIndex == dgvMain.Columns["ResponseColumn"].Index) 
                    && DataProcessing.AuthorizationService.CurrentUser.UserRole() 
                    == DataProcessing.AuthorizationService.User.Role.Lead)
                {
                    CellChangeReadOnly(dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex], false);
                }
                else if (e.ColumnIndex == dgvMain.Columns["ResponseColumn"].Index
                    && TC_WinForms.DataProcessing.AuthorizationService.CurrentUser.UserRole() 
                    == DataProcessing.AuthorizationService.User.Role.Implementer)
                {
                    CellChangeReadOnly(dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex], false);
                }
                else if (e.ColumnIndex == dgvMain.Columns["CommentColumn"].Index
                    || e.ColumnIndex == dgvMain.Columns["PictureNameColumn"].Index)
                {
                    CellChangeReadOnly(dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex], false);
                }
                else if((executionWork.techTransition?.Name.Contains( "Повторить") ?? false)
                    && (e.ColumnIndex == dgvMain.Columns["Staff"].Index
                    || e.ColumnIndex == dgvMain.Columns["TechTransitionName"].Index))
                {
                    // пропускаю действие, т.к. отрисовка цвета уже произошла
                    //SetCellBackColor(dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex], Color.Yellow);
                }
                else if ((!executionWork.techTransition.IsReleased 
                            || !executionWork.techOperationWork.techOperation.IsReleased)
                        && (e.ColumnIndex == dgvMain.Columns["TechTransitionName"].Index
                            || e.ColumnIndex == dgvMain.Columns[2].Index))
                {
                    // пропускаю действие, т.к. отрисовка цвета уже произошла
                }
                else
                {
                    CellChangeReadOnly(dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex], true);
                }
            }
        }
    }

    public void CellChangeReadOnly(DataGridViewCell cell, bool isReadOnly)
    {
        // Делаем ячейку редактируемой
        cell.ReadOnly = isReadOnly;
        // Выделить строку цветом
        cell.Style.BackColor = isReadOnly ? Color.White : Color.LightGray;
    }
    private void SetCellBackColor(DataGridViewCell cell, Color color)
    {
        cell.Style.BackColor = color;
    }

    private void DgvMain_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;

        // Пропуск заголовков колонок и строк, и первой строки
        if (e.RowIndex < 1 || e.ColumnIndex < 0)
        {
            if (e.ColumnIndex == dgvMain.ColumnCount - 2)
            {
                e.AdvancedBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
            }
            e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.Single;
            return;
        }

        if (e.ColumnIndex == dgvMain.ColumnCount - 2)
        {
            e.AdvancedBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
        }

        if (IsTheSameCellValue(e.ColumnIndex, e.RowIndex) && e.ColumnIndex == 2)
        {
            e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
        }
        else
        {
            e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.Single;
        }

        if (e.ColumnIndex >= 6)
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
        if (row == 0)
        {
            return false;
        }
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
        int maxOrder = -1;

        if (TechOperationWorksList.Count > 0)
        {
            maxOrder = TechOperationWorksList.Max(m => m.Order);
        }

        // Данный механизм отвечает за добавление удалённого объекта с таким же Id
        // был удалён в соответствии с п154 замечаний
        //var vb = TechOperationWorksList.Where(s => s.techOperation == TechOperat && s.Delete == true).ToList();
        if (false)//vb.Count > 0)
        {
            //vb[0].Delete = false;
        }
        else
        {
            TechOperationWork techOperationWork = new TechOperationWork();
            techOperationWork.techOperation = TechOperat;
            techOperationWork.technologicalCard = TehCarta;
            techOperationWork.NewItem = true;
            techOperationWork.Order = maxOrder + 1;

            TechOperationWorksList.Add(techOperationWork);


            if (TechOperat.Category == "Типовая ТО")
            {
                foreach (TechTransitionTypical item in TechOperat.techTransitionTypicals)
                {
                    var temp = item.TechTransition;
                    if (temp == null)
                    {
                        temp = context.TechTransitions.Single(s => s.Id == item.TechTransitionId);
                    }
                    AddTechTransition(temp, techOperationWork, item);
                }
            }
        }
    }


    public void AddTechTransition(TechTransition tech, TechOperationWork techOperationWork, TechTransitionTypical techTransitionTypical = null, CoefficientForm coefficient = null)
    {
        TechOperationWork TOWork = TechOperationWorksList.Single(s => s == techOperationWork);
        // Данный механизм отвечает за добавление удалённого объекта с таким же Id
        // был удалён в соответствии с п154 замечаний
        //var exec = TOWork.executionWorks.Where(w => w.techTransition == tech && w.Delete == true).ToList();
        if (false)//exec.Count > 0)
        {
            //var one = exec[0];
            //one.Delete = false;
        }
        else
        {
            int max = 0;
            if (TOWork.executionWorks.Count > 0)
            {
                max = TOWork.executionWorks.Max(w => w.Order);
            }

            ExecutionWork techOpeWork = new ExecutionWork();

            if (tech.Name != "Повторить" && tech.Name != "Повторить п.")
            {
                techOpeWork.IdGuid = Guid.NewGuid();
                techOpeWork.techOperationWork = TOWork;
                techOpeWork.NewItem = true;
                techOpeWork.techTransition = tech;
                techOpeWork.Value = tech.TimeExecution;
                if (coefficient != null)
                {
                    techOpeWork.Coefficient = coefficient.GetCoefficient;
                    techOpeWork.Value = coefficient.GetValue;
                }
                techOpeWork.Order = max + 1;
                TOWork.executionWorks.Add(techOpeWork);
            }
            else
            {
                //ExecutionWork techOpeWork = new ExecutionWork();
                techOpeWork.IdGuid = Guid.NewGuid();
                techOpeWork.techOperationWork = TOWork;
                techOpeWork.NewItem = true;
                techOpeWork.Repeat = true;
                techOpeWork.Order = max + 1;
                techOpeWork.techTransition = tech;
                //techOpeWork.Value = tech.TimeExecution;
                TOWork.executionWorks.Add(techOpeWork);
            }

            if (techTransitionTypical != null)
            {
                techOpeWork.Etap = techTransitionTypical.Etap;
                techOpeWork.Posled = techTransitionTypical.Posled;
                techOpeWork.Coefficient = techTransitionTypical.Coefficient;
                techOpeWork.Comments = techTransitionTypical.Comments ?? "";

                techOpeWork.Value = string.IsNullOrEmpty(techTransitionTypical.Coefficient) ? tech.TimeExecution : WorkParser.EvaluateExpression(tech.TimeExecution + "*" + techTransitionTypical.Coefficient);
            }

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

    public void DeleteTechTransit(Guid IdGuid, TechOperationWork techOperationWork) //todo - IdGuid используется только для удаления тех операций. Можно оптимизировать этот процесс и убрать поле IdGuid из ExecutionWork
    {
        TechOperationWork TOWork = TechOperationWorksList.Single(s => s == techOperationWork);
        var vb = TOWork.executionWorks.SingleOrDefault(s => s.IdGuid == IdGuid);
        if (vb != null)
        {

            if (techOperationWork.techOperation.Category == "Типовая ТО")
            {
                if (vb.Repeat == false)
                {
                    return;
                }
            }

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

        //using (var context = new MyDbContext())
        //{
        //    var TC = context.TechnologicalCards.Single(s => s.Id == 1);

        //    TechOperation techOperation = new TechOperation();
        //    techOperation.Name = "Установка автовышки";
        //    context.TechOperations.Add(techOperation);

        //    TechOperationWork techOperationWork = new TechOperationWork();
        //    techOperationWork.techOperation = techOperation;
        //    techOperationWork.technologicalCard = TC;

        //    var techOperation2 = new TechOperation();
        //    techOperation2.Name = "Подготовка к работе с люльки";
        //    context.TechOperations.Add(techOperation2);

        //    TechOperationWork techOperationWork2 = new TechOperationWork();
        //    techOperationWork2.techOperation = techOperation2;
        //    techOperationWork2.technologicalCard = TC;




        //    TechTransition techTransition = new TechTransition();
        //    techTransition.Name = "Определить место установки техники";
        //    techTransition.TimeExecution = 3;
        //    context.TechTransitions.Add(techTransition);

        //    ExecutionWork executionWork = new ExecutionWork();
        //    executionWork.techTransition = techTransition;
        //    executionWork.techOperationWork = techOperationWork;
        //    context.ExecutionWorks.Add(executionWork);

        //    techTransition = new TechTransition();
        //    techTransition.Name = "Установить автовышку";
        //    techTransition.TimeExecution = 5.5;
        //    context.TechTransitions.Add(techTransition);

        //    executionWork = new ExecutionWork();
        //    executionWork.techTransition = techTransition;
        //    executionWork.techOperationWork = techOperationWork;
        //    context.ExecutionWorks.Add(executionWork);

        //    techTransition = new TechTransition();
        //    techTransition.Name = "Установить ограждение рабочей зоны";
        //    techTransition.TimeExecution = 5;
        //    context.TechTransitions.Add(techTransition);

        //    executionWork = new ExecutionWork();
        //    executionWork.techTransition = techTransition;
        //    executionWork.techOperationWork = techOperationWork;
        //    context.ExecutionWorks.Add(executionWork);

        //    techTransition = new TechTransition();
        //    techTransition.Name = "Подготовить инструменты и материалы";
        //    techTransition.TimeExecution = 4;
        //    context.TechTransitions.Add(techTransition);

        //    executionWork = new ExecutionWork();
        //    executionWork.techTransition = techTransition;
        //    executionWork.techOperationWork = techOperationWork;
        //    context.ExecutionWorks.Add(executionWork);

        //    techTransition = new TechTransition();
        //    techTransition.Name = "Загрузить в люльку инструменты и материалы";
        //    techTransition.TimeExecution = 3;
        //    context.TechTransitions.Add(techTransition);

        //    executionWork = new ExecutionWork();
        //    executionWork.techTransition = techTransition;
        //    executionWork.techOperationWork = techOperationWork2;
        //    context.ExecutionWorks.Add(executionWork);

        //    techTransition = new TechTransition();
        //    techTransition.Name = "Надеть страховочную привязь";
        //    techTransition.TimeExecution = 1.5;
        //    context.TechTransitions.Add(techTransition);

        //    executionWork = new ExecutionWork();
        //    executionWork.techTransition = techTransition;
        //    executionWork.techOperationWork = techOperationWork2;
        //    context.ExecutionWorks.Add(executionWork);

        //    techTransition = new TechTransition();
        //    techTransition.Name = "Войти в люльку, закрепиться удерживающим стропом, закрыть дверь на запорное устройство";
        //    techTransition.TimeExecution = 1;
        //    context.TechTransitions.Add(techTransition);

        //    executionWork = new ExecutionWork();
        //    executionWork.techTransition = techTransition;
        //    executionWork.techOperationWork = techOperationWork2;
        //    context.ExecutionWorks.Add(executionWork);


        //    context.SaveChanges();
        //}
    }

    private void button2_Click(object sender, EventArgs e)
    {
        // в случае Режима просмотра форма не открывается
        if (_tcViewState.IsViewMode)
            return;

        // Проверяем, была ли форма создана и не была ли закрыта
        if (_editForm == null || _editForm.IsDisposed)
        {
            _editForm = new AddEditTechOperationForm(this);
        }

        // Выводим форму на передний план
        _editForm.Show();
        _editForm.BringToFront();

        HasChanges = true;
    }

    private void button1_Click_1(object sender, EventArgs e)
    {
        //context = new MyDbContext();
        //context.ChangeTracker.Clear();

        // TehCarta.Staff_TCs = Staff_TC;

        List<TechOperationWork> AllDele = TechOperationWorksList.Where(w => w.Delete == true).ToList();
        foreach (TechOperationWork techOperationWork in AllDele)
        {
            TechOperationWorksList.Remove(techOperationWork);
            if (techOperationWork.NewItem == false)
            {
                context.TechOperationWorks.Remove(techOperationWork);
            }
        }

        foreach (TechOperationWork techOperationWork in TechOperationWorksList)
        {
            var allDel = techOperationWork.executionWorks.Where(w => w.Delete == true).ToList();
            foreach (ExecutionWork executionWork in allDel)
            {
                techOperationWork.executionWorks.Remove(executionWork);
            }

            var to = context.TechOperationWorks.SingleOrDefault(s => techOperationWork.Id != 0 && s.Id == techOperationWork.Id);
            if (to == null)
            {
                context.TechOperationWorks.Add(techOperationWork);
            }
            else
            {
                to = techOperationWork;
            }

            foreach (ToolWork toolWork in techOperationWork.ToolWorks)
            {
                if (TehCarta.Tool_TCs.SingleOrDefault(s => s.Child == toolWork.tool) == null)
                {
                    Tool_TC tool = new Tool_TC();
                    tool.Child = toolWork.tool;
                    tool.Quantity = toolWork.Quantity;
                    TehCarta.Tool_TCs.Add(tool);
                }
            }

            foreach (ComponentWork componentWork in techOperationWork.ComponentWorks)
            {
                if (TehCarta.Component_TCs.SingleOrDefault(s => s.Child == componentWork.component) == null)
                {
                    Component_TC Comp = new Component_TC();
                    Comp.Child = componentWork.component;
                    Comp.Quantity = componentWork.Quantity;
                    TehCarta.Component_TCs.Add(Comp);
                }
            }
        }


        try
        {
            context.SaveChanges();
            // MessageBox.Show("Успешно сохранено");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message + "\n" + exception.InnerException);
        }
    }

    public bool HasChanges { get; set; }
    public async Task SaveChanges()
    {
        button1_Click_1(null, null);
    }

    private string ConvertListToRangeString(List<int> numbers)
    {
        if (numbers == null || !numbers.Any())
            return string.Empty;

        // Сортировка списка
        numbers.Sort();

        StringBuilder stringBuilder = new StringBuilder();
        int start = numbers[0];
        int end = start;

        for (int i = 1; i < numbers.Count; i++)
        {
            // Проверяем, идут ли числа последовательно
            if (numbers[i] == end + 1)
            {
                end = numbers[i];
            }
            else
            {
                // Добавляем текущий диапазон в результат
                if (start == end)
                    stringBuilder.Append($"{start}, ");
                else
                    stringBuilder.Append($"{start}-{end}, ");

                // Начинаем новый диапазон
                start = end = numbers[i];
            }
        }

        // Добавляем последний диапазон
        if (start == end)
            stringBuilder.Append($"{start}");
        else
            stringBuilder.Append($"{start}-{end}");

        return stringBuilder.ToString().TrimEnd(',', ' ');


    }

    private void TechOperationForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        //if (HasChanges)
        //{
        //    var result = MessageBox.Show("Вы хотите сохранить изменения перед закрытием?", "Сохранение изменений", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        //    if (result == DialogResult.Yes)
        //    {
        //        button1_Click_1(null, null);
        //    }
        //    else if (result == DialogResult.Cancel)
        //    {
        //        e.Cancel = true;
        //    }
        //}
    }

    public void HighlightExecutionWorkRow(ExecutionWork executionWork, bool scrollToRow = false)
    {
        if (executionWork == null)
            return;

        foreach (DataGridViewRow row in dgvMain.Rows)
        {
            if (row.Cells[0].Value is ExecutionWork currentWork 
                && currentWork.Id == executionWork.Id 
                && currentWork.techTransitionId == executionWork.techTransitionId
                && currentWork.techOperationWorkId == executionWork.techOperationWorkId
                )
            {
                dgvMain.ClearSelection(); // Снимите выделение со всех строк
                row.Selected = true; // Выделите найденную строку
                if (scrollToRow)
                {
                    dgvMain.FirstDisplayedScrollingRowIndex = row.Index; // Прокрутите до выделенной строки
                }
                break;
            }
        }
    }



}
