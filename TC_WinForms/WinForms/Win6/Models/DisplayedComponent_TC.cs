using TcModels.Models.IntermediateTables;

namespace TC_WinForms.WinForms.Win6.Models;

public class DisplayedComponent_TC : BaseDisplayedEntity
{
	public override Dictionary<string, string> GetPropertiesNames()
	{
		var baseDict = base.GetPropertiesNames();

		baseDict.Add(nameof(Category), "Категория");
		baseDict.Add(nameof(TotalPrice), "Стоимость, руб. без НДС");

		return baseDict;
	}
	public override List<string> GetPropertiesOrder()
	{
		var baseList = base.GetPropertiesOrder();

		baseList.Insert(5, nameof(TotalPrice));

		return baseList;
	}

	public DisplayedComponent_TC()
	{

	}
	public DisplayedComponent_TC(Component_TC obj)
	{
		ChildId = obj.ChildId;
		ParentId = obj.ParentId;
		Order = obj.Order;

		Name = obj.Child.Name;
		Type = obj.Child.Type;

		Unit = obj.Child.Unit;
		Quantity = obj.Quantity;
		Formula = obj.Formula;
		Price = obj.Child.Price ?? 0;
		Description = obj.Child.Description;
		Manufacturer = obj.Child.Manufacturer;
		Category = obj.Child.Categoty;
		ClassifierCode = obj.Child.ClassifierCode;
		Note = obj.Note;
		IsReleased = obj.Child.IsReleased;

		//previousOrder = Order; // устанавливается вместе с Order
	}

	public double TotalPrice => (int)(Price * Quantity);
	public string Category { get; set; } = "StandComp";

}
