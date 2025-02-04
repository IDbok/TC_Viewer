using Serilog;
using System.ComponentModel;
using System.Data;
using TC_WinForms.DataProcessing;
using TC_WinForms.DataProcessing.Helpers;
using TC_WinForms.DataProcessing.Utilities;
using TC_WinForms.Extensions;
using TC_WinForms.Interfaces;
using TC_WinForms.Services;
using TcModels.Models;
using TcModels.Models.Interfaces;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TcModels.Models.TechnologicalCard;

namespace TC_WinForms.WinForms;

public partial class Win7_1_TCs : Form, ILoadDataAsyncForm, IPaginationControl//, ISaveEventForm
{
	#region Fields

	private readonly ILogger _logger;
	private readonly User.Role _accessLevel;

	private readonly int _minRowHeight = 20;
	private readonly int _minComboboxWidth = 160;

	private readonly int _linesPerPage = 50;

	private DbConnector _dbCon = new DbConnector();

	private List<DisplayedTechnologicalCard> _displayedTechnologicalCards;
	private BindingList<DisplayedTechnologicalCard> _bindingList;

	//private List<DisplayedTechnologicalCard> _changedObjects = new List<DisplayedTechnologicalCard>();
	private List<DisplayedTechnologicalCard> _newObjects = new List<DisplayedTechnologicalCard>();
	//private List<DisplayedTechnologicalCard> _deletedObjects = new List<DisplayedTechnologicalCard>();
	//private DisplayedTechnologicalCard _newObject;

	public bool _isDataLoaded = false;
	//private bool isFiltered = false;

	public string setSearch => txtSearch.Text;

	PaginationControlService<DisplayedTechnologicalCard> paginationService;

	#endregion

	#region EventsAndProperties

	public event EventHandler<PageInfoEventArgs> PageInfoChanged;
	public PageInfoEventArgs? PageInfo { get; set; }
	public void RaisePageInfoChanged()
	{
		PageInfoChanged?.Invoke(this, PageInfo);
	}


	#endregion

	#region Constructor

	public Win7_1_TCs(User.Role accessLevel)
	{
		_logger = Log.Logger.ForContext<Win7_1_TCs>();
		_logger.Information("Инициализация окна Win7_1_TCs для роли {AccessLevel}", accessLevel);

		_accessLevel = accessLevel;

		InitializeComponent();
		AccessInitialization();

		// Улучшаем производительность DataGridView, уменьшаем мерцание
		dgvMain.DoubleBuffered(true);
	}


	#endregion

	#region FormLoad

	private async void Win7_1_TCs_Load(object sender, EventArgs e)
	{
		_logger.Information("Загрузка формы Win7_1_TCs");

		this.Enabled = false;
		dgvMain.Visible = false;
		progressBar.Visible = true;

		var stopwatch = System.Diagnostics.Stopwatch.StartNew();

		try
		{
			if (!_isDataLoaded)
			{
				_logger.Information("Начало загрузки данных из базы");
				await LoadDataAsync();
				_logger.Information("Данные успешно загружены");
			}
		}
		catch (Exception ex)
		{
			_logger.Error("Ошибка при загрузке данных: {ExceptionMessage}", ex.Message);
			MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
		}
		finally
		{
			stopwatch.Stop();
			_logger.Information("Данные загружены за {ElapsedMilliseconds} мс", stopwatch.ElapsedMilliseconds);

			progressBar.Visible = false;

			// Настройка DataGridView после загрузки
			SetDGVColumnsSettings();
			DisplayedEntityHelper.SetupDataGridView<DisplayedTechnologicalCard>(dgvMain);
			if (Program.IsTestMode) // В тестовом режиме показываем Id
			{
				dgvMain.Columns[nameof(DisplayedTechnologicalCard.Id)].Visible = true;
			}

			// Инициализация комбобоксов фильтра
			SetupNetworkVoltageComboBox();
			SetupTypeComboBox();
			SetupTcStatusComboBox();

			// Подписываемся на события изменения фильтров
			cbxNetworkVoltageFilter.SelectedIndexChanged += ComboBoxFilter_Changed;
			cbxTypeFilter.SelectedIndexChanged += ComboBoxFilter_Changed;
			cbxStatusFilter.SelectedIndexChanged += ComboBoxFilter_Changed;


			// Настраиваем отрисовку DataGridView
			dgvMain.RowPostPaint += dgvMain_RowPostPaint;
			dgvMain.ResizeRows(_minRowHeight);

			dgvMain.Visible = true;
			Enabled = true;
			_isDataLoaded = true;
		}

	}

	public async Task LoadDataAsync()
	{
		_logger.Information("Начало асинхронной загрузки данных для Win7_1_TCs");

		try
		{
			var allCards = await Task.Run(() => _dbCon.GetObjectList<TechnologicalCard>());

			// Для ролей ProjectManager и User — отображаем только Approved
			if (_accessLevel == User.Role.ProjectManager || _accessLevel == User.Role.User)
			{
				_displayedTechnologicalCards = allCards
					.Where(tc => tc.Status == TechnologicalCardStatus.Approved)
					.OrderBy(tc => tc.Article)
					.Select(tc => new DisplayedTechnologicalCard(tc))
					.ToList();
					//await Task.Run(() => allCards
					//.Where(tc => tc.Status == TechnologicalCardStatus.Approved)
					//.OrderBy(tc => tc.Article)
					//.Select(tc => new DisplayedTechnologicalCard(tc))
					//.ToList()
					//);
			}
			else
			{
				_displayedTechnologicalCards = allCards
					.OrderBy(tc => tc.Article)
					.Select(tc => new DisplayedTechnologicalCard(tc))
					.ToList();
				//await Task.Run(() => _dbCon.GetObjectList<TechnologicalCard>()
				//	.Select(tc => new DisplayedTechnologicalCard(tc)).OrderBy(tc => tc.Article).ToList());
			}

			paginationService = new PaginationControlService<DisplayedTechnologicalCard>(_linesPerPage, _displayedTechnologicalCards);
			UpdateDisplayedData();
		}
		catch (Exception e)
		{
			_logger.Error("Ошибка при загрузке данных: {ExceptionMessage}", e.Message);
			MessageBox.Show("Ошибка загрузки данных: " + e.Message);
		}


	}

	#endregion

	#region AccessControl

	private void AccessInitialization()
	{
		var controlAccess = new Dictionary<User.Role, Action>
		{
			[User.Role.Lead] = () => { },

			[User.Role.Implementer] = () => SetupImplementerLimitedAccess(),

			[User.Role.ProjectManager] = () => SetupLimitedAccess(),

			[User.Role.User] = () => SetupLimitedAccess(),
		};

		if (controlAccess.TryGetValue(_accessLevel, out var action))
		{
			action?.Invoke();
		}
	}

	private void SetupLimitedAccess()
	{
		SetupImplementerLimitedAccess();

		// скрыть фильтры по статусу
		pnlFilterAdditional.Visible = false;
		//cbxStatusFilter.Visible = false;
		//lblStatusFilter.Visible = false;

		// уменьшить высоту панели фильтров
		//pnlControls.Height -= 44;
	}

	private void SetupImplementerLimitedAccess()
	{
		// Оставляем видимой только кнопку просмотра
		HideAllButtonsExcept(new List<System.Windows.Forms.Button> { btnViewMode });

		// Смещаем кнопку
		btnViewMode.Location = btnDeleteTC.Location;
	}
	private void HideAllButtonsExcept(List<System.Windows.Forms.Button> visibleButtons)
	{
		foreach (var button in pnlControlBtns.Controls.OfType<System.Windows.Forms.Button>())
		{
			button.Visible = visibleButtons.Contains(button);
		}
	}

	#endregion

	#region UIEventHandlers

	private void Win7_1_TCs_SizeChanged(object sender, EventArgs e)
	{
		dgvMain.ResizeRows(_minRowHeight);
	}

	private void txtSearch_TextChanged(object sender, EventArgs e)
	{
		FilterTechnologicalCards();
	}

	private void ComboBoxFilter_Changed(object sender, EventArgs e)
	{
		FilterTechnologicalCards();

		if (sender is ComboBox comboBox)
		{
			// Изменяем ширину самого ComboBox под размер выбранного текста (в разумных пределах)
			var selectedValue = comboBox.SelectedItem;
			var selectedText = selectedValue?.ToString();

			if (selectedValue is KeyValuePair<object, string> pair)
			{
				selectedText = pair.Value;
			}

			var maxWidth = 220;
			var minWidth = _minComboboxWidth;
			var width = TextRenderer.MeasureText(selectedText, comboBox.Font).Width + 20;

			comboBox.Width = width < minWidth 
				? minWidth
				: width < maxWidth 
					? width 
					: maxWidth;
		}
	}

	private void dgvMain_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
	{
		var row = dgvMain.Rows[e.RowIndex];

		if (row.DataBoundItem is not DisplayedTechnologicalCard displayedCard)
			return;

		if (_accessLevel != User.Role.User && _accessLevel != User.Role.ProjectManager)
		{
			row.HeaderCell.Style.BackColor = displayedCard.Status switch
			{
				TechnologicalCardStatus.Created => Color.LightGray,
				TechnologicalCardStatus.Draft => Color.Yellow,
				TechnologicalCardStatus.Remarked => Color.Orange,
				TechnologicalCardStatus.Approved => Color.LightGreen,
				TechnologicalCardStatus.Rejected => Color.LightCoral,
				TechnologicalCardStatus.Completed => Color.SteelBlue,
				_ => row.HeaderCell.Style.BackColor
			};
		}

		// Рисуем заново заголовок строки, чтобы применить стиль
		var rect = e.RowBounds;
		var gridBrush = new SolidBrush(dgvMain.GridColor);
		var backColorBrush = new SolidBrush(row.HeaderCell.Style.BackColor);
		var foreColorBrush = new SolidBrush(row.HeaderCell.Style.ForeColor);

		e.Graphics.FillRectangle(backColorBrush, rect.Left, rect.Top, dgvMain.RowHeadersWidth, rect.Height);
		e.Graphics.DrawString((e.RowIndex + 1).ToString(),
			e.InheritedRowStyle.Font,
			foreColorBrush,
			rect.Left + 5,
			rect.Top + ((rect.Height - e.InheritedRowStyle.Font.Height) / 2));
	}

	#endregion

	#region CRUD_Buttons

	private void btnViewMode_Click(object sender, EventArgs e)
	{
		if (dgvMain.SelectedRows.Count != 1)
		{
			MessageBox.Show("Выберите одну карту.");
			return;
		}

		var selectedRow = dgvMain.SelectedRows[0];

		if (selectedRow.Cells["Id"].Value is not int id)
		{
			MessageBox.Show("Неверный идентификатор.");
			return;
		}

		var openedForm = CheckOpenFormService.FindOpenedForm<Win6_new>(id);
		if (openedForm != null)
		{
			openedForm.BringToFront();
			return;
		}

		if (id == 0)
		{
			MessageBox.Show("Карта ещё не добавлена в БД.");
			return;
		}

		var win6 = new Win6_new(id, role: _accessLevel, viewMode: true);
		win6.Show();
	}

	private void btnCreateTC_Click(object sender, EventArgs e)
	{
		var objEditor = new Win7_1_TCs_Window(role: _accessLevel);
		objEditor.AfterSave = (createObj) => { AddObjectInDataGridView(createObj); return Task.CompletedTask; };
		objEditor.Show();
	}

	private void btnUpdateTC_Click(object sender, EventArgs e)
	{
		if (_newObjects.Count != 0)
		{
			MessageBox.Show("Сохраните добавленные карты!");
			return;
		}

		UpdateSelected();
	}

	private async void btnDeleteTC_Click(object sender, EventArgs e)
	{
		await DisplayedEntityHelper.DeleteSelectedObject<DisplayedTechnologicalCard, TechnologicalCard>(
			dgvMain,
			_bindingList, 
			_displayedTechnologicalCards
		);
	}

	#endregion

	#region Pagination

	public void GoToNextPage()
	{
		paginationService.GoToNextPage();
		UpdateDisplayedData();
	}

	public void GoToPreviousPage()
	{
		paginationService.GoToPreviousPage();
		UpdateDisplayedData();
	}

	private void UpdateDisplayedData()
	{
		var pageData = paginationService.GetPageData();
		if (pageData != null)
		{
			_bindingList = new BindingList<DisplayedTechnologicalCard>(pageData);
			dgvMain.DataSource = _bindingList;
			dgvMain.ResizeRows(_minRowHeight);

			PageInfo = paginationService.GetPageInfo();
			RaisePageInfoChanged();
		}
		else
		{
			_logger.Warning("Не удалось получить данные для отображения на странице");
			_bindingList = new BindingList<DisplayedTechnologicalCard>();
			dgvMain.DataSource = _bindingList;
		}
	}

	#endregion

	#region Filtering

	private void FilterTechnologicalCards()
	{
		if (!_isDataLoaded) return;

		try
		{
			var filteredList = ApplyFilters();
			paginationService.SetAllObjectList(filteredList);
			UpdateDisplayedData();
		}
		catch (Exception e)
		{
			_logger.Error("Ошибка фильтрации: {ExceptionMessage}", e.Message);
		}
	}

	private List<DisplayedTechnologicalCard> ApplyFilters()
	{
		var searchText = string.IsNullOrWhiteSpace(txtSearch.Text) || txtSearch.Text == "Поиск" 
			? null 
			: txtSearch.Text;

		var networkVoltageFilter = cbxNetworkVoltageFilter.SelectedItem?.ToString();
		var typeFilter = cbxTypeFilter.SelectedItem?.ToString();

		TechnologicalCardStatus? status = null;
		if (cbxStatusFilter.SelectedItem is KeyValuePair<object, string> pair && !EnumExtensions.IsAllKey(pair.Key))
		{
			status = (TechnologicalCardStatus)pair.Key;
		}

		return _displayedTechnologicalCards.Where(card =>
			(searchText == null 
				|| (card.Article?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) 
				|| (card.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) 
				|| (card.TechnologicalProcessType?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) 
				|| (card.TechnologicalProcessName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) 
				|| (card.Parameter?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) 
				|| (card.FinalProduct?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) 
				|| (card.Applicability?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) 
				|| (card.Note?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)) &&
			(networkVoltageFilter == "Все" || card.NetworkVoltage.ToString() == networkVoltageFilter) &&
			(typeFilter == "Все" || card.Type == typeFilter) &&
			(status == null || card.Status == status)
		).ToList();
	}
	#endregion

	#region UpdateOrAddHelpers

	private void UpdateSelected()
	{
		if (dgvMain.SelectedRows.Count != 1)
		{
			MessageBox.Show("Выберите одну карту для редактирования.");
			return;
		}

		var selectedRow = dgvMain.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
		if (selectedRow?.Cells["Id"].Value is not int objId)
		{
			MessageBox.Show("Неверный идентификатор.");
			return;
		}

		if (objId == 0)
		{
			MessageBox.Show("Карта ещё не добавлена в БД.");
			return;
		}

		var openedForm = CheckOpenFormService.FindOpenedForm<Win7_1_TCs_Window>(objId);
		if (openedForm != null)
		{
			openedForm.BringToFront();
			return;
		}

		var objEditor = new Win7_1_TCs_Window(objId, role: _accessLevel);
		objEditor.AfterSave = (updatedObj) => { UpdateObjectInDataGridView(updatedObj); return Task.CompletedTask; };
		objEditor.Show();
	}

	public void UpdateObjectInDataGridView(TechnologicalCard modelObject)
	{
		UpdateOrAddObjectInGrid(new DisplayedTechnologicalCard(modelObject));
	}

	public void AddObjectInDataGridView(TechnologicalCard modelObject)
	{
		UpdateOrAddObjectInGrid(new DisplayedTechnologicalCard(modelObject));
	}

	private void UpdateOrAddObjectInGrid(DisplayedTechnologicalCard newCard)
	{
		var existingCard = _displayedTechnologicalCards.FirstOrDefault(obj => obj.Id == newCard.Id);
		if (existingCard != null)
		{
			UpdateDisplayedObject(existingCard, newCard);
		}
		else
		{
			_displayedTechnologicalCards.Insert(0, newCard);
		}
		FilterTechnologicalCards();
	}

	private void UpdateDisplayedObject(DisplayedTechnologicalCard target, DisplayedTechnologicalCard source)
	{
		// Оставляем Id, меняем остальные поля
		target.Article = source.Article;
		target.Version = source.Version;
		target.Name = source.Name;
		target.Type = source.Type;
		target.NetworkVoltage = source.NetworkVoltage;
		target.TechnologicalProcessType = source.TechnologicalProcessType;
		target.TechnologicalProcessName = source.TechnologicalProcessName;
		target.TechnologicalProcessNumber = source.TechnologicalProcessNumber;
		target.Parameter = source.Parameter;
		target.FinalProduct = source.FinalProduct;
		target.Applicability = source.Applicability;
		target.Note = source.Note;
		target.DamageType = source.DamageType;
		target.RepairType = source.RepairType;
		target.IsCompleted = source.IsCompleted;
		target.Status = source.Status;
		target.Description = source.Description;
	}

	#endregion

	#region DgvSettings

	void SetDGVColumnsSettings()
	{
		// Автоподбор ширины столбцов
		dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		dgvMain.RowHeadersWidth = 25;
		dgvMain.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

		var autosizeColumn = new List<string>
		{
			nameof(DisplayedTechnologicalCard.Article),
			nameof(DisplayedTechnologicalCard.Type),
			nameof(DisplayedTechnologicalCard.NetworkVoltage),
                //nameof(DisplayedTechnologicalCard.TechnologicalProcessType),
                //nameof(DisplayedTechnologicalCard.TechnologicalProcessName),
                nameof(DisplayedTechnologicalCard.Parameter),
                //nameof(DisplayedTechnologicalCard.FinalProduct),
                // nameof(DisplayedTechnologicalCard.Applicability),
                // nameof(DisplayedTechnologicalCard.Note),
                nameof(DisplayedTechnologicalCard.IsCompleted),
			nameof(DisplayedTechnologicalCard.Id),
			nameof(DisplayedTechnologicalCard.Version),
			nameof(DisplayedTechnologicalCard.IsDynamic),
		};

		foreach (var column in autosizeColumn)
		{
			dgvMain.Columns[column].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
		}

		dgvMain.Columns[nameof(DisplayedTechnologicalCard.Status)].Width = 35;
		dgvMain.Columns[nameof(DisplayedTechnologicalCard.Status)].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
	}

	#endregion

	#region ComboBoxSetup

	private void SetupNetworkVoltageComboBox() =>
		SetupComboBox(cbxNetworkVoltageFilter, _displayedTechnologicalCards.Select(tc => tc.NetworkVoltage).Distinct());

	private void SetupTypeComboBox() =>
		SetupComboBox(cbxTypeFilter, _displayedTechnologicalCards.Select(tc => tc.Type).Distinct());

	private void SetupTcStatusComboBox()
	{
		// Получаем значения enum с пунктом "Все"
		var items = EnumExtensions.GetEnumWithAll<TechnologicalCard.TechnologicalCardStatus>();

		cbxStatusFilter.DisplayMember = "Value";
		cbxStatusFilter.ValueMember = "Key";
		cbxStatusFilter.DataSource = items;

		AdjustComboBoxWidth(cbxStatusFilter, _minComboboxWidth);
	}

	void SetupComboBox<T>(ComboBox comboBox, IEnumerable<T> source, string allText = "Все")
	{
		var items = new List<string> { allText };
		items.AddRange(source.Select(v => v?.ToString() ?? string.Empty));
		comboBox.DataSource = items;

		AdjustComboBoxWidth(comboBox, _minComboboxWidth);
	}


	private void AdjustComboBoxWidth(ComboBox comboBox, int minWidth)
	{
		// Преобразуем элементы ComboBox в строки для вычисления их ширины
		int maxWidth = comboBox.Items.Cast<object>()
			.Select(item => item is KeyValuePair<object, string> pair ? pair.Value : item.ToString())
			.Select(text => TextRenderer.MeasureText(text, comboBox.Font).Width)
			.Max() + 20;

		// Устанавливаем ширину с учетом минимального значения
		comboBox.DropDownWidth = Math.Max(minWidth, maxWidth);
	}

	#endregion

	#region NestedClasses

	private class DisplayedTechnologicalCard : INotifyPropertyChanged, IDisplayedEntity, IIdentifiable
	{
		private int _id;
		private string _article;
		private string _name;
		private string? _description;
		private string _version = "0.0.0.0";
		private string _type;
		private float _networkVoltage;
		private string? _technologicalProcessType;
		private string? _technologicalProcessName;
		private string? _technologicalProcessNumber;
		private string? _parameter;
		private string? _finalProduct;
		private string? _applicability;
		private string? _note;
		private string? _damageType;
		private string? _repairType;
		private bool _isCompleted;
		private bool _isDynamic;
		private TechnologicalCardStatus _status;

		public DisplayedTechnologicalCard() { }

		public DisplayedTechnologicalCard(TechnologicalCard tc)
		{
			Id = tc.Id;
			Article = tc.Article;
			Name = tc.Name ?? string.Empty;
			Description = tc.Description;
			Version = tc.Version;
			Type = tc.Type;
			NetworkVoltage = tc.NetworkVoltage;
			TechnologicalProcessType = tc.TechnologicalProcessType;
			TechnologicalProcessName = tc.TechnologicalProcessName;
			TechnologicalProcessNumber = tc.TechnologicalProcessNumber;
			Parameter = tc.Parameter;
			FinalProduct = tc.FinalProduct;
			Applicability = tc.Applicability;
			Note = tc.Note;
			DamageType = tc.DamageType;
			RepairType = tc.RepairType;
			IsCompleted = tc.IsCompleted;
			IsDynamic = tc.IsDynamic;
			Status = tc.Status;
		}

		public int Id
		{
			get => _id;
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged(nameof(Id));
				}
			}
		}

		public string Article
		{
			get => _article;
			set
			{
				if (_article != value)
				{
					_article = value;
					OnPropertyChanged(nameof(Article));
				}
			}
		}

		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			}
		}

		public string Description
		{
			get => _description;
			set
			{
				if (_description != value)
				{
					_description = value;
					OnPropertyChanged(nameof(Description));
				}
			}
		}

		public string Version
		{
			get => _version;
			set
			{
				if (_version != value)
				{
					_version = value;
					OnPropertyChanged(nameof(Version));
				}
			}
		}

		public string Type
		{
			get => _type;
			set
			{
				if (_type != value)
				{
					_type = value;
					OnPropertyChanged(nameof(Type));
				}
			}
		}

		public float NetworkVoltage
		{
			get => _networkVoltage;
			set
			{
				if (_networkVoltage != value)
				{
					_networkVoltage = value;
					OnPropertyChanged(nameof(NetworkVoltage));
				}
			}
		}

		public string TechnologicalProcessType
		{
			get => _technologicalProcessType;
			set
			{
				if (_technologicalProcessType != value)
				{
					_technologicalProcessType = value;
					OnPropertyChanged(nameof(TechnologicalProcessType));
				}
			}
		}

		public string TechnologicalProcessName
		{
			get => _technologicalProcessName;
			set
			{
				if (_technologicalProcessName != value)
				{
					_technologicalProcessName = value;
					OnPropertyChanged(nameof(TechnologicalProcessName));
				}
			}
		}

		public string TechnologicalProcessNumber
		{
			get => _technologicalProcessNumber;
			set
			{
				if (_technologicalProcessNumber != value)
				{
					_technologicalProcessNumber = value;
					OnPropertyChanged(nameof(TechnologicalProcessNumber));
				}
			}
		}

		public string Parameter
		{
			get => _parameter;
			set
			{
				if (_parameter != value)
				{
					_parameter = value;
					OnPropertyChanged(nameof(Parameter));
				}
			}
		}

		public string FinalProduct
		{
			get => _finalProduct;
			set
			{
				if (_finalProduct != value)
				{
					_finalProduct = value;
					OnPropertyChanged(nameof(FinalProduct));
				}
			}
		}

		public string Applicability
		{
			get => _applicability;
			set
			{
				if (_applicability != value)
				{
					_applicability = value;
					OnPropertyChanged(nameof(Applicability));
				}
			}
		}

		public string Note
		{
			get => _note;
			set
			{
				if (_note != value)
				{
					_note = value;
					OnPropertyChanged(nameof(Note));
				}
			}
		}

		public string DamageType
		{
			get => _damageType;
			set
			{
				if (_damageType != value)
				{
					_damageType = value;
					OnPropertyChanged(nameof(DamageType));
				}
			}
		}

		public string RepairType
		{
			get => _repairType;
			set
			{
				if (_repairType != value)
				{
					_repairType = value;
					OnPropertyChanged(nameof(RepairType));
				}
			}
		}

		public bool IsCompleted
		{
			get => _isCompleted;
			set
			{
				if (_isCompleted != value)
				{
					_isCompleted = value;
					OnPropertyChanged(nameof(IsCompleted));
				}
			}
		}

		public bool IsDynamic
		{
			get => _isDynamic;
			set
			{
				if (_isDynamic != value)
				{
					_isDynamic = value;
					OnPropertyChanged(nameof(IsDynamic));
				}
			}
		}

		public TechnologicalCardStatus Status
		{
			get => _status;
			set
			{
				if (_status != value)
				{
					_status = value;
					OnPropertyChanged(nameof(Status));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public Dictionary<string, string> GetPropertiesNames() => new Dictionary<string, string>
		{
			{ nameof(Id), "ID" },
			{ nameof(Article), "Артикул" },
			{ nameof(Version), "Версия" },
			{ nameof(Name), "Название" },
			{ nameof(Type), "Тип карты" },
			{ nameof(NetworkVoltage), "Сеть, кВ" },
			{ nameof(TechnologicalProcessType), "Тип тех. процесса" },
			{ nameof(TechnologicalProcessName), "Тех. процесс" },
			{ nameof(Parameter), "Параметр" },
			{ nameof(FinalProduct), "Конечный продукт" },
			{ nameof(Applicability), "Применимость тех. карты" },
			{ nameof(Note), "Примечания" },
			{ nameof(IsCompleted), "Наличие" },
			{ nameof(Status), "Ст." },
			{ nameof(IsDynamic), "ДК" }

		};
		
		public List<string> GetPropertiesOrder() => new List<string>
		{
				nameof(Article),
				nameof(Type),
				nameof(NetworkVoltage),
				nameof(TechnologicalProcessType),
				nameof(TechnologicalProcessName),
				nameof(Parameter),
				nameof(FinalProduct),
				nameof(Applicability),
				nameof(Note),
				nameof(IsDynamic),

                    //nameof(Status)
                    // nameof(IsCompleted),
                    // nameof(Id),
                    // nameof(Version),
            };
		
		public List<string> GetRequiredFields() => new List<string>
		{
				nameof(Article),
				nameof(Type),
				nameof(NetworkVoltage)
		};
	}

	#endregion
}
