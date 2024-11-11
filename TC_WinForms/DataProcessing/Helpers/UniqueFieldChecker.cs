using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TcDbConnector;
using TcModels.Models.Helpers;
using TcModels.Models.Interfaces;

namespace TC_WinForms.DataProcessing.Helpers;

public class UniqueFieldChecker<T> 
    where T : class, IHasUniqueConstraints<T>
{
    //private readonly MyDbContext _context;

    public UniqueFieldChecker(/*MyDbContext? context = null*/)
    {

    }

    /// <summary>
    /// Проверяет, является ли сущность уникальной по условиям, заданным в методе GetUniqueConstraints
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<UniqueCheckResult> IsUniqueAsync(T entity)
    {
        using (var context = new MyDbContext())
        {
            var result = new UniqueCheckResult();

            // Получаем все условия уникальности
            var constraints = entity.GetUniqueConstraints();

            // Проверяем, реализует ли объект интерфейс IIdentifiable
            int? existingId = (entity is IIdentifiable identifiableEntity) ? identifiableEntity.Id : (int?)null;

            // Проверяем каждое условие, исключая объект с таким же Id (если указан)
            foreach (var uniqueConstraint in constraints)
            {
                var query = context.Set<T>().Where(uniqueConstraint.Constraint);

                if (existingId.HasValue && existingId != 0)
                {
                    query = query.Where(e => (e as IIdentifiable)!.Id != existingId.Value);
                }

                if (await query.AnyAsync())
                {
                    // Добавляем сообщение об ошибке, если ограничение не выполнено
                    result.FailedConstraints.Add(uniqueConstraint.ErrorMessage);
                }
            }

            // Все условия уникальности выполнены
            return result;
        }

        
    }

    public static async Task<bool> IsPropertiesUnique(T obj)
    {
        var uniqueFieldChecker = new UniqueFieldChecker<T>();
        var result = await uniqueFieldChecker.IsUniqueAsync(obj);
        if (!result.IsUnique)
        {
            string message = string.Join("\n", result.FailedConstraints);

            // Сообщение об ошибки
            MessageBox.Show(message, "Ошибка при сохранении", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return false;
        }

        return true;
    }
}
