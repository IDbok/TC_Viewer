using System.Linq.Expressions;

namespace TcModels.Models.Interfaces;

public interface IHasUniqueConstraints<T>
{
    IEnumerable<Expression<Func<T, bool>>> GetUniqueConstraints();
}

