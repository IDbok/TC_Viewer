namespace TcModels.Models.Interfaces
{
	public interface INameable: IIdentifiable
    {
        string Name { get; set; }
    }
}
