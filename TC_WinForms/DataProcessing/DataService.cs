using System.Drawing;
using TcDbConnector;
using TcModels.Models.TcContent;

namespace TC_WinForms.DataProcessing;

public static class DataService // временное решение использовать статический метод. В дальнейшем хорошо бы применить DI
{
    private static Cache _cache = new Cache(TimeSpan.FromMinutes(60)); // 60 минут кэша
    private static DbConnector dbCon = new DbConnector();

    public static List<Component> GetComponents()
    {
        return GetData<Component>();
    }

    private static List<T> GetData<T>() where T : class
    {
        // Ключ кэша основан на имени типа T, чтобы различать данные разных типов
        string cacheKey = typeof(T).FullName;

        if (cacheKey == null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }
        // Используем кэш и функцию для загрузки данных из БД
        return _cache.Get(cacheKey, () => LoadDataFromDatabase<T>());
    }

    private static List<T> LoadDataFromDatabase<T>() where T : class
    {
        // Загружаем список объектов типа Component из базы данных
        try
        {
            if (typeof(T) == typeof(Component))
            {
                return dbCon.GetObjectList<Component>(includeLinks: true).Cast<T>().ToList();
            }
            else if (typeof(T) == typeof(Machine))
            {
                return dbCon.GetObjectList<Machine>(includeLinks: true).Cast<T>().ToList();
            }
            else
            {
                // Если тип не поддерживается, выбрасываем исключение или возвращаем пустой список
                Console.WriteLine($"Неизвестный тип данных: {typeof(T).Name}");
                return new List<T>();
            }

        }
        catch (Exception ex)
        {
            // Логирование ошибки или другое обработка исключений
            Console.WriteLine($"Ошибка при загрузке данных из БД: {ex.Message}");
            return new List<T>(); // Возвращаем пустой список в случае ошибки
        }
    }
}

