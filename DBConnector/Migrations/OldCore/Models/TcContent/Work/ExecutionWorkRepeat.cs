namespace TcDbConnector.Migrations.OldCore.Models.TcContent;

public class ExecutionWorkRepeat
{
    public int Id { get; set; }

    public int ParentExecutionWorkId { get; set; }
    public ExecutionWork ParentExecutionWork { get; set; }

    public int ChildExecutionWorkId { get; set; }
    public ExecutionWork ChildExecutionWork { get; set; }

    public string NewCoefficient { get; set; } = string.Empty;
    public string NewEtap { get; set; } = string.Empty;
    public string NewPosled { get; set; } = string.Empty;
}

