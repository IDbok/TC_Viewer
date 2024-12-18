#if DEBUG
namespace TC_WinForms.WinForms.Win6.Models;

using TcModels.Models.IntermediateTables;

public partial class Win6_Protection_Design : BaseContentForm<DisplayedProtection_TC, Protection_TC>
{
	protected override DataGridView DgvMain => new DataGridView();
	protected override Panel PnlControls => new Panel();
	protected override IList<Protection_TC> TargetTable => new List<Protection_TC>();

	protected override void LoadObjects() { }
	protected override void SaveReplacedObjects() { }
	protected override Protection_TC CreateNewObject(BaseDisplayedEntity dObj)
	{
		return new Protection_TC();
	}
}
#endif
