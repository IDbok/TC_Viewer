using System.Data;

namespace TC_WinForms.WinForms.Controls;
public partial class SearchBox<T> : UserControl
{
	private IEnumerable<T> _dataSource;

	/// <summary>
	/// Делегат, который возвращает строковое представление объекта типа T.
	/// Например, можно передать что-то вроде (x => x.Name) или (x => x.ToString()).
	/// </summary>
	public Func<T, string> DisplayMemberFunc { get; set; } = x => x.ToString();

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
	/// Выбранный элемент из списка (если есть).
	/// </summary>
	public T SelectedItem { get; private set; }

	/// <summary>
	/// Событие, возникающее при выборе элемента из списка.
	/// </summary>
	public event EventHandler SelectedItemChanged;

	public SearchBox()
	{
		InitializeComponent();
		// Прячем список до первой фильтрации (чтобы не было пустого списка)
		listBoxSuggestions.Visible = false;

		// Подписываемся на события
		textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
		listBoxSuggestions.Click += ListBoxSuggestions_Click;
		listBoxSuggestions.MouseMove += ListBoxSuggestions_MouseMove;
	}

	/// <summary>
	/// Обработчик при изменении текста в textBoxSearch.
	/// </summary>
	private void TextBoxSearch_TextChanged(object? sender, EventArgs e)
	{
		RefreshSuggestions();
	}

	/// <summary>
	/// Метод обновляет список вариантов (listBoxSuggestions) 
	/// согласно введённому тексту.
	/// </summary>
	private void RefreshSuggestions()
	{
		if (_dataSource == null || !(_dataSource.Any()))
		{
			listBoxSuggestions.Visible = false;
			return;
		}

		var searchText = textBoxSearch.Text.Trim().ToLower();
		if (string.IsNullOrEmpty(searchText))
		{
			// Если поле пустое — можно показывать все элементы 
			// или, например, скрывать список. Это на ваше усмотрение:
			listBoxSuggestions.DataSource = _dataSource.ToList();
			listBoxSuggestions.Visible = true;
		}
		else
		{
			// Фильтрация
			var filtered = _dataSource
				.Where(item => DisplayMemberFunc(item)
					.ToLower()
					.Contains(searchText))
				.ToList();

			listBoxSuggestions.DataSource = filtered;
			listBoxSuggestions.Visible = filtered.Any();
		}
	}

	/// <summary>
	/// Обработчик клика по списку — выбираем элемент.
	/// </summary>
	private void ListBoxSuggestions_Click(object? sender, EventArgs e)
	{
		if (listBoxSuggestions.SelectedItem != null)
		{
			SelectItem((T)listBoxSuggestions.SelectedItem);
		}
	}

	/// <summary>
	/// При наведении курсора и движении мыши, меняем выделение в списке.
	/// </summary>
	private void ListBoxSuggestions_MouseMove(object? sender, MouseEventArgs e)
	{
		int index = listBoxSuggestions.IndexFromPoint(e.Location);
		if (index >= 0 && index < listBoxSuggestions.Items.Count)
		{
			listBoxSuggestions.SelectedIndex = index;
		}
	}

	/// <summary>
	/// Устанавливаем текущий элемент как выбранный.
	/// </summary>
	private void SelectItem(T item)
	{
		SelectedItem = item;
		textBoxSearch.Text = DisplayMemberFunc(item);

		listBoxSuggestions.Visible = false;
		SelectedItemChanged?.Invoke(this, EventArgs.Empty);
	}

	// По желанию можно добавить метод очистки:
	public void ClearSelection()
	{
		textBoxSearch.Clear();
		SelectedItem = default(T);
		listBoxSuggestions.Visible = false;
	}
}
