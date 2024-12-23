namespace TcModels.Models.Interfaces
{
	public interface IRequiredProperties
    {
        List<string> GetPropertiesRequired { get; }
        Dictionary<string, string> GetPropertiesNames { get; }
    }
}
