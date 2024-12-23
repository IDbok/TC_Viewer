namespace TcModels.Models.Interfaces
{
	public interface IIntermediateDisplayedEntity : IDisplayedEntity
    {
        List<string> GetKeyFields();
    }
}
