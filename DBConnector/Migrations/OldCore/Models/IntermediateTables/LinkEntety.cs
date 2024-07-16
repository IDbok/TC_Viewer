using TcDbConnector.Migrations.OldCore.Models.Interfaces;
using TcDbConnector.Migrations.OldCore.Models.TcContent.Work;

namespace TcDbConnector.Migrations.OldCore.Models.IntermediateTables;

public class LinkEntety : IIdentifiable, IUpdatableEntity
{
    public int Id { get; set; }
    public string Link { get; set; } = null!;
    public string? Name { get; set; }
    public bool IsDefault { get; set; } = false;

    public override string ToString()
    {
        return  Link;//Name ??
    }

    public void ApplyUpdates(IUpdatableEntity source)
    {
        if (source is LinkEntety sourceObject)
        {
            Link = sourceObject.Link;
            Name = sourceObject.Name;
            IsDefault = sourceObject.IsDefault;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is LinkEntety other)
        {
            return this.Id == other.Id; // Сравнение происходит по Id
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode(); // Хэш-код зависит только от Id
    }
}
