using System.Linq.Expressions;

namespace TcModels.Models.Helpers;

public class UniqueConstraint<T>
{
    public Expression<Func<T, bool>> Constraint { get; }
    public string ErrorMessage { get; }

    public UniqueConstraint(Expression<Func<T, bool>> constraint, string errorMessage)
    {
        Constraint = constraint;
        ErrorMessage = errorMessage;
    }
}
