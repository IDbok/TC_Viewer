#if DEBUG
using TC_WinForms.WinForms;
using TC_WinForms.WinForms.Win6.Models;
using TcModels.Models.IntermediateTables;
using static TC_WinForms.WinForms.Win6_Component;

public partial class Win6_Component_Design : BaseContentFormWithFormula<DisplayedComponent_TC, Component_TC>
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
