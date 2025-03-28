using System.Collections;
using System.Data;

namespace TC_WinForms.WinForms.Controls;

public class SelectedItemChangedEventArgs<T> : EventArgs
{
	public T SelectedItem { get; private set; }

	public SelectedItemChangedEventArgs(T selectedItem)
	{
		SelectedItem = selectedItem;
	}
}

public partial class SearchBox<T> : UserControl
{
	// todo: починить отображение выпадающего списка + 
	// todo: избавиться от лишних вызовов SetListBoxInParentForm


	private IEnumerable<T> _dataSource = Enumerable.Empty<T>();

	/// <summary>
	/// Делегат, который возвращает строковое представление объекта типа T.
	/// Например, можно передать что-то вроде (x => x.Name) или (x => x.ToString()).
	/// </summary>
	public Func<T, string> DisplayMemberFunc { get; set; } = x => x?.ToString() ?? string.Empty;

	/// <summary>
	/// Делегат, определяющий, как искать в объекте.
	/// Например: (x => $"{x.Name} {x.Article}") для поиска по Name и Article.
	/// </summary>
	public Func<T, string> SearchCriteriaFunc { get; set; } = x => x?.ToString() ?? string.Empty;

	/// <summary>
	/// Минимальная ширина контрола.
	/// </summary>
	public int MinControlWidth { get; set; } = 350;

	/// <summary>
	/// Максимальная ширина контрола.
	/// </summary>
	public int MaxControlWidth { get; set; } = 500;

	/// <summary>
	/// Высота выпадающего списка.
	/// </summary>
	public int DropDownHeight { get; set; } = 250;


	/// <summary>
	/// Источник данных, из которого будет производиться поиск.
	/// </summary>
	public IEnumerable<T> DataSource
	{
		get => _dataSource;
		set
		{
			_dataSource = value;
			RefreshSuggestions();
		}
	}

	/// <summary>
	/// Текст в поле поиска.
	/// </summary>
	private string _previousText = string.Empty;

	/// <summary>
	/// Предыдущий выбранный элемент из списка (если есть).
	/// </summary>
	private T? _previousItem;


	/// <summary>
	/// Выбранный элемент из списка (если есть).
	/// </summary>
	public T SelectedItem { get; private set; }

	/// <summary>
	/// Событие, возникающее при выборе элемента из списка.
	/// </summary>
	public event EventHandler<SelectedItemChangedEventArgs<T>> SelectedItemChanged;

	// функционал с выпадающем списком поверх других элементов

	//// Внутри UserControl SearchBox<T> добавьте следующее поле:
	private ListBox? _dropDownListBox;

// В конструкторе вашего SearchBox<T>:
	public SearchBox()
	{
		InitializeComponent();
		textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
		textBoxSearch.LostFocus += TextBoxSearch_LostFocus;
		textBoxSearch.KeyDown += TextBoxSearch_KeyDown;
		textBoxSearch.Enter += TextBoxSearch_Enter;
	}


	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		// Ищем главную форму:
		SetListBoxInParentForm();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		// Подпишемся на изменение текста
		textBoxSearch.TextChanged += (s, ev) => AdjustControlWidthToText();

		// Вызовем один раз, если хотим, чтобы ширина учла текст по умолчанию
		AdjustControlWidthToText();
	}

	/// <summary>
	/// Перерасчитывает ширину контрола по размеру введённого текста и вписывает в заданные границы.
	/// </summary>
	private void AdjustControlWidthToText()
	{
		// Если TextBox пуст, можно задать базовую ширину, либо оставить минимальную
		string text = textBoxSearch.Text;
		if (string.IsNullOrEmpty(text))
		{
			this.Width = MinControlWidth;
			return;
		}

		using (Graphics g = textBoxSearch.CreateGraphics())
		{
			// Вычисляем, насколько «длинный» текст
			SizeF textSize = g.MeasureString(text, textBoxSearch.Font);

			// Добавляем небольшой запас (рамки TextBox, отступы и т.п.)
			int desiredWidth = (int)textSize.Width + 20;

			// Фиксируем в пределах [MinControlWidth..MaxControlWidth]
			if (desiredWidth < MinControlWidth)
				desiredWidth = MinControlWidth;
			if (desiredWidth > MaxControlWidth)
				desiredWidth = MaxControlWidth;

			// Выставляем ширину всего UserControl. 
			// Так как textBoxSearch.Dock = DockStyle.Top, он растянется по ширине контрола.
			this.Width = desiredWidth;
		}
	}


	private void SetListBoxInParentForm()
	{
		Form? parentForm = this.FindForm();
		if (parentForm != null)
		{
			_dropDownListBox = new ListBox
			{
				Visible = false,
				ScrollAlwaysVisible = true,
				IntegralHeight = false // Позволит контролировать высоту вручную
			};

			_dropDownListBox.Click += DropDownListBox_Click; //ListBoxSuggestions_Click;//
			_dropDownListBox.MouseMove += DropDownListBox_MouseMove;

			_dropDownListBox.DrawMode = DrawMode.OwnerDrawVariable;
			_dropDownListBox.MeasureItem += DropDownListBox_MeasureItem;
			_dropDownListBox.DrawItem += DropDownListBox_DrawItem;


			parentForm.Controls.Add(_dropDownListBox);
			_dropDownListBox.BringToFront();
		}
	}

	// Методы отображения и скрытия ListBox
	private void ShowDropDown()
	{
		if (_dropDownListBox == null) return;
		// Проверка должна быть на DataSource, а не на Items
		if (_dropDownListBox.DataSource is not IList list || list.Count == 0)
		{
			_dropDownListBox.Visible = false;
			return;
		}

		//Point textBoxPos = this.textBoxSearch.PointToScreen(Point.Empty);

		var form = this.FindForm();
		if (form == null) return;

		Point locationOnForm = form.PointToClient(textBoxSearch.PointToScreen(new Point(0, textBoxSearch.Height)));

		_dropDownListBox.Location = locationOnForm;//location;
		_dropDownListBox.Width = textBoxSearch.Width;
		//_dropDownListBox.Height = Math.Min(150, _dropDownListBox.PreferredHeight); // высота по содержимому, но не больше 150px
		_dropDownListBox.Height = DropDownHeight; // для теста фиксированная высота

		_dropDownListBox.Visible = true;
		_dropDownListBox.BringToFront();

		// Для диагностики временно добавь эту строку
		_dropDownListBox.Refresh();
	}

	private void HideDropDown()
	{
		if (_dropDownListBox != null)
			_dropDownListBox.Visible = false;
	}

	// Вызов при каждом обновлении текста:
	private void RefreshSuggestions()
	{
		if (_dropDownListBox == null) return;

		if (_dataSource == null || !(_dataSource.Any()) || string.IsNullOrWhiteSpace(textBoxSearch.Text))
		{
			HideDropDown();
			return;
		}
		string searchText = textBoxSearch.Text.ToLower();

		var filtered = _dataSource
			.Where(item => SearchCriteriaFunc(item).ToLower().Contains(searchText))
			.ToList();

		_dropDownListBox.DataSource = null;
		_dropDownListBox.DataSource = filtered;

		if (filtered.Any())
			ShowDropDown();
		else
			HideDropDown();
	}

	// Обработчик выбора
	private void DropDownListBoxSelect()
	{
		if (_dropDownListBox == null) return;

		if (_dropDownListBox.SelectedIndex >= 0)
		{
			if (_dropDownListBox.DataSource is List<T> dataSource)
			{
				SelectedItem = dataSource[_dropDownListBox.SelectedIndex];
				textBoxSearch.Text = DisplayMemberFunc(SelectedItem);
				
				// Вызываем новое событие с объектом T
				SelectedItemChanged?.Invoke(this, new SelectedItemChangedEventArgs<T>(SelectedItem));

				HideDropDown();
			}
			else
			{
				// Handle the error or log it
				throw new InvalidCastException("DataSource is not of type List<T>");
			}
		}
	}


	private void DropDownListBox_MouseMove(object? sender, MouseEventArgs e)
	{
		if (_dropDownListBox == null) return;
		int index = _dropDownListBox.IndexFromPoint(e.Location);
		if (index >= 0 && index < _dropDownListBox.Items.Count)
			_dropDownListBox.SelectedIndex = index;
	}

	private void DropDownListBox_Click(object? sender, EventArgs e)
	{
		DropDownListBoxSelect();
	}

	// Скрытие при потере фокуса
	private void TextBoxSearch_LostFocus(object? sender, EventArgs e)
	{
		if (_dropDownListBox != null && !_dropDownListBox.Focused)
			HideDropDown();
	}


	// Навигация по клавиатуре
	private void TextBoxSearch_KeyDown(object? sender, KeyEventArgs e)
	{
		if (_dropDownListBox == null) return;

		// Закрываем выпадающий список, если виден, при нажатии Esc
		if (e.KeyCode == Keys.Escape)
		{
			// Отменим ввод и вернём старые значения
			CancelInput();

			// Сбросим фокус, если нужно
			textBoxSearch.SelectionStart = textBoxSearch.Text.Length;
			return;
		}

		if (_dropDownListBox.Visible)
		{
			if (e.KeyCode == Keys.Down)
			{
				_dropDownListBox.Focus();
				if (_dropDownListBox.Items.Count > 0)
					_dropDownListBox.SelectedIndex = 0;
			}
		}
	}

	private void TextBoxSearch_Enter(object? sender, EventArgs e)
	{
		// Запомним текущие «старые» данные
		_previousText = textBoxSearch.Text;
		_previousItem = SelectedItem;
	}

	private void TextBoxSearch_TextChanged(object? sender, EventArgs e)
	{
		RefreshSuggestions();
		//ShowDropDown();
	}

	private void DropDownListBox_MeasureItem(object? sender, MeasureItemEventArgs e)
	{
		// Индекс может быть -1, если ListBox пустой или рисует «мимо»:
		if (e.Index < 0) return;

		if (_dropDownListBox == null) return;

		// Достаем элемент (строку), которую надо измерить
		string? itemText = _dropDownListBox.Items[e.Index].ToString();
		if (string.IsNullOrEmpty(itemText))
		{
			e.ItemHeight = (int)e.Graphics.MeasureString(" ", _dropDownListBox.Font).Height;
			return;
		}

		// Ограничение ширины — можно взять ширину самого ListBox (или чуть меньше, чтобы было красивее)
		int maxWidth = _dropDownListBox.Width;
		// меряем строку с учётом переноса
		SizeF size = e.Graphics.MeasureString(
			itemText,
			_dropDownListBox.Font,
			maxWidth);

		// Выставляем высоту строки
		e.ItemHeight = (int)Math.Ceiling(size.Height);
	}

	private void DropDownListBox_DrawItem(object? sender, DrawItemEventArgs e)
	{
		// Если индекс «мимо» — выходим
		if (e.Index < 0) return;

		if (_dropDownListBox == null) return;

		// Получаем текст
		string? itemText = _dropDownListBox.Items[e.Index].ToString() ?? "";

		// Сначала заливаем фон (чтобы выделение/фокус работали штатно)
		e.DrawBackground();

		// Рисуем сам текст. Можно управлять переносом через StringFormat
		using (var brush = new SolidBrush(e.ForeColor))
		{
			var format = new StringFormat
			{
				FormatFlags = 0,
				Trimming = StringTrimming.Word,
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Near
			};

			// Рисуем строку внутри прямоугольника e.Bounds
			if(e.Font != null)
				e.Graphics.DrawString(itemText, e.Font, brush, e.Bounds, format);
		}

		// Рисуем пунктирную рамку вокруг выделенного элемента (если нужно)
		e.DrawFocusRectangle();
	}


	public void SetSelectedItem(T item, bool invokeChanges = true)
	{
		SetListBoxInParentForm();

		// очистить текст поисковой строки
		ClearTextBox();

		if (item == null) return;

		SelectedItem = item;

		//// отписать от событий  TextBoxSearch_TextChanged перед изменением текста
		textBoxSearch.TextChanged -= TextBoxSearch_TextChanged;

		var displayText = DisplayMemberFunc(item);
		// Принудительно задать текст и обновить отображение контрола
		textBoxSearch.Text = displayText;
		//textBoxSearch.Select(textBoxSearch.Text.Length, 0); // устанавливаем курсор в конец
		//textBoxSearch.Refresh(); // принудительно обновляем контрол

		//textBoxSearch.Invalidate();
		//textBoxSearch.Update();


		textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
		if (invokeChanges)
			SelectedItemChanged?.Invoke(this, new SelectedItemChangedEventArgs<T>(SelectedItem));
		//HideDropDown();

	}

	private void ClearTextBox() {
		textBoxSearch.TextChanged -= TextBoxSearch_TextChanged;
		textBoxSearch.Clear();
		textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
	}

	/// <summary>
	/// Отмена ввода и возврат к предыдущему значению.
	/// </summary>
	public void CancelInput()
	{
		if (_previousItem == null) return;
		// 1. Снимем обработчик TextChanged, чтобы не вызывать RefreshSuggestions() и т.п.
		textBoxSearch.TextChanged -= TextBoxSearch_TextChanged;

		// 2. Вернём старое значение текста
		textBoxSearch.Text = _previousText;

		// 3. Вернём старый SelectedItem
		SelectedItem = _previousItem;

		// 4. Подпишемся обратно, чтобы в дальнейшем текст реагировал на ввод
		textBoxSearch.TextChanged += TextBoxSearch_TextChanged;

		// ВАЖНО: event SelectedItemChanged тут не вызываем!
		HideDropDown();
	}

}
