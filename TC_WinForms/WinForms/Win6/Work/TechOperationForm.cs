using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;
using System.Text;
using TC_WinForms.DataProcessing;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Diagram;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace TC_WinForms.WinForms.Work;

public partial class TechOperationForm : Form, ISaveEventForm, IViewModeable, IOnActivationForm
{
	private ILogger _logger;

	private readonly TcViewState _tcViewState;

    //private bool _isViewMode;
    //private bool _isCommentViewMode;
    private bool _isMachineViewMode;

    public MyDbContext context { get; private set; }

    public readonly int _tcId;

    private List<TechOperationDataGridItem> TechOperationDataGridItems = new List<TechOperationDataGridItem>();
    private AddEditTechOperationForm? _editForm;

    public List<TechOperationWork> TechOperationWorksList = null!;
    public TechnologicalCard TehCarta = null!;

    public bool CloseFormsNoSave { get; set; } = false;

    public TechOperationForm(int tcId, TcViewState tcViewState, MyDbContext context)//,  bool viewerMode = false)
    {
        this._tcViewState = tcViewState;
        this.context = context;
        this._tcId = tcId;

		_logger = Log.Logger
			.ForContext<TechOperationForm>()
			.ForContext("TcId", _tcId);

		_logger.Information("Инициализация формы.");

		//_isViewMode = viewerMode;
		InitializeComponent();
        dgvMain.CellPainting += DgvMain_CellPainting;
        dgvMain.CellFormatting += DgvMain_CellFormatting;
        dgvMain.CellEndEdit += DgvMain_CellEndEdit;
        dgvMain.CellMouseEnter += DgvMain_CellMouseEnter;

        this.KeyPreview = true;
        this.KeyDown += new KeyEventHandler(Form_KeyDown);

        _tcViewState.ViewModeChanged += OnViewModeChanged;

        //this.FormClosed += (sender, e) => this.Dispose();
    }

    private void TechOperationForm_Load(object sender, EventArgs e)
    {
        // Блокировка формы на время загрузки данных
        this.Enabled = false;

        // спросить у пользователя, какой загрузкой воспользоваться
        TehCarta = _tcViewState.TechnologicalCard;
        TechOperationWorksList = _tcViewState.TechOperationWorksList;
        //await LoadDataAsync8(tcId);

        UpdateGrid();
        SetCommentViewMode();
        SetMachineViewMode();
        SetViewMode();

        this.Enabled = true;
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
        pnlControls.Visible = !_tcViewState.IsViewMode;
    }

    private void OnViewModeChanged()
    {
        UpdateGrid();
    }

	#region Обработка нажатия клавиш (Ctrl + C / V) + вывод информации о выделении

	/// <summary>
	/// Обработчик события нажатия клавиш в форме.
	/// </summary>
	private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
			// Вставка данных из буфера обмена только если не включёр режим просмотра
			if (_tcViewState.IsViewMode)
				PasteClipboardValue();
			else
				PasteCopiedData();
			
            e.Handled = true;
		}
        // Новая обработка Ctrl + C
		else if (e.Control && e.KeyCode == Keys.C)
		{
            if (_tcViewState.IsViewMode)
				CopyClipboardValue();
			else
				CopyData();     // Выводим информацию о выделении
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Delete)
        {
            DeleteCellValue();
            e.Handled = true;
        }
    }

	/// <summary>
	/// Копирует информацию о выделенном объекта в зависимости от того, какая ячейка или какие строки выделены.
	/// </summary>
	private void CopyData()
	{
		GetSelectedDataInfo(
            out List<int> selectedRowIndices, 
            out CopyScopeEnum? copyScope);

		if (selectedRowIndices.Count == 0)
			return;

		try
		{
            // Копируем текст ячейки в буфер обмена
			CopyClipboardValue();
			// Если копируются данные не из текстовой облости, то сохраняем их в TcCopyData
			if (copyScope != CopyScopeEnum.Text)
				TcCopyData.SetCopyDate(selectedRowIndices.Select(i => TechOperationDataGridItems[i]).ToList(), copyScope);
		}
		catch
		(Exception ex)
		{
			MessageBox.Show(ex.Message);
			return;
		}
	}

    private int? GetColumnIndex(CopyScopeEnum copyScope)
    {
		switch (copyScope)
		{
			case CopyScopeEnum.Staff:
				return dgvMain.Columns["Staff"].Index;
			case CopyScopeEnum.Protections:
				return dgvMain.Columns["Protection"].Index;
			case CopyScopeEnum.Text:
				return dgvMain.Columns["Text"].Index;
			case CopyScopeEnum.Machines:
				return dgvMain.Columns["Machine"].Index;
			case CopyScopeEnum.TechOperation:
				return dgvMain.Columns["TechOperation"].Index;
			default:
				return null;
		}
	}

	private void GetSelectedDataInfo(out List<int> selectedRowIndices, out CopyScopeEnum? copyScope)
	{
		// Соберём все уникальные индексы строк, где есть выделенные ячейки.
		var selectedRows = dgvMain.SelectedCells
			.Cast<DataGridViewCell>()
			.Distinct()
			.ToList();

		selectedRowIndices = selectedRows
			.Select(c => c.RowIndex)
			.Distinct()
			.OrderBy(idx => idx)
			.ToList();
		
		copyScope = null;
		if (selectedRowIndices.Count == 1 && dgvMain.SelectedCells.Count == 1)
		{
			// Одна ячейка
			var cell = dgvMain.SelectedCells[0];
			copyScope = GetCopyScopeByCell(cell);
		}
        else if (selectedRowIndices.Count > 1)
        {
            copyScope = CopyScopeEnum.RowRange;
		}
	}

	private void PasteCopiedData()
    {
		List<int> selectedRowIndices;
		CopyScopeEnum? copyScope;

		GetSelectedDataInfo(out selectedRowIndices, out copyScope);

		if (selectedRowIndices.Count == 0)
			return;

		if (copyScope == null)
        {
            MessageBox.Show("Не удалось определить тип копирования.");
            return;
		}    

        if(TcCopyData.GetCopyTcId() != _tcId && copyScope != CopyScopeEnum.Text)
		{
			MessageBox.Show("Данные не могут быть вставлены в другую ТК.");
			return;
		}

		var selectedItems = selectedRowIndices.Select(i => TechOperationDataGridItems[i]).ToList();

		if (copyScope == CopyScopeEnum.Staff)
		{
			// проверяем есть ли в скопированных данных информация
			if (TcCopyData.CopyScope != CopyScopeEnum.Staff) { return; }

			if (selectedItems.Count != 1) { throw new Exception("Ошибка при вставке данных. Обработка выделенных данных Персонал."); }
			var setectedEw = selectedItems[0].executionWorkItem;
			if (setectedEw == null)
			{
				MessageBox.Show("В данной строке невозможно вставить связь с Персоналом");
				return;
			}

            UpdateStaffInRow(selectedRowIndices[0], setectedEw, TcCopyData.FullItems[0].executionWorkItem.Staffs);
		}
		else if (copyScope == CopyScopeEnum.Protections)
		{
			// проверяем есть ли в скопированных данных информация
			if (TcCopyData.CopyScope != CopyScopeEnum.Protections) { return; }

			if (selectedItems.Count != 1) { throw new Exception("Ошибка при вставке данных. Обработка выделенных данных СЗ."); }
			var setectedEw = selectedItems[0].executionWorkItem;
			if (setectedEw == null)
			{
				MessageBox.Show("В данной строке невозможно вставить связь с СЗ");
				return;
			}

			UpdateProtectionsInRow(selectedRowIndices[0], setectedEw, TcCopyData.FullItems[0].executionWorkItem.Protections);
		}
		else if (copyScope == CopyScopeEnum.ToolOrComponents)
		{
            //MessageBox.Show("Вставка компонента/инструмента");
		}
		else if(copyScope == CopyScopeEnum.Text)
        {
            PasteClipboardValue();
		}
	}

	private void UpdateStaffInRow(int rowIndices, ExecutionWork setectedEw, List<Staff_TC> copiedStaff)
	{
		// проверка на наличие изменений
		if (setectedEw.Staffs.Count == copiedStaff.Count)
		{
			foreach (var staff in copiedStaff)
			{
				if (!setectedEw.Staffs.Contains(staff))
				{
					return;
				}
			}
		}

		var currentCopyScope = CopyScopeEnum.Staff;
		var columnIndex = GetColumnIndex(currentCopyScope) 
            ?? throw new Exception($"Не найден столбец соответствующий типу {currentCopyScope}");
		var newStaffSymbols = "";
		setectedEw.Staffs.Clear();

		if (copiedStaff.Count > 0)
		{
			foreach (var staff in copiedStaff)
			{
				setectedEw.Staffs.Add(staff);
			}

			newStaffSymbols = string.Join(",", setectedEw.Staffs.Select(s => s.Symbol));
		}

		UpdateCellValue(rowIndices, (int)columnIndex, newStaffSymbols);
	}

	private void UpdateProtectionsInRow(int rowIndices, ExecutionWork setectedEw, List<Protection_TC> copiedProtections)
	{
		var currentCopyScope = CopyScopeEnum.Protections;
		var protectionsList = setectedEw.Protections;
		// проверка на наличие изменений
		if (protectionsList.Count == copiedProtections.Count)
		{
			foreach (var obj in copiedProtections)
			{
				if (!protectionsList.Contains(obj))
				{
					return;
				}
			}
		}

		var columnIndex = GetColumnIndex(currentCopyScope)
			?? throw new Exception($"Не найден столбец соответствующий типу {currentCopyScope}");
		var newCellValue = "";
		protectionsList.Clear();

		if (copiedProtections.Count > 0)
		{
			foreach (var obj in copiedProtections)
			{
				protectionsList.Add(obj);
			}
            
            var objectOrderList = protectionsList.Select(s => s.Order).ToList();
			newCellValue = ConvertListToRangeString(objectOrderList);
		}

		UpdateCellValue(rowIndices, (int)columnIndex, newCellValue);
	}

	/// <summary>
	/// Возвращает тип копирования по ячейке.
	/// </summary>
	/// <param name="copyScope"></param>
	/// <param name="cell"></param>
	/// <returns></returns>
	private CopyScopeEnum? GetCopyScopeByCell(DataGridViewCell cell)
	{
        CopyScopeEnum? copyScope = null;
		string columnName = dgvMain.Columns[cell.ColumnIndex].HeaderText;
        int cellRowIndex = cell.RowIndex;

		var techOperationDataGridItem = TechOperationDataGridItems[cellRowIndex];

		switch (columnName)
		{
			case "Исполнитель":
				copyScope = CopyScopeEnum.Staff;
				break;
			case "№ СЗ":
				copyScope = CopyScopeEnum.Protections;
				break;
			case "Технологические операции":
				copyScope = CopyScopeEnum.TechOperation;
				break;
            case "Технологические переходы":
				if (techOperationDataGridItem.ItsTool || techOperationDataGridItem.ItsComponent)
					copyScope = CopyScopeEnum.ToolOrComponents;
				else
					copyScope = CopyScopeEnum.Row;
                break;
			case "Примечание":
			case "Рис.":
            case "Замечание":
			case "Ответ":
				copyScope = CopyScopeEnum.Text;
				break;
		}

		return copyScope;
	}

	/// <summary>
	/// Копирование значения текущей ячейки (если она не пустая) в буфер обмена.
	/// </summary>
	private void CopyClipboardValue()
	{
		if (dgvMain.CurrentCell != null)
		{
			var cellValue = dgvMain.CurrentCell.Value?.ToString();
			if (!string.IsNullOrEmpty(cellValue))
			{
				Clipboard.SetText(cellValue);
                //TcCopyData.SetCopyText(cellValue);
			}
			else
			{
                //TcCopyData.Clear();
				Clipboard.Clear(); 
			}
		}
	}

	/// <summary>
	/// Удаление значения из текущей ячейки (у вас уже реализовано).
	/// </summary>
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

	/// <summary>
	/// Вставка значения из буфера обмена в текущую ячейку (ваша логика уже была, оставим как есть).
	/// </summary>
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

	#endregion

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
                if (_editForm?.IsDisposed == false)
                    _editForm.UpdateLocalTP();
            }
        }
        else if (e.ColumnIndex == dgvMain.Columns["CommentColumn"].Index)
        {
            var idd = (ExecutionWork)dgvMain.Rows[e.RowIndex].Cells[0].Value;
            var gg = (string)dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            var itsTool = TechOperationDataGridItems[e.RowIndex].ItsTool;
            var ItsComponent = TechOperationDataGridItems[e.RowIndex].ItsComponent;
            
            if (idd != null)
            {
                if (gg == idd.Comments) return;

                idd.Comments = gg;
                HasChanges = true;
                if(_editForm?.IsDisposed == false)
                    _editForm.UpdateLocalTP();
            }
            else if (itsTool || ItsComponent)
            {
                var techWork = TechOperationDataGridItems[e.RowIndex].TechOperationWork;
                var toolComponentName = (string)dgvMain.Rows[e.RowIndex].Cells[4].Value;
                     
                if (itsTool)
                {
                    var editedTool = techWork.ToolWorks.Where(t => toolComponentName.Contains(t.tool.Name)).FirstOrDefault();
                    editedTool.Comments = gg;
                }
                else
                {
                    var editedComp = techWork.ComponentWorks.Where(t => toolComponentName.Contains(t.component.Name)).FirstOrDefault();
                    editedComp.Comments = gg;
                }

                var isEditFormActive = _editForm?.IsDisposed == false;
                HasChanges = true;

                if (isEditFormActive && itsTool)
                    _editForm.UpdateInstrumentLocal();
                else if (isEditFormActive && ItsComponent)
                    _editForm.UpdateComponentLocal();
            }
        }
    }

    public bool GetDontSaveData()
    {
        return HasChanges;
    }


    public void UpdateGrid()
    {
        try // временная заглушка от ошибки возникающей при переключении на другую форму в процессе загрузки данных
        {
            var offScroll = dgvMain.FirstDisplayedScrollingRowIndex;

            ClearAndInitializeGrid();
            PopulateDataGrid();
            SetCommentViewMode();
            SetMachineViewMode();

            if (offScroll < dgvMain.Rows.Count && offScroll > 0)
                dgvMain.FirstDisplayedScrollingRowIndex = offScroll;

            // проверить, открыта ли форма с БС и обновить ее
            var bsForm = CheckOpenFormService.FindOpenedForm<DiagramForm>(_tcId);
            if (bsForm != null)
            {
                bsForm.UpdateVisualData();
            }
        }
        catch (Exception ex)
        {
            //MessageBox.Show(ex.Message);
        }

    }

    /// <summary>
    /// Очищает существующие строки и колонки в DataGridView и инициализирует новые колонки.
    /// </summary>
    private void ClearAndInitializeGrid()
    {
        dgvMain.Rows.Clear();
        TechOperationDataGridItems.Clear();

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
            dgvMain.Columns.Add("Machine" + i, "Время " + tehCartaMachineTC.Child.Name + ", мин.");
            i++;
        }

        dgvMain.Columns.Add("Protection", "№ СЗ");
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

        foreach (DataGridViewColumn col in dgvMain.Columns)
        {
            col.FillWeight = col.HeaderText.Contains("Время") ? GetFillWeight("Время") : GetFillWeight(col.HeaderText);
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.MinimumWidth = col.HeaderText.Contains("Время") ? 50 :(int)col.FillWeight;
            col.MinimumWidth = col.HeaderText.Contains("Замечание") || col.HeaderText.Contains("Ответ") ? 200 : (int)col.FillWeight;
            col.Resizable = col.HeaderText.Contains("Замечание") || col.HeaderText.Contains("Ответ") ? DataGridViewTriState.True : DataGridViewTriState.NotSet;
        }

        dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvMain.Columns[2].Frozen = true;

    }

    private float GetFillWeight(string FieldName)
    {
        switch (FieldName)
        {
            case "":
                return 100;
            case "№":
                return 30;
            case "Технологические операции":
            case "Технологические переходы":
            case "Примечание":
            case "Рис.":
                return 140;
            case "Исполнитель":
                return 120;
            case "Время":
            case "№ СЗ":
                return 50;
            case "Замечание":
            case "Ответ":
                return 300;
            default:
                return 100;
        }
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
                TechOperationDataGridItems.Add(new TechOperationDataGridItem
                {
                    Nomer = -1,
                    TechOperationWork = techOperationWork,
                    TechOperation = $"№{techOperationWork.Order} {techOperationWork.techOperation.Name}",
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
                    TechOperationWork = techOperationWork,
                    TechOperation = $"№{techOperationWork.Order} {techOperationWork.techOperation.Name}",
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
                    executionWorkItem = executionWork, // todo: зачем хранить 2 ссылки на объект ТП?

                    PictureName = executionWork.PictureName,
                    IdTO = techOperationWork.techOperation.Id
                };

                if (itm.TechTransitionValue == "-1")
                {
                    itm.TechTransitionValue = "Ошибка";
                }

                TechOperationDataGridItems.Add(itm);
                nomer++;
            }

            foreach (var toolWork in techOperationWork.ToolWorks.Where(t => t.IsDeleted == false).ToList())
            {
                TechOperationDataGridItems.Add(new TechOperationDataGridItem
                {
                    Nomer = nomer,
                    Staff = "",
                    TechOperation = $"№{techOperationWork.Order} {techOperationWork.techOperation.Name}",
                    TechTransition = $"{toolWork.tool.Name}   {toolWork.tool.Type}    {toolWork.tool.Unit}",
                    TechTransitionValue = toolWork.Quantity.ToString(),
                    ItsTool = true,
                    Comments = toolWork.Comments ?? "",
                    TechOperationWork = techOperationWork
                });
                nomer++;
            }

            foreach (var componentWork in techOperationWork.ComponentWorks.Where(t => t.IsDeleted == false).ToList())
            {
                TechOperationDataGridItems.Add(new TechOperationDataGridItem
                {
                    Nomer = nomer,
                    Staff = "",
                    TechOperation = $"№{techOperationWork.Order} {techOperationWork.techOperation.Name}",
                    TechTransition = $"{componentWork.component.Name}   {componentWork.component.Type}    {componentWork.component.Unit}",
                    TechTransitionValue = componentWork.Quantity.ToString(),
                    ItsComponent = true,
                    Comments = componentWork.Comments ?? "",
                    TechOperationWork = techOperationWork
                });
                nomer++;
            }
        }

        CalculateEtapTimes();// CalculateStageTimesUpdated();// CalculateEtapTimes();
        AddRowsToGrid();
    }



    #region Расчёт времени этапов

    private void CalculateEtapTimes() 
    {
        // Group items by ParallelIndex
        var parallelGroups = GroupByParallelIndex(TechOperationDataGridItems);

        // Process each group
        foreach (var group in parallelGroups)
        {
            var currentGroupType = group.Item1;
            var currentGroupList = group.Item2;
            var currentGroupProducts = group.Item3;

            switch (currentGroupType)
            {
                case GroupType.Single:
                    ProcessSingleGroup(currentGroupList);
                    break;
                case GroupType.Etap:
                    ProcessEtapGroup(currentGroupList);
                    break;
                case GroupType.ParallelIndex:
                    ProcessParallelGroup(currentGroupList);
                    break;
            }
            currentGroupProducts.ForEach(i => i.TimeEtap = "-1");
        }
    }

    /// <summary>
    /// Группирует строки по Индексу параллельности или этапу. 
    /// При отсутствии одного из этих признаков добавляет в стак лист с один элементом
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    private Stack<(GroupType, List<TechOperationDataGridItem>, List<TechOperationDataGridItem>)> 
        GroupByParallelIndex(List<TechOperationDataGridItem> items)
    {
        var parallelGroups = new Stack<(GroupType, List<TechOperationDataGridItem>, List<TechOperationDataGridItem>)>();

        var currentParallelIndex = 0;
        var currentEtap = "";

        foreach (var item in items)
        {
            if (item.Work)
            {
                int? parallelIndex = item.techWork.techOperationWork.GetParallelIndex();

                if (parallelIndex == null)
                {
                    // проверка на наличие этапа
                    if (item.Etap == "" || item.Etap == "0")
                    {
                        // обнуляем этап
                        currentEtap = "";

                        parallelGroups.Push((GroupType.Single, 
                            new List<TechOperationDataGridItem> { item },
                            new List<TechOperationDataGridItem>()));

                        continue;
                    }
                    else
                    {
                        // проверка на смену этапа
                        if (currentEtap != item.Etap)
                        {
                            currentEtap = item.Etap;

                            parallelGroups.Push((GroupType.Etap, 
                                new List<TechOperationDataGridItem> { item },
                                new List<TechOperationDataGridItem>()));

                            continue;
                        }
                        else
                        {
                            // добавляем в текущий список
                            parallelGroups.Peek().Item2.Add(item);
                        }
                    }
                }
                else
                {
                    // обнуляем этап ????
                    currentEtap = "";

                    if (currentParallelIndex != parallelIndex)
                    {

                        currentParallelIndex = parallelIndex.Value;

                        parallelGroups.Push((GroupType.ParallelIndex, 
                            new List<TechOperationDataGridItem> { item } ,
                            new List<TechOperationDataGridItem>()));
                    }
                    else
                    {
                        parallelGroups.Peek().Item2.Add(item);
                    }
                }
            }
            else
            {
                if (parallelGroups.Count == 0)
                {
                    parallelGroups.Push((GroupType.Single,
                                new List<TechOperationDataGridItem>(),
                                new List<TechOperationDataGridItem> { item }));
                }

                parallelGroups.Peek().Item3.Add(item);
            }

        }

        return parallelGroups;
    }
    enum GroupType
    {
        Single,
        Etap, 
        ParallelIndex,
    }

    private double ProcessEtapGroup(List<TechOperationDataGridItem> etapGroup)
    {
        var executionWorks = new List<ExecutionWork>();

        foreach (var item in etapGroup)
        {
            if (item.Work)
            {
                executionWorks.Add(item.techWork);
            }
        }

        // Process ExecutionWorks based on Posled
        foreach (var executionWork in executionWorks)
        {
            if (!string.IsNullOrEmpty(executionWork.Posled) && executionWork.Posled != "0")
            {
                var allSum = executionWorks
                    .Where(w => w.Posled == executionWork.Posled && w.Value != -1)
                    .Sum(s => s.Value);
                executionWork.TempTimeExecution = allSum;
            }
            else
            {
                executionWork.TempTimeExecution = executionWork.Value == -1 ? 0 : executionWork.Value;
            }
        }

        // Adjust listMach if necessary
        if (etapGroup.Count > 1)
        {
            var col = etapGroup[0].listMach.Count;
            for (int i = 0; i < col; i++)
            {
                bool tr = etapGroup.Any(item => item.listMach[i]);
                if (tr)
                {
                    foreach (var item in etapGroup)
                    {
                        item.listMach[i] = true;
                    }
                }
            }
        }

        // Calculate the maximum TempTimeExecution among ExecutionWorks
        double maxTime = CalculateMaxEtapTime(etapGroup); //executionWorks.Max(m => m.TempTimeExecution);


        etapGroup.ForEach(item => item.TimeEtap = "-1");
        etapGroup[0].TimeEtap = maxTime.ToString();

        return maxTime;
    }

    private void ProcessSingleGroup(List<TechOperationDataGridItem> etapGroup)
    {
        foreach (var item in etapGroup)
        {
            item.TimeEtap = item.techWork.Value.ToString();
        }
    }

    private void ProcessParallelGroup(List<TechOperationDataGridItem> etapGroup)
    {
        var times = new List<double>();

        var groups2 = etapGroup.GroupBy(g => g.techWork.techOperationWork.GetSequenceGroupIndex()).ToList();
        foreach (var group in groups2)
        {
            // if (sequenceGroupIndex == null) то в группы выделяются ТО
            if (group.Key == null)
            {
                var nullIndexGroups = group.ToList().GroupBy(g => g.techWork.techOperationWorkId).ToList();

                foreach (var parGroup in nullIndexGroups)
                {

                    times.Add(CalculateMaxEtapTime(parGroup.ToList()));
                }
            }
            else
            {
                var sequenceTimes = new List<double>();
                var sequenceGroups = group.GroupBy(g => g.techWork.techOperationWorkId).ToArray();

                foreach(var seqGroup in sequenceGroups)
                {
                    sequenceTimes.Add(CalculateMaxEtapTime(seqGroup.ToList()));
                }

                times.Add(sequenceTimes.Sum());
            }
        }

        // присвоить это время первому шагу. Остальным присвоить -1
        etapGroup.ForEach(i => i.TimeEtap = "-1");
        etapGroup[0].TimeEtap = times.Max().ToString();
    }

    private double CalculateMaxEtapTime(List<TechOperationDataGridItem> etapGroup)
    {
        // todo: проверка на наличие различных этапов внутри группы
        // Если есть разные этапы, то группируем по этапам и выполняем расчёт времени рекурсивно
        var etapGroups = etapGroup.GroupBy(g => g.Etap).ToList();
        if (etapGroups.Count > 1)
        {
            var times = new List<double>();
            foreach (var group in etapGroups)
            {
                times.Add(CalculateMaxEtapTime(group.ToList()));
            }

            return times.Max();
        }

        // Если этап у всех операций равен 0 или "",
        // то суммировать время всех операций в группе
        if (etapGroup.All(a => a.Etap == "" || a.Etap == "0" 
            //|| a.Etap == etapGroup[0].Etap
            ))
        {
            return etapGroup.Sum(s => s.techWork.Value);
        }

        var executionWorks = etapGroup.Where(w => w.Work).Select(s => s.techWork).ToList();

        var executionTimes = new List<double>();

        var sequenceTimes = new List<double>();

        foreach (var item in etapGroup)
        {
            if (item.Work)
            {
                var executionWork = item.techWork;

                // Если у шага нет группы параллельности, то считаем его выполнение последовательным
                if (executionWork.Etap == "" || executionWork.Etap == "0")
                {
                    executionTimes.Add(executionWork.Value);
                    continue;
                }

                if (!string.IsNullOrEmpty(executionWork.Posled) && executionWork.Posled != "0")
                {
                    var allSum = executionWorks
                        .Where(w => w.Posled == executionWork.Posled && w.Value != -1 
                            && w.Etap == executionWork.Etap && w.Etap != "0" && w.Etap != "")
                        .Sum(s => s.Value);

                    executionTimes.Add(allSum);
                }
                else
                {
                    executionTimes.Add(executionWork.Value);
                }
            }
        }

        executionTimes.Add(sequenceTimes.Sum());

        return executionTimes.Max();
    }

    #endregion



    /// <summary>
    /// Добавляет строки в DataGridView на основе подготовленного списка TechOperationDataGridItem.
    /// </summary>
    private void AddRowsToGrid()
    {
        foreach (var techOperationDataGridItem in TechOperationDataGridItems)
        {
            var str = new List<object>();
            var obj = context.Set<TechOperation>().Where(to => to.Id == techOperationDataGridItem.IdTO).FirstOrDefault();

            if (techOperationDataGridItem.techWork != null && techOperationDataGridItem.techWork.Repeat)
            {
                var repeatNumList = techOperationDataGridItem.techWork.ExecutionWorkRepeats
                    .Select(executionWorkRepeat => TechOperationDataGridItems.SingleOrDefault(s => s.techWork == executionWorkRepeat.ChildExecutionWork))
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
                    AddRowToGrid(str, Color.Yellow, Color.Yellow, Color.Pink);
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
                AddRowToGrid(str, Color.Empty, Color.Pink, Color.Pink);
            }
            else if(!obj.IsReleased)
            {
                AddRowToGrid(str, Color.Empty, Color.Empty, Color.Pink);
            }
            else if (techOperationDataGridItem.executionWorkItem != null && !techOperationDataGridItem.executionWorkItem.techTransition.IsReleased)
            {
                AddRowToGrid(str, Color.Empty, Color.Pink);
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
                //Получаем прошлую строку для сравнения, нужно ли объединение
                var prevStr = TechOperationDataGridItems.Where(t => t.Nomer == techOperationDataGridItem.Nomer - 1).FirstOrDefault();
                if(prevStr != null)
                {
                    //Проверка является ли прошлая строка не последовательной и проверка различия с прошлым значением(чтобы не объеденять строки ТП и инструментументов которые последовательны)
                    var isPreviousStrParallel = prevStr.Etap != "0" && techOperationDataGridItem.Etap != prevStr.Etap;
                    
                    if(prevStr.ItsTool || prevStr.ItsComponent)
                        techOperationDataGridItem.TimeEtap = prevStr.TimeEtap;//Если прошлая строка инструмент или компонент - копируем её значение
                    else
                        techOperationDataGridItem.TimeEtap = isPreviousStrParallel ? "-1" : "";

                    str.Add(techOperationDataGridItem.TimeEtap);
                }
                else
                {
                    str.Add("");
                }
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
            else if (TechOperationDataGridItems[e.RowIndex].ItsTool || TechOperationDataGridItems[e.RowIndex].ItsComponent)
            {
                if(e.ColumnIndex == dgvMain.Columns["CommentColumn"].Index)
                {
                    CellChangeReadOnly(dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex], false);
                }
                else if (e.ColumnIndex == dgvMain.Columns["PictureNameColumn"].Index)
                {
                    CellChangeReadOnly(dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex], true);
                }
                else if ((e.ColumnIndex == dgvMain.Columns["RemarkColumn"].Index
                    || e.ColumnIndex == dgvMain.Columns["ResponseColumn"].Index))
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

    /// <summary>
    /// Проверяет ячейку на совпадение с предыдущей ячейкой в столбце dgvMain
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <returns></returns>
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
        int maxOrder = 0;

        if (TechOperationWorksList.Count > 0)
        {
            maxOrder = TechOperationWorksList.Max(m => m.Order);
        }

        TechOperationWork techOperationWork = new TechOperationWork();
        techOperationWork.techOperation = TechOperat;
        techOperationWork.technologicalCard = TehCarta;
        techOperationWork.NewItem = true;
        techOperationWork.Order = ++maxOrder;

        TechOperationWorksList.Add(techOperationWork);
        context.TechOperationWorks.Add(techOperationWork);

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

    public void AddTechTransition(TechTransition tech, TechOperationWork techOperationWork, TechTransitionTypical techTransitionTypical = null, CoefficientForm coefficient = null)
    {
        TechOperationWork TOWork = TechOperationWorksList.Single(s => s == techOperationWork);

        int max = 0;
        if (TOWork.executionWorks.Count > 0)
        {
            max = TOWork.executionWorks.Max(w => w.Order);
        }

        ExecutionWork techOpeWork = new ExecutionWork
        {
            IdGuid = Guid.NewGuid(),
            techOperationWork = TOWork,
            NewItem = true,
            techTransition = tech,
            Order = ++max
        };

        TOWork.executionWorks.Add(techOpeWork);
        context.ExecutionWorks.Add(techOpeWork);

        if (tech.Name == "Повторить" || tech.Name == "Повторить п.")
        {
            techOpeWork.Repeat = true;
        }
        else
        {
            techOpeWork.Value = tech.TimeExecution;
            if (coefficient != null)
            {
                techOpeWork.Coefficient = coefficient.GetCoefficient;
                techOpeWork.Value = coefficient.GetValue;
            }
        }

        if (techTransitionTypical != null)
        {
            techOpeWork.Etap = techTransitionTypical.Etap;
            techOpeWork.Posled = techTransitionTypical.Posled;
            techOpeWork.Coefficient = techTransitionTypical.Coefficient;
            techOpeWork.Comments = techTransitionTypical.Comments ?? "";

            techOpeWork.Value = string.IsNullOrEmpty(techTransitionTypical.Coefficient) 
                ? tech.TimeExecution 
                : MathScript.EvaluateExpression(tech.TimeExecution + "*" + techTransitionTypical.Coefficient);
        }

    }

    public void DeleteTechOperation(TechOperationWork TechOperat)
    {
        var vb = TechOperationWorksList.SingleOrDefault(s => s == TechOperat);
        if (vb != null)
        {
            vb.Delete = true;
            TechOperationWorksList.Remove(vb);

            var deletedDTW = _tcViewState.DiagramToWorkList.Where(d => d.techOperationWork == TechOperat).FirstOrDefault();
            if (deletedDTW != null)
                _tcViewState.DiagramToWorkList.Remove(deletedDTW);

            context.Remove(vb);
        }
    }

    public void MarkToDeleteToolWork(TechOperationWork work, ToolWork tool)
    {
        var vb = work.ToolWorks.SingleOrDefault(s => s == tool);
        if (vb != null)
        {
            vb.IsDeleted = true;
            work.ToolWorks.Remove(vb);

            var relatedItems = context.DiagramShagToolsComponent.Where(s => s.toolWorkId == vb.Id).ToList();
            foreach (var item in relatedItems)
            {
                context.Remove(item);
            }

        }
    }

    public void MarkToDeleteComponentWork(TechOperationWork work, ComponentWork comp)
    {
        var vb = work.ComponentWorks.SingleOrDefault(s => s == comp);
        if (vb != null)
        {
            vb.IsDeleted = true;
            work.ComponentWorks.Remove(vb);

            var relatedItems = context.DiagramShagToolsComponent
                .Where(s => s.componentWorkId == vb.Id).ToList();
            
            foreach (var item in relatedItems)
            {
                context.Remove(item);
            }
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
            //TOWork.executionWorks.Remove(vb);
        }
    }

    private void button2_Click(object sender, EventArgs e)
    {
        _logger.LogUserAction("Редактирование хода работ.");

		// в случае Режима просмотра форма не открывается
		if (_tcViewState.IsViewMode)
            return;

        // Проверяем, была ли форма создана и не была ли закрыта
        if (_editForm == null || _editForm.IsDisposed)
        {
            _editForm = new AddEditTechOperationForm(this, _tcViewState);
        }

        // Выводим форму на передний план
        _editForm.Show();
        _editForm.BringToFront();

        HasChanges = true; // todo: Устанавливаем флаг изменений если реально были изменения
    }

    private void button1_Click_1(object sender, EventArgs e)
    {
        //context = new MyDbContext();
        //context.ChangeTracker.Clear();

        // TehCarta.Staff_TCs = Staff_TC;
        DbConnector dbCon = new DbConnector();

        List<TechOperationWork> AllDele = TechOperationWorksList.Where(w => w.Delete == true).ToList();
        foreach (TechOperationWork techOperationWork in AllDele)
        {
            // todo: проверить, что удаляются все связанные элементы
            //foreach (ToolWork delTool in techOperationWork.ToolWorks)
            //{
            //    dbCon.DeleteRelatedToolComponentDiagram(delTool.Id, true);
            //}

            //foreach (ComponentWork delComp in techOperationWork.ComponentWorks)
            //{
            //    dbCon.DeleteRelatedToolComponentDiagram(delComp.Id, false);
            //}

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

            var delTools = techOperationWork.ToolWorks.Where(w => w.IsDeleted == true).ToList();

            foreach (ToolWork delTool in delTools)
            {
                dbCon.DeleteRelatedToolComponentDiagram(delTool.Id, true);
                techOperationWork.ToolWorks.Remove(delTool);
            }



            foreach (ToolWork toolWork in techOperationWork.ToolWorks)
            {
                if (TehCarta.Tool_TCs.SingleOrDefault(s => s.Child == toolWork.tool) == null)
                {
                    Tool_TC tool = new Tool_TC();
                    tool.Child = toolWork.tool;
                    tool.Order = TehCarta.Tool_TCs.Count + 1;
                    tool.Quantity = toolWork.Quantity;
                    TehCarta.Tool_TCs.Add(tool);
                }
            }

            var delComponents = techOperationWork.ComponentWorks.Where(w => w.IsDeleted == true).ToList();

            foreach (var delComp in delComponents)
            {
                dbCon.DeleteRelatedToolComponentDiagram(delComp.Id, false);
                techOperationWork.ComponentWorks.Remove(delComp);
            }

            foreach (ComponentWork componentWork in techOperationWork.ComponentWorks)
            {
                if (TehCarta.Component_TCs.SingleOrDefault(s => s.Child == componentWork.component) == null)
                {
                    Component_TC Comp = new Component_TC();
                    Comp.Child = componentWork.component;
                    Comp.Order = TehCarta.Component_TCs.Count + 1;
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
        _editForm?.Close();
    }


    public void SelectCurrentRow(TechOperationWork work, ExecutionWork executionWork = null)
    {
        if (work == null)
            return;

        TechOperationDataGridItem? dgvItem = TechOperationDataGridItems.FirstOrDefault(t => (executionWork != null && t.techWork == executionWork) 
                                                                                            || (executionWork == null && t.TechOperationWork == work));

        if (dgvItem != null)
        {
            dgvMain.ClearSelection();
            dgvMain.FirstDisplayedScrollingRowIndex = TechOperationDataGridItems.IndexOf(dgvItem);
            dgvMain.Rows[TechOperationDataGridItems.IndexOf(dgvItem)].Selected = true;
        }

    }

	public void OnActivate()
	{
        if (_tcViewState.TechnologicalCard.IsDynamic 
            && _editForm != null && !_editForm.IsDisposed)
		    _editForm.OnActivate();
	}

	//public void HighlightExecutionWorkRow(ExecutionWork executionWork, bool scrollToRow = false)
	//{
	//    if (executionWork == null)
	//        return;

	//    foreach (DataGridViewRow row in dgvMain.Rows)
	//    {
	//        if (row.Cells[0].Value is ExecutionWork currentWork 
	//            && currentWork.Id == executionWork.Id 
	//            && currentWork.techTransitionId == executionWork.techTransitionId
	//            && currentWork.techOperationWorkId == executionWork.techOperationWorkId
	//            )
	//        {
	//            dgvMain.ClearSelection(); // Снимите выделение со всех строк
	//            row.Selected = true; // Выделите найденную строку
	//            if (scrollToRow)
	//            {
	//                dgvMain.FirstDisplayedScrollingRowIndex = row.Index; // Прокрутите до выделенной строки
	//            }
	//            break;
	//        }
	//    }
	//}

	/// <summary>
	/// Добавляет новую строку в dgvMain по указанному индексу.
	/// </summary>
	/// <param name="rowIndex">Индекс, по которому следует вставить строку.</param>
	/// <param name="values">Массив значений для новой строки.</param>
	public void AddRowByIndex(int rowIndex, object[] values)
	{
		if (rowIndex < 0 || rowIndex > dgvMain.Rows.Count)
			throw new ArgumentOutOfRangeException(nameof(rowIndex),
				"Индекс строки вне допустимого диапазона.");

		// Проверяем, совпадает ли количество значений с количеством столбцов
		if (values.Length != dgvMain.Columns.Count)
		{
			throw new ArgumentException("Количество переданных значений " +
										"не совпадает с количеством столбцов в dgvMain.");
		}

		dgvMain.Rows.Insert(rowIndex, values);

		// Можно добавить дополнительную логику форматирования
		// (например, назначить цвет фона для ячеек или выставить ReadOnly)
		// DataGridViewRow insertedRow = dgvMain.Rows[rowIndex];
		// insertedRow.Cells[...].Style.BackColor = Color.LightYellow;
	}

	/// <summary>
	/// Обновляет значение ячейки таблицы dgvMain по указанным индексам.
	/// </summary>
	/// <param name="rowIndex">Индекс строки.</param>
	/// <param name="columnIndex">Индекс столбца.</param>
	/// <param name="newValue">Новое значение ячейки.</param>
	public void UpdateCellValue(int rowIndex, int columnIndex, object newValue)
	{
		if (rowIndex < 0 || rowIndex >= dgvMain.Rows.Count)
			throw new ArgumentOutOfRangeException(nameof(rowIndex),
				"Индекс строки вне допустимого диапазона.");

		if (columnIndex < 0 || columnIndex >= dgvMain.Columns.Count)
			throw new ArgumentOutOfRangeException(nameof(columnIndex),
				"Индекс столбца вне допустимого диапазона.");

		dgvMain.Rows[rowIndex].Cells[columnIndex].Value = newValue;

		// Если нужно сразу же отобразить изменения в интерфейсе,
		// можно принудительно вызвать перерисовку:
		// dgvMain.Invalidate();
	}

}
