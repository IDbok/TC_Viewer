namespace TcModels.Models.Interfaces
{
	public interface ISaveEventForm//todo: пересмотреть логику работы интерфейса сохранения
    {
        bool GetDontSaveData();
        bool HasChanges { get; }
        Task SaveChanges();

        bool CloseFormsNoSave { get; set; }

    }
}
