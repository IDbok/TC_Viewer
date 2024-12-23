namespace TcModels.Models.TcContent.Work
{
	public interface IUpdatableEntity
    {
        void ApplyUpdates(IUpdatableEntity source);
    }
}
