using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcDbConnector.Interfaces
{
    interface IRepository<T> where T : class
    {
        IEnumerable<T> GetListObjects(); // получение всех объектов
        Task<T> GetObject(int id); // получение одного объекта по id
        Task<List<T>> GetObjects(int id); // получение списка объектов по id
        Task CreateObject(T item); // создание объекта
        Task<bool> CreateObject(List<T> item); // создание объекта
        Task UpdateObject(List<T> item); // обновление объекта
        Task<bool> DeleteObject(List<int> id); // удаление объекта по id
    }
}
