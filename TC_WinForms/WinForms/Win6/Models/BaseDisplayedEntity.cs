using System.ComponentModel;
using TC_WinForms.Interfaces;
using TcModels.Models.Interfaces;

namespace TC_WinForms.WinForms.Win6.Models;

public interface IFormulaItem
{
	string? Formula { get; }
	double? Quantity { get; set; }
	string Name { get; }
	string? Type { get; }
}

public abstract class BaseDisplayedEntity :
	INotifyPropertyChanged,
	IIntermediateDisplayedEntity,
	IOrderable,
	IPreviousOrderable,
	IReleasable,
	IFormulaItem
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public int ChildId { get; set; }
	public int ParentId { get; set; }

	private int order;
	private int previousOrder;
	public int Order
	{
		get => order;
		set
		{
			if (order != value)
			{
				previousOrder = order;
				order = value;
				OnPropertyChanged(nameof(Order));
			}
		}
	}
	public int PreviousOrder => previousOrder;

	private double quantity;
	public double? Quantity
	{
		get => quantity;
		set
		{
			if (quantity != value)
			{
				quantity = value ?? 0;
				OnPropertyChanged(nameof(Quantity));
			}
		}
	}
	private string? formula;
	public string? Formula
	{
		get => formula;
		set
		{
			if (formula != value)
			{
				formula = value;
				OnPropertyChanged(nameof(Formula));
			}
		}
	}

	private string? note;
	public string? Note
	{
		get => note;
		set
		{
			if (note != value)
			{
				note = value;
				OnPropertyChanged(nameof(Note));
			}
		}
	}

	public string Name { get; set; }
	public string? Type { get; set; }
	public string Unit { get; set; }
	public float? Price { get; set; }
	public string? Description { get; set; }
	public string? Manufacturer { get; set; }
	public string ClassifierCode { get; set; }

	public bool IsReleased { get; set; }

	// Поля для сохранения старых значений
	private Dictionary<string, object> oldValueDict = new Dictionary<string, object>();

	public object GetOldValue(string propertyName)
	{
		if (oldValueDict.ContainsKey(propertyName))
		{
			return oldValueDict[propertyName];
		}

		return null;
	}

	// Абстрактные или виртуальные методы для получения списков полей.
	// При необходимости можно сделать их virtual и переопределять.
	public virtual Dictionary<string, string> GetPropertiesNames()
	{
		return new Dictionary<string, string>
				{
					{ nameof(ChildId), "ID" },
					{ nameof(ParentId), "ID тех. карты" },
					{ nameof(Order), "№" },
					{ nameof(Quantity), "Кол-во" },
					{ nameof(Formula), "Формула" },
					{ nameof(Note), "Примечание" },

					{ nameof(Name), "Наименование" },
					{ nameof(Type), "Тип (исполнение)" },
					{ nameof(Unit), "Ед.изм." },
					{ nameof(Price), "Стоимость за ед., руб. без НДС" },
					{ nameof(Description), "Описание" },
					{ nameof(Manufacturer), "Производители (поставщики)" },
					{ nameof(ClassifierCode), "Код в classifier" },
				};
	}
	public virtual List<string> GetPropertiesOrder()
	{
		return new List<string>
				{
					nameof(Order),		// 0
					nameof(Name),		// 1
					nameof(Type),		// 2
					nameof(ChildId),	// 3
					nameof(Unit),		// 4
					nameof(Formula),	// 5
					nameof(Quantity),	// 6
					nameof(Note),		// 7
				};
	}
	public virtual List<string> GetRequiredFields()
	{
		return new List<string>
				{
					nameof(ChildId) ,
					nameof(ParentId) ,
					nameof(Order),
				};
	}
	public virtual List<string> GetKeyFields()
	{
		return new List<string>
				{
					nameof(ChildId),
					nameof(ParentId),
				};
	}
}

