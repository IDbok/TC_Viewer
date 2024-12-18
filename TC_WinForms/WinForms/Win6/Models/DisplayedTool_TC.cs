using TcModels.Models.IntermediateTables;

namespace TC_WinForms.WinForms.Win6.Models;

public class DisplayedTool_TC : BaseDisplayedEntity
{
	public override Dictionary<string, string> GetPropertiesNames()
	{
		var baseDict = base.GetPropertiesNames();

		baseDict.Add(nameof(Category), "Категория");

		return baseDict;
	}

	public DisplayedTool_TC()
	{

	}
	public DisplayedTool_TC(Tool_TC obj)
	{
		ChildId = obj.ChildId;
		ParentId = obj.ParentId;
		Order = obj.Order;

		Name = obj.Child.Name;
		Type = obj.Child.Type;

		Unit = obj.Child.Unit;
		Quantity = obj.Quantity;
		//Formula = "123";
		Price = obj.Child.Price;
		Description = obj.Child.Description;
		Manufacturer = obj.Child.Manufacturer;
		Category = obj.Child.Categoty;
		ClassifierCode = obj.Child.ClassifierCode;
		Note = obj.Note;

		IsReleased = obj.Child.IsReleased;
	}
	public string Category { get; set; } = "Tool";
}
