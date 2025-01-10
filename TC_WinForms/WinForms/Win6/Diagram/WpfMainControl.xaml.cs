using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TC_WinForms.WinForms.Win6.Models;
using TcDbConnector;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Diagram;

/// <summary>
/// Основной контейнер для WPF форм диаграммы технологической карты.
/// Хранит в себе список WpfTo, отвечающих за отображение связанных ТО.
/// </summary>
public partial class WpfMainControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
{
	#region Fields

	private readonly TcViewState _tcViewState;
    public readonly MyDbContext _dbContext;

    private int _tcId;
    public TechnologicalCard _technologicalCard;
	public DiagramForm _diagramForm; // используется только для проверки (фиксации) наличия изменений

    private List<TechOperationWork> _techOperationWorksList = new();
	/// <summary>Список всех TechOperationWork (технологических операций), доступных для добавления в диаграмму.</summary>
	public List<TechOperationWork> TechOperationWorksList
	{
		get => _techOperationWorksList;
	}

	/// <summary>Список существующих DiagamToWork (элементов диаграммы) для данной ТК.</summary>
	private List<DiagamToWork> _diagramToWorkList = new();

	/// <summary>Список диаграмм, удалённых с формы, которые пока не удалены из контекста БД.</summary>
	private List<DiagamToWork> _deletedDiagrams = new();

	/// <summary>Коллекция WpfTo для отображения на форме.</summary>
	public ObservableCollection<WpfTo> Children { get; set; } = new();

	private System.Windows.Point _mouseStartPosition; // Начальная позиция мыши
	private System.Windows.Point _scrollStartOffset; // Начальное смещение скролла
	private bool _isMiddleButtonPressed = false; // Флаг для нажатия средней кнопки мыши

	#endregion

	#region Properties

	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Режим отображения комментариев.</summary>
	public bool IsCommentViewMode => _tcViewState.IsCommentViewMode;

	/// <summary>Глобальный признак режима «Только просмотр».</summary>
	private bool IsViewMode => _tcViewState.IsViewMode;

	/// <summary>
	/// Возвращает <c>true</c>, если контролы должны быть скрыты в режиме «Только просмотр».
	/// </summary>
	public bool IsHiddenInViewMode => !IsViewMode;

	private double _currentScale = 1;

	public double CurrentScale
	{
		get => _currentScale;
		set
		{
			if (_currentScale != value)
			{
				_currentScale = value;

				_tcViewState.DiagramScale = value;

				OnPropertyChanged(nameof(CurrentScale));

				if (ContentScaleTransform is ScaleTransform scaleTransform)
				{
					scaleTransform.ScaleX = _currentScale;
					scaleTransform.ScaleY = _currentScale;
				}
			}
		}
	}


	/// <summary>
	/// Минимальное значение масштаба.
	/// </summary>
	public double MinScaleValue => MinScale;

	/// <summary>
	/// Максимальное значение масштаба.
	/// </summary>
	public double MaxScaleValue => MaxScale;

	#endregion

	#region Constants

	private const double MinScale = 0.1; // Минимальный масштаб
	private const double MaxScale = 2.0; // Максимальный масштаб

	#endregion

	#region Constructor

	public WpfMainControl()
    {
        InitializeComponent();
    }

    public WpfMainControl(int tcId, DiagramForm _diagramForm, TcViewState tcViewState, MyDbContext context)
    {
        InitializeComponent();
        DataContext = this;

        _tcViewState = tcViewState;
        this._tcId = tcId;
        this._diagramForm = _diagramForm;
        this._dbContext = context;

		if (_tcViewState.DiagramScale != null)
		{
			CurrentScale = (double)_tcViewState.DiagramScale;
		}

		// Подпишемся на событие изменения режима просмотра
		_tcViewState.ViewModeChanged += OnViewModeChanged;
    }

	#endregion

	#region Event Handlers

	private void OnViewModeChanged()
	{
		OnPropertyChanged(nameof(IsHiddenInViewMode));
	}

	/// <summary>
	/// Событие, возникающее при загрузке UserControl.
	/// </summary>
	private async void OnLoad(object sender, EventArgs e)
	{
		this.Visibility = Visibility.Collapsed;

		// в _tcViewState уже загружены все необходимые данные,
		_technologicalCard = _tcViewState.TechnologicalCard;
		_techOperationWorksList = _tcViewState.TechOperationWorksList;
		_diagramToWorkList = _tcViewState.DiagramToWorkList;

		// Добавляем диаграммы в визуальное дерево
		AddDiagramsToChildren();
		UpdateNumbering();

		this.Visibility = Visibility.Visible;
	}

	/// <summary>
	/// Обработка клика на кнопку "Добавить следующее ТО".
	/// </summary>
	private void Button_Click(object sender, RoutedEventArgs e)
	{
		if (CheckIfTOIsAvailable())
		{
			Children.Add(new WpfTo(this, _tcViewState));
			_diagramForm.HasChanges = true;
			UpdateNumbering();
		}
	}

	/// <summary>
	/// Обработчик нажатия клавиш на UserControl (увеличение/уменьшение масштаба по Ctrl + +/-).
	/// </summary>
	private void UserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (Keyboard.Modifiers == ModifierKeys.Control)
		{
			// Ctrl + '+' || Ctrl + NumPad '+'
			if (e.Key == Key.OemPlus || e.Key == Key.Add)
			{
				CurrentScale = Math.Clamp(CurrentScale + 0.1, MinScale, MaxScale);
				e.Handled = true;
			}
			// Ctrl + '-' || Ctrl + NumPad '-'
			else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
			{
				CurrentScale = Math.Clamp(CurrentScale - 0.1, MinScale, MaxScale);
				e.Handled = true;
			}
		}
	}

	/// <summary>
	/// Обработчик прокрутки колеса мыши для изменения масштаба.
	/// </summary>
	private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (Keyboard.Modifiers == ModifierKeys.Control)
		{
			double delta = e.Delta > 0 ? 0.1 : -0.1;

			CurrentScale = Math.Clamp(CurrentScale + delta, MinScale, MaxScale);

			e.Handled = true;
		}
	}

	/// <summary>
	/// Обработчик изменения значения слайдера масштаба.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (ContentScaleTransform is ScaleTransform scaleTransform)
		{
			double scale = e.NewValue;

			//CurrentScale = Math.Clamp(scale, MinScale, MaxScale); // todo: почему устанавливает 0.1 загрузки формы?
			scaleTransform.ScaleX = scale;
			scaleTransform.ScaleY = scale;
		}
	}

	private void ScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle) // Проверяем, нажата ли средняя кнопка мыши
		{
			_isMiddleButtonPressed = true;
			_mouseStartPosition = e.GetPosition(ZoomableScrollViewer); // Запоминаем позицию мыши
			_scrollStartOffset = new System.Windows.Point(ZoomableScrollViewer.HorizontalOffset, ZoomableScrollViewer.VerticalOffset); // Запоминаем начальное смещение

			ZoomableScrollViewer.CaptureMouse(); // Захватываем мышь для отслеживания движения
			ZoomableScrollViewer.Cursor = System.Windows.Input.Cursors.Hand; // Меняем курсор на "руку"
			e.Handled = true;
		}
	}

	private void ScrollViewer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (_isMiddleButtonPressed) // Проверяем, удерживается ли средняя кнопка мыши
		{
			var currentMousePosition = e.GetPosition(ZoomableScrollViewer); // Получаем текущую позицию мыши
			var delta = currentMousePosition - _mouseStartPosition; // Вычисляем смещение мыши

			// Прокручиваем ScrollViewer в зависимости от смещения
			ZoomableScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset.X - delta.X);
			ZoomableScrollViewer.ScrollToVerticalOffset(_scrollStartOffset.Y - delta.Y);
			e.Handled = true;
		}
	}

	private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle) // Если отпущена средняя кнопка мыши
		{
			_isMiddleButtonPressed = false;
			ZoomableScrollViewer.ReleaseMouseCapture(); // Освобождаем захват мыши
			ZoomableScrollViewer.Cursor = System.Windows.Input.Cursors.Arrow; // Возвращаем стандартный курсор
			e.Handled = true;
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Удаляет WpfControlTO из дерева (и соответствующий DiagamToWork).
	/// </summary>
	public void DeleteControlTO(WpfControlTO controlTO)
	{
		// найти и удалить controlTO из ListWpfControlTO (WpfTo)
		for (int i = Children.Count - 1; i >= 0; i--)
		{
			WpfTo wpfTo = Children[i];
			for (int j = wpfTo.Children.Count - 1; j >= 0; j--)
			{
				var wpfToSq = wpfTo.Children[j];
				if (wpfToSq.Children.Contains(controlTO))
				{
					// Помечаем диаграмму на удаление
					_deletedDiagrams.Add(controlTO.diagamToWork);
					_technologicalCard.DiagamToWork.Remove(controlTO.diagamToWork);

					// Удаляем сам контрол
					wpfToSq.DeleteWpfControlTO(controlTO);

					UpdateNumbering();
					break;
				}
			}
		}
	}

	/// <summary>
	/// Перенумерация всех элементов диаграммы (TO, параллели, последовательности, шаги...).
	/// </summary>
	internal void UpdateNumbering()
	{
		int counter = 1;

		int Order1 = 1; // порядковый номер отображения TO
		int Order2 = 1; // порядковый номер отображения параллельных операций
		int Order3 = 1;
		int Order4 = 1;

		foreach (WpfTo wpfToItem in Children)
		{
			foreach (var wpfToSq in wpfToItem.Children)
				foreach (var wpfControlToItem in wpfToSq.Children)
				{
					if (wpfControlToItem.diagamToWork != null) 
                        wpfControlToItem.diagamToWork.Order = Order1; // todo: можно ли инкрементировать здесь? ( почему тут логика отличается?)

					Order1++;
					Order2 = 1;
					Order3 = 1;
					Order4 = 1;

					// Параллельные операции
					foreach (WpfParalelno wpfParallelItem in wpfControlToItem.Children)
					{
						if (wpfParallelItem.diagramParalelno != null) 
                            wpfParallelItem.diagramParalelno.Order = Order2++;

						Order3 = 1;
						Order4 = 1;

						// Последовательности
						foreach (WpfPosledovatelnost item3 in wpfParallelItem.ListWpfPosledovatelnost.Children)
						{
							if (item3.diagramPosledov != null) 
                                item3.diagramPosledov.Order = Order3++;

							Order4 = 1;

							foreach (WpfShag item4 in item3.ListWpfShag.Children)
							{
								if (item4.diagramShag != null)
								{
									item4.diagramShag.Order = Order4++;
								}
								// todo: проверка на то, что номер не изменился
								item4.SetNomer(counter);
								counter++;
							}
						}
					}
				}

		}

		// Если не «Только просмотр», значит были изменения
		if (!_tcViewState.IsViewMode)
		{
			_diagramForm.HasChanges = true;
		}
	}

	/// <summary>
	/// Сохранение данных диаграммы.
	/// </summary>
	/// <param name="saveContext"></param>
	public void Save(bool saveContext = false)
	{
		UpdateNumbering();

		try
		{
			SaveAllChildren();
			_diagramForm.HasChanges = false;
		}
		catch (Exception)
		{

		}

		DeleteDiagramsFromContext(saveContext: true);

	}

	/// <summary>
	/// Перемещение целого блока (WpfTo) вверх или вниз в списке Children.
	/// </summary>
	/// <param name="wpfTo"></param>
	/// <param name="direction"></param>
	public void ChangeOrder(WpfTo wpfTo, MoveDirection direction)
	{
		var index = Children.IndexOf(wpfTo);

		switch (direction)
		{
			case MoveDirection.Up:
				if (index > 0)
				{
					Children.Move(index, index - 1);
				}
				break;

			case MoveDirection.Down:
				if (index < Children.Count - 1)
				{
					Children.Move(index, index + 1);
				}
				break;
		}

		UpdateNumbering();
	}

	/// <summary>
	/// Проверяет, что есть ещё TechOperationWorks, не представленные на блок-схеме.
	/// Если нет – показывает сообщение и возвращает false.
	/// </summary>
	public bool CheckIfTOIsAvailable()
	{
		var allDiagramToWorks = GetAllDiagramToWorks();
		if (allDiagramToWorks.Count >= _techOperationWorksList.Count)
		{
			System.Windows.Forms.MessageBox.Show("Все существующие в ТК операции уже представлены на блок-схеме");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Возвращает список TechOperationWork, которые ещё не добавлены в диаграмму.
	/// </summary>
	public List<TechOperationWork> GetAvailableTechOperationWorks()
	{
		var notAvailable = GetAllDiagramToWorks()
				.Select(dtw => dtw.techOperationWork)
				.Distinct()
				.ToList();

		return _techOperationWorksList
			.Where(tow => !notAvailable.Contains(tow))
			.ToList();
	}

	/// <summary>
	/// Возвращает все DiagamToWork, уже добавленные в Children.
	/// </summary>
	public List<DiagamToWork> GetAllDiagramToWorks()
	{
		var diagramToWorks = new List<DiagamToWork>();

		foreach (WpfTo wpfToItem in Children)
			foreach (var wpfToSq in wpfToItem.Children)
				foreach (var wpfControlToItem in wpfToSq.Children)
					if (wpfControlToItem.diagamToWork != null)
					{
						diagramToWorks.Add(wpfControlToItem.diagamToWork);
					}
				
		return diagramToWorks;
	}

	/// <summary>
	/// Удаляет WpfTo из Children.
	/// </summary>
	public void DeleteWpfTO(WpfTo wpfTo)
	{
		Children.Remove(wpfTo);
	}

	/// <summary>
	/// Переинициализирует форму полностью.
	/// </summary>
	public void ReinitializeForm()
	{
		try
		{
			// удалить текущие данные
			Children.Clear();

			_diagramForm.ReloadElementHost(new WpfMainControl(_tcId, _diagramForm, _tcViewState, _dbContext));

		}
		catch (Exception ex)
		{
			// Обработка ошибок
			System.Windows.Forms.MessageBox.Show($"Ошибка переинициализации формы: {ex.Message}");
		}
	}

	/// <summary>
	/// Если DiagamToWork по указанному TechOperationWork уже когда-то был удалён, возвращает его.
	/// Иначе – <c>null</c>.
	/// </summary>
	public DiagamToWork? CheckInDeletedDiagrams(TechOperationWork techOperationWork)
	{
		return _deletedDiagrams.FirstOrDefault(d => d.techOperationWork == techOperationWork);
	}

	/// <summary>
	/// Удаляет DiagamToWork из списка удалённых диаграмм (возвращает обратно в ТК).
	/// </summary>
	public void DeleteFromDeletedDiagrams(DiagamToWork diagramToWork)
	{
		_deletedDiagrams.Remove(diagramToWork);
		_technologicalCard.DiagamToWork.Add(diagramToWork);
	}


	#endregion

	#region Private Methods

	/// <summary>
	/// Вспомогательный метод для добавления диаграммы в коллекцию Children (с учётом группировки по параллелям).
	/// </summary>
	private void AddDiagramsToChildren()
    {
        // Сгруппировать по ParallelIndex, если ParallelIndex = null, то записать в отдельную группу
        var groups = _diagramToWorkList
            .GroupBy(g => g.ParallelIndex != null ? g.GetParallelIndex() : g.Order.ToString())
			.OrderBy(o => o.FirstOrDefault()?.Order)
			.ToList();

        foreach (var group in groups)
		{
			// Если ключ нулевой (ParallelIndex = null), это одиночная группа
			if (group.Key == null)
			{
				foreach (var item in group)
				{
					AddDiagramsToChildren(item);
				}
			}
			else
			{
				AddDiagramsToChildren(group.OrderBy(x => x.Order).ToList());
			}
        }
    }

	/// <summary>
	/// Добавляет список DiagamToWork как параллельную группу (WpfTo).	
	/// </summary>
	private void AddDiagramsToChildren(List<DiagamToWork> diagamToWorks, int? indexPosition = null)
    {
        var wpfTo = new WpfTo(this, _tcViewState, diagamToWorks);

        if (indexPosition != null)
        {
            Children.Insert(indexPosition.Value, wpfTo);
        }
        else
        {
            Children.Add(wpfTo);
        }
    }

	/// <summary>
	/// Добавляет одиночный DiagamToWork как параллельную группу (состоящую из одного элемента).
	/// </summary>
	private void AddDiagramsToChildren(DiagamToWork diagamToWork, int? indexPosition = null)
    {
        AddDiagramsToChildren(new List<DiagamToWork> { diagamToWork }, indexPosition);
    }

	/// <summary>
	/// Проходит по всему дереву Children и вызывает методы сохранения у конечных элементов (шагов).
	/// </summary>
	private void SaveAllChildren()
	{
		foreach (WpfTo wpfToItem in Children)
			foreach (WpfToSequence wpfToSq in wpfToItem.Children)
				foreach (WpfControlTO wpfControlToItem in wpfToSq.Children)
					foreach (WpfParalelno item2 in wpfControlToItem.Children)
						foreach (WpfPosledovatelnost item3 in item2.ListWpfPosledovatelnost.Children)
							foreach (WpfShag item4 in item3.ListWpfShag.Children)
							{
								item4.SaveCollection();
							}

	}

	/// <summary>
	/// Удаление диаграммы из контекста БД.
	/// </summary>
	/// <param name="saveContext"></param>
	private void DeleteDiagramsFromContext(bool saveContext = false)
	{
		try
		{
			// Зануляем поля в удалённых диаграммах
			foreach (DiagamToWork item in _deletedDiagrams)
			{
				item.techOperationWork = null;
			}

			// Удаляем из контекста диаграммы без techOperationWork
			var bbn = _dbContext.DiagamToWork
				.Where(w => w.techOperationWork == null).ToList();
			foreach (DiagamToWork item in bbn)
			{
				_dbContext.DiagamToWork.Remove(item);
			}

			if (saveContext)
			{
				_dbContext.SaveChanges();
			}

			_tcViewState.DiagramToWorkList = _technologicalCard.DiagamToWork;
			_tcViewState.TechnologicalCard.DiagamToWork = _technologicalCard.DiagamToWork;
		}
		catch (Exception ex)
		{
			System.Windows.Forms.MessageBox.Show(ex.Message);
		}

	}

	protected void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}


	#endregion
}

