namespace TcModels.Models.Interfaces
{
	public interface ISaveEventForm
    {
        bool GetDontSaveData();
        bool HasChanges { get; }
        Task SaveChanges();

        bool CloseFormsNoSave { get; set; }

    }
}
