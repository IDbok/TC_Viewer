#if DEBUG
using TcModels.Models.IntermediateTables;

namespace TC_WinForms.WinForms.Win6.Models;

public partial class Win6_Tool_Design : BaseContentForm<DisplayedTool_TC, Tool_TC>
{
	protected override DataGridView DgvMain => new DataGridView();
	protected override Panel PnlControls => new Panel();
	protected override IList<Tool_TC> TargetTable => new List<Tool_TC>();

	protected override void LoadObjects() { }
	protected override void SaveReplacedObjects() { }
	protected override Tool_TC CreateNewObject(BaseDisplayedEntity dObj)
	{
		return new Tool_TC();
	}
}
#endif
