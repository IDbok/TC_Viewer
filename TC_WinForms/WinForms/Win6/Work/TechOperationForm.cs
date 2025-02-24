using Serilog;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
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

		dgvMain.MouseDown += DataGridView_MouseDown;

        SetContextMenuSetings();

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

	#region Инициализация контекстного меню и его элементов

	private ContextMenuStrip contextMenu;

	// Пункты для копирования
	private ToolStripMenuItem copyTextItem;
	private ToolStripMenuItem copyStaffItem;
	private ToolStripMenuItem copyTechOperationItem;
	private ToolStripMenuItem copyProtectionsItem;
	private ToolStripMenuItem copyRowItem;

	private ToolStripSeparator separatorItem;

	// Пункты для вставки
	private ToolStripMenuItem pasteTextItem;
	private ToolStripMenuItem pasteStaffItem;
	private ToolStripMenuItem pasteRowItem;
	private ToolStripMenuItem pasteTechOperationItem;
	private ToolStripMenuItem pasteProtectionsItem;


	/// <summary>
	/// Настраивает контекстное меню для DataGridView (копировать/вставить).
	/// </summary>
	private void SetContextMenuSetings()
	{
		contextMenu = new ContextMenuStrip();

		// 1) Пункты "Копировать"
		copyTextItem = new ToolStripMenuItem("Копировать текст");
		copyStaffItem = new ToolStripMenuItem("Копировать персонал");
		copyRowItem = new ToolStripMenuItem("Копировать строку");
		copyTechOperationItem = new ToolStripMenuItem("Копировать техоперацию");
		copyProtectionsItem = new ToolStripMenuItem("Копировать СИЗ");

		copyTextItem.Click += (s, e) => CopyClipboardValue();
		copyStaffItem.Click += (s, e) => CopyData();// CopyStaff_Click;// TODO: или использовать метод CopyData()
		copyRowItem.Click += (s, e) => CopyData();
		copyTechOperationItem.Click += (s, e) => CopyData();
		copyProtectionsItem.Click += (s, e) => CopyData();

		// 2) Пункты "Вставить"
		pasteTextItem = new ToolStripMenuItem("Вставить текст");
		pasteStaffItem = new ToolStripMenuItem("Вставить персонал");
		pasteRowItem = new ToolStripMenuItem("Вставить строку");
		pasteTechOperationItem = new ToolStripMenuItem("Вставить техоперацию");
		pasteProtectionsItem = new ToolStripMenuItem("Вставить СИЗ");

		pasteTextItem.Click += (s, e) => PasteClipboardValue();
		pasteStaffItem.Click += (s, e) => PasteCopiedData();// PasteStaff_Click;
		pasteRowItem.Click += (s, e) => PasteCopiedData();
		pasteTechOperationItem.Click += (s, e) => PasteCopiedData();
		pasteProtectionsItem.Click += (s, e) => PasteCopiedData();

		// 3) Разделитель
		separatorItem = new ToolStripSeparator();

		// Добавляем все пункты (или группируем в под-меню).
		contextMenu.Items.Add(copyStaffItem);
		contextMenu.Items.Add(copyTechOperationItem);
		contextMenu.Items.Add(copyProtectionsItem);
		contextMenu.Items.Add(copyRowItem);
		contextMenu.Items.Add(copyTextItem);

		contextMenu.Items.Add(separatorItem);

		contextMenu.Items.Add(pasteStaffItem);

		contextMenu.Items.Add(pasteRowItem);
		contextMenu.Items.Add(pasteTechOperationItem);
		contextMenu.Items.Add(pasteProtectionsItem);
		contextMenu.Items.Add(pasteTextItem);
	}

	private void UpdateContextMenuItems(CopyScopeEnum? selectedScope) // ?? добавить параметр поле readonly ??
	{
		// Скрываем или делаем Disabled все пункты
		copyTextItem.Visible = false;
		copyStaffItem.Visible = false;
		copyRowItem.Visible = false;
		copyTechOperationItem.Visible = false;
		copyProtectionsItem.Visible = false;

		separatorItem.Visible = true;

		pasteTextItem.Visible = false;
		pasteStaffItem.Visible = false;
		pasteRowItem.Visible = false;
		pasteTechOperationItem.Visible = false;
		pasteProtectionsItem.Visible = false;

		pasteTextItem.Enabled = false;
		pasteStaffItem.Enabled = false;
		pasteRowItem.Enabled = false;
		pasteTechOperationItem.Enabled = false;
		pasteProtectionsItem.Enabled = false;


		copyRowItem.Text = "Копировать строку";
		pasteRowItem.Text = "Вставить строку";

		// Если кликнули "мимо" (scope == null), контекстное меню может быть пустым
		if (selectedScope == null)
			return;

		// --- Показываем/прячем пункты КОПИРОВАНИЯ в зависимости от scope ---
		switch (selectedScope)
		{
			case CopyScopeEnum.Staff:
				// Пользователь кликнул ячейку "Исполнитель"
				copyTextItem.Visible = true;
				copyStaffItem.Visible = true;          // Можно копировать персонал
				//copyRowItem.Visible = true;  // И строку
				break;

			case CopyScopeEnum.Protections:
				copyTextItem.Visible = true;
				copyProtectionsItem.Visible = true;
				//copyRowItem.Visible = true;
				break;

			case CopyScopeEnum.ToolOrComponents:
				// Инструменты/компоненты
				copyTextItem.Visible = true;
				copyRowItem.Visible = true;
				break;

			case CopyScopeEnum.TechTransition:
				copyTextItem.Visible = true;
				copyRowItem.Visible = true;
				break;
			case CopyScopeEnum.Row:
				copyRowItem.Visible = true;
				break;
			case CopyScopeEnum.RowRange:
				copyRowItem.Visible = true;
				copyRowItem.Text = "Копировать строки";
				break;

			case CopyScopeEnum.TechOperation:
				// Клик по ячейке "Технологические операции"
				copyRowItem.Visible = true;
				copyTechOperationItem.Visible = true;
				break;

			case CopyScopeEnum.Text:
				// Просто текстовая ячейка (Примечание, Рис., Замечание)
				copyTextItem.Visible = true;
				//copyRowItem.Visible = true;
				break;
		}

		// --- Проверяем, что лежит в буфере TcCopyData, 
		//     и какие пункты "ВСТАВИТЬ" имеет смысл включить ---
		var copiedScope = TcCopyData.CopyScope;

		if (selectedScope != CopyScopeEnum.RowRange)
			switch (copiedScope)
			{
				case CopyScopeEnum.Staff:
					// В буфере лежит персонал
					// Если текущая ячейка позволяет вставлять персонал (например, scope == Staff),
					// то делаем видимым пункт "Вставить персонал"
					if (selectedScope == CopyScopeEnum.Staff)
					{
						pasteStaffItem.Visible = true;
						pasteStaffItem.Enabled = true;
					}

					// Или хотим, чтобы при scope == Row тоже был доступен "Вставить персонал"?
					// Можно добавить это условие при необходимости.
					break;

				case CopyScopeEnum.TechTransition:
				case CopyScopeEnum.Row:
				case CopyScopeEnum.RowRange:
				case CopyScopeEnum.ToolOrComponents:
					// В буфере одна или несколько строк
					// Если текущая ячейка позволяет вставлять строки — включаем пункт
					if (selectedScope == CopyScopeEnum.TechTransition
					 || selectedScope == CopyScopeEnum.Row
					 || selectedScope == CopyScopeEnum.RowRange
					 || selectedScope == CopyScopeEnum.ToolOrComponents) // если хотим «вставить» в инструменты/компоненты
					{
						pasteRowItem.Visible = true;
						pasteRowItem.Enabled = true;

						if (copiedScope == CopyScopeEnum.RowRange)
							pasteRowItem.Text = "Вставить строки";

						if (copiedScope == CopyScopeEnum.ToolOrComponents)
							pasteRowItem.Text = "Вставить инструменты/компоненты";
					}
					break;

				case CopyScopeEnum.TechOperation:
					// Полная технологическая операция
					// Вставить имеет смысл, если текущая ячейка — это Row / RowRange / Staff / TechOperation
					// (по вашим правилам)
					if (selectedScope == CopyScopeEnum.TechOperation
					 || selectedScope == CopyScopeEnum.Row)
					{
						pasteTechOperationItem.Visible = true;
						pasteTechOperationItem.Enabled = true;
					}
					break;

				case CopyScopeEnum.Protections:
					// В буфере СИЗ
					if (selectedScope == CopyScopeEnum.Protections)
					{
						pasteProtectionsItem.Visible = true;
						pasteProtectionsItem.Enabled = true;
					}
					break;

				case CopyScopeEnum.Text:
					if (selectedScope == CopyScopeEnum.Row)
					{
						separatorItem.Visible = false;
						break;
					}

					pasteTextItem.Visible = true;
					if(selectedScope == CopyScopeEnum.Text)
						pasteTextItem.Enabled = true;
					// Просто текст. Обычно "Вставить" доступно только для текстовых колонок
					// Если scope == Text, можно показывать пункт "Вставить текст" (или общий).
					break;

				default:
					// Ничего не копировали
					separatorItem.Visible = false;
					break;
			}

		// Пример: можно также проверять, включён ли режим просмотра:
		if (_tcViewState.IsViewMode)
		{
			// Если режим только для просмотра, то пункты "Вставить" отключаем
			pasteStaffItem.Visible = false;
			pasteRowItem.Visible = false;
			pasteTechOperationItem.Visible = false;
			pasteProtectionsItem.Visible = false;
		}
	}

	#endregion

	#region Обработка клика правой кнопкой (контекстное меню)

	private void DataGridView_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			// Если включён режим просмотра, то контекстное меню не показываем
			if (_tcViewState.IsViewMode) return;

			// Получаем координаты ячейки, на которую кликнули ПКМ
			var hitTestInfo = dgvMain.HitTest(e.X, e.Y);
			if (hitTestInfo.RowIndex >= 0 && hitTestInfo.ColumnIndex >= 0)
			{

				var clickedCell = dgvMain.Rows[hitTestInfo.RowIndex].Cells[hitTestInfo.ColumnIndex];

				// Проверяем, входит ли эта ячейка в текущее выделение
				bool alreadySelected = clickedCell.Selected;

				// Если не входит – снимаем всё и выделяем только её
				if (!alreadySelected)
				{
					dgvMain.ClearSelection();
					clickedCell.Selected = true;
					dgvMain.CurrentCell = clickedCell;
				}

				//dgvMain.ClearSelection();
				//dgvMain.Rows[hitTestInfo.RowIndex].Cells[hitTestInfo.ColumnIndex].Selected = true;
				//dgvMain.CurrentCell = dgvMain[hitTestInfo.ColumnIndex, hitTestInfo.RowIndex];

				// Вычисляем scope
				GetSelectedDataInfo(out _, out CopyScopeEnum? scope);

				// Настраиваем видимость/активность пунктов контекстного меню
				UpdateContextMenuItems(scope);

				// Показываем контекстное меню
				if(scope != null)
					contextMenu.Show(dgvMain, e.Location);
			}
		}
	}

	#endregion

	#region Обработка нажатия клавиш (Ctrl + C/V/Delete)

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
				PasteCopiedData();// вставляет объекты (строки, инструменты, компоненты)

			e.Handled = true;
		}
        // Новая обработка Ctrl + C
		else if (e.Control && e.KeyCode == Keys.C)
		{
            if (_tcViewState.IsViewMode)
				CopyClipboardValue();
			else
				CopyData();
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Delete)
        {
            DeleteCellValue(); // очистить текущее значение ячейки
			e.Handled = true;
        }
    }


	#endregion

	#region Копирование данных

	/// <summary>
	/// Основной метод копирования данных: определяем, что именно копируем (ячейку, строку, диапазон, инструмент, персонал).
	/// </summary>
	private void CopyData()
	{
		GetSelectedDataInfo(
            out List<int> selectedRowIndices, 
            out CopyScopeEnum? selectedScope);

		if (selectedRowIndices.Count == 0)
			return;

		try
		{
            // Копируем текст ячейки в буфер обмена
			CopyClipboardValue();

			// Если копируется НЕ просто текст, то сохраняем объекты в TcCopyData
			// Для примера: инструменты, компоненты, строки, вся TechOperation
			switch (selectedScope)
			{
				case CopyScopeEnum.TechOperation:
					// Если копируем всю ТО (как тип операции), например
					// Или просто текст – тогда уже скопировали в Clipboard. 
					// Для CopyScopeEnum.TechOperation — реализуем логику ниже.
					CopyTechOperationIfNeeded(selectedRowIndices, selectedScope.Value);
					break;
				case CopyScopeEnum.TechTransition:
				case CopyScopeEnum.Row:
				case CopyScopeEnum.RowRange:
				case CopyScopeEnum.ToolOrComponents:
				case CopyScopeEnum.Staff:
				case CopyScopeEnum.Protections:
				case CopyScopeEnum.Text:
					TcCopyData.SetCopyDate(
						selectedRowIndices
							.Select(i => TechOperationDataGridItems[i])
							.ToList(),
						_tcViewState.FormGuid,
						selectedScope.Value
					);
					break;
				default:
					// Нет подходящего варианта – выходим
					break;
			}
		}
		catch
		(Exception ex)
		{
			MessageBox.Show(ex.Message);
			return;
		}
	}

	/// <summary>
	/// Отдельного метода для копирования всей TechOperation целиком
	/// </summary>
	private void CopyTechOperationIfNeeded(List<int> selectedRowIndices, CopyScopeEnum copyScope)
	{
		if (copyScope != CopyScopeEnum.TechOperation) return;

		var selectedItems = selectedRowIndices
			.Select(i => TechOperationDataGridItems[i])
			.ToList();

		// Проверяем, что все выбранные строки относятся к одной и той же ТО
		if (selectedItems.Select(i => i.TechOperationWork.Id).Distinct().Count() > 1)
		{
			MessageBox.Show("Выбраны строки из разных ТО. Выделите строки из одной ТО.");
			return;
		}

		var selectedTow = selectedItems[0].TechOperationWork;
		var allItemsInThisTo = TechOperationDataGridItems
			.Where(i => i.TechOperationWork.Id == selectedTow.Id)
			.ToList();
		TcCopyData.SetCopyDate(allItemsInThisTo, _tcViewState.FormGuid, CopyScopeEnum.TechOperation);
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
				TcCopyData.SetCopyText(cellValue);
			}
			else
			{
				TcCopyData.Clear();
			}
		}
	}

	#endregion

    #region Вставка данных

    /// <summary>
    /// Основной метод вставки ранее скопированных объектов (Shift + Insert / Ctrl + V).
    /// </summary>
    private void PasteCopiedData()
    {
        GetSelectedDataInfo(out List<int> selectedRowIndices, out CopyScopeEnum? selectedScope);
        if (selectedRowIndices.Count == 0 || selectedScope == null) return;

        // Очищаем список вставленных данных
        TcCopyData.PastedEw.Clear();

        var selectedItems = selectedRowIndices.Select(i => TechOperationDataGridItems[i]).ToList();

        // Смотрим, что у нас лежит в TcCopyData
        switch (selectedScope)
        {
            case CopyScopeEnum.Staff:
                PasteStaffScope(selectedRowIndices);
                break;
            case CopyScopeEnum.Protections:
                PasteProtectionsScope(selectedRowIndices);
                break;
            case CopyScopeEnum.ToolOrComponents:
			case CopyScopeEnum.TechTransition:
			case CopyScopeEnum.Row:
            case CopyScopeEnum.RowRange:
                PasteToolsOrRows(selectedRowIndices, selectedScope.Value);
                break;
            case CopyScopeEnum.TechOperation:
                PasteWholeTechOperation(selectedRowIndices);
                break;
            case CopyScopeEnum.Text:
                PasteClipboardValue(); // просто вставка текста
                break;
            default:
                MessageBox.Show("Не удалось определить тип вставки.");
                break;
        }
    }

	/// <summary>
	/// Вставляет персонал (Staff) в текущую строку.
	/// </summary>
	private void PasteStaffScope(List<int> selectedRowIndices)
	{
		if (TcCopyData.CopyScope != CopyScopeEnum.Staff) return;
		if (selectedRowIndices.Count != 1)
			throw new Exception("Ошибка: для вставки персонала выделите ровно одну строку.");

		var selectedItem = TechOperationDataGridItems[selectedRowIndices[0]];
        var ew = selectedItem.WorkItem as ExecutionWork;
        if (ew == null)
        {
            MessageBox.Show("В данной строке невозможно вставить связь с Персоналом");
            return;
        }

        // Обновляем связь ExecutionWork -> Staffs
        var copiedEw = TcCopyData.FullItems[0].WorkItem as ExecutionWork;
		if (copiedEw == null)
		{
			MessageBox.Show("Ошибка при вставке данных. Некоректный скопированный объект");
			return;
		}

		UpdateStaffInRow(
			selectedRowIndices[0],
			ew!,
			copiedEw.Staffs
		);
	}

	/// <summary>
	/// Вставляет средства защиты (Protections) в текущую строку.
	/// </summary>
	private void PasteProtectionsScope(List<int> selectedRowIndices)
	{
		if (TcCopyData.CopyScope != CopyScopeEnum.Protections) return;
		if (selectedRowIndices.Count != 1)
			throw new Exception("Ошибка: для вставки СИЗ выделите ровно одну строку.");

		var selectedItem = TechOperationDataGridItems[selectedRowIndices[0]];
		var ew = selectedItem.WorkItem as ExecutionWork;
		if (ew == null)
		{
			MessageBox.Show("В данной строке невозможно вставить связь с СЗ");
			return;
		}
		var copiedEw = TcCopyData.FullItems[0].WorkItem as ExecutionWork;
		if (copiedEw == null)
		{
			MessageBox.Show("Ошибка при вставке данных. Некоректный скопированный объект");
			return;
		}
		UpdateProtectionsInRow(
			selectedRowIndices[0],
			ew,
			copiedEw.Protections
		);
	}

	/// <summary>
	/// Вставляет инструменты/компоненты либо новые строки (ExecutionWork).
	/// </summary>
	private void PasteToolsOrRows(List<int> selectedRowIndices, CopyScopeEnum selectedScope)
	{
		// Если копируем инструменты/компоненты
		if ((selectedScope == CopyScopeEnum.ToolOrComponents || selectedScope == CopyScopeEnum.TechTransition || selectedScope == CopyScopeEnum.Row || selectedScope == CopyScopeEnum.RowRange) &&
			TcCopyData.CopyScope == CopyScopeEnum.ToolOrComponents)
		{
			if (selectedRowIndices.Count != 1)
				throw new Exception("Ошибка: для вставки инструментов/компонентов выделите ровно одну строку (ячейку).");

			var tow = TechOperationDataGridItems[selectedRowIndices[0]].TechOperationWork;
			InsertToolAndComponent(tow, TcCopyData.FullItems, updateDataGrid: true);
			return;
		}

		// Если копируем одну или несколько строк (Row / RowRange)
		if ((selectedScope == CopyScopeEnum.TechTransition || selectedScope == CopyScopeEnum.Row || selectedScope == CopyScopeEnum.RowRange) &&
			(TcCopyData.CopyScope == CopyScopeEnum.TechTransition || 
			TcCopyData.CopyScope == CopyScopeEnum.Row ||
			 TcCopyData.CopyScope == CopyScopeEnum.RowRange ||
			 TcCopyData.CopyScope == CopyScopeEnum.TechOperation))
		{
			var rowIndex = selectedRowIndices[0] + 1; // вставляем после текущей строки
			var selectedTow = TechOperationDataGridItems[selectedRowIndices[0]].TechOperationWork;

			// Вставка одного шага (одной строки)
			if (TcCopyData.CopyScope == CopyScopeEnum.Row || TcCopyData.CopyScope == CopyScopeEnum.TechTransition)
			{
				PasteAsNewRow(rowIndex, selectedTow, TcCopyData.FullItems[0], updateDataGrid: true);
			}
			// Вставка нескольких шагов сразу
			else if (TcCopyData.CopyScope == CopyScopeEnum.RowRange)
			{
				if (TcCopyData.GetCopyFormGuId() != _tcViewState.FormGuid && selectedTow.techOperation.IsTypical)
				{
					MessageBox.Show("Вставка строк из другой ТК в типовую операцию не поддерживается.");
					return;
				}

				int iterator = 0;
				foreach (var copiedItem in TcCopyData.FullItems)
				{
					PasteAsNewRow(rowIndex + iterator, selectedTow, copiedItem, updateDataGrid: false);
					iterator++;
				}
				UpdateGrid();
			}
            else if (TcCopyData.CopyScope == CopyScopeEnum.TechOperation)
            {
                PasteWholeTechOperation(selectedRowIndices);
			}
		}
	}

	/// <summary>
	/// Вставляет сразу все данные из скопированной Технологической операции в другую (CopyScopeEnum.TechOperation).
	/// </summary>
	private void PasteWholeTechOperation(List<int> selectedRowIndices)
	{
		// У нас уже CopyScopeEnum.TechOperation
		// проверяем, совпадает ли с TcCopyData.CopyScope
		if (TcCopyData.CopyScope != CopyScopeEnum.TechOperation) return;
		if (selectedRowIndices.Count != 1)
			throw new Exception("Ошибка: выделите ровно одну строку для вставки ТО.");

		var rowIndex = selectedRowIndices[0] + 1;
		var selectedTow = TechOperationDataGridItems[selectedRowIndices[0]].TechOperationWork;

		// Сначала вставим все ExecutionWork (обычные переходы)
		var copiedEwItems = TcCopyData.FullItems
			.Where(i => i.WorkItemType == WorkItemType.ExecutionWork)
			.ToList();

		int iterator = 0;
		foreach (var copiedItem in copiedEwItems)
		{
			PasteAsNewRow(rowIndex + iterator, selectedTow, copiedItem, updateDataGrid: false);
			iterator++;
		}

		// Вставим инструменты и компоненты
		var copiedToolCompItems = TcCopyData.FullItems
			.Where(i => i.WorkItemType == WorkItemType.ToolWork
					 || i.WorkItemType == WorkItemType.ComponentWork)
			.ToList();

		InsertToolAndComponent(selectedTow, copiedToolCompItems, updateDataGrid: false);

		UpdateGrid();
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
				var args = new DataGridViewCellEventArgs(
                    dgvMain.CurrentCell.ColumnIndex, 
                    dgvMain.CurrentCell.RowIndex
                );
				DgvMain_CellEndEdit(dgvMain, args);
			}
		}
	}

	#endregion

	#region Удаление значения ячейки

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

	#endregion

	#region Методы-helpers для определения области копирования / вставки

	/// <summary>
	/// Возвращает список выделенных строк (индексы) и предполагаемый тип копирования (scope).
	/// </summary>
	private void GetSelectedDataInfo(out List<int> selectedRowIndices, out CopyScopeEnum? copyScope)
	{
		// Соберём все уникальные индексы строк, где есть выделенные ячейки.
		var selectedCells = dgvMain.SelectedCells
			.Cast<DataGridViewCell>()
			.Distinct()
			.ToList();

		var selectedRows = dgvMain.SelectedRows
			.Cast<DataGridViewRow>()
			.Distinct()
			.ToList();

		selectedRowIndices = selectedCells
			.Select(c => c.RowIndex)
			.Distinct()
			.OrderBy(idx => idx)
			.ToList();

		if (!selectedRowIndices.Any())
		{
			copyScope = null;
			return;
		}

		if (selectedRows.Count == 1)
		{
			copyScope = CopyScopeEnum.Row;
			return;
		}
		else if (selectedRows.Count > 1)
		{
			copyScope = CopyScopeEnum.RowRange;
			return;
		}

		if (selectedCells.Count == 1)
		{
			var cell = selectedCells[0];
			copyScope = GetCopyScopeByCell(cell);
			return;
		}
		// в случае выделения нескольких ячеек
		copyScope = null;
	}

	/// <summary>
	/// Определяем тип копирования по названию столбца и типу строки (WorkItemType).
	/// </summary>
	private CopyScopeEnum? GetCopyScopeByCell(DataGridViewCell cell)
	{
		string columnName = dgvMain.Columns[cell.ColumnIndex].HeaderText;
		int rowIndex = cell.RowIndex;

		var item = TechOperationDataGridItems[rowIndex];

		var isToolOrComponent = item.ItsTool || item.ItsComponent;
		// Если это инструмент / компонент:
		//if (item.ItsTool || item.ItsComponent)
		//	return CopyScopeEnum.ToolOrComponents;

		// Иначе смотрим по названию столбца
		return columnName switch
		{
			"Исполнитель" => isToolOrComponent ? CopyScopeEnum.ToolOrComponents : CopyScopeEnum.Staff,
			"№ СЗ" => isToolOrComponent ? CopyScopeEnum.ToolOrComponents : CopyScopeEnum.Protections,
			"Технологические операции" => CopyScopeEnum.TechOperation,
			"Технологические переходы" => isToolOrComponent ? CopyScopeEnum.ToolOrComponents : CopyScopeEnum.TechTransition,
			"Примечание" or "Рис."
			  or "Замечание" or "Ответ" or "№"
											=> CopyScopeEnum.Text,
			_ => null
		};
	}

	/// <summary>
	/// Возвращает индекс столбца по типу копирования (Staff, Protections, и т.д.).
	/// </summary>
	private int? GetColumnIndex(CopyScopeEnum copyScope)
	{
		return copyScope switch
		{
			CopyScopeEnum.Staff => dgvMain.Columns["Staff"].Index,
			CopyScopeEnum.Protections => dgvMain.Columns["Protection"].Index,
			CopyScopeEnum.Text => dgvMain.Columns["Text"].Index,           // TODO: проверить, есть ли реально "Text" в колонках
			CopyScopeEnum.Machines => dgvMain.Columns["Machine"].Index,
			CopyScopeEnum.TechOperation => dgvMain.Columns["TechOperation"].Index, 
			_ => null
		};
	}

	#endregion

	#region Методы вставки (PasteAsNewRow, UpdateStaff, UpdateProtections, InsertToolAndComponent)

	/// <summary>
	/// Вставляет данные (ExecutionWork) как новую строку в указанную TechOperationWork.
	/// </summary>
	private void PasteAsNewRow(int rowIndex, TechOperationWork selectedToTow, TechOperationDataGridItem copiedItem, bool updateDataGrid = false)
	{
		// Проверка на тип WorkItem
		if (copiedItem.WorkItemType != WorkItemType.ExecutionWork
			|| copiedItem.WorkItem is not ExecutionWork copiedEw)
		{
			throw new Exception("Ошибка при вставке: не ExecutionWork или пустое значение.");
		}

		// Для типовых ТО возможно вставлять только повторы
		if (selectedToTow.techOperation.IsTypical && !copiedEw.Repeat)
		{
			MessageBox.Show("В типовую операцию можно вставлять только 'Повторить'.");
			return;
		}

		// Если выбранная ТО пустая (без ТП), заменяем строку с ТО на новую с ТП  
		if (selectedToTow.executionWorks.Count == 0)
		{
			var towItemIndex = TechOperationDataGridItems.FindIndex(i => i.TechOperationWork == selectedToTow);
			if (towItemIndex == -1)
			{
				throw new Exception("Ошибка при вставке: некоректная ссылка на выбранное ТО.");
			}
			rowIndex = towItemIndex;
		}

		// Создаём дубликат, если копируем из другой ТК
		if (TcCopyData.GetCopyFormGuId() != _tcViewState.FormGuid)
			copiedEw = CloneExecutionWorkForAnotherTC(copiedEw);

		var techTransition = copiedEw.techTransition;
		if (techTransition == null) return;

		// Вставляем новую строку
		var newEw = InsertNewRow(
			copiedEw.techTransition ?? throw new Exception("Ошибка при вставке: нет TechTransition в скопированном объектке."),
			selectedToTow,
			rowIndex,
			copiedEw.Repeat ? copiedEw.ExecutionWorkRepeats : null,
			coefficient: copiedEw.Coefficient,
			updateDataGrid: updateDataGrid,
			comment: copiedEw.Comments,
			pictureName: copiedEw.PictureName
		);

		newEw.TempGuid = copiedEw.IdGuid;
		TcCopyData.PastedEw.Add(newEw);

		try
		{
			UpdateProtectionsInRow(rowIndex, newEw, copiedEw.Protections, updateDataGrid: updateDataGrid);
			UpdateStaffInRow(rowIndex, newEw, copiedEw.Staffs, updateDataGrid: updateDataGrid);
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Ошибка при вставке: {ex.Message}");
		}
		// todo: что с механизмами?
		// todo: что с группой паралельности и последовательности?
	}

	/// <summary>
	/// Создаёт дубликат ExecutionWork, если скопировано из другой ТК.
	/// </summary>
	private ExecutionWork CloneExecutionWorkForAnotherTC(ExecutionWork copiedEw)
	{
		var newEw = new ExecutionWork
		{
			// пока отключаю поля, которые не используются
			IdGuid = copiedEw.IdGuid,
			techTransitionId = copiedEw.techTransitionId,
			//techTransition = copiedEw.techTransition,

			//techOperationWorkId = copiedEw.techOperationWorkId, 

			//Etap = copiedEw.Etap,
			//Posled = copiedEw.Posled,

			TempGuid = copiedEw.TempGuid,

			Order = copiedEw.Order,
			RowOrder = copiedEw.RowOrder,

			Coefficient = copiedEw.Coefficient,

			Repeat = copiedEw.Repeat,
			ExecutionWorkRepeats = copiedEw.ExecutionWorkRepeats,

			Protections = copiedEw.Protections,
			Staffs = copiedEw.Staffs,

            Comments = copiedEw.Comments,
            PictureName = copiedEw.PictureName,
            //Vopros = copiedEw.Vopros,
            //Otvet = copiedEw.Otvet,
        };

        // При необходимости подтянуть нужные объекты из текущего контекста
        newEw.techTransition = context.TechTransitions
            .FirstOrDefault(t => t.Id == copiedEw.techTransitionId);

		if (copiedEw.Repeat)
		{
			var repeatEws = new List<ExecutionWorkRepeat>();
			bool isEmptyRepeat = false;
			// ищим в уже существующих ТП ТП с аналогичным guidId
			// заменим скопированные объекты на существующие в данной контексте
			foreach (var ewRepeat in copiedEw.ExecutionWorkRepeats)
			{
				var newEwRepeats = TcCopyData.PastedEw.FirstOrDefault(ew => ew.TempGuid == ewRepeat.ChildExecutionWork.IdGuid);
				if (newEwRepeats == null)
				{
					MessageBox.Show($"ТП Повторить (строка {copiedEw.RowOrder}) будет вставленны пустым, т.к. не все повторяемые переходы были выделенны при копировании.");
					// выйти из цикла
					isEmptyRepeat = true;

					break;
				}

				repeatEws.Add(new ExecutionWorkRepeat
				{
					ParentExecutionWork = new ExecutionWork(), // вренное значение. Должно быть заменено на новый созданный в InsertNewRow ТП Повтора
					ChildExecutionWork = newEwRepeats,
					NewCoefficient = ewRepeat.NewCoefficient,
					NewEtap = ewRepeat.NewEtap,
					NewPosled = ewRepeat.NewPosled
				});
			}

			if (isEmptyRepeat) { repeatEws = new List<ExecutionWorkRepeat>(); }

            newEw.ExecutionWorkRepeats = repeatEws;
		}

		return newEw;
	}

	/// <summary>
	/// Обновляет (вставляет) персонал в указанный ExecutionWork.
	/// </summary>
	public void UpdateStaffInRow(int rowIndex, ExecutionWork selectedEw, List<Staff_TC> copiedStaff, bool updateDataGrid = true)
	{
		if (selectedEw == null) throw new ArgumentNullException(nameof(selectedEw));
		if (copiedStaff == null) throw new ArgumentNullException(nameof(copiedStaff));

		var columnIndex = GetColumnIndex(CopyScopeEnum.Staff)
			?? throw new Exception("Не найден столбец под персонал (Staff).");

		// если ТК копирования не совпадает с текущей ТК, то находим совпадабщие объекты и создаём новые
		// Заменяем на сущствующий в случае совпадения id персонала и его символа.
		// В других случаях заменяем на новый объект с новым символом.
		// (Доп) Логика выбора символа: при отсутствии символа у сущ - х объектов добавляем новый с тем символом который был при копировании.
		if (TcCopyData.GetCopyFormGuId() != _tcViewState.FormGuid)
			copiedStaff = MergeStaffFromAnotherTc(copiedStaff);

		// Удаляем из списка персонала тех, кого нет в скопированном списке и добавляем новых
		var staffSet = new HashSet<Staff_TC>(copiedStaff);
		selectedEw.Staffs.RemoveAll(staff => !staffSet.Contains(staff));
		foreach (var staff in copiedStaff)
		{
			if (!selectedEw.Staffs.Contains(staff))
				selectedEw.Staffs.Add(staff);
		}

		if (updateDataGrid)
		{
			// Формируем строку с символами
			var newStaffSymbols = string.Join(",", selectedEw.Staffs.OrderBy(s => s.Symbol).Select(s => s.Symbol));
			// Обновляем ячейку в таблице
			UpdateCellValue(rowIndex, (int)columnIndex, newStaffSymbols);
		}

		List<Staff_TC> MergeStaffFromAnotherTc(List<Staff_TC> copiedStaff)
		{
			copiedStaff = copiedStaff.ToList();

			var copiedStaffData = copiedStaff.Select(s => new { ChildId = s.ChildId, Symbol = s.Symbol, Staff_TC = s }).ToList();
			var existingStaff_tcs = TehCarta.Staff_TCs;
			foreach (var copiedStc in copiedStaffData)
			{
				var existingStaff_tc = existingStaff_tcs
					.FirstOrDefault(st => st.ChildId == copiedStc.ChildId && st.Symbol == copiedStc.Symbol);

				var oldCopiedStc = copiedStaff.Find(st => st == copiedStc.Staff_TC);
				if (oldCopiedStc == null)
				{ throw new Exception("Ошибка при копирования персонала. Ошибка 1246"); }

				copiedStaff.Remove(oldCopiedStc);

				if (existingStaff_tc == null)
				{
					// поиск существующего в ТК персонала по id
					var addingStaff = existingStaff_tcs.Select(st => st.Child).FirstOrDefault(s => s.Id == copiedStc.ChildId);
					if (addingStaff == null)
					{
						addingStaff = context.Staffs.FirstOrDefault(s => s.Id == copiedStc.ChildId);
						//addingStaff = copiedStc.Staff_TC.Child;
					}

					// проверка на наличие символа
					var addingSymbol = copiedStc.Symbol;
					if (existingStaff_tcs.Select(st => st.Symbol).Contains(addingSymbol))
					{
						addingSymbol = GetNewSymbol(existingStaff_tcs);
					}

					// если персонала нет в ТК, то добавляем его с новым символом
					var newStaff = new Staff_TC
					{
						ParentId = TehCarta.Id,
						ChildId = copiedStc.ChildId,
						Child = addingStaff,
						Symbol = addingSymbol,

						Order = existingStaff_tcs.Count + 1
					};

					copiedStaff.Add(newStaff);

					// добавить персонал в ТК
					TehCarta.Staff_TCs.Add(newStaff);
				}
				else
				{
					// если персонал уже есть в ТК, то заменяем на него объект в списке скопированного персонала
					copiedStaff.Add(existingStaff_tc);
				}
			}

			return copiedStaff;


			static string GetNewSymbol(List<Staff_TC> existingStaff_tcs)
			{
				var existingSymbols = existingStaff_tcs.Select(st => st.Symbol).ToList();
				//все символы которые начинаются на "Нов." и содержат в себе число
				var newSymbols = existingSymbols.Where(s => s.StartsWith("Нов.") && int.TryParse(s.Replace("Нов.", ""), out _)).ToList();
				// если нет символов начинающихся на "Нов." то добавляем "Нов.1"
				if (newSymbols.Count == 0)
					return "Нов.1";
				// если есть символы начинающиеся на "Нов." то добавляем новый символ с максимальным числом
				var maxNumber = newSymbols.Select(s => int.Parse(s.Replace("Нов.", ""))).Max();
				return $"Нов.{maxNumber + 1}";
			}
		}
	}

	/// <summary>
	/// Обновляет (вставляет) СИЗ (Protections) в указанный ExecutionWork
	/// </summary>
	public void UpdateProtectionsInRow(int rowIndices, ExecutionWork selectedEw, List<Protection_TC> copiedProtections, bool updateDataGrid = true)
	{
		if (selectedEw == null) throw new ArgumentNullException(nameof(selectedEw));
		if (copiedProtections == null) throw new ArgumentNullException(nameof(copiedProtections));

		var columnIndex = GetColumnIndex(CopyScopeEnum.Protections)
		    ?? throw new Exception("Не найден столбец для СИЗ.");


		if (TcCopyData.GetCopyFormGuId() != _tcViewState.FormGuid)
			copiedProtections = MergeProtectionsFromAnotherTc(copiedProtections);

		var protSet = new HashSet<Protection_TC>(copiedProtections);
		selectedEw.Protections.RemoveAll(obj => !protSet.Contains(obj));
		foreach (var protection in copiedProtections)
		{
			if (!selectedEw.Protections.Contains(protection))
				selectedEw.Protections.Add(protection);
		}

		if (updateDataGrid)
		{
			// Формируем строку с номерами СЗ
			var objectOrderList = selectedEw.Protections.Select(s => s.Order).ToList();
			var newCellValue = string.Join(",", ConvertListToRangeString(objectOrderList));
			UpdateCellValue(rowIndices, (int)columnIndex, newCellValue);
		}

		List<Protection_TC> MergeProtectionsFromAnotherTc(List<Protection_TC> copiedProtections)
		{
			copiedProtections = copiedProtections.ToList();
			var existingObject_tcs = TehCarta.Protection_TCs;
			var newCopiedProtections = new List<Protection_TC>();
			foreach (var copiedOtc in copiedProtections)
			{
				var existingObject_tc = existingObject_tcs
					.FirstOrDefault(st => st.ChildId == copiedOtc.ChildId);

				if (existingObject_tc == null)
				{
					var addingObject = context.Protections.FirstOrDefault(s => s.Id == copiedOtc.ChildId); // todo: хочется уйти от контекста, чтобы вынести в сервис
																										   // без контекста (при добавлении напрямую из copiedOtc.Child) выдаёт ошибку при сохранении
					if (addingObject == null)
					{ throw new Exception("Ошибка при копировании СЗ. Ошибка 1246"); }

					// если персонала нет в ТК, то добавляем его с новым символом
					var newObject_tc = new Protection_TC
					{
						ParentId = TehCarta.Id,
						ChildId = copiedOtc.ChildId,
						Child = addingObject,
						Quantity = copiedOtc.Quantity,
						Note = copiedOtc.Note,

						Order = existingObject_tcs.Count + 1
					};

					newCopiedProtections.Add(newObject_tc);

					// добавить персонал в ТК
					TehCarta.Protection_TCs.Add(newObject_tc);
				}
				else
				{
					// если персонал уже есть в ТК, то заменяем на него объект в списке скопированного персонала
					newCopiedProtections.Add(existingObject_tc);
				}
			}

			copiedProtections = newCopiedProtections;
			return copiedProtections;
		}
	}

	/// <summary>
	/// Вставляет инструменты и компоненты из списка copiedRows в выбранную TechOperationWork.
	/// </summary>
	private void InsertToolAndComponent(TechOperationWork selectedTow, List<TechOperationDataGridItem> copiedRows, bool updateDataGrid = false)
	{
		var toolRows = copiedRows.Where(r => r.WorkItemType == WorkItemType.ToolWork).ToList();
		var componentRows = copiedRows.Where(r => r.WorkItemType == WorkItemType.ComponentWork).ToList();

		bool isDGChanged = false;

		// === Инструменты ===
		if (toolRows.Count > 0)
		{
			var toolWorks = toolRows.Select(r => r.WorkItem).Cast<ToolWork>().ToList();
			foreach (var tw in toolWorks)
			{
				// Добавляем инструмент, если его ещё нет в selectedTow
				bool alreadyExists = selectedTow.ToolWorks.Any(o => o.toolId == tw.toolId);
				if (!alreadyExists)
				{
                    Tool addingObject = ResolveToolInCurrentTc(tw);
					selectedTow.ToolWorks.Add(new ToolWork
					{
						toolId = tw.toolId,
						tool = addingObject,
						Quantity = tw.Quantity,
						Comments = tw.Comments
					});
					isDGChanged = true;
				}
			}
		}

		// === Компоненты ===
		if (componentRows.Count > 0)
		{
			var copiedComponentWorks = componentRows.Select(r => r.WorkItem).Cast<ComponentWork>().ToList();
			var copiedComponents = copiedComponentWorks.Select(tw => tw.component).ToList();
			// добавляем скопированные инструменты, если их нет в ТО
			foreach (var cw in copiedComponentWorks)
			{
				bool alreadyExists = selectedTow.ToolWorks.Any(o => o.toolId == cw.componentId);
				if (!alreadyExists)
				{
					Component addingObject = ResolveComponentInCurrentTc(cw);
					selectedTow.ComponentWorks.Add(new ComponentWork
					{
						componentId = cw.componentId,
						component = addingObject,
						Quantity = cw.Quantity,
						Comments = cw.Comments
					});
					isDGChanged = true;
				}
			}
		}

		if (isDGChanged && updateDataGrid)
			UpdateGrid();

		Tool ResolveToolInCurrentTc(ToolWork tw)
		{
			if (TcCopyData.GetCopyFormGuId() == _tcViewState.FormGuid)
				return tw.tool;
            else
            {
				var existionObject = TehCarta.Tools.FirstOrDefault(o => o.Id == tw.toolId);
				if (existionObject == null)
				{
					existionObject = context.Tools.FirstOrDefault(o => o.Id == tw.toolId);
					if (existionObject == null)
						throw new Exception("Ошибка при копировании инструментов. Ошибка 1246");

					TehCarta.Tool_TCs.Add(new Tool_TC
					{
						ParentId = TehCarta.Id,
						ChildId = tw.toolId,
						Child = existionObject,
						Quantity = tw.Quantity,
						Note = tw.Comments,

						Order = TehCarta.Tool_TCs.Count + 1
					});
				}
                return existionObject;
			}
		}

		Component ResolveComponentInCurrentTc(ComponentWork componentWork)
		{
			if (TcCopyData.GetCopyFormGuId() == _tcViewState.FormGuid)
				return componentWork.component;
            else
            {
				var existionObject = TehCarta.Components.FirstOrDefault(o => o.Id == componentWork.componentId);
				if (existionObject == null)
				{
					existionObject = context.Components.FirstOrDefault(o => o.Id == componentWork.componentId);
					if (existionObject == null)
						throw new Exception("Ошибка при копировании компонентов. Ошибка 1246");

					TehCarta.Component_TCs.Add(new Component_TC
					{
						ParentId = TehCarta.Id,
						ChildId = componentWork.componentId,
						Child = existionObject,
						Quantity = componentWork.Quantity,
						Note = componentWork.Comments,

						Order = TehCarta.Component_TCs.Count + 1
					});
				}

				return existionObject;
			}
			
		}
	}

	public ExecutionWork InsertNewRow(TechTransition techTransition,  TechOperationWork techOperationWork,
        int? insertIndex = null, List<ExecutionWorkRepeat>? executionWorksRepeats = null,
        string? coefficient = null, bool updateDataGrid = true,
        string? comment = null, string? pictureName = null)

	{
		var newEw = AddNewExecutionWork(techTransition, techOperationWork, insertIndex: insertIndex, coefficientValue: coefficient, comment: comment, pictureName: pictureName);

        if (newEw == null) throw new Exception("Ошибка при добавлении нового перехода.");

		// если новый переход - повтор, то добавляем к нему повторяемые переходы
		if (newEw.Repeat && executionWorksRepeats != null)
		{
            foreach (var ewRepeat in executionWorksRepeats)
            {
                var newEwRepeat = new ExecutionWorkRepeat
                {
                    ParentExecutionWork = newEw,
                    ChildExecutionWork = ewRepeat.ChildExecutionWork,
                    NewCoefficient = ewRepeat.NewCoefficient,
                    NewEtap = ewRepeat.NewEtap,
                    NewPosled = ewRepeat.NewPosled
                };
				newEw.ExecutionWorkRepeats.Add(ewRepeat);
			}
		}

        if (updateDataGrid)
		    UpdateGrid(); // todo: заменить на вставку по индексу и пересчёт номеров строк. Сложность с повторами
        // при замене номеров строк перебором Items,
        // для повтором можно аналогично пересчитать название исходя из новых индексов входящий в него EW

        return newEw;
	}


	#endregion

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

		dgvMain.Columns.Add("ExecutionWorkItem", "");
        dgvMain.Columns.Add("Order", "");
        dgvMain.Columns.Add("TechOperationName", "");
        dgvMain.Columns.Add("Staff", "Исполнитель");
        dgvMain.Columns.Add("TechTransitionName", "Технологические переходы");
        dgvMain.Columns.Add("TimeValue", "");
        dgvMain.Columns.Add("EtapValue", "");

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
			PopulateTechOperationDataGridItems(techOperationWork, ref nomer);
		}

		CalculateEtapTimes();
        AddRowsToGrid();
    }

	private void PopulateTechOperationDataGridItems(TechOperationWork techOperationWork, ref int nomer) // todo: вставка по индексу?
	{
		var executionWorks = techOperationWork.executionWorks.Where(w => !w.Delete).OrderBy(o => o.Order).ToList();

		if (executionWorks.Count == 0)
		{
            TechOperationDataGridItems.Add(new TechOperationDataGridItem(techOperationWork));
		}

		foreach (var executionWork in executionWorks)
		{
			var item = CreateExecutionWorkItem(techOperationWork, executionWork, nomer);
			TechOperationDataGridItems.Add(item);
			nomer++;
		}

		foreach (var toolWork in techOperationWork.ToolWorks.Where(t => t.IsDeleted == false).ToList())
		{
			TechOperationDataGridItems.Add(new TechOperationDataGridItem(techOperationWork, toolWork, nomer));
			nomer++;
		}

		foreach (var componentWork in techOperationWork.ComponentWorks.Where(t => t.IsDeleted == false).ToList())
		{
			TechOperationDataGridItems.Add(new TechOperationDataGridItem(techOperationWork,componentWork, nomer));
			nomer++;
		}
	}

	private TechOperationDataGridItem CreateExecutionWorkItem(
	TechOperationWork techOperationWork,
	ExecutionWork executionWork,
	int nomer)
	{
		// Генерация Guid, если он не задан
		if (executionWork.IdGuid == Guid.Empty)
		{
			executionWork.IdGuid = Guid.NewGuid();
		}

		// Установка порядкового номера строки
		executionWork.RowOrder = nomer;

		// Строка со списком персонала
		var staffStr = string.Join(",", executionWork.Staffs.Select(s => s.Symbol));

		// Строка с "диапазоном" номеров защит
		var protectList = executionWork.Protections.Select(p => p.Order).ToList();
		var protectStr = ConvertListToRangeString(protectList);

		// Список bool, отражающий, какие машины входят (пример в исходном коде)
		var mach = TehCarta.Machine_TCs
			.Select(tc => executionWork.Machines.Contains(tc))
			.ToList();

		// Создаём объект TechOperationDataGridItem
        
        var item = new TechOperationDataGridItem(techOperationWork, executionWork, nomer, staffStr, mach, protectStr);

		// Дополнительная проверка значения
		if (item.TechTransitionValue == "-1")
		{
			item.TechTransitionValue = "Ошибка";
		}

		return item;
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
        foreach (var item in TechOperationDataGridItems) //techOperationDataGridItem
		{
			AddRowToDataGrid(item);
		}
	}

	private void AddRowToDataGrid(TechOperationDataGridItem item, int? rowIndex = null)
	{
		// Проверка, что индекс строки не выходит за границы
		if (rowIndex != null && rowIndex < 0)
		{
			rowIndex = 0;
		}
		else if (rowIndex != null && rowIndex > dgvMain.Rows.Count)
		{
			rowIndex = dgvMain.Rows.Count;
		}

		// Находим TechOperation из контекста, чтобы проверить IsReleased
		var obj = context.Set<TechOperation>()
						 .FirstOrDefault(to => to.Id == item.IdTO);

		// Определяем, какие цвета будем использовать в зависимости от условий
		if (item.techWork != null && item.techWork.Repeat)
		{

			if (!obj.IsReleased)
			{
				// Повтор, но ТО не выпущена
				AddRowToGrid(item, Color.Yellow, Color.Yellow, Color.Pink, insertIndex: rowIndex);
			}
			else
				// Повтор с выпушенной ТО
				AddRowToGrid(item, Color.Yellow, Color.Yellow, insertIndex: rowIndex);
		}
		else if (item.ItsTool || item.ItsComponent)
		{
			// Инструмент или компонент
			AddRowToGrid(item,
						 item.ItsComponent ? Color.Salmon : Color.Aquamarine,
						 item.ItsComponent ? Color.Salmon : Color.Aquamarine, insertIndex: rowIndex);
		}
		else if (!obj.IsReleased && item.executionWorkItem != null && !item.executionWorkItem.techTransition.IsReleased)
		{
			// Тех.операция не выпущена (но переход выпущен или отсутствует)
			AddRowToGrid(item, Color.Empty, Color.Pink, Color.Pink, insertIndex: rowIndex);
		}
		else if (!obj.IsReleased)
		{
			// Тех.операция не выпущена
			AddRowToGrid(item, Color.Empty, Color.Empty, Color.Pink, insertIndex: rowIndex);
		}
		else if (item.executionWorkItem != null && !item.executionWorkItem.techTransition.IsReleased)
		{
			// Тех.операция выпущена, но переход не выпущен
			AddRowToGrid(item, Color.Empty, Color.Pink, insertIndex: rowIndex);
		}
		else
		{
			// Всё выпущено
			AddRowToGrid(item, Color.Empty, Color.Empty, insertIndex: rowIndex);
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

	/// <summary>
	/// Добавляет одну строку в DataGridView на основе TechOperationDataGridItem.
	/// </summary>
	/// <param name="item">Экземпляр TechOperationDataGridItem, содержащий все данные для формирования строки.</param>
	/// <param name="backColor1">Цвет фона для ячейки №3 (исполнитель).</param>
	/// <param name="backColor2">Цвет фона для ячейки №4 (тех.переход).</param>
	/// <param name="backColor3">Цвет фона для ячейки №2 (тех.операция).</param>
	/// <param name="insertIndex">Индекс, по которому нужно вставить строку. Если не задан (null), строка добавляется в конец.</param>
	private void AddRowToGrid(
		TechOperationDataGridItem item,
		Color? backColor1 = null,
		Color? backColor2 = null,
		Color? backColor3 = null,
        int? insertIndex = null)
	{
		// Формируем список объектов для добавления в строку
		var rowData = new List<object>();

		// Проверяем, есть ли Repeat
		if (item.techWork != null && item.techWork.Repeat)
		{
			// Формируем строку "Повторить п."
			var repeatNumList = item.techWork.ExecutionWorkRepeats
				.Select(r => TechOperationDataGridItems.SingleOrDefault(s => s.techWork == r.ChildExecutionWork))
				.Where(bn => bn != null)
				.Select(bn => bn.Nomer)
				.ToList();

			var strP = ConvertListToRangeString(repeatNumList);

			rowData.Add(item.executionWorkItem);
			rowData.Add(item.Nomer.ToString());
			rowData.Add(item.TechOperation);
			rowData.Add(item.Staff);
			rowData.Add("Повторить п." + strP);
			rowData.Add(item.TechTransitionValue);
			rowData.Add(item.TimeEtap);
		}
		else
		{
			rowData.Add(item.executionWorkItem);
			rowData.Add(item.Nomer != -1 ? item.Nomer.ToString() : "");
			rowData.Add(item.TechOperation);
			rowData.Add(item.Staff);
			rowData.Add(item.TechTransition);
			rowData.Add(item.TechTransitionValue);
			rowData.Add(item.TimeEtap);
		}

		// Добавляем столбцы "машины" (через ваш метод AddMachineColumns)
		AddMachineColumns(item, rowData);

		// Добавляем оставшиеся ячейки (СЗ, комментарии, рисунок, замечание, ответ и т.д.)
		rowData.Add(item.Protections);
		// Если в techWork есть комментарий, используем его, иначе общий Comments
		rowData.Add(item.techWork?.Comments ?? item.Comments);
		rowData.Add(item.PictureName);
		rowData.Add(item.Vopros);
		rowData.Add(item.Otvet);

		// Создаём DataGridViewRow и заполняем ячейки готовыми данными.
		var newRow = new DataGridViewRow();
		newRow.CreateCells(dgvMain, rowData.ToArray());

		//  Добавляем/вставляем в dgvMain:
		//    - если insertIndex не задан, добавляем в конец,
		var actualIndex = insertIndex ?? dgvMain.Rows.Count;
		dgvMain.Rows.Insert(actualIndex, newRow);

		// Применяем цветовое оформление ячеек (если цвета заданы).
		ApplyCellColors(newRow, backColor1, backColor2, backColor3);
	}

	/// <summary>
	/// Применяет указанные цвета к ячейкам (3,4,2) строки newRow.
	/// </summary>
	/// <param name="row">Строка, к которой применяются цвета.</param>
	/// <param name="color1">Цвет для ячейки [3].</param>
	/// <param name="color2">Цвет для ячейки [4].</param>
	/// <param name="color3">Цвет для ячейки [2].</param>
	private void ApplyCellColors(
		DataGridViewRow row,
		Color? color1,
		Color? color2,
		Color? color3)
	{
		if (color1.HasValue)
			row.Cells[3].Style.BackColor = color1.Value;
		if (color2.HasValue)
			row.Cells[4].Style.BackColor = color2.Value;
		if (color3.HasValue)
			row.Cells[2].Style.BackColor = color3.Value;
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

        if (TechOperat.IsTypical)
        {
			if (TechOperat.techTransitionTypicals.Count == 0)
			{
                _logger.Warning($"Типовая ТО (id:{TechOperat.Id}) с пустым TechTransitionTypicals");
				TechOperat.techTransitionTypicals = context.TechTransitionTypicals.Where(s => s.TechOperationId == TechOperat.Id).ToList();
			}

			foreach (TechTransitionTypical item in TechOperat.techTransitionTypicals)
            {
                var temp = item.TechTransition;
                if (temp == null)
                {
                    temp = context.TechTransitions.Single(s => s.Id == item.TechTransitionId);
                }
                AddNewExecutionWork(temp, techOperationWork, item);
            }
        }
    }

    public ExecutionWork AddNewExecutionWork(TechTransition tech, TechOperationWork techOperationWork, TechTransitionTypical techTransitionTypical = null,
        int? insertIndex = null, string? coefficientValue = null, string? comment = null, string? pictureName = null)
	{
		// Определяем порядковый номер нового ТП в ТО
		TechOperationWork TOWork = TechOperationWorksList.Single(s => s == techOperationWork);

		var currentEwInTow = TOWork.executionWorks.Where(ew => !ew.Delete);
		var lastEwInTow = currentEwInTow.LastOrDefault();
		int lastEwOrder = lastEwInTow?.Order ?? 0;
		int? newEwOrderInTo = null;

		var prevEW = currentEwInTow.OrderBy(e => e.Order).LastOrDefault();
		var rowOrder = prevEW?.RowOrder + 1;

		if (currentEwInTow.Count() == 0 && !TOWork.techOperation.IsTypical) 
			// для типовых ТО insertIndex будет 0 т.к. вдальнейшем таблица обновляется и RowOrder будет выставлен автоматически
		{
			var towItemIndex = TechOperationDataGridItems.FindIndex(i => i.TechOperationWork == TOWork);
			if (towItemIndex == -1)
			{
				throw new Exception("Ошибка при вставке");
			}
			insertIndex = towItemIndex;
			rowOrder = towItemIndex + 1;
			newEwOrderInTo = 1;
		}
		else if (insertIndex != null && lastEwInTow != null)
		{
			// если индекс не null, то вставляем в указанное место

			// определить положение в ТО
			// если последний элемента нет, то вставляем в конец ТО
			// если есть, то определяем его индекс в таблице
			int lastEwIndex = lastEwInTow.RowOrder;

			// сравниваем полученный индекс с индексом вставки
			// если индекс вставки меньше чем индекс последнего элемента ТО
			if (lastEwIndex > insertIndex.Value)
			{
				var firstInTow = currentEwInTow.OrderBy(ew => ew.RowOrder).FirstOrDefault();
				// то его порядок будет меньше на разницу между индексом последнего элемента и индексом вставки
				newEwOrderInTo = firstInTow!.Order + (insertIndex.Value - firstInTow.RowOrder) + 1;

				// если этот порядок меньше 1 то устанавливаем его в 1
				if (newEwOrderInTo < 1)
					newEwOrderInTo = 1;

				//Если порядковый номер уже есть в списке, то увеличиваем порядковый номер у всех последующих элементов
				foreach (var item in currentEwInTow.Where(w => w.Order >= newEwOrderInTo))
				{
					item.Order++;
				}
			}
			// если больше или равно, то вставляем последним 
		}
        else
        {
            if (lastEwInTow == null)
			{
				insertIndex = 0;
			}
			else 
			    insertIndex = lastEwInTow?.RowOrder + 1;
		}

		// если индекс null, то вставляем в конец ТО newEwOrder = null
		if (newEwOrderInTo == null) newEwOrderInTo = lastEwOrder + 1;

		// Если номер не последний в списке ТО, то нужно увеличить порядковый номер у всех последующих элементов
		var ewWithGreaterOrder = currentEwInTow.Where(ew => ew.Order >= newEwOrderInTo);
		foreach (var ew in ewWithGreaterOrder)
		{
			ew.Order++;
		}

		ExecutionWork newEw = new ExecutionWork
        {
            IdGuid = Guid.NewGuid(),
            techOperationWork = TOWork,
            NewItem = true,
            techTransition = tech,
            Order = newEwOrderInTo.Value, // номер в ТО
			RowOrder = insertIndex!.Value, // по сути nomer (порядоковый номер в таблице ХР)
			Comments = comment ?? "",
			PictureName = pictureName ?? "",
			Repeat = false
		};

        TOWork.executionWorks.Add(newEw);
        context.ExecutionWorks.Add(newEw);

        if (tech.Name == "Повторить" || tech.Name == "Повторить п.")
        {
            newEw.Repeat = true;
        }

        if (coefficientValue != null)
		{
			newEw.Coefficient = coefficientValue;
			var coefDict = _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);
			newEw.Value = MathScript.EvaluateCoefficientExpression(coefficientValue,coefDict, tech.TimeExecution.ToString());
		}
		else
		{
			newEw.Value = tech.TimeExecution;
		}

		if (techTransitionTypical != null)
        {
            newEw.Etap = techTransitionTypical.Etap;
            newEw.Posled = techTransitionTypical.Posled;
            newEw.Coefficient = techTransitionTypical.Coefficient;
            newEw.Comments = techTransitionTypical.Comments ?? "";

            newEw.Value = string.IsNullOrEmpty(techTransitionTypical.Coefficient) 
                ? tech.TimeExecution 
                : MathScript.EvaluateExpression(tech.TimeExecution + "*" + techTransitionTypical.Coefficient);
        }

        return newEw;

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

            if (techOperationWork.techOperation.IsTypical)
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

	//public void HighlightExecut  ionWorkRow(ExecutionWork executionWork, bool scrollToRow = false)
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
