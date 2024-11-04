using TcModels.Models.Helpers;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent.Work;

namespace TcModels.Models.TcContent;

//1. Требования к составу бригады и квалификации
public class Staff : INameable, IDGViewable, IUpdatableEntity, IRequiredProperties, IVersionable, IReleasable,
    IValidatable, IHasUniqueConstraints<Staff>

{
    public static List<string> GetChangeablePropertiesNames { get; } = new List<string>
        {
            nameof(Name),
            nameof(Type),
            nameof(Functions),
            nameof(CombineResponsibility),
            nameof(Qualification),
            nameof(Comment),

        };
    
    public Dictionary<string, string> GetPropertiesNames { get; } = new Dictionary<string, string>
        {
            { nameof(Id), "ID" },
            { nameof(Name), "Название" },
            { nameof(Type), "Тип" },
            { nameof(Functions), "Функции" },
            { nameof(CombineResponsibility), "Возможность совмещения обязанностей" },
            { nameof(Qualification), "Квалификация" },
            { nameof(Comment), "Комментарии" },
        };
    
    public static Dictionary<string, int> GetPropertiesOrder { get; } = new Dictionary<string, int>
        {
            { nameof(Id), 0 },
            { nameof(Name), 1 },
            { nameof(Type), 2 },
            { nameof(Functions), 3 },
            { nameof(CombineResponsibility), 4 },
            { nameof(Qualification), 5 },
            { nameof(Comment), 6 },

        };
    
    public List<string> GetPropertiesRequired { get; } = new List<string> // todo: переделать на интерфейс IVaidatable
        {
            nameof(Name) ,
            nameof(Type) ,
            nameof(Functions) ,
            nameof(Qualification),
        };
    
    static private EModelType modelType = EModelType.Staff;
    public EModelType ModelType { get { return modelType; } }

    public List<TechnologicalCard> TechnologicalCards { get; set; } = new();
    public List<Staff_TC> Staff_TCs { get; set; } = new();
    public Staff()
    {

    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Functions { get; set; }
    public string? CombineResponsibility { get; set; }
    public string Qualification { get; set; }
    public string? Comment { get; set; }

    public bool IsReleased { get; set; } = false;
    public int? CreatedTCId { get; set; } = null;

    public int OriginalId { get; set; }
    public int Version { get; set; }
    public DateTime UpdateDate { get; set; }

    public List<Staff> RelatedStaffs { get; private set; } = new List<Staff>();
    public string? ClassifierCode { get; set; }
    

    public void ApplyUpdates(IUpdatableEntity source)
    {
        if (source is Staff sourceCard)
        {
            Name = sourceCard.Name;
            Type = sourceCard.Type;
            Functions = sourceCard.Functions;
            CombineResponsibility = sourceCard.CombineResponsibility;
            Qualification = sourceCard.Qualification;
            Comment = sourceCard.Comment;
            CompareRelatedStaffs( sourceCard.RelatedStaffs);

            IsReleased = sourceCard.IsReleased;
            CreatedTCId = sourceCard.CreatedTCId;

            ClassifierCode = sourceCard.ClassifierCode;

            OriginalId = sourceCard.OriginalId;
        }
    }
    private void CompareRelatedStaffs(List<Staff> sourceRelatedStaffs)
    {
        var relatedStaffsToRemove = new List<Staff>();
        foreach (var relatedStaff in RelatedStaffs)
        {
            if (!sourceRelatedStaffs.Contains(relatedStaff))
            {
                relatedStaffsToRemove.Add(relatedStaff);
            }
        }

        foreach (var relatedStaff in relatedStaffsToRemove)
        {
            RelatedStaffs.Remove(relatedStaff);
        }

        foreach (var relatedStaff in sourceRelatedStaffs)
        {
            if (!RelatedStaffs.Contains(relatedStaff))
            {
                RelatedStaffs.Add(relatedStaff);
            }
        }
    }

    public string ToSring()
    {
        return $"{Id} {Name} {Type} {CombineResponsibility} {Qualification}" +
            $"\n{Comment}";
    }

    public override bool Equals(object obj)
    {
        if (obj is Staff other)
        {
            return this.Id == other.Id; // Сравнение происходит по Id
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode(); // Хэш-код зависит только от Id
    }


    public void UpdateCombineResponsibility()
    {
        if (RelatedStaffs != null)
        {
            CombineResponsibility = string.Join(",\n", RelatedStaffs.Select(r => $"{r.Name}: {r.Type}"));
        }
    }

    public void AddRelatedStaff(Staff staff)
    {
        var relationship = RelatedStaffs.FirstOrDefault(r => r.Id == staff.Id);
        if (relationship != null)
        {
            return;
        }

        RelatedStaffs.Add(staff);
        UpdateCombineResponsibility();
    }

    public void RemoveRelatedStaff(Staff staff)
    {
        var relationship = RelatedStaffs.FirstOrDefault(r => r.Id == staff.Id);
        if (relationship != null)
        {
            RelatedStaffs.Remove(relationship);
            UpdateCombineResponsibility();
        }
    }

    public void ReplaceRelatedStaffs(List<Staff> newRelatedStaffs)
    {
        RelatedStaffs.Clear();
        RelatedStaffs.AddRange(newRelatedStaffs);
        UpdateCombineResponsibility();
    }

    public string[] GetRequiredProperties()
    {
        return new[] {

                nameof(Name) ,
                nameof(Type) ,
                nameof(Functions) ,
                nameof(Qualification),
            };
    }

    public IEnumerable<UniqueConstraint<Staff>> GetUniqueConstraints()
    {
        // Возвращаем условия уникальности
        yield return new UniqueConstraint<Staff>(
            x => x.Name == this.Name && x.Type == this.Type,
            "Комбинация полей 'Наименование' и 'Тип' должна быть уникальной."
        );

        if (!string.IsNullOrEmpty(this.ClassifierCode))
        {
            yield return new UniqueConstraint<Staff>(
                x => x.ClassifierCode == this.ClassifierCode,
                "Поле 'Код в classifier' должно быть уникальным."
            );
        }
    }
}
