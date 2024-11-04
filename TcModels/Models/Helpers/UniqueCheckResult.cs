namespace TcModels.Models.Helpers;

public class UniqueCheckResult
{
    public bool IsUnique => !FailedConstraints.Any();
    public List<string> FailedConstraints { get; } = new List<string>();
}
