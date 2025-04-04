using TcModels.Models.IntermediateTables;

namespace TC_WinForms.WinForms.Win6.Models;

public class DisplayedProtection_TC : BaseDisplayedEntity
{
	public DisplayedProtection_TC()
	{

	}
	public DisplayedProtection_TC(Protection_TC obj)
	{
		ChildId = obj.ChildId;
		ParentId = obj.ParentId;
		Order = obj.Order;

		Name = obj.Child.Name;
		Type = obj.Child.Type;
		Note = obj.Note;
        Unit = obj.Child.Unit;
		Quantity = obj.Quantity;
		Price = obj.Child.Price;
		Description = obj.Child.Description;
		Manufacturer = obj.Child.Manufacturer;
		ClassifierCode = obj.Child.ClassifierCode;

		IsReleased = obj.Child.IsReleased;
	}
}

