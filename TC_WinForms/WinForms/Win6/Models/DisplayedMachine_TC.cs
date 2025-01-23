using TcModels.Models.IntermediateTables;

namespace TC_WinForms.WinForms.Win6.Models;

public class DisplayedMachine_TC : BaseDisplayedEntity
{
	public bool OutlayCount { get; set; }
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
        OutlayCount = obj.IsInOutlayCount;

        Unit = obj.Child.Unit;
		Quantity = obj.Quantity;
		Price = obj.Child.Price;
		Description = obj.Child.Description;
		Manufacturer = obj.Child.Manufacturer;
		ClassifierCode = obj.Child.ClassifierCode;
		Note = obj.Note;

		IsReleased = obj.Child.IsReleased;
	}
    public override Dictionary<string, string> GetPropertiesNames()
    {
        var baseDict = base.GetPropertiesNames();

        baseDict.Add(nameof(OutlayCount), "Участвует в подсчете затрат");

        return baseDict;
    }
    public override List<string> GetPropertiesOrder()
    {
        var baseList = base.GetPropertiesOrder();

        baseList.Insert(7, nameof(OutlayCount));

        return baseList;
    }
}
