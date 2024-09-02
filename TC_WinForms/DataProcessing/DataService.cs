using System.Drawing;
using TcDbConnector;
using TcModels.Models.TcContent;

namespace TC_WinForms.DataProcessing;

public class DataService
{
    private Cache<string, object> _cache = new Cache<string, object>(TimeSpan.FromMinutes(60)); // 60 минут кэша
    private DbConnector dbCon = new DbConnector();

    public List<Component> GetData()
    {
        // Используем кэш и функцию для загрузки данных из БД
        return _cache.Get(nameof(Component), LoadDataFromDatabase);
    }

    private List<Component> LoadDataFromDatabase(string dataName)
    {
        // Загружаем список объектов типа Component из базы данных
        try
        {
            // Предполагается, что GetObjectList<Component> возвращает IEnumerable<Component>
            return dbCon.GetObjectList<Component>(includeLinks: true).ToList();
        }
        catch (Exception ex)
        {
            // Логирование ошибки или другое обработка исключений
            Console.WriteLine($"Ошибка при загрузке данных из БД: {ex.Message}");
            return new List<Component>(); // Возвращаем пустой список в случае ошибки
        }
    }
}

