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


	private IEnumerable<T> _dataSource;

	/// <summary>
	/// Делегат, который возвращает строковое представление объекта типа T.
	/// Например, можно передать что-то вроде (x => x.Name) или (x => x.ToString()).
	/// </summary>
	public Func<T, string> DisplayMemberFunc { get; set; } = x => x.ToString();

	/// <summary>
	/// Делегат, определяющий, как искать в объекте.
	/// Например: (x => $"{x.Name} {x.Article}") для поиска по Name и Article.
	/// </summary>
	public Func<T, string> SearchCriteriaFunc { get; set; } = x => x.ToString();

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
	public event EventHandler<SelectedItemChangedEventArgs<T>> SelectedItemChanged;

	// функционал с выпадающем списком поверх других элементов

	//// Внутри UserControl SearchBox<T> добавьте следующее поле:
	private ListBox _dropDownListBox;

// В конструкторе вашего SearchBox<T>:
	public SearchBox()
	{
		InitializeComponent();
		textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
		textBoxSearch.LostFocus += TextBoxSearch_LostFocus;
		textBoxSearch.KeyDown += TextBoxSearch_KeyDown;
	}


	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		// Ищем главную форму:
		SetListBoxInParentForm();
	}

	private void SetListBoxInParentForm()
	{
		Form parentForm = this.FindForm();
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

			parentForm.Controls.Add(_dropDownListBox);
			_dropDownListBox.BringToFront();
		}
	}

	// Методы отображения и скрытия ListBox
	private void ShowDropDown()
	{
		// Проверка должна быть на DataSource, а не на Items
		if (_dropDownListBox.DataSource is not IList list || list.Count == 0)
		{
			_dropDownListBox.Visible = false;
			return;
		}

		//Point textBoxPos = this.textBoxSearch.PointToScreen(Point.Empty);
		Point locationOnForm = this.FindForm().PointToClient(textBoxSearch.PointToScreen(new Point(0, textBoxSearch.Height)));

		_dropDownListBox.Location = locationOnForm;//location;
		_dropDownListBox.Width = textBoxSearch.Width;
		//_dropDownListBox.Height = Math.Min(150, _dropDownListBox.PreferredHeight); // высота по содержимому, но не больше 150px
		_dropDownListBox.Height = 150; // для теста фиксированная высота

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


	private void DropDownListBox_MouseMove(object sender, MouseEventArgs e)
	{
		int index = _dropDownListBox.IndexFromPoint(e.Location);
		if (index >= 0 && index < _dropDownListBox.Items.Count)
			_dropDownListBox.SelectedIndex = index;
	}

	private void DropDownListBox_Click(object sender, EventArgs e)
	{
		DropDownListBoxSelect();
	}

	// Скрытие при потере фокуса
	private void TextBoxSearch_LostFocus(object sender, EventArgs e)
	{
		if (!_dropDownListBox.Focused)
			HideDropDown();
	}

	//private void TextBoxSearch_LostFocus(object sender, EventArgs e)
	//{
	//	//if (!_dropDownListBox.Visible)
	//	//	return;

	//	//// Проверка, на какой контрол перешёл фокус
	//	//var activeControl = FindForm()?.ActiveControl;

	//	//if (activeControl != _dropDownListBox)
	//	//{
	//	//	HideDropDown();
	//	//}
	//}


	// Навигация по клавиатуре
	private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
	{
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

	private void TextBoxSearch_TextChanged(object? sender, EventArgs e)
	{
		RefreshSuggestions();
		//ShowDropDown();
	}

	public void SetSelectedItem(T item, bool invokeChanges = true)
	{
		SetListBoxInParentForm();

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


}
