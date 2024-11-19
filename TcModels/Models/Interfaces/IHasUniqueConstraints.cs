using TcModels.Models.Helpers;

namespace TcModels.Models.Interfaces;

public interface IHasUniqueConstraints<T>
{
    IEnumerable<UniqueConstraint<T>> GetUniqueConstraints();
}


