using TcModels.Models.TcContent;

namespace TC_WinForms.WinForms.Work;

public class TechOperationDataGridItem
{
	public int Nomer { get; set; }
	public int IdTO => TechOperationWork.techOperation.Id;

	public string TechOperation { get; set; } = null!; // todo - изменить на более понятное имя (например, TechOperationName)

	public string Staff { get; set; } = "";

	public string TechTransition { get; set; } = ""; // todo - изменить на более понятное имя

	public string TechTransitionValue { get; set; } = "";

	public TechOperationWork TechOperationWork;

	public string Protections { get; set; } = "";

	public bool ItsTool = false;

	public bool ItsComponent = false;


	public string Etap = "";
	public string Posled = "";

	public string TimeEtap = "";

	public bool Work = false;

	[Obsolete($"Поле усторело, используйте {nameof(WorkItem)}")]
	public ExecutionWork? techWork;
	public List<bool> listMach = new List<bool>();
	public List<string> listMachStr = new List<string>();

	public string Comments = "";

	public string Vopros = "";
	public string Otvet = "";

	public string PictureName = "";

	[Obsolete($"Поле усторело, используйте {nameof(WorkItem)}")]
	public ExecutionWork? executionWorkItem=null;

	public double TotalTime { get; set; } 
	public double TimeEtapValue { get; set; }

	public WorkItemType WorkItemType { get; set; } = WorkItemType.None;
	public object? WorkItem { get; set; } // поле для хранения объекта, связанного с работой (например, ToolWork, ComponentWork, ExecutionWork)

	public TechOperationDataGridItem(TechOperationWork techOperationWork, int order)
	{
		Nomer = order;
		TechOperationWork = techOperationWork;
		TechOperation = $"№{techOperationWork.Order} {techOperationWork.techOperation.Name}";
	}
	public TechOperationDataGridItem(TechOperationWork techOperationWork) : this(techOperationWork, -1) { }

	public TechOperationDataGridItem(TechOperationWork techOperationWork, ExecutionWork executionWork, int order, string staffStr,  List<bool> mach, string protectStr) 
		: this(techOperationWork, order)
	{
		Staff = staffStr;
		Protections = protectStr; 
		listMach = mach;

		WorkItem = executionWork;
		WorkItemType = WorkItemType.ExecutionWork;

		// todo: старый способ работы с ExecutionWork, убрать после перехода на WorkItem
		Work = true;
		techWork = executionWork;
		executionWorkItem = executionWork; //  зачем 2 ссылки на один объект ??

		TechTransition = executionWork.techTransition?.Name ?? "";
		TechTransitionValue = executionWork.Value.ToString();
		Etap = executionWork.Etap;
		Posled = executionWork.Posled;
		Comments = executionWork.Comments ?? "";
		Vopros = executionWork.Vopros;
		Otvet = executionWork.Otvet;
		PictureName = executionWork.PictureName ?? "";
	}

	public TechOperationDataGridItem(TechOperationWork techOperationWork, ToolWork toolWork, int order) 
		: this(techOperationWork, order)
	{
		WorkItem = toolWork;
		WorkItemType = WorkItemType.ToolWork;

		ItsTool = true;
		
		TechTransition = $"{toolWork.tool.Name}   {toolWork.tool.Type}    {toolWork.tool.Unit}";
		TechTransitionValue = toolWork.Quantity.ToString();
		Comments = toolWork.Comments ?? "";
	}

	public TechOperationDataGridItem(TechOperationWork techOperationWork, ComponentWork componentWork, int order) 
		: this(techOperationWork, order)
	{
		WorkItem = componentWork;
		WorkItemType = WorkItemType.ComponentWork;

		ItsComponent = true;

		TechTransition = $"{componentWork.component.Name}   {componentWork.component.Type}    {componentWork.component.Unit}";
		TechTransitionValue = componentWork.Quantity.ToString();
		Comments = componentWork.Comments ?? "";
	}
}

public enum WorkItemType
{
	None,
	ExecutionWork,
	ComponentWork,
	ToolWork
}
