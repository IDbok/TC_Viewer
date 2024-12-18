#if DEBUG
namespace TC_WinForms.WinForms.Win6.Models;

using TcModels.Models.IntermediateTables;

public partial class Win6_Machine_Design : BaseContentForm<DisplayedMachine_TC, Machine_TC>
{
	protected override DataGridView DgvMain => new DataGridView();
	protected override Panel PnlControls => new Panel();
	protected override IList<Machine_TC> TargetTable => new List<Machine_TC>();

	protected override void LoadObjects() { }
	protected override void SaveReplacedObjects() { }
	protected override Machine_TC CreateNewObject(BaseDisplayedEntity dObj)
	{
		return new Machine_TC();
	}
}
#endif
