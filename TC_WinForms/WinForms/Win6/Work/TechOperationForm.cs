using Serilog;
using System;
using System.Data;
using System.Linq;
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
			if (copyScope != CopyScopeEnum.Text && copyScope != CopyScopeEnum.TechOperation)
				TcCopyData.SetCopyDate(selectedRowIndices.Select(i => TechOperationDataGridItems[i]).ToList(), copyScope);
            else if (copyScope == CopyScopeEnum.TechOperation)
			{
                var selectedItems = selectedRowIndices.Select(i => TechOperationDataGridItems[i]).ToList();
				// проверка, что выбраны строки из одной ТО
				if (selectedItems.Select(i => i.TechOperationWork.Id).Distinct().Count() > 1)
				{
					MessageBox.Show("Выбраны строки из разных ТО. Выделите строки из одной ТО.");
					return;
				}
                var selectedTow = selectedItems[0].TechOperationWork;

                // выделяем скопированные ТП, и фильтруем инструменты и компоненты (их будем вставлять из ТО)
				var copiedItems = TechOperationDataGridItems.Where(i => i.TechOperationWork.Id == selectedTow.Id).ToList();
				TcCopyData.SetCopyDate(copiedItems, CopyScopeEnum.TechOperation);
			}
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

		var cell = dgvMain.SelectedCells[0];
		copyScope = GetCopyScopeByCell(cell);

		if (copyScope == CopyScopeEnum.ToolOrComponents || (selectedRowIndices.Count == 1 && dgvMain.SelectedCells.Count == 1))
		{
            return;
		}

		if (selectedRowIndices.Count == 1)
        {
            copyScope = CopyScopeEnum.Row;
        }
        else if (selectedRowIndices.Count > 1)
        {
            copyScope = CopyScopeEnum.RowRange;
        }
	}

	private void PasteCopiedData()
    {
		List<int> selectedRowIndices;
		CopyScopeEnum? selectedScope;

		GetSelectedDataInfo(out selectedRowIndices, out selectedScope);

		if (selectedRowIndices.Count == 0)
			return;

		if (selectedScope == null)
        {
            MessageBox.Show("Не удалось определить тип копирования.");
            return;
		}    

  //      if(TcCopyData.GetCopyTcId() != _tcId && 
  //          selectedScope != CopyScopeEnum.Text && 
  //          selectedScope != CopyScopeEnum.Staff && 
  //          selectedScope != CopyScopeEnum.Protections &&
		//	TcCopyData.CopyScope != CopyScopeEnum.ToolOrComponents)
		//{
		//	MessageBox.Show("Данные не могут быть вставлены в другую ТК.");
		//	return;
		//}

		var selectedItems = selectedRowIndices.Select(i => TechOperationDataGridItems[i]).ToList();

		if (selectedScope == CopyScopeEnum.Staff)
        {
            // проверяем есть ли в скопированных данных информация
            if (TcCopyData.CopyScope != CopyScopeEnum.Staff) { return; }

			// todo: добавить возможность вставки данных из скопированного ТП

			if (selectedItems.Count != 1) { throw new Exception("Ошибка при вставке данных. Обработка выделенных данных Персонал."); }
            var setectedEw = selectedItems[0].executionWorkItem;
            if (setectedEw == null)
            {
                MessageBox.Show("В данной строке невозможно вставить связь с Персоналом");
                return;
            }

            UpdateStaffInRow(selectedRowIndices[0], setectedEw, TcCopyData.FullItems[0].executionWorkItem.Staffs);
        }
        else if (selectedScope == CopyScopeEnum.Protections)
        {
            // проверяем есть ли в скопированных данных информация
            if (TcCopyData.CopyScope != CopyScopeEnum.Protections) { return; }

            // todo: добавить возможность вставки данных из скопированного ТП

            if (selectedItems.Count != 1) { throw new Exception("Ошибка при вставке данных. Обработка выделенных данных СЗ."); }
            var setectedEw = selectedItems[0].executionWorkItem;
            if (setectedEw == null)
            {
                MessageBox.Show("В данной строке невозможно вставить связь с СЗ");
                return;
            }

            UpdateProtectionsInRow(selectedRowIndices[0], setectedEw, TcCopyData.FullItems[0].executionWorkItem.Protections);
        }
        else if (( selectedScope == CopyScopeEnum.ToolOrComponents || selectedScope == CopyScopeEnum.Row || selectedScope == CopyScopeEnum.RowRange) 
            && TcCopyData.CopyScope == CopyScopeEnum.ToolOrComponents)
		{

			if (TcCopyData.CopyScope != CopyScopeEnum.ToolOrComponents) return;

			if (selectedItems.Count != 1) { throw new Exception("Ошибка при вставке данных. Обработка выделенных данных Инструменты/Компоненты."); }

			var selectedTow = selectedItems[0].TechOperationWork;
			if (selectedTow == null) { throw new Exception("Ошибка при вставке данных. Обработка выделенных данных Инструменты/Компоненты."); }

			// проверка все ли строки содержат являются компонентами или инструментами
			InsertToolAndComponent(selectedTow, TcCopyData.FullItems, updateDataGrid: true);
		}
		else if (selectedScope == CopyScopeEnum.Row)
		{
            if (TcCopyData.CopyScope != CopyScopeEnum.Row 
                && TcCopyData.CopyScope != CopyScopeEnum.RowRange 
                && TcCopyData.CopyScope != CopyScopeEnum.TechOperation) 
                return;

			var rowIndex = selectedRowIndices[0] + 1; // тавляем после выделенной строки

            var selectedTow = selectedItems[0].TechOperationWork;

			if (TcCopyData.CopyScope == CopyScopeEnum.Row)
            {
				PasteAsNewRow(rowIndex, selectedTow, TcCopyData.FullItems[0], updateDataGrid: true);
			}
			else if (TcCopyData.CopyScope == CopyScopeEnum.RowRange)
			{
				var iterator = 0;
				foreach (var copiedItem in TcCopyData.FullItems)
				{
					PasteAsNewRow(rowIndex, selectedTow, copiedItem, updateDataGrid: true);
					rowIndex++;
					iterator++;
				}

				UpdateGrid(); // todo: выяснить, почему не обновляется таблица после UpdateProtectionsInRow и UpdateStaffInRow
			}
            else if (TcCopyData.CopyScope == CopyScopeEnum.TechOperation)
			{
				//  вставляем все ТП из скопированных данных в выделенную ТО
				var iterator = 0;
                var copiedEwItems = TcCopyData.FullItems.Where(i => i.WorkItemType == WorkItemType.ExecutionWork).ToList();

				foreach (var copiedItem in copiedEwItems)
				{
					PasteAsNewRow(rowIndex, selectedTow, copiedItem, updateDataGrid: false);
					rowIndex++;
					iterator++;
				}

                var copiedToolandComponentItems = TcCopyData.FullItems.Where(i => i.WorkItemType == WorkItemType.ToolWork 
                || i.WorkItemType == WorkItemType.ComponentWork)
                    .ToList();

				InsertToolAndComponent(selectedTow, copiedToolandComponentItems, updateDataGrid: false);

				UpdateGrid();
			}
		}
		else if (selectedScope == CopyScopeEnum.Text)
		{
			PasteClipboardValue();
		}
		else
        {
			MessageBox.Show("Не удалось определить тип копирования.");
		}
	}

	private void PasteAsNewRow(int rowIndex, TechOperationWork copyToTow, TechOperationDataGridItem copiedItem, bool updateDataGrid = false)
	{
		var copiedEw = copiedItem.WorkItem as ExecutionWork;

		if (copiedItem.WorkItemType != WorkItemType.ExecutionWork || copiedEw == null)
		{
			throw new Exception("Ошибка при вставке данных. Обработка выделенных данных.");
		}

		var isTypicalTo = copyToTow.techOperation.IsTypical;
		// для типовой ТО возможно вставить только повтор
		if (isTypicalTo && !copiedEw.Repeat)
		{
			MessageBox.Show("В типовую операцию возможна вставта только повторов.");
			return;
		}

		if (TcCopyData.GetCopyTcId() != _tcId)
        {
			// заменим скопированные объекты на существующие в данной контексте
			copiedEw.techTransition = context.TechTransitions.FirstOrDefault(t => t.Id == copiedEw.techTransitionId);
		}

		var techTransition = copiedEw.techTransition;
		if (techTransition == null) return;

		ExecutionWork newEw;

		if (copiedEw.Repeat)
		{
			var repeatEws = copiedEw.ExecutionWorkRepeats.ToList();
            if (TcCopyData.GetCopyTcId() != _tcId)
            {
                //MessageBox.Show("Вставка повтора из другой ТК не поддерживается.");
                //return;
                // заменим скопированные объекты на существующие в данной контексте
                //foreach (var ewRepeat in repeatEws)
                //            {
                //                //var newEwRepeats = context.ExecutionWorks.FirstOrDefault(ew => ew.Id == ewRepeat.ChildExecutionWorkId);
                //                //if (newEwRepeats == null) throw new Exception("Ошибка при вставке повтора.");
                //                //ewRepeat.ChildExecutionWork = newEwRepeats;
                //            }
                repeatEws = new List<ExecutionWorkRepeat>();
			}

			if (repeatEws == null) throw new Exception("Ошибка при вставке повтора.");

			newEw = InsertNewRow(techTransition, copyToTow, rowIndex, repeatEws, coefficient: copiedEw.Coefficient, updateDataGrid: updateDataGrid);
		}
		else
            newEw = InsertNewRow(techTransition, copyToTow, rowIndex, coefficient: copiedEw.Coefficient, updateDataGrid: updateDataGrid);

        UpdateProtectionsInRow(rowIndex, newEw, copiedEw.Protections, updateDataGrid: updateDataGrid);
        UpdateStaffInRow(rowIndex, newEw, copiedEw.Staffs, updateDataGrid: updateDataGrid);
		// todo: что с механизмами?
		// todo: что с группой паралельности и последовательности?
	}

	public ExecutionWork InsertNewRow(TechTransition techTransition,  TechOperationWork techOperationWork,
        int? insertIndex = null, List<ExecutionWorkRepeat>? executionWorksRepeats = null,
        string? coefficient = null, bool updateDataGrid = true)

	{
		var newEw = AddNewExecutionWork(techTransition, techOperationWork, insertIndex: insertIndex, coefficientValue: coefficient);

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

    public void UpdateStaffInRow(int rowIndex, ExecutionWork selectedEw, List<Staff_TC> copiedStaff, bool updateDataGrid = true)
	{
		if (selectedEw == null) throw new ArgumentNullException(nameof(selectedEw));
		if (copiedStaff == null) throw new ArgumentNullException(nameof(copiedStaff));

		var currentCopyScope = CopyScopeEnum.Staff;
		var columnIndex = GetColumnIndex(currentCopyScope)
			?? throw new Exception($"Не найден столбец соответствующий типу {currentCopyScope}");

		// если ТК копирования не совпадает с текущей ТК, то находим совпадабщие объекты и создаём новые
        // Заменяем на сущствующий в случае совпадения id персонала и его символа.
        // В других случаях заменяем на новый объект с новым символом.
        // (Доп) Логика выбора символа: при отсутствии символа у сущ - х объектов добавляем новый с тем символом который был при копировании.
        if (TcCopyData.GetCopyTcId() != _tcId)
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
						//addingStaff = context.Staffs.FirstOrDefault(s => s.Id == copiedStc.ChildId);
						addingStaff = copiedStc.Staff_TC.Child;
					}

					// проверка на наличие символа
					var addingSymbol = copiedStc.Symbol;
                    if(existingStaff_tcs.Select(st => st.Symbol).Contains(addingSymbol))
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
		}

		var staffSet = new HashSet<Staff_TC>(copiedStaff);

		// Удаляем объекты, которых нет в copiedStaff
		selectedEw.Staffs.RemoveAll(staff => !staffSet.Contains(staff));

		// Добавляем новые объекты из copiedStaff
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

	public void UpdateProtectionsInRow(int rowIndices, ExecutionWork selectedEw, List<Protection_TC> copiedProtections, bool updateDataGrid = true)
	{
		if (selectedEw == null) throw new ArgumentNullException(nameof(selectedEw));
		if (copiedProtections == null) throw new ArgumentNullException(nameof(copiedProtections));

		var currentCopyScope = CopyScopeEnum.Protections;
		var columnIndex = GetColumnIndex(currentCopyScope)
			?? throw new Exception($"Не найден столбец соответствующий типу {currentCopyScope}");

		if (TcCopyData.GetCopyTcId() != _tcId)
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
		}

		var objectSet = new HashSet<Protection_TC>(copiedProtections);

		// Удаляем объекты, которых нет в copiedStaff
		selectedEw.Protections.RemoveAll(obj => !objectSet.Contains(obj));

		// Добавляем новые объекты из copiedStaff
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
	}

	private void InsertToolAndComponent(TechOperationWork selectedTow, List<TechOperationDataGridItem> copiedRows, bool updateDataGrid = false)
	{
		if (copiedRows.Any(r => r.WorkItemType != WorkItemType.ToolWork && r.WorkItemType != WorkItemType.ComponentWork))
		{
			MessageBox.Show("Вставка возможна только для строк с инструментами или компонентами.");
			return;
		}

		bool IsDGChanged = false;

		var toolRows = copiedRows.Where(r => r.WorkItemType == WorkItemType.ToolWork).ToList();



		if (toolRows.Count > 0)
		{
			var toolWorks = toolRows.Select(r => r.WorkItem).Cast<ToolWork>().ToList();


			// добавляем скопированные инструменты, если их нет в ТО
			foreach (var toolWork in toolWorks)
			{
				if (selectedTow.ToolWorks.Where(o => o.toolId == toolWork.toolId).Count() == 0)
				{
                    Tool addingObject = toolWork.tool;
					if (TcCopyData.GetCopyTcId() != _tcId)
					{
						var existionObject = TehCarta.Tools.FirstOrDefault(o => o.Id == toolWork.toolId); 
                        if (existionObject == null)
						{
							existionObject = context.Tools.FirstOrDefault(o => o.Id == toolWork.toolId);
                            if (existionObject == null)
							    throw new Exception("Ошибка при копировании инструментов. Ошибка 1246");

							TehCarta.Tool_TCs.Add(new Tool_TC
							{
								ParentId = TehCarta.Id,
								ChildId = toolWork.toolId,
								Child = existionObject,
								Quantity = toolWork.Quantity,
								Note = toolWork.Comments,

                                Order = TehCarta.Tool_TCs.Count + 1
							});
						}

						addingObject = existionObject;
					}

					selectedTow.ToolWorks.Add(new ToolWork
					{
						toolId = toolWork.toolId,
						tool = addingObject,
						Quantity = toolWork.Quantity,
						Comments = toolWork.Comments
					});


					if (!IsDGChanged)
						IsDGChanged = true;
				}
			}
		}

		var componentRows = copiedRows.Where(r => r.WorkItemType == WorkItemType.ComponentWork).ToList();

		if (componentRows.Count > 0)
		{
			var copiedComponentWorks = componentRows.Select(r => r.WorkItem).Cast<ComponentWork>().ToList();
			var copiedComponents = copiedComponentWorks.Select(tw => tw.component).ToList();
			// добавляем скопированные инструменты, если их нет в ТО
			foreach (var componentWork in copiedComponentWorks)
			{
				if (selectedTow.ComponentWorks.Where(o => o.componentId == componentWork.componentId).Count() == 0)
				{
					Component addingObject = componentWork.component;
					if (TcCopyData.GetCopyTcId() != _tcId)
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

								Order = TehCarta.Tool_TCs.Count + 1
							});
						}

						addingObject = existionObject;
					}

					selectedTow.ComponentWorks.Add(new ComponentWork
					{
						componentId = componentWork.componentId,
						component = addingObject,
						Quantity = componentWork.Quantity,
						Comments = componentWork.Comments
					});

					if (!IsDGChanged)
						IsDGChanged = true;
				}
			}
		}

		if (IsDGChanged && updateDataGrid)
			UpdateGrid();
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

        if (techOperationDataGridItem.ItsTool || techOperationDataGridItem.ItsComponent)
        { copyScope = CopyScopeEnum.ToolOrComponents; }
        else
        {
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
					copyScope = CopyScopeEnum.Row;
					break;
				case "Примечание":
				case "Рис.":
				case "Замечание":
				case "Ответ":
                case "№":
					copyScope = CopyScopeEnum.Text;
					break;
			}
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
        CoefficientForm coefficient = null, int? insertIndex = null, string? coefficientValue = null) // todo: убрать CoefficientForm в качестве параметра и передать только коэффициент, а значение вычислять внутри метода
	{

		// Определяем порядковый номер нового ТП в ТО
		var lastEwInTow = techOperationWork.executionWorks.OrderBy(ew => ew.RowOrder).LastOrDefault();
		int lastEwOrder = lastEwInTow?.Order ?? 0;
		int? newEwOrderInTo = null;

		if (insertIndex != null && lastEwInTow != null)
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
				var firstInTow = techOperationWork.executionWorks.OrderBy(ew => ew.RowOrder).FirstOrDefault();
				// то его порядок будет меньше на разницу между индексом последнего элемента и индексом вставки
				newEwOrderInTo = firstInTow!.Order + (insertIndex.Value - firstInTow.RowOrder) + 1;

				// если этот порядок меньше 1 то устанавливаем его в 1
				if (newEwOrderInTo < 1)
					newEwOrderInTo = 1;

				//Если порядковый номер уже есть в списке, то увеличиваем порядковый номер у всех последующих элементов
				foreach (var item in techOperationWork.executionWorks.Where(w => w.Order >= newEwOrderInTo))
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
                //todo: как быть, если в ТО нет элементов?
                insertIndex = 0;
			}
			else 
			    insertIndex = lastEwInTow?.RowOrder + 1;
		}

		// если индекс null, то вставляем в конец ТО newEwOrder = null
		if (newEwOrderInTo == null) newEwOrderInTo = lastEwOrder + 1;

		TechOperationWork TOWork = TechOperationWorksList.Single(s => s == techOperationWork);

        var prevEW = TOWork.executionWorks.OrderBy(e => e.Order).LastOrDefault();
        var rowOrder = prevEW?.RowOrder+1;

        // todo: если номер не последний в списке ТО, то нужно увеличить порядковый номер у всех последующих элементов

		ExecutionWork newEw = new ExecutionWork
        {
            IdGuid = Guid.NewGuid(),
            techOperationWork = TOWork,
            NewItem = true,
            techTransition = tech,
            Order = newEwOrderInTo.Value, // номер в ТО
			RowOrder = insertIndex!.Value, // по сути nomer (порядоковый номер в таблице ХР)
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

		//if (coefficient != null)
		//{
		//    newEw.Coefficient = coefficient.GetCoefficient;
		//    newEw.Value = coefficient.GetValue;
		//}
		//else
		//{
		//    newEw.Value = tech.TimeExecution;
		//}

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
