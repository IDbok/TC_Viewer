namespace TcModels.Models.TcContent;

public class ExecutionWorkRepeat
{
    public int Id { get; set; }

    public int ParentExecutionWorkId { get; set; }
    public ExecutionWork ParentExecutionWork { get; set; } = null!;

	public int ChildExecutionWorkId { get; set; }
    public ExecutionWork ChildExecutionWork { get; set; } = null!;

	public string NewCoefficient { get; set; } = string.Empty; // todo: поменять на nullable
    public string NewEtap { get; set; } = string.Empty;
    public string NewPosled { get; set; } = string.Empty;
}

