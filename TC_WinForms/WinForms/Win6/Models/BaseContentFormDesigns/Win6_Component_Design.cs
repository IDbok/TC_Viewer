namespace TC_WinForms.WinForms.Win6.Models;
#if DEBUG
using TcModels.Models.IntermediateTables;

public partial class Win6_Component_Design : BaseContentForm<DisplayedComponent_TC, Component_TC>
{
	protected override DataGridView DgvMain => new DataGridView();
	protected override Panel PnlControls => new Panel();
	protected override IList<Component_TC> TargetTable => new List<Component_TC>();

	protected override void LoadObjects() { }
	protected override void SaveReplacedObjects() { }
	protected override Component_TC CreateNewObject(BaseDisplayedEntity dObj)
	{
		return new Component_TC();
	}
}
#endif
