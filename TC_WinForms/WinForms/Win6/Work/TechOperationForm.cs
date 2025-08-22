using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;
using System.Diagnostics;
using System.Text;
using TC_WinForms.DataProcessing;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TC_WinForms.WinForms.Diagram;
using TC_WinForms.WinForms.Win6.Healpers;
using TC_WinForms.WinForms.Win6.ImageEditor;
using TC_WinForms.WinForms.Win6.Models;
using TC_WinForms.WinForms.Win6.Work;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace TC_WinForms.WinForms.Work;

public partial class TechOperationForm : Form, ISaveEventForm, IViewModeable, IOnActivationForm, IFormWithObjectId
{
    private readonly Stopwatch _dataLoadSw = new Stopwatch(); // общее время TechOperationForm_Load
    
    private readonly Stopwatch _gridSw = new Stopwatch();  // общее время UpdateGrid()
    private readonly Stopwatch _paintSw = new Stopwatch();  // от завершения UpdateGrid до первого Paint
    private volatile bool _awaitingFirstPaint;               // флажок ожидания первой отрисовки

    private ILogger _logger;

	private readonly TcViewState _tcViewState;
    private MainGridColumns _gridColumns;

    private bool _isMachineViewMode;

    // Словарь для хранения ширины столбцов
    private Dictionary<string, int> _columnWidths = new Dictionary<string, int>();

    public MyDbContext context { get; private set; }

    public readonly int _tcId;

	private List<TechOperationDataGridItem> TechOperationDataGridItems = new List<TechOperationDataGridItem>();
    private AddEditTechOperationForm? _editForm;

    public List<TechOperationWork> TechOperationWorksList = null!;
    public TechnologicalCard TehCarta = null!;
    private Dictionary<long, ImageOwner> copiedImageData = new Dictionary<long, ImageOwner>();
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

		InitializeComponent();

        dgvMain.CellPainting += DgvMain_CellPainting;
        dgvMain.CellFormatting += DgvMain_CellFormatting;
        dgvMain.Scroll += (_, __) => dgvMain.InvalidateColumn(2);
        dgvMain.Paint += DgvMain_Paint;     // отметим момент первой отрисовки
        this.Shown += (_, __) => _logger.Information("Форма показана пользователю.");
        dgvMain.CellEndEdit += DgvMain_CellEndEdit;
        dgvMain.CellMouseEnter += DgvMain_CellMouseEnter;
		dgvMain.MouseDown += DataGridView_MouseDown;
        dgvMain.CellContentDoubleClick += DgvMain_CellContentDoubleClick;
        dgvMain.CellContentClick += DgvMain_CellContentClick;
        _tcViewState.ViewModeChanged += OnViewModeChanged;
		this.KeyPreview = true;
		this.KeyDown += new KeyEventHandler(Form_KeyDown);

		SetContextMenuSetings();
    }

    private void DgvMain_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _tcViewState.IsViewMode)
            return;

        // Проверяем, что клик был по колонке с кнопкой
        if (dgvMain.Columns[e.ColumnIndex].Name == "PictureNameColumn" && dgvMain[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell buttonCell &&
                buttonCell.Value?.ToString() == "Добавить рисунок")
        {
            var cell = dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ExecutionWork ew)
            {
                var editor = new Win6_ImageEditor(ew, _tcViewState, context);
                editor.AfterSave = async (savedObj) =>
                {
                    RefreshPictureNameColumn();
                };

                editor.Show();
            }
        }
    }

    public void RefreshPictureNameColumn()
    {
        var sw = Stopwatch.StartNew();
        int updated = 0;

        for (int rowIndex = 0; rowIndex < dgvMain.Rows.Count; rowIndex++)
        {
            if (dgvMain.Rows[rowIndex].Cells[0].Value is ExecutionWork ew)
            {
                UpdatePictureCell(rowIndex, ew);
                updated++;
            }
        }

        sw.Stop();
        _logger.Debug("RefreshPictureNameColumn: {Updated}/{Rows} строк за {ms} мс.",
                      updated, dgvMain.Rows.Count, sw.ElapsedMilliseconds);
    }

    private void DgvMain_CellContentDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_gridColumns.TechTransitionName.Index == e.ColumnIndex
            && dgvMain.Rows[e.RowIndex].Cells[0].Value is ExecutionWork ew
            && ew.techTransition.IsRepeatAsInTcTransition())
		{
            if(ew.RepeatsTCId == null)
            {
                MessageBox.Show("Возможно, это старый вариант технологического перехода, который не работал как ссылка на ТП других ТК. Для того, чтобы перейти на связаную ТК, нужно пересоздать этот ТП, выбрать нужную ТК и отметить пункты на которые хотите сослаться.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
			var result = MessageBox.Show("Открыть связанную ТК?", "Открытие ТК", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if(result == DialogResult.Yes)
			{
				var relatedTc = new Win6_new((int)ew.RepeatsTCId,_tcViewState.UserRole, _tcViewState.IsViewMode);
				relatedTc.Show();
            }
		}
    }

    private void TechOperationForm_Load(object sender, EventArgs e)
    {
        _dataLoadSw.Restart();

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

        _dataLoadSw.Stop();
        _logger.Information(
            "Форма готова: загрузка данных и первичное построение заняли {ms} мс. Rows={Rows}, Cols={Cols}.",
            _dataLoadSw.ElapsedMilliseconds, dgvMain.Rows.Count, dgvMain.Columns.Count);
    }

	/// <summary>
	/// Переключает отображение колонок "Замечание" и "Ответ" 
	/// в соответствии с режимом отображения замечаний в _tcViewState.
	/// </summary>
	public void SetCommentViewMode()
    {
        _gridColumns.Remark.Visible = _tcViewState.IsCommentViewMode;
        _gridColumns.Response.Visible = _tcViewState.IsCommentViewMode;
    }

	/// <summary>
	/// Включает или выключает видимость столбцов механизмов,
	/// исходя из флага <paramref name="isMachineViewMode"/> 
	/// либо глобального состояния Win6_new.isMachineViewMode.
	/// </summary>
	public void SetMachineViewMode(bool? isMachineViewMode = null)
    {
        if (isMachineViewMode != null)
            _isMachineViewMode = (bool)isMachineViewMode;
        else
            _isMachineViewMode = Win6_new.isMachineViewMode;

        foreach(DataGridViewColumn col in dgvMain.Columns)
        {
            if (col.IsRole(ColumnRole.TimeOfMechanism))
            {
                col.Visible = _isMachineViewMode;
            }
        }
    }

	/// <summary>
	/// Устанавливает или обновляет общий «режим просмотра» 
	/// (скрытие/отображение управляющей панели <c>pnlControls</c>).
	/// </summary>
	/// <param name="isViewMode">
	/// Если передано значение не испозуется, 
	/// берётся текущее состояние <c>_tcViewState.IsViewMode</c>.
	/// </param>
	public void SetViewMode(bool? isViewMode = null)
    {
        pnlControls.Visible = !_tcViewState.IsViewMode;
    }

	/// <summary>
	/// Обработчик события изменения режима просмотра 
	/// (вызывается при переключении <c>_tcViewState.IsViewMode</c>).
	/// Производит обновление грида.
	/// </summary>
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
	private ToolStripMenuItem copyImageItem;

    private ToolStripSeparator separatorItem1;

	private ToolStripMenuItem openEditFormItem;

	private ToolStripSeparator separatorItem2;

	// Пункты для вставки
	private ToolStripMenuItem pasteTextItem;
	private ToolStripMenuItem pasteStaffItem;
	private ToolStripMenuItem pasteRowItem;
	private ToolStripMenuItem pasteTechOperationItem;
	private ToolStripMenuItem pasteProtectionsItem;
    private ToolStripMenuItem pasteImageItem;

    // Дополнительные пункты
    //private ToolStripMenuItem openRelatedTc;
    private ToolStripMenuItem openImageEditor;
    private ToolStripMenuItem changeRemarkStatusItem;
    private ToolStripMenuItem goToNextRemarkItem;

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
        copyImageItem = new ToolStripMenuItem("Копировать изображения");

		copyTextItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Копировать текст' в контекстном меню.");
			CopyClipboardValue();
		};
		copyStaffItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Копировать персонал' в контекстном меню.");
			CopyData();
		};
		copyRowItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Копировать строку' в контекстном меню.");
			CopyData();
		};
		copyTechOperationItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Копировать техоперацию' в контекстном меню.");
			CopyData();
		};
		copyProtectionsItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Копировать СИЗ' в контекстном меню.");
			CopyData();
		};

        copyImageItem.Click += (s, e) =>
        {
            _logger.LogUserAction("Выбрал пункт меню 'Копировать изображения' в контекстном меню.");
            CopyData();
        };

		// 2) Пункты "Вставить"
		pasteTextItem = new ToolStripMenuItem("Вставить текст");
		pasteStaffItem = new ToolStripMenuItem("Вставить персонал");
		pasteRowItem = new ToolStripMenuItem("Вставить строку");
		pasteTechOperationItem = new ToolStripMenuItem("Вставить техоперацию");
		pasteProtectionsItem = new ToolStripMenuItem("Вставить СИЗ");
        pasteImageItem = new ToolStripMenuItem("Вставить изображения");

        pasteTextItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Вставить текст' в контекстном меню.");
			PasteClipboardValue();
		};
		pasteStaffItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Вставить персонал' в контекстном меню.");
			PasteCopiedData();
		};
		pasteRowItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Вставить строку' в контекстном меню.");
			PasteCopiedData();
		};
		pasteTechOperationItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Вставить техоперацию' в контекстном меню.");
			PasteCopiedData();
		}		;
		pasteProtectionsItem.Click += (s, e) => {
			_logger.LogUserAction("Выбрал пункт меню 'Вставить СИЗ' в контекстном меню.");
			PasteCopiedData();
		};

        pasteImageItem.Click += (s, e) => {
            _logger.LogUserAction("Выбрал пункт меню 'Вставить изображения' в контекстном меню.");
            PasteCopiedData();
        };

        // 3) Дополнительные пункты
        openImageEditor = new ToolStripMenuItem("Редактировать изображения");

        openImageEditor.Click += (s, e) =>
        {
            _logger.LogUserAction("Выбрал пункт меню 'Редактировать изображения' в контекстном меню.");
            EditImageData();
        };

        // 3) Разделитель
        separatorItem1 = new ToolStripSeparator();
		separatorItem2 = new ToolStripSeparator();


		openEditFormItem = new ToolStripMenuItem("Открыть в редакторе");
        changeRemarkStatusItem = new ToolStripMenuItem("Открыть замечание");
        goToNextRemarkItem = new ToolStripMenuItem("Следующее замечание");

        openEditFormItem.Click += (s, e) => {
            _logger.LogUserAction("Выбрал пункт меню 'Открыть в редакторе' в контекстном меню.");
            OpenEditFormBySelectedObject();
        };

        changeRemarkStatusItem.Click += (s, e) => {
            _logger.LogUserAction("Выбрал пункт меню 'Изменить статус замечания' в контекстном меню.");
            ChangeRamrkStatus();
        };

        goToNextRemarkItem.Click += (s, e) => {
            _logger.LogUserAction("Выбрал пункт меню 'Следующее замечание' в контекстном меню.");
            GoToNextRemark();
        };

        //openRelatedTc = new ToolStripMenuItem("Открыть связанную тех. карту");
        //openRelatedTc.Click += (s, e) =>
        //{
        //	_logger.LogUserAction("Выбрал пункт меню 'Открыть связанную тех. карту' в контекстном меню.");
        //	OpenRelatedTc();
        //};

        // Добавляем все пункты (или группируем в под-меню).
        contextMenu.Items.Add(copyStaffItem);
		contextMenu.Items.Add(copyTechOperationItem);
		contextMenu.Items.Add(copyProtectionsItem);
		contextMenu.Items.Add(copyRowItem);
		contextMenu.Items.Add(copyTextItem);
		contextMenu.Items.Add(copyImageItem);

        contextMenu.Items.Add(separatorItem1);

		contextMenu.Items.Add(openEditFormItem);
        contextMenu.Items.Add(openImageEditor);
        contextMenu.Items.Add(changeRemarkStatusItem);
        contextMenu.Items.Add(goToNextRemarkItem);

        contextMenu.Items.Add(separatorItem2);

		contextMenu.Items.Add(pasteStaffItem);

		contextMenu.Items.Add(pasteRowItem);
		contextMenu.Items.Add(pasteTechOperationItem);
		contextMenu.Items.Add(pasteProtectionsItem);
		contextMenu.Items.Add(pasteTextItem);
		contextMenu.Items.Add(pasteImageItem);
    }

    /// <summary>
    /// Настраивает видимость и доступность пунктов контекстного меню 
    /// (копировать/вставить) в зависимости от выбранной области копирования.
    /// </summary>
    /// <param name="selectedScope">
    /// Если null — пункты меню скрываются по умолчанию.
    /// </param>
    private void UpdateContextMenuItems(CopyScopeEnum? selectedScope) // ?? добавить параметр поле readonly ??
	{
		// Скрываем или делаем Disabled все пункты
		copyTextItem.Visible = false;
		copyStaffItem.Visible = false;
		copyRowItem.Visible = false;
		copyTechOperationItem.Visible = false;
		copyProtectionsItem.Visible = false;
        copyImageItem.Visible = false;

        openImageEditor.Visible = false;
        changeRemarkStatusItem.Visible = false;
        goToNextRemarkItem.Visible = false;

        separatorItem1.Visible = false;
		openEditFormItem.Visible = false;
		separatorItem2.Visible = true;

        pasteImageItem.Visible = false;
		pasteTextItem.Visible = false;
		pasteStaffItem.Visible = false;
		pasteRowItem.Visible = false;
		pasteTechOperationItem.Visible = false;
		pasteProtectionsItem.Visible = false;

        pasteImageItem.Enabled = false;
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

		var isVisibleOrViewMode = _tcViewState.IsViewMode ? false : true;

		// --- Показываем/прячем пункты КОПИРОВАНИЯ в зависимости от scope ---
		switch (selectedScope)
		{
			case CopyScopeEnum.Staff:
				// Пользователь кликнул ячейку "Исполнитель"
				copyTextItem.Visible = true;
				copyStaffItem.Visible = true;          // Можно копировать персонал
				ShowOpenEditFormItem();
				//copyRowItem.Visible = true;  // И строку
				break;

			case CopyScopeEnum.Protections:
				copyTextItem.Visible = true;
				copyProtectionsItem.Visible = true;
				ShowOpenEditFormItem();
				//copyRowItem.Visible = true;
				break;

			case CopyScopeEnum.ToolOrComponents:
				// Инструменты/компоненты
				copyTextItem.Visible = true;
				copyRowItem.Visible = true;
				ShowOpenEditFormItem();
				break;

			case CopyScopeEnum.TechTransition:
				copyTextItem.Visible = true;
				copyRowItem.Visible = true;
				ShowOpenEditFormItem();
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
				ShowOpenEditFormItem();
				break;

			case CopyScopeEnum.Text:
				// Просто текстовая ячейка (Примечание, Рис., Замечание)
				copyTextItem.Visible = true;
				//copyRowItem.Visible = true;
				break;
            case CopyScopeEnum.ImageData:
                // Клик по ячейке "Рис."
                copyImageItem.Visible = true;
                ShowImageEditButton();
                break;
            case CopyScopeEnum.Remark:
                //changeRemarkStatusItem.Visible = true;
                ShowRemarktatusButton();
                goToNextRemarkItem.Visible = true;
                copyTextItem.Visible = true;
                break;

        }

        // --- Проверяем, что лежит в буфере TcCopyData, 
        //     и какие пункты "ВСТАВИТЬ" имеет смысл включить ---
        var copiedScope = TcCopyData.CopyScope;

		if (selectedScope != CopyScopeEnum.RowRange)
			switch (copiedScope)
			{
                case CopyScopeEnum.ImageData:
                    if (selectedScope == CopyScopeEnum.ImageData)
                    {
                        pasteImageItem.Visible = true;
                        pasteImageItem.Enabled = true;
                    }
                    break;
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

                case CopyScopeEnum.Remark:
				case CopyScopeEnum.Text:
					if (selectedScope == CopyScopeEnum.Row)
					{
						separatorItem2.Visible = false;
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
					separatorItem2.Visible = false;
					break;
			}

		// Пример: можно также проверять, включён ли режим просмотра:
		if (_tcViewState.IsViewMode)
		{
			// Если режим только для просмотра, то пункты "Вставить" отключаем
			pasteStaffItem.Visible = false;
			pasteRowItem.Visible = false;
            pasteImageItem.Visible = false; 
            openImageEditor.Visible = false;
			pasteTechOperationItem.Visible = false;
			pasteProtectionsItem.Visible = false;
		}

        void ShowImageEditButton()
        {
            var cell = dgvMain.CurrentCell;
            if (cell is DataGridViewButtonCell buttonCell && buttonCell.Value?.ToString() == "Добавить рисунок")
            {
                // Есть кнопка "Добавить рисунок" — не показываем пункт редактирования
                openImageEditor.Visible = false;
            }
            else
            {
                // Кнопки нет — можно редактировать
                openImageEditor.Visible = true;
                openImageEditor.Text = "Редактировать изображения";
                separatorItem1.Visible = isVisibleOrViewMode;
            }
        }

        void ShowRemarktatusButton()
        {
            var selectedCells = dgvMain.SelectedCells
            .Cast<DataGridViewCell>()
            .Distinct()
            .Select(c => c.RowIndex)
            .OrderBy(idx => idx)
            .ToList();

            var selectedItem = selectedCells.Select(i => TechOperationDataGridItems[i]).FirstOrDefault();

            if (selectedItem != null && selectedItem.WorkItem is IRamarkable remarkItem && !string.IsNullOrEmpty(remarkItem.Remark))
            {
                separatorItem1.Visible = isVisibleOrViewMode;
                changeRemarkStatusItem.Visible = true;
                changeRemarkStatusItem.Text = selectedItem.IsRemarkClosed ? "Открыть замечание" : "Закрыть замечание";
            }
        }

		void ShowOpenEditFormItem()
		{
			separatorItem1.Visible = isVisibleOrViewMode;
			openEditFormItem.Visible = isVisibleOrViewMode;
		}
	}

	#endregion


	#region Обработка клика правой кнопкой (контекстное меню)

	/// <summary>
	/// Обработчик события нажатия ПКМ (MouseDown) в DataGridView. 
	/// Определяет, куда кликнули, выделяет нужную ячейку и отображает контекстное меню.
	/// </summary>
	private void DataGridView_MouseDown(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			// Если включён режим просмотра, то контекстное меню не показываем
			if (_tcViewState.IsViewMode  &&
				( _tcViewState.UserRole == AuthorizationService.User.Role.User
				|| _tcViewState.UserRole == AuthorizationService.User.Role.ProjectManager)
				) return;

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
	private void Form_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.V)
		{
			_logger.Debug("Пользователь нажал Ctrl+V (строка { RowIndex}, столбец { ColumnIndex}).",
				dgvMain.CurrentCell?.RowIndex,
				dgvMain.CurrentCell?.ColumnIndex);
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
			_logger.Debug("Пользователь нажал Ctrl+C для копирования в строке {RowIndex}, столбце {ColumnIndex}.",
					   dgvMain.CurrentCell?.RowIndex,
					   dgvMain.CurrentCell?.ColumnIndex);

			if (_tcViewState.IsViewMode 
				|| _tcViewState.UserRole == AuthorizationService.User.Role.User 
				|| _tcViewState.UserRole == AuthorizationService.User.Role.ProjectManager)
				CopyClipboardValue();
			else
				CopyData();
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Delete)
		{
			_logger.Debug("Пользователь нажал Delete: очистка значения ячейки (строка {RowIndex}, столбец {ColumnIndex}).",
				dgvMain.CurrentCell?.RowIndex,
				dgvMain.CurrentCell?.ColumnIndex);
			DeleteCellValue(); // очистить текущее значение ячейки
			e.Handled = true;
        }
		else if (e.Control && e.KeyCode == Keys.O)
		{
			_logger.Debug("Пользователь нажал Ctrl+O для открытия редактора ХР (строка {RowIndex}, столбец {ColumnIndex}).",
				dgvMain.CurrentCell?.RowIndex,
				dgvMain.CurrentCell?.ColumnIndex);
			if (!_tcViewState.IsViewMode)
				OpenEditFormBySelectedObject();
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
		_logger.Debug("Начато копирование данных (CopyData).");

		GetSelectedDataInfo(
            out List<int> selectedRowIndices, 
            out CopyScopeEnum? selectedScope);

		if (selectedRowIndices.Count == 0)
		{
			_logger.Debug("Не выбрано ни одной строки для копирования. Операция копирования прервана.");
			return;
		}

		try
		{
			if (selectedScope.HasValue)
			{
				_logger.Information("Выполняется копирование. Scope: {Scope}. Количество выбранных строк: {RowsCount}",
									selectedScope.Value, selectedRowIndices.Count);
			}

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
				case CopyScopeEnum.ImageData:
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
		catch (Exception ex)
		{
			_logger.Error(ex, "Ошибка при копировании данных (CopyData).");
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
		_logger.Debug("Начато вставление данных (PasteCopiedData).");

		GetSelectedDataInfo(out List<int> selectedRowIndices, out CopyScopeEnum? selectedScope);
		if (selectedRowIndices.Count == 0 || selectedScope == null)
		{
			_logger.Debug("Не выбрано ни одной строки или неопределён Scope для вставки. Операция вставки прервана.");
			return;
		}

		_logger.Information("Выполняется вставка. Target Scope: {Scope}. Количество выделенных строк: {RowsCount}",
						   selectedScope.Value, selectedRowIndices.Count);
		_logger.Information("Содержимое буфера TcCopyData: Scope={CopyScope}, Кол-во FullItems={FullItemsCount}",
				   TcCopyData.CopyScope, TcCopyData.FullItems.Count);

		try
		{
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
                case CopyScopeEnum.ImageData:
                    PasteImageDataScope(selectedRowIndices);
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
			if (_editForm != null && !_editForm.IsDisposed)
			{
				_editForm.RefreshActiveTabData();
			}
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Ошибка при вставке данных (PasteCopiedData).");
			MessageBox.Show($"Ошибка при вставке: {ex.Message}");
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

        if (TechOperationDataGridItems[selectedRowIndices[0]].WorkItem is not ExecutionWork ew)
        {
            MessageBox.Show("В данной строке невозможно вставить связь с Персоналом");
            return;
        }

        // Обновляем связь ExecutionWork -> Staffs
		if (TcCopyData.FullItems[0].WorkItem is not ExecutionWork copiedEw)
		{
			MessageBox.Show("Ошибка при вставке данных. Некоректный скопированный объект");
			return;
		}

		UpdateStaffInRow(
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

		if (TechOperationDataGridItems[selectedRowIndices[0]].WorkItem is not ExecutionWork ew)
		{
			MessageBox.Show("В данной строке невозможно вставить связь с СЗ");
			return;
		}
		if (TcCopyData.FullItems[0].WorkItem is not ExecutionWork copiedEw)
		{
			MessageBox.Show("Ошибка при вставке данных. Некоректный скопированный объект");
			return;
		}
		UpdateProtectionsInRow(
			ew,
			copiedEw.Protections
		);
	}

    private void PasteImageDataScope(List <int> selectedRowIndexes)
    {
        if (TcCopyData.CopyScope != CopyScopeEnum.ImageData) return;

        if(selectedRowIndexes.Count != 1)
            throw new Exception("Ошибка: для вставки изображений выделите ровно одну строку.");

        if (TechOperationDataGridItems[selectedRowIndexes[0]].WorkItem is not ExecutionWork ew)
        {
            MessageBox.Show("В данной строке невозможно вставить связь с изображения");
            return;
        }
        if (TcCopyData.FullItems[0].WorkItem is not ExecutionWork copiedEw)
        {
            MessageBox.Show("Ошибка при вставке данных. Некоректный скопированный объект");
            return;
        }
        UpdateImagesInRow(
            selectedRowIndexes[0],
            ew,
            copiedEw.ImageList
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

				for (int i = 0; i < TcCopyData.FullItems.Count; i++)
				{
					PasteAsNewRow(rowIndex + i, selectedTow, TcCopyData.FullItems[i], updateDataGrid: false);
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
	/// Удаление значения из текущей ячейки.
	/// </summary>
	private void DeleteCellValue()
	{
		if (dgvMain.CurrentCell != null && !dgvMain.CurrentCell.ReadOnly)
		{
			_logger.Debug("Очищаем значение ячейки (DeleteCellValue). Ячейка: {RowIndex}-{ColIndex}, Предыдущее значение: {Value}",
						  dgvMain.CurrentCell.RowIndex,
						  dgvMain.CurrentCell.ColumnIndex,
						  dgvMain.CurrentCell.Value);

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

        //if (cell is DataGridViewButtonCell || (columnName == "Рис." && isToolOrComponent))
        //    return null;

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
			"Примечание" or "Ответ" or "№" => CopyScopeEnum.Text,
            "Рис." => CopyScopeEnum.ImageData,
            "Замечание" => CopyScopeEnum.Remark,
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

		if (copiedEw.techTransition == null) return;
		try
		{
			// Создаём дубликат, если копируем из другой ТК
			if (TcCopyData.GetCopyFormGuId() != _tcViewState.FormGuid)
				copiedEw = CloneExecutionWorkForAnotherTC(copiedEw);

            // Вставляем новую строку
            var newEw = InsertNewExecutionWork(
				copiedEw.techTransition ?? throw new Exception("Ошибка при вставке: нет TechTransition в скопированном объектке."),
				selectedToTow,
				rowIndex,
				copiedEw.Repeat ? copiedEw.ExecutionWorkRepeats : null,
				coefficient: copiedEw.Coefficient,
				updateDataGrid: updateDataGrid,
				comment: copiedEw.Comments,
				repeatTcId: copiedEw.RepeatsTCId ?? 0
			);

            newEw.TempGuid = copiedEw.IdGuid;
			TcCopyData.PastedEw.Add(newEw);

		
			UpdateProtectionsInRow( newEw, copiedEw.Protections, updateDataGrid: updateDataGrid);
			UpdateStaffInRow( newEw, copiedEw.Staffs, updateDataGrid: updateDataGrid);
            UpdateImagesInRow(rowIndex, newEw, copiedEw.ImageList, updateDataGrid: updateDataGrid);
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

            ImageList = copiedEw.ImageList,

			RepeatsTCId = copiedEw.RepeatsTCId,
            //Vopros = copiedEw.Vopros,
            //Otvet = copiedEw.Otvet,
        };

		var isRepeatAsInTc = copiedEw.techTransition.IsRepeatAsInTcTransition();
		// проверка на то, чтобы ТП в соотвествии с ТК не ссылался на вставляемую ТК
		if (isRepeatAsInTc && copiedEw.RepeatsTCId == _tcViewState.TechnologicalCard.Id)
		{
			// временно устанавливаю заглужку. Копирование ТП, которые ссылается на текущую ТК, не поддерживается
			throw new Exception("Ошибка при вставке: ТП ссылается на текущую ТК.");
			// todo: доработать логику

			// заменяем ТП на повторить
			// Заменяем все повторяемые ТП на имеющиеся в текущем контексте
			// проверяем, что все повторяемые ТП есть в текущем контексте
			// проверяем, что ТП вставляется на позицию ниже всех повторяемых ТП
		}
		else
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
				ExecutionWork? newEwRepeats;
				if (isRepeatAsInTc)
				{
					// заменяем на существующий в текущем контексте
					newEwRepeats = context.ExecutionWorks.FirstOrDefault(ew => ew.Id == ewRepeat.ChildExecutionWork.Id);
				}
				else
					newEwRepeats = TcCopyData.PastedEw.FirstOrDefault(ew => ew.TempGuid == ewRepeat.ChildExecutionWork.IdGuid);

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
	public void UpdateStaffInRow(ExecutionWork selectedEw, List<Staff_TC> copiedStaff, bool updateDataGrid = true)
	{
		if (selectedEw == null) throw new ArgumentNullException(nameof(selectedEw));
		if (copiedStaff == null) throw new ArgumentNullException(nameof(copiedStaff));

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
			UpdateCellValue(selectedEw.RowOrder - 1, _gridColumns.Staff.Index, newStaffSymbols);
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

    public void UpdateImagesInRow(int rowIndex, ExecutionWork selectedEw, List<ImageOwner> copiedImages, bool updateDataGrid = true)
    {
        if (selectedEw == null) throw new ArgumentNullException(nameof(selectedEw));
        if (copiedImages == null) throw new ArgumentNullException(nameof(copiedImages));

        if (TcCopyData.GetCopyFormGuId() != _tcViewState.FormGuid)
            copiedImages = MergeImagesFromAnotherTc(copiedImages);

        var imgSet = new HashSet<ImageOwner>(copiedImages);
        selectedEw.ImageList.RemoveAll(obj => !imgSet.Contains(obj));
        foreach (var image in copiedImages)
        {
            if (!selectedEw.ImageList.Contains(image))
            {
                selectedEw.ImageList.Add(image);
            }
        }

        if (updateDataGrid)
        {
            UpdatePictureCell(rowIndex, selectedEw);
        }

        List <ImageOwner> MergeImagesFromAnotherTc(List<ImageOwner> copiedImages)
        {
            copiedImages = copiedImages.ToList();
            //Получаем уникальные мзображения
            var existingObjects_tcs = TehCarta.ImageList.ToList();
            var newCopiedImages = new List<ImageOwner>();
            foreach(var copiedObj in copiedImages)
            {
                if(copiedImageData.TryGetValue(copiedObj.ImageStorageId, out ImageOwner existingObject_tc))
                {
                    // если персонал уже есть в ТК, то заменяем на него объект в списке скопированного персонала
                    newCopiedImages.Add(existingObject_tc);
                }
                else
                {
                    if (copiedObj.ImageStorage == null)
                    { throw new Exception("Ошибка при копировании изображений. Изображение не обнаружено"); }

                    var newImage = ImageService.CreateNewImage(copiedObj.ImageStorage);
                    var newObject_tc = new ImageOwner
                    {
                        ImageStorageId = newImage.Id,
                        ImageStorage = newImage,
                        TechnologicalCardId = TehCarta.Id,
                        TechnologicalCard = TehCarta,
                        Name = copiedObj.Name,
                        Role = copiedObj.Role,
                        Number = copiedObj.Role == ImageRole.Image
                        ? (TehCarta.ImageList.Where(i => i.Role == ImageRole.Image).Count() == 0 ? 1 : TehCarta.ImageList.Where(i => i.Role == ImageRole.Image).Select(i => i.Number).Max() + 1)
                        : TehCarta.ImageList.Where(i => i.Role == ImageRole.ExecutionScheme).Count() + 1
                    };

                    

                    newCopiedImages.Add(newObject_tc);

                    // добавить персонал в ТК
                    TehCarta.ImageList.Add(newObject_tc);
                    copiedImageData.Add(copiedObj.ImageStorageId, newObject_tc);
                    context.Entry(newObject_tc).State = EntityState.Added;
                    context.Entry(newImage).State = EntityState.Added;
                }
            }

            copiedImages = newCopiedImages;
            return copiedImages;
        }
    }

	/// <summary>
	/// Обновляет (вставляет) СИЗ (Protections) в указанный ExecutionWork
	/// </summary>
	public void UpdateProtectionsInRow(ExecutionWork selectedEw, List<Protection_TC> copiedProtections, bool updateDataGrid = true)
	{
		if (selectedEw == null) throw new ArgumentNullException(nameof(selectedEw));
		if (copiedProtections == null) throw new ArgumentNullException(nameof(copiedProtections));

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
			UpdateCellValue(selectedEw.RowOrder - 1, _gridColumns.Protection.Index, newCellValue);
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
				bool alreadyExists = selectedTow.ComponentWorks.Any(o => o.componentId == cw.componentId);
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

					if(!TehCarta.Tool_TCs.Any(o => o.ChildId == existionObject.Id))
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

					if (!TehCarta.Component_TCs.Any(o => o.ChildId == existionObject.Id))
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

	/// <summary>
	/// Создаёт новый переход (ExecutionWork) в указанной технологической операции
	/// и при необходимости обновляет DataGridView.
	/// </summary>
	/// <param name="techTransition">Технологический переход, на базе которого создаётся новая строка.</param>
	/// <param name="techOperationWork">TechOperationWork, в который добавляется новый переход.</param>
	/// <param name="insertIndex">
	/// (Необязательный) индекс, по которому вставляется строка в общем списке. 
	/// Если null – добавляется в конец.
	/// </param>
	/// <param name="executionWorksRepeats">
	/// Список повторяемых переходов (если новый переход является "Повторить").
	/// </param>
	/// <param name="coefficient">
	/// Строка с коэффициентом для расчёта времени. 
	/// Если не задана, берётся исходное значение времени <paramref name="techTransition"/>.
	/// </param>
	/// <param name="updateDataGrid">
	/// Нужно ли вызывать <c>UpdateGrid()</c> после вставки.
	/// </param>
	/// <param name="comment">Начальный комментарий к новому переходу.</param>
	/// <param name="pictureName">Имя рисунка для нового перехода.</param>
	/// <returns>Созданный объект <see cref="ExecutionWork"/>.</returns>
	/// <exception cref="Exception">Если создать новый переход не удалось.</exception>
	public ExecutionWork InsertNewExecutionWork(TechTransition techTransition,  TechOperationWork techOperationWork,
        int? insertIndex = null, List<ExecutionWorkRepeat>? executionWorksRepeats = null,
        string? coefficient = null, bool updateDataGrid = true,
        string? comment = null, string? pictureName = null, int repeatTcId = 0)

	{
		if (techTransition == null) throw new ArgumentNullException(nameof(techTransition));
		// проверка на то, чтобы ссылочное ТП не было вставлено раньше ТП на которое оно ссылается
		if (executionWorksRepeats != null && executionWorksRepeats.Count > 0 
			&& techTransition.IsRepeatTypeTransition() 
			&& insertIndex != null)
		{
			if(techTransition.IsRepeatTransition())
			{
				// проверка, что все ТП повтора имеют номомер строки меньше вставляемого ТП
				var isAllRepeatBefore = executionWorksRepeats.All(ew => ew.ChildExecutionWork.RowOrder < insertIndex + 1);

				if (!isAllRepeatBefore)
				{
					throw new Exception("Ошибка при вставке: повторы должны быть вставлены после основного перехода.");
				}

			}
			else if (techTransition.IsRepeatAsInTcTransition() && repeatTcId == _tcViewState.TechnologicalCard.Id)
			{
				throw new Exception("Ошибка при вставке: ТП \"Выполнить в соотвествии с ТК\" ссылается на текущую ТК.");
			}
		}

		var newEw = AddNewExecutionWork(techTransition, techOperationWork, 
			insertIndex: insertIndex,
			coefficientValue: coefficient, 
			comment: comment, 
			pictureName: pictureName, 
			repeatTcId: repeatTcId,
			executionWorkRepeats: executionWorksRepeats != null ? executionWorksRepeats : null);

        if (newEw == null) {
			_logger.Error("InsertNewExecutionWork: AddNewExecutionWork вернул null! Бросаем исключение.");
			throw new Exception("Ошибка при добавлении нового перехода.");
		}

		_logger.Debug("Создан новый ExecutionWork IdGuid={Guid}, Order={Order}, Repeat={Repeat}.",
				  newEw.IdGuid, newEw.Order, newEw.Repeat);

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
				newEw.ExecutionWorkRepeats.Add(newEwRepeat);
                context.Entry(newEwRepeat).State = Microsoft.EntityFrameworkCore.EntityState.Added;//дополнительное подкрепление статуса нового объекта, для корректного сохранения
                                                                                                   //todo:возможно, это лишнее, проверить и удалить
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


    /// <summary>
    /// Обработчик завершения редактирования ячейки <c>dgvMain</c>. 
    /// Присваивает соответствующим свойствам ExecutionWork (или Tool/Component) новое значение.
    /// </summary>
    private void DgvMain_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        var col = dgvMain.Columns[e.ColumnIndex];
        //var ew = dgvMain.Rows[e.RowIndex].Cells[0].Value as ExecutionWork;
        var text = dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;

		text = string.IsNullOrEmpty(text) ? "" : text;

		if (col == _gridColumns.Response)
        {
            if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ExecutionWork ew && ew != null)
            {
                if (text == null)
                {
                    text = "";
                }
                ew.Reply = text;
                HasChanges = true;
            }
            else if(dgvMain.Rows[e.RowIndex].Cells[0].Value is ToolWork tool && tool != null)
            {
                if (text == null)
                {
                    text = "";
                }
                tool.Reply = text;
                HasChanges = true;
            }
            else if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ComponentWork comp && comp != null)
            {
                if (text == null)
                {
                    text = "";
                }
                comp.Reply = text;
                HasChanges = true;
            }

        }
        else if (col == _gridColumns.Remark)
        {
            if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ExecutionWork ew && ew != null)
            {
                if (text == null)
                {
                    text = "";
                }
                ew.Remark = text;
                HasChanges = true;
            }
            else if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ToolWork tool && tool != null)
            {
                if (text == null)
                {
                    text = "";
                }
                tool.Remark = text;
                HasChanges = true;
            }
            else if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ComponentWork comp && comp != null)
            {
                if (text == null)
                {
                    text = "";
                }
                comp.Remark = text;
                HasChanges = true;
            }
        }
        else if (col == _gridColumns.PictureName)
        {

            if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ExecutionWork ew && ew != null)
            {

                HasChanges = true;
                if (_editForm?.IsDisposed == false)
                    _editForm.UpdateLocalTP();
            }
        }
        else if (col == _gridColumns.Comment)
        {
            var itsTool = TechOperationDataGridItems[e.RowIndex].ItsTool;
            var ItsComponent = TechOperationDataGridItems[e.RowIndex].ItsComponent;
            
            if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ExecutionWork ew && ew != null)
            {
                if (text == ew.Comments) return;

                ew.Comments = text;
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
                    editedTool.Comments = text;
                }
                else
                {
                    var editedComp = techWork.ComponentWorks.Where(t => toolComponentName.Contains(t.component.Name)).FirstOrDefault();
                    editedComp.Comments = text;
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

    /// <summary>
    /// Обновляет DataGridView: очищает и заново заполняет все строки/колонки,
    /// затем при необходимости восстанавливает позицию прокрутки и, если открыта,
    /// обновляет форму диаграмм (DiagramForm) в оконном режиме.
    /// </summary>
    public void UpdateGrid()
    {
		// todo: лишнее обновление грида при первой загрузки странички редактирования
		_logger.Debug("Начато обновление грида (UpdateGrid).");
        _gridSw.Restart();

        try // временная заглушка от ошибки возникающей при переключении на другую форму в процессе загрузки данных
        {
            var offScroll = dgvMain.FirstDisplayedScrollingRowIndex;

            var step = Stopwatch.StartNew();
            HandleColumnWidths();
            _logger.Debug("HandleColumnWidths(save) заняло {ms} мс.", step.ElapsedMilliseconds);

            step.Restart();
            ClearAndInitializeGrid();
            _logger.Debug("ClearAndInitializeGrid заняло {ms} мс.", step.ElapsedMilliseconds);

            step.Restart();
            SetCommentViewMode();
            _logger.Debug("SetCommentViewMode заняло {ms} мс.", step.ElapsedMilliseconds);

            step.Restart();
            PopulateDataGrid();
            _logger.Debug("PopulateDataGrid заняло {ms} мс.", step.ElapsedMilliseconds);

            step.Restart();
            SetMachineViewMode();
            _logger.Debug("SetMachineViewMode заняло {ms} мс.", step.ElapsedMilliseconds);

            step.Restart();
            HandleColumnWidths(isSaveMode: false);
            _logger.Debug("HandleColumnWidths(restore) заняло {ms} мс.", step.ElapsedMilliseconds);

            if (offScroll < dgvMain.Rows.Count && offScroll > 0)
                dgvMain.FirstDisplayedScrollingRowIndex = offScroll;

            // Обновление окна диаграмм, если открыто
            var bsForm = CheckOpenFormService.FindOpenedForm<DiagramForm>(_tcId);
            if (bsForm != null)
            {
				_logger.Debug("Обновляем DiagramForm открытое в оконном режиме (UpdateVisualData).");
				bsForm.UpdateVisualData();
            }

			_logger.Debug("Грид обновлён успешно (UpdateGrid завершён).");
		}
        catch (Exception ex)
		{
			_logger.Error(ex, "Ошибка при обновлении грида (UpdateGrid).");
		}
        finally
        {
            _gridSw.Stop();
            _logger.Information("UpdateGrid занял {ElapsedMs} мс.", _gridSw.ElapsedMilliseconds);

            // Готовимся замерить «чистое» время до первого Paint
            _awaitingFirstPaint = true;
            _paintSw.Restart();
        }
    }

    /// <summary>
    /// Универсальный метод для сохранения/восстановления ширины столбцов
    /// actionType: "save" - сохранить, "restore" - восстановить
    /// </summary>
    /// <param name="isSaveMode"></param>
    private void HandleColumnWidths(bool isSaveMode = true)
    {
        if (isSaveMode)
        {
            _columnWidths.Clear();
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                _columnWidths[column.Name] = column.Width;
            }
            _logger.Debug("Ширины столбцов сохранены.");
        }
        else if (!isSaveMode &&_columnWidths.Count > 0)
        {
            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                if (_columnWidths.TryGetValue(column.Name, out int width))
                {
                    column.Width = width;
                }
            }
            _logger.Debug("Ширины столбцов восстановлены.");
        }
    }

    /// <summary>
    /// Очищает существующие строки и колонки в DataGridView и инициализирует новые колонки.
    /// </summary>
    private void ClearAndInitializeGrid()
    {
		_logger.Debug("Очищаем строки и колонки DataGridView, инициализируем новые (ClearAndInitializeGrid).");

        dgvMain.SuspendLayout();
        try {
            dgvMain.Rows.Clear();
            TechOperationDataGridItems.Clear();

            dgvMain.AutoGenerateColumns = false;
            dgvMain.DataSource = null;
            dgvMain.Columns.Clear();

            // Добавляем столбцы в DataGridView
            dgvMain.AddColumn("ExecutionWorkItem", "", visible: false,
                fillWeigth: 100, minWidth:100);
            dgvMain.AddColumn("Order", "№", width: 30,
                fillWeigth: 30, minWidth: 30);
            dgvMain.AddColumn("TechOperationName", "Технологические операции",
                fillWeigth: 140, minWidth: 140);
            dgvMain.AddColumn("Staff", "Исполнитель", width: 120,
                fillWeigth: 120, minWidth: 120);
            dgvMain.AddColumn("TechTransitionName", "Технологические переходы",
                fillWeigth: 140, minWidth: 140);
            dgvMain.AddColumn("TimeValue", "Время действ., мин.",
                fillWeigth: 50, minWidth: 50);
            dgvMain.AddColumn("EtapValue", "Время этапа, мин.",
                fillWeigth: 50, minWidth: 50);

            int i = 0;
            foreach (Machine_TC m in TehCarta.Machine_TCs) {
                dgvMain.AddColumn($"Machine{i}", $"Время {m.Child.Name}, мин.",
                    fillWeigth: 50, minWidth: 50,
                    role: ColumnRole.TimeOfMechanism);
                i++;
            }

            dgvMain.AddColumn("Protection", "№ СЗ", fillWeigth: 50, minWidth: 50);
            dgvMain.AddColumn("CommentColumn", "Примечание", fillWeigth: 140, minWidth: 140);
            dgvMain.AddColumn("PictureNameColumn", "Рис.", fillWeigth: 140, minWidth: 140);
            dgvMain.AddColumn("RemarkColumn", "Замечание", resizable: DataGridViewTriState.True,
                fillWeigth: 300, minWidth: 200,
                role: ColumnRole.Remarks);
            dgvMain.AddColumn("ResponseColumn", "Ответ", resizable: DataGridViewTriState.True,
                fillWeigth: 300, minWidth: 200,
                role: ColumnRole.Remarks);

            // Захватываем ссылки на колонки для дальнейшего кода
            _gridColumns = MainGridColumns.Capture(dgvMain);

            // Устанавливаем дополнительные свойства для столбцов
            _gridColumns.Remark.HeaderCell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Italic | FontStyle.Bold);
            _gridColumns.Remark.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
            _gridColumns.Response.HeaderCell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Italic | FontStyle.Bold);
            _gridColumns.Response.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Italic);

            _gridColumns.TechOperationName.Frozen = true;
            _gridColumns.TechTransitionName.Frozen = true;

            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            _logger.Debug("DataGridView инициализирован: {ColumnCount} столбцов.",
                     dgvMain.Columns.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при очистке и инициализации DataGridView (ClearAndInitializeGrid).");
            throw new Exception("Ошибка при очистке и инициализации DataGridView.", ex);
        }
        finally { dgvMain.ResumeLayout(); }        
	}

    /// <summary>
    /// Заполняет DataGridView данными из списка технологических операций.
    /// </summary>
    private void PopulateDataGrid()
    {
		_logger.Debug("Заполняем DataGridView данными (PopulateDataGrid).");

		var techOperationWorksListLocal = 
            TechOperationWorksList.Where(w => !w.Delete).OrderBy(o => o.Order).ToList();
        _logger.Debug("Найдено {Count} TechOperationWorks для вывода.", techOperationWorksListLocal.Count);

        var mapSw = Stopwatch.StartNew();
        TechOperationDataGridItems = CreateToGridItemService
            .PopulateTechOperationDataGridItems(techOperationWorksListLocal, TehCarta.Machine_TCs);
        mapSw.Stop();
        _logger.Debug("Маппинг в TechOperationDataGridItems занял {ms} мс. Items={Count}",
                      mapSw.ElapsedMilliseconds, TechOperationDataGridItems.Count);

        AddRowsToGrid();

		_logger.Debug("PopulateDataGrid завершён. Итоговое число строк для отображения: {RowCount}",
				 TechOperationDataGridItems.Count);
	}

    /// <summary>
    /// Перерисовывает ячейку PictureNameColumn в указанной строке.
    /// Если ImageList пуст — ставит кнопку, иначе текст с диапазоном рисунков.
    /// </summary>
    private void UpdatePictureCell(int rowIndex, ExecutionWork ew)
    {
        int pictureColumnIndex = _gridColumns.PictureName.Index;
        DataGridViewCell newCell;

        if (ew.ImageList == null || !ew.ImageList.Any())
        {
            newCell = CreateAddImageButtonCell();
        }
        else
        {
            var images = ew.ImageList ?? new List<ImageOwner>();

            newCell = new DataGridViewTextBoxCell
            {
                Value = FormatImageReferences(images)
            };
        }

        dgvMain.Rows[rowIndex].Cells[pictureColumnIndex] = newCell;
        dgvMain.InvalidateCell(pictureColumnIndex, rowIndex);
    }
    /// <summary>
    /// Преобразует список чисел в компактный текст, например [1,4,5,6,8] ⇒ "Рис. 1, Рис. 4–6, Рис. 8"
    /// </summary>
    private string FormatImageReferences(List<ImageOwner> images)
    {
        // Группируем изображения по типу
        var groups = images
            .Where(img => img.Number > 0)
            .GroupBy(img => img.Role)
            .OrderBy(g => g.Key); // Сортируем по типу для консистентности

        var resultParts = new List<string>();

        foreach (var group in groups)
        {
            // Получаем префикс для типа группы
            string prefix = group.Key == ImageRole.ExecutionScheme ? "СИ" : "Рис";

            // Для группы получаем отсортированные номера
            var numbers = group.Select(img => img.Number)
                              .Distinct()
                              .OrderBy(n => n)
                              .ToList();

            // Формируем диапазоны без префикса
            var ranges = new List<string>();
            int? start = 0, end = 0;

            foreach (var n in numbers)
            {
                if (start == 0)
                {
                    start = end = n;
                }
                else if (n == end + 1)
                {
                    end = n;
                }
                else
                {
                    ranges.Add(RangeToString(start.Value, end.Value));
                    start = end = n;
                }
            }

            if (start != 0)
            {
                ranges.Add(RangeToString(start.Value, end.Value));
            }

            // Объединяем диапазоны и добавляем префикс только один раз
            if (ranges.Any())
            {
                resultParts.Add($"{prefix} {string.Join(", ", ranges)}");
            }
        }

        return string.Join("; ", resultParts);
    }

    /// <summary>
    ///  На основе начального и конечного значения возвращает либо строку в ввиде диапазона начало-конец, либо одиночное значение начала, если оба числа совпадают
    /// </summary>
    /// <param name="startValue">Начальное значение диапазона</param>
    /// <param name="endValue">Конечное значение диапазона</param>
    /// <returns></returns>
    private string RangeToString(int startValue, int endValue)
    {
        return startValue == endValue ? $"{startValue}" : $"{startValue}–{endValue}";
    }

    private Dictionary<int, bool> _toReleased = new();
    private Dictionary<int, string> _tcArticleById = new();

    /// <summary>
    /// Добавляет строки в DataGridView на основе подготовленного списка TechOperationDataGridItem.
    /// </summary>
    private void AddRowsToGrid()
    {
        int total = TechOperationDataGridItems.Count;
        if (total == 0) return;

        int nextReport = 10; // %
        int reportStep = 10;
        int i = 0;
        var sw = Stopwatch.StartNew();
        var swrows = Stopwatch.StartNew();

        var swDataPrepare = Stopwatch.StartNew();
        var toIds = TechOperationDataGridItems.Select(i => i.IdTO).Distinct().ToList();
        _toReleased = context.Set<TechOperation>()
            .AsNoTracking()
            .Where(to => toIds.Contains(to.Id))
            .Select(to => new { to.Id, to.IsReleased })
            .ToDictionary(x => x.Id, x => x.IsReleased);

        var repeatTcIds = TechOperationDataGridItems
            .Select(i => (i.WorkItem as ExecutionWork)?.RepeatsTCId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        _tcArticleById = context.TechnologicalCards
            .AsNoTracking()
            .Where(tc => repeatTcIds.Contains(tc.Id))
            .Select(tc => new { tc.Id, tc.Article })
            .ToDictionary(x => x.Id, x => x.Article);
        swDataPrepare.Stop();
        _logger.Debug("Подготовка данных для добавления строк заняла {ms} мс. TOs={Count}, TC Articles={TCCount}",
                            swDataPrepare.ElapsedMilliseconds, toIds.Count, _tcArticleById.Count);

        var rows = new List<DataGridViewRow>(TechOperationDataGridItems.Count);
        foreach (var item in TechOperationDataGridItems)
        {
            i++;

            Color? c1, c2, c3;
            DetectCellColors(item, out c1, out c2, out c3);

            // Собираем строку (без вставки в грид)
            var row = BuildRow(item, c1, c2, c3);
            rows.Add(row);

            if (total >= 40)
            {
                int percent = (int)((long)i * 100 / total);
                if (percent >= nextReport)
                {

                    _logger.Debug("Добавление строк: {Percent}% ({Done}/{Total}) заняло {ms}", percent, i, total,
                                    swrows.ElapsedMilliseconds);
                    nextReport += reportStep;
                    swrows.Restart();
                }
            }
        }

        dgvMain.SuspendLayout();
        try
        {
            // Пакетное добавление
            dgvMain.Rows.AddRange(rows.ToArray());
        }
        finally
        {
            dgvMain.ResumeLayout();
        }

        swrows.Stop();
        sw.Stop();
        _logger.Information("Добавление всех строк завершено за {ms} мс. Всего: {Total}",
                            sw.ElapsedMilliseconds, total);

        void DetectCellColors(TechOperationDataGridItem item, out Color? c1, out Color? c2, out Color? c3)
        {
            // --- логика выбора цветов = как в AddRowToDataGrid(...) ---
            //var itemTo = context.Set<TechOperation>().FirstOrDefault(to => to.Id == item.IdTO);
            bool itemToIsReleased = _toReleased.TryGetValue(item.IdTO, out var rel) ? rel : true;

            c1 = null;
            c2 = null;
            c3 = null;
            if (item.WorkItem is ExecutionWork ew && ew.Repeat)
            {
                c1 = ew.RepeatsTCId == null ? Color.Yellow : Color.Khaki;
                c2 = c1;
                c3 = (!itemToIsReleased) ? Color.Pink : Color.Empty;
            }
            else if (item.ItsTool || item.ItsComponent)
            {
                c1 = item.ItsComponent ? Color.Salmon : Color.Aquamarine;
                c2 = c1;
            }
            else if (!itemToIsReleased && item.executionWorkItem != null && !item.executionWorkItem.techTransition.IsReleased)
            {
                c2 = Color.Pink;
                c3 = Color.Pink;
            }
            else if (!itemToIsReleased)
            {
                c3 = Color.Pink;
            }
            else if (item.executionWorkItem != null && !item.executionWorkItem.techTransition.IsReleased)
            {
                c2 = Color.Pink;
            }
        }
    }

    private DataGridViewRow BuildRow(
    TechOperationDataGridItem item,
    Color? backColor1 = null,
    Color? backColor2 = null,
    Color? backColor3 = null)
    {
        var rowData = new List<object>();

        rowData.Add(item.WorkItem ?? string.Empty);

        if (item.WorkItem is ExecutionWork ew && ew.Repeat)
        {
            var repeatNumList = ew.ExecutionWorkRepeats.Select(r => r.ChildExecutionWork.RowOrder).ToList();
            string strP = repeatNumList.Count != 0 ? ConvertListToRangeString(repeatNumList) : "(нет данных)";

            rowData.Add(item.Nomer.ToString());
            rowData.Add(item.TechOperation);
            rowData.Add(item.Staff);
            if (ew.RepeatsTCId != null)
            {
                _tcArticleById.TryGetValue(ew.RepeatsTCId.Value, out var article);
                rowData.Add(article != null
                    ? $"Выполнить в соответствии с {article} п.{strP}"
                    : $"Выполнить по ТК ID:{ew.RepeatsTCId} п.{strP}");
            }
            else
            {
                rowData.Add("Повторить п." + strP);
            }

            rowData.Add(item.TechTransitionValue);
            rowData.Add(item.TimeEtap);
        }
        else
        {
            rowData.Add(item.Nomer != -1 ? item.Nomer.ToString() : "");
            rowData.Add(item.TechOperation);
            rowData.Add(item.Staff);
            rowData.Add(item.TechTransition);
            rowData.Add(item.TechTransitionValue);
            rowData.Add(item.TimeEtap);
        }

        // «машинные» столбцы
        AddMachineColumns(item, rowData);

        // остальные колонки
        rowData.Add(item.Protections);
        rowData.Add(item.Comments);
        rowData.Add(string.Empty); // PictureNameColumn — заполним ниже
        rowData.Add(item.Vopros);
        rowData.Add(item.Otvet);

        // Создаём саму строку
        var newRow = new DataGridViewRow();
        newRow.CreateCells(dgvMain, rowData.ToArray());

        // Картинки / кнопка «Добавить изображение»
        int picCol = dgvMain.Columns["PictureNameColumn"].Index;
        if (newRow.Cells[0].Value is ExecutionWork exWor)
        {
            if (_tcViewState.IsViewMode)
            {
                var images = exWor.ImageList ?? new List<ImageOwner>();
                newRow.Cells[picCol].Value = images.Any() ? FormatImageReferences(images) : string.Empty;
            }
            else
            {
                if (exWor.ImageList == null || exWor.ImageList.Count == 0)
                    newRow.Cells[picCol] = CreateAddImageButtonCell();
                else
                {
                    var images = exWor.ImageList ?? new List<ImageOwner>();
                    newRow.Cells[picCol].Value = FormatImageReferences(images);
                }
            }
        }
        else
        {
            newRow.Cells[picCol].Value = string.Empty;
        }

        // Цвета (как в ApplyCellColors)
        ApplyCellColors(newRow, backColor1, backColor2, backColor3);

        return newRow;
    }

    /// <summary>
    /// Добавляет данные о машинах в строки DataGridView на основе TechOperationDataGridItem.
    /// </summary>
    /// <param name="techOperationDataGridItem">Объект TechOperationDataGridItem, содержащий данные о текущей технологической операции.</param>
    /// <param name="str">Список объектов, представляющий строку данных для добавления в DataGridView.</param>
    private void AddMachineColumns(TechOperationDataGridItem techOperationDataGridItem, List<object> str)
    {
        techOperationDataGridItem.listMachStr = new List<string>();///////////////////////

        for (var index = 0; index < TehCarta.Machine_TCs.Count; index++)
        {
            if (techOperationDataGridItem.listMachStr.Count == 0 && techOperationDataGridItem.listMach.Count > 0)
            {
                bool isMachineChecked = techOperationDataGridItem.listMach[index];
				string value;
                value = isMachineChecked ? techOperationDataGridItem.TechTransitionValue : "";
                str.Add(value);
            }
            else
            {
                //Получаем прошлую строку для сравнения, нужно ли объединение
                // todo: оптимизировать, чтобы не искать каждый раз. можно передать в метод TechOperationDataGridItem
                var prevStr = TechOperationDataGridItems
                    .Where(t => t.Nomer == techOperationDataGridItem.Nomer - 1).FirstOrDefault();
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
		if (color1.HasValue) // Staff
			row.Cells[3].Style.BackColor = color1.Value;
		if (color2.HasValue) // ТП
			row.Cells[4].Style.BackColor = color2.Value;
		if (color3.HasValue) // TO
			row.Cells[2].Style.BackColor = color3.Value;
	}

	private void DgvMain_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            // Получение имени столбца по индексу
            string columnName = dgvMain.Columns[e.ColumnIndex].HeaderText;

            // Проверка имени столбца
            if (columnName == "Время действ., мин.")
            {
                if (dgvMain.Rows[e.RowIndex].Cells[0].Value is ExecutionWork executionWork)
                {
                    if (!string.IsNullOrEmpty(executionWork.Coefficient) && executionWork.techTransition != null)
                        dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = executionWork.techTransition.TimeExecution + executionWork.Coefficient;
                }
            }
        }
    }

    private void DgvMain_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        var col = dgvMain.Columns[e.ColumnIndex];
        // Первую строку всегда показывать
        //if (e.RowIndex < 0) // todo: Исправли с == 0  на  < 0 т.е мешает применению стиля к первой строке. Пока не поянятно, зачем была созданна.
        //    return;

        if (col == _gridColumns.TechOperationName) // "Технологические операции"
        {
            // Первая видимая строка ВСЕГДА показывает текст
            if (!IsFirstVisibleRow(e.RowIndex) && IsSameAsPrev(e.ColumnIndex, e.RowIndex))
            {
                e.Value = string.Empty;
                e.FormattingApplied = true;
            }
        }

        // Если это столбцов с временем этапа и механизмов (индекс >= 6), и значение ячейки равно "-1",
        // то ячейка остается пустой.
        if (col.IsRole(ColumnRole.TimeOfMechanism))
        {
            if ((e.Value as string) == "-1")
            {
                e.Value = string.Empty;
                e.FormattingApplied = true;
            }
        }

        var row = dgvMain.Rows[e.RowIndex];
        var cell = row.Cells[e.ColumnIndex];
        // Если мы находимся в режиме редактирования ТК (_tcViewState.IsViewMode == false),
        // проверяем возможность редактирования ячейки.
        if (!_tcViewState.IsViewMode)
        {
            if (row.Cells[0].Value is ExecutionWork executionWork)
            {
                if (col.IsRole(ColumnRole.Remarks)
                    && DataProcessing.AuthorizationService.CurrentUser.UserRole() 
                    == DataProcessing.AuthorizationService.User.Role.Lead)
                {
                    CellChangeReadOnly(cell, false);
                }
                else if (col == _gridColumns.Response
                    && TC_WinForms.DataProcessing.AuthorizationService.CurrentUser.UserRole() 
                    == DataProcessing.AuthorizationService.User.Role.Implementer)
                {
                    CellChangeReadOnly(cell, false);
                }
                else if (col == _gridColumns.Comment)
                {
                    CellChangeReadOnly(cell, false);
                }
                else if (col == _gridColumns.PictureName)
                {
                    cell.ReadOnly = true;
                    var currentColor = cell.Style.BackColor;
                    if (currentColor == Color.White || currentColor == Color.LightGray || currentColor.IsEmpty)
                    {
                        cell.Style.BackColor = _tcViewState.IsViewMode ? Color.White : Color.LightGray;
                    }
                }
                else if(executionWork.techTransition.IsRepeatTypeTransition()
					&& (col == _gridColumns.TechTransitionName || col == _gridColumns.Staff))
                {
                    // пропускаю действие, т.к. отрисовка цвета уже произошла
                    //SetCellBackColor(dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex], Color.Yellow);
                }
                else if (executionWork?.techTransition != null && executionWork.techOperationWork != null && executionWork.techOperationWork?.techOperation != null
                        && !executionWork.techTransition.IsRepeatAsInTcTransition()
                        && (!executionWork.techTransition.IsReleased || !executionWork.techOperationWork.techOperation.IsReleased)
                        && (col == _gridColumns.TechTransitionName || col == _gridColumns.TechOperationName))
                {
                    // пропускаю действие, т.к. отрисовка цвета уже произошла
                }
                else
                {
                    CellChangeReadOnly(cell, true);
                }
            }
            else if (TechOperationDataGridItems[e.RowIndex].ItsTool || TechOperationDataGridItems[e.RowIndex].ItsComponent)
            {
                if(col == _gridColumns.Comment)
                {
                    CellChangeReadOnly(cell, false);
                }
                else if (col == _gridColumns.PictureName)
                {
                    CellChangeReadOnly(cell, true);
                }
                else if (col.IsRole(ColumnRole.Remarks))
                {
                    CellChangeReadOnly(cell, false);
                }
            }
            else if(!TechOperationDataGridItems[e.RowIndex].ItsTool
                && !TechOperationDataGridItems[e.RowIndex].ItsComponent
                && TechOperationDataGridItems[e.RowIndex].WorkItem == null) // todo: проверить, можно ли получить значение из techOperationDataGridItem из строки
            {
                if (col == _gridColumns.Comment)
                {
                    CellChangeReadOnly(cell, true);
                }
            }

            if (e.RowIndex >= 0 && col == _gridColumns.Remark)
            {
                if (row.Cells[0].Value is IRamarkable remarkItem)
                {
                    if (remarkItem.IsRemarkClosed)
                    {
                        e.Value = "Замечание закрыто"; // Текст, который увидит пользователь
                        e.CellStyle.ForeColor = Color.Gray; // Серый цвет текста
                    }
                    else
                    {
                        e.CellStyle.ForeColor = dgvMain.DefaultCellStyle.ForeColor; // Возвращаем стандартный цвет
                    }
                }
            }
        }
    }

    public void CellChangeReadOnly(DataGridViewCell cell, bool isReadOnly)
    {
        // Делаем ячейку редактируемой
        cell.ReadOnly = isReadOnly;
		// Проверяем, нет ли уже «особого» цвета, который нужно сохранить.
		// Пусть считаем особым любой цвет, кроме белого и LightGray.
		var currentColor = cell.Style.BackColor;

		// Если сейчас цвет белый или LightGray, то можно менять;
		// иначе оставляем «как есть».
		if (currentColor == Color.White || currentColor == Color.LightGray || currentColor.IsEmpty)
		{
			cell.Style.BackColor = isReadOnly ? Color.White : Color.LightGray;
		}
	}

    private void DgvMain_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.AdvancedBorderStyle == null) return;
        
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

        if (e.ColumnIndex == 2) // Технологические операции
        {
            bool merged = !IsFirstVisibleRow(e.RowIndex) && IsSameAsPrev(e.ColumnIndex, e.RowIndex);
            e.AdvancedBorderStyle.Top = merged
                ? DataGridViewAdvancedCellBorderStyle.None
                : DataGridViewAdvancedCellBorderStyle.Single;
        }
        else
        {
            e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.Single;
        }

        if (e.ColumnIndex >= 6)
        {
            var bb = e.Value?.ToString() ?? string.Empty;
            if (bb == "-1")
            {
                e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            }
        }

    }

    /// <summary>
    /// Проверяет, является ли строка первой видимой строкой в DataGridView.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    bool IsFirstVisibleRow(int row)
    {
        int first = dgvMain.FirstDisplayedScrollingRowIndex; // -1, если пусто
        return first >= 0 && row == first;
    }

    /// <summary>
    /// Определяет, является ли значение в ячейке текущей строки и столбца таким же, как в предыдущей строке.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    bool IsSameAsPrev(int column, int row)
    {
        if (row <= 0) return false;

        var v1 = dgvMain[column, row].Value?.ToString();
        var v0 = dgvMain[column, row - 1].Value?.ToString();
        if (v1 == null || v0 == null) return false;

        return string.Equals(v1, v0, StringComparison.Ordinal);
    }

    /// <summary>
    /// Этот метод отслеживает время отрисорки DataGridView
    /// </summary>
    private void DgvMain_Paint(object? sender, PaintEventArgs e)
    {
        if (_awaitingFirstPaint)
        {
            _awaitingFirstPaint = false;
            _paintSw.Stop();
            _logger.Information(
                "Первая отрисовка таблицы завершена за {ElapsedMs} мс. Rows={Rows}, Cols={Cols}.",
                _paintSw.ElapsedMilliseconds, dgvMain.Rows.Count, dgvMain.Columns.Count);
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

            foreach (TechTransitionTypical item in TechOperat.techTransitionTypicals.OrderBy(o => o.Order))
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
        int? insertIndex = null, string? coefficientValue = null, string? comment = null, string? pictureName = null,
        int repeatTcId = 0, List<ExecutionWorkRepeat> executionWorkRepeats = null)
	{
		_logger.Information("Добавление нового ExecutionWork в TO '{TechOpName}' (ID={TechOpId}) на позицию {Index}. " +
					  "Transition='{TransitionName}' (ID={TransitionId}).",
			techOperationWork.techOperation.Name,
			techOperationWork.techOperation.Id,
			insertIndex ?? -1,
			tech.Name,
			tech.Id
		);

		// Определяем порядковый номер нового ТП в ТО
		TechOperationWork TOWork = TechOperationWorksList.Single(s => s == techOperationWork);

		var currentEwInTow = TOWork.executionWorks.Where(ew => !ew.Delete);
		var lastEwInTow = currentEwInTow.OrderBy(e => e.Order).LastOrDefault();
		int? newEwOrderInTo = null;

		var rowOrder = lastEwInTow?.RowOrder + 1;

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

		int lastEwOrder = lastEwInTow?.Order ?? 0;
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
			Repeat = false
		};

        TOWork.executionWorks.Add(newEw);
        context.ExecutionWorks.Add(newEw);

        if (tech.IsRepeatTypeTransition())
        {
            newEw.Repeat = true;
			if (tech.IsRepeatAsInTcTransition())
				newEw.RepeatsTCId = repeatTcId;
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

        if (executionWorkRepeats != null)
        {
			newEw.Value = CalculateExecutionWorksPovtor(executionWorkRepeats);
        }

        if (techTransitionTypical != null)
        {
            newEw.Etap = techTransitionTypical.Etap;
            newEw.Posled = techTransitionTypical.Posled;
            newEw.Coefficient = techTransitionTypical.Coefficient;
            newEw.Comments = techTransitionTypical.Comments ?? "";
            newEw.Order = techTransitionTypical.Order;

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

    private void button1_Click_1(object sender, EventArgs e) //  Перенести в метод SaveChanges
	{
		_logger.Information("Cохранение изменений Ход работ.");

		DbConnector dbCon = new DbConnector();

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
            _logger.Information("Данные успешно сохранены в БД (TechOperationForm).");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Ошибка при сохранении данных Ход работ.");
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
		_logger.Information("Форма TechOperationForm закрывается. Запрошено закрытие со статусом: {CloseReason}", e.CloseReason);
		_editForm?.Close();
    }


    public void SelectCurrentRow(TechOperationWork work, ExecutionWork? executionWork = null)
    {
        if (work == null)
            return;

        TechOperationDataGridItem? dgvItem = TechOperationDataGridItems.FirstOrDefault(t => (executionWork != null && t.techWork == executionWork) 
                                                                                            || (executionWork == null && t.TechOperationWork == work));

        if (dgvItem != null)
        {
			int index = TechOperationDataGridItems.IndexOf(dgvItem);
			if (index >= 0 && index < dgvMain.Rows.Count)
			{
				dgvMain.ClearSelection();
				dgvMain.Rows[index].Selected = true;
				dgvMain.FirstDisplayedScrollingRowIndex = index;
            }
        }
    }

	public void OnActivate()
	{
        if (_tcViewState.TechnologicalCard.IsDynamic 
            && _editForm != null && !_editForm.IsDisposed)
		    _editForm.OnActivate();
	}

	/// <summary>
	/// Добавляет новую строку в dgvMain по указанному индексу.
	/// </summary>
	/// <param name="rowIndex">Индекс, по которому следует вставить строку.</param>
	/// <param name="values">Массив значений для новой строки.</param>
	public void AddRowByIndex(int rowIndex, object[] values) // todo: проверить, используется ли этот метод?
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

    private void EditImageData()
    {
        if (_tcViewState.IsViewMode) return;

        GetSelectedDataInfo(out List<int> selectedRowIndices, out CopyScopeEnum? selectedScope);

        if (selectedScope == null) return;

        var selectedItems = selectedRowIndices.Select(i => TechOperationDataGridItems[i]).ToList();
        var selectedItem = selectedItems[0];

        if (selectedItem == null) return;

        switch (selectedScope)
        {
            case CopyScopeEnum.ImageData:
                if(selectedItem.WorkItem is ExecutionWork ew)
                {
                    var editor = new Win6_ImageEditor(ew, _tcViewState, context);
                    editor.AfterSave = async (savedObj) =>
                    {
                        RefreshPictureNameColumn();
                    };

                    editor.Show();
                }
                break;
            default:
                break;
        }
    }

    private void OpenEditFormBySelectedObject()
	{
		if (_tcViewState.IsViewMode) return;

		GetSelectedDataInfo(out List<int> selectedRowIndices, out CopyScopeEnum? selectedScope);

		if (selectedScope == null) return;

		var selectedItems = selectedRowIndices.Select(i => TechOperationDataGridItems[i]).ToList();
		var selectedItem = selectedItems[0];

		if (selectedItem == null) return;

		if (_editForm == null || _editForm.IsDisposed)
		{
			_editForm = new AddEditTechOperationForm(this, _tcViewState);
		}

		_editForm.Enabled = false;
		_editForm.Show();
		_editForm.BringToFront(); // не выделяется необходимая строка при 

		switch (selectedScope)
		{
			case CopyScopeEnum.TechOperation:
				_ = SetOperationAndOpenPage(selectedItem, "tabPageTO");
				break;
			case CopyScopeEnum.Row:
				if(!SetTransitionAndOpenPage(selectedItem, "tabPageTP"))
				{
					_ = SetOperationAndOpenPage(selectedItem, "tabPageTO");
				};
				break;
			case CopyScopeEnum.TechTransition:
				_ = SetTransitionAndOpenPage(selectedItem, "tabPageTP");
				break;
			case CopyScopeEnum.Staff:
				_ = SetTransitionAndOpenPage(selectedItem, "tabPageStaff");				
				break;
			case CopyScopeEnum.Protections:
				_ = SetTransitionAndOpenPage(selectedItem, "tabPageProtection");
				break;
			case CopyScopeEnum.ToolOrComponents:
				var isTool = selectedItem.WorkItemType == WorkItemType.ToolWork;
				_ = SetOperationAndOpenPage(selectedItem, selectedItem.ItsTool ? "tabPageTool" : "tabPageComponent");
				if (selectedItem.WorkItemType == WorkItemType.ComponentWork && selectedItem.WorkItem is ComponentWork compWork)
					_editForm.HighlighComponentLocalRow(compWork);
				else if (selectedItem.WorkItemType == WorkItemType.ToolWork && selectedItem.WorkItem is ToolWork toolWork)
					_editForm.HighlightInstrumentLocalRow(toolWork);
				break;
			default:
				break;
		}
		_editForm.Enabled = true;
		_editForm.Activate();

		bool SetTransitionAndOpenPage(TechOperationDataGridItem selectedItem, string pageName)
		{
			if (_editForm == null || _editForm.IsDisposed) throw new Exception();

			if (selectedItem.WorkItemType == WorkItemType.ExecutionWork
								&& selectedItem.WorkItem is ExecutionWork selectedTPRow)
			{
				_editForm.SelectPageTab(pageName);
                _editForm.SelectTP(selectedTPRow);

                return true;
			}

			return false;
		}

		bool SetOperationAndOpenPage(TechOperationDataGridItem selectedItem, string pageName)
		{
			if (_editForm == null || _editForm.IsDisposed) throw new Exception();

			if (selectedItem.TechOperationWork != null)
			{
				_editForm.SelectTO(selectedItem.TechOperationWork);
				_editForm.SelectPageTab(pageName);

				return true;
			}

			return false;
		}
	}

    private void ChangeRamrkStatus()
    {
        var selectedCells = dgvMain.SelectedCells
            .Cast<DataGridViewCell>()
            .Distinct()
            .Select(c => c.RowIndex)
            .OrderBy(idx => idx)
            .ToList();

        var selectedItem = selectedCells.Select(i => TechOperationDataGridItems[i]).FirstOrDefault();

        if (selectedItem.WorkItem is IRamarkable remarkItem)
        {
            remarkItem.IsRemarkClosed = !remarkItem.IsRemarkClosed;
            selectedItem.IsRemarkClosed = !selectedItem.IsRemarkClosed;
        }

        dgvMain.InvalidateCell(_gridColumns.Remark.Index, TechOperationDataGridItems.IndexOf(selectedItem));
    }

    private void GoToNextRemark()
    {
        var currentIndex = dgvMain.SelectedCells
            .Cast<DataGridViewCell>()
            .Distinct()
            .Select(c => c.RowIndex)
            .OrderBy(idx => idx)
            .FirstOrDefault();

        dgvMain.ClearSelection();

        int minIndex = TechOperationDataGridItems.Where(t => !string.IsNullOrEmpty(t.Vopros) && !t.IsRemarkClosed && TechOperationDataGridItems.IndexOf(t) > currentIndex)
                                                        .OrderBy(t => TechOperationDataGridItems.IndexOf(t))
                                                        .Select(t => TechOperationDataGridItems.IndexOf(t))
                                                        .FirstOrDefault();

        if (minIndex != null && minIndex != 0)
        {
            dgvMain.Rows[minIndex].Cells[_gridColumns.Remark.Index].Selected = true;
            dgvMain.FirstDisplayedScrollingRowIndex = minIndex;
        }
        else
        {
            var firstRemarkIndex = TechOperationDataGridItems.Where(t => !string.IsNullOrEmpty(t.Vopros) && !t.IsRemarkClosed)
                                                        .OrderBy(t => TechOperationDataGridItems.IndexOf(t))
                                                        .Select(t => TechOperationDataGridItems.IndexOf(t))
                                                        .FirstOrDefault();

            dgvMain.Rows[firstRemarkIndex].Cells[_gridColumns.Remark.Index].Selected = true;
            dgvMain.FirstDisplayedScrollingRowIndex = firstRemarkIndex;
        }
    }

    private double CalculateExecutionWorksPovtor(List<ExecutionWorkRepeat> executionWorkRepeats)
	{
        double totalValue = 0;
        try
        {
            var coefDict = _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);

            foreach (var repeat in executionWorkRepeats)
            {
                if (repeat.ChildExecutionWork.Delete)
                    continue;

                var value = repeat.ChildExecutionWork.Value;
                totalValue += MathScript.EvaluateCoefficientExpression(repeat.NewCoefficient, coefDict, value.ToString());
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при пересчете значения повтора");
            string errorMessage = ex.InnerException?.Message ?? ex.Message;

            MessageBox.Show($"Ошибка при пересчете значения повтора:\n\n{errorMessage}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            totalValue = -1;
        }

        return totalValue;
    }
	
    private DataGridViewButtonCell CreateAddImageButtonCell()
    {
        return new DataGridViewButtonCell
        {
            Value = "Добавить рисунок",
            Style =
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter, //выставляет текст кнопки по центру
            }
        };
    }

    public void UpdateTimeValue()//todo: дописать код так, чтобы он пересчитывал конкретные ТП, в которых содержатся нужные коэффициенты
    {
        var coefDict = _tcViewState.TechnologicalCard.Coefficients.ToDictionary(c => c.Code, c => c.Value);
        var a = TechOperationDataGridItems.Where(e => e.WorkItem is ExecutionWork ew).ToList();

        foreach (var row in a)
        {
            var executionWork = row.WorkItem as ExecutionWork;
            var time = executionWork.techTransition.TimeExecution.ToString().Replace(",", ".");
            if(!executionWork.Repeat)
                executionWork.Value = MathScript.EvaluateCoefficientExpression(executionWork.Coefficient, coefDict, time);
            else
            {
                double totalValue = 0;
                foreach (var repeat in executionWork.ExecutionWorkRepeats)
                {
                    if (repeat.ChildExecutionWork.Delete)
                        continue;

                    var value = repeat.ChildExecutionWork.Value;

                    totalValue += MathScript.EvaluateCoefficientExpression(repeat.NewCoefficient, coefDict, value.ToString());
                }
                executionWork.Value = totalValue;
            }
        }
    }

    public int GetObjectId()
    {
        return _tcId;
    }
}
