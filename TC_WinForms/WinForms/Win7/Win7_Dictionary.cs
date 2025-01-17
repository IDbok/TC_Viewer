namespace TC_WinForms.WinForms;

public partial class Win7_Dictionary : Form
{
	public Win7_Dictionary()
	{
		InitializeComponent();

		// Заполнение таблицы
		RefreshTables();
	}

	private void RefreshTables()
	{
		var listData = new List<string>()
		{
			"Технологические операции",
			"Тип Тех карты",
			"Напряжения ТК"

		};

		foreach (var item in listData) 
		{ 
			listBox1.Items.Add(item);
		}


		//// Очистка таблицы
		//dataGridView1.Rows.Clear();
		//// Заполнение таблицы
		//foreach (var techOperation in _techOperations)
		//{
		//	dataGridView1.Rows.Add(techOperation.Id, techOperation.Name, techOperation.Category, techOperation.IsReleased);
		//}
	}
}
