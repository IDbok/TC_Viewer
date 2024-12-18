using TcModels.Models.IntermediateTables;

namespace TC_WinForms.WinForms.Win6.Models;

internal class DisplayedMachine_TC : BaseDisplayedEntity
{
	public DisplayedMachine_TC()
	{

	}
	public DisplayedMachine_TC(Machine_TC obj)
	{
		ChildId = obj.ChildId;
		ParentId = obj.ParentId;
		Order = obj.Order;

		Name = obj.Child.Name;
		Type = obj.Child.Type;

		Unit = obj.Child.Unit;
		Quantity = obj.Quantity;
		Price = obj.Child.Price;
		Description = obj.Child.Description;
		Manufacturer = obj.Child.Manufacturer;
		ClassifierCode = obj.Child.ClassifierCode;

		IsReleased = obj.Child.IsReleased;
	}
}
