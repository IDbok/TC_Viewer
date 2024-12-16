namespace TcModels.Models.Interfaces
{
	public interface IDisplayedEntity
    {
        Dictionary<string, string> GetPropertiesNames();
        List<string> GetPropertiesOrder();
        List<string> GetRequiredFields();
    }
}
