namespace TcModels.Models
{
    public interface ISaveableForm
    {
        public T DataToSave<T>();
        public string GetPath();
    }
}
