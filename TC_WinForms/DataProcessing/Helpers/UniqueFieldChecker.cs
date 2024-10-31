using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TcDbConnector;
using TcModels.Models.Interfaces;

namespace TC_WinForms.DataProcessing.Helpers;

public class UniqueFieldChecker<T> 
    where T : class, IHasUniqueConstraints<T>
{
    private readonly MyDbContext _context;

    public UniqueFieldChecker(MyDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Проверяет, является ли сущность уникальной по условиям, заданным в методе GetUniqueConstraints
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<bool> IsUniqueAsync(T entity)
    {
        // Получаем все условия уникальности
        var constraints = entity.GetUniqueConstraints();

        // Проверяем, реализует ли объект интерфейс IIdentifiable
        int? existingId = (entity is IIdentifiable identifiableEntity) ? identifiableEntity.Id : (int?)null;

        // Проверяем каждое условие, исключая объект с таким же Id (если указан)
        foreach (var constraint in constraints)
        {
            var query = _context.Set<T>().Where(constraint);

            if (existingId.HasValue)
            {
                query = query.Where(e => (e as IIdentifiable)!.Id != existingId.Value);
            }

            if (await query.AnyAsync())
            {
                // Если хотя бы одно условие не выполнено, возвращаем false
                return false;
            }

            //var combinedConstraint = constraint;

            //// Если Id указан, изключаем сравнение с объектом с таким же Id
            //if (existingId.HasValue)
            //{
            //    // Объединяем текущее условие уникальности с проверкой на исключение Id
            //    var parameter = Expression.Parameter(typeof(T));
            //    var idProperty = Expression.Property(parameter, "Id");
            //    var idComparison = Expression.NotEqual(idProperty, Expression.Constant(existingId.Value));

            //    // Создаем комбинированное условие
            //    combinedConstraint = Expression.Lambda<Func<T, bool>>(
            //        Expression.AndAlso(constraint.Body, idComparison),
            //        constraint.Parameters[0]
            //    );
            //}

            //if (await _context.Set<T>().AnyAsync(combinedConstraint))
            //{
            //    // Если хотя бы одно условие не выполнено, возвращаем false
            //    return false;
            //}
        }

        // Все условия уникальности выполнены
        return true;
    }
}
