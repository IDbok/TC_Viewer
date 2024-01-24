namespace TcModels.Models.Interfaces
{
    public interface ISaveableForm
    {
        public T DataToSave<T>();
        public string GetPath();
    }
}
