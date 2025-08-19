using AutoMapper;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

public enum CloneObjectType
{
    TechnologicalCard,
    TechOperationWork,
    ExecutionWork,
    DiagamToWork
}

public class OpenProfile : Profile
{
    
    public OpenProfile(CloneObjectType type)
    {
        switch (type)
        {
            case CloneObjectType.TechnologicalCard:
                TCProfile();
                break;
            case CloneObjectType.TechOperationWork:
                TOWProfile();
                break;
            case CloneObjectType.ExecutionWork:
                EWProfile();
                break;
            case CloneObjectType.DiagamToWork:
                DTWProfile();
                break;
        }
    }

    public void TOWProfile()
    {
        CreateMap<TechOperationWork, TechOperationWork>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TechnologicalCardId, opt => opt.Ignore())
            .ForMember(dest => dest.executionWorks, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                var parallelIndex = src.GetParallelIndex();
                var sequenceGroupIndex = src.GetSequenceGroupIndex();

                if (parallelIndex.HasValue)
                {
                    if (sequenceGroupIndex.HasValue)
                        dest.SetSequenceGroupIndex(sequenceGroupIndex.Value);

                    dest.SetParallelIndex(parallelIndex.Value);
                }
            });

        CreateMap<ToolWork, ToolWork>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Remark, opt => opt.Ignore())
            .ForMember(dest => dest.Reply, opt => opt.Ignore())
            .ForMember(dest => dest.IsRemarkClosed, opt => opt.Ignore())
            .ForMember(dest => dest.techOperationWorkId, opt => opt.Ignore());

        CreateMap<ComponentWork, ComponentWork>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Remark, opt => opt.Ignore())
            .ForMember(dest => dest.Reply, opt => opt.Ignore())
            .ForMember(dest => dest.IsRemarkClosed, opt => opt.Ignore())
            .ForMember(dest => dest.techOperationWorkId, opt => opt.Ignore());
    }

    public void EWProfile()
    {
        CreateMap<ExecutionWork, ExecutionWork>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.techOperationWorkId, opt => opt.Ignore())
            .ForMember(dest => dest.Staffs, opt => opt.Ignore())
            .ForMember(dest => dest.Protections, opt => opt.Ignore())
            .ForMember(dest => dest.Machines, opt => opt.Ignore())
            .ForMember(dest => dest.ExecutionWorkRepeats, opt => opt.Ignore())
            .ForMember(dest => dest.Remark, opt => opt.Ignore())
            .ForMember(dest => dest.Reply, opt => opt.Ignore())
            .ForMember(dest => dest.IsRemarkClosed, opt => opt.Ignore())
            .ForMember(dest => dest.ImageList, opt => opt.Ignore()); // Добавлено для игнорирования связей
    }

    public void TCProfile()
    {
        // Настройка копирования ImageOwner с созданием нового ImageStorage
        CreateMap<ImageOwner, ImageOwner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TechnologicalCardId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageStorageId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageStorage, opt => opt.MapFrom(src =>
                new ImageStorage
                {
                    ImageBase64 = src.ImageStorage.ImageBase64,
                    Name = src.ImageStorage.Name,
                    Category = src.ImageStorage.Category,
                    MimeType = src.ImageStorage.MimeType,
                    StorageType = src.ImageStorage.StorageType,
                    FilePath = src.ImageStorage.FilePath
                }))
            .ForMember(dest => dest.ExecutionWorks, opt => opt.Ignore())
            .ForMember(dest => dest.DiagramShags, opt => opt.Ignore());

        CreateMap<ImageStorage, ImageStorage>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Остальные маппинги для TC
        CreateMap<Staff_TC, Staff_TC>()
            .ForMember(dest => dest.ParentId, opt => opt.Ignore())
            .ForMember(dest => dest.IdAuto, opt => opt.Ignore());

        CreateMap<Component_TC, Component_TC>()
            .ForMember(dest => dest.ParentId, opt => opt.Ignore());

        CreateMap<Tool_TC, Tool_TC>()
            .ForMember(dest => dest.ParentId, opt => opt.Ignore());

        CreateMap<Protection_TC, Protection_TC>()
            .ForMember(dest => dest.ParentId, opt => opt.Ignore());

        CreateMap<Machine_TC, Machine_TC>()
            .ForMember(dest => dest.ParentId, opt => opt.Ignore());

        CreateMap<Coefficient, Coefficient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<TechnologicalCard, TechnologicalCard>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TechOperationWorks, opt => opt.Ignore())
            .ForMember(dest => dest.DiagamToWork, opt => opt.Ignore())
            .ForMember(dest => dest.ImageList, opt => opt.Ignore());
    }

    public void DTWProfile()
    {
        CreateMap<DiagamToWork, DiagamToWork>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.technologicalCardId, opt => opt.Ignore())
            .ForMember(dest => dest.techOperationWorkId, opt => opt.Ignore())
                        .ForMember(dest => dest.techOperationWork, opt => opt.Ignore());

        CreateMap<DiagramParalelno, DiagramParalelno>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DiagamToWorkId, opt => opt.Ignore())
            .ForMember(dest => dest.techOperationWorkId, opt => opt.Ignore())
                        .ForMember(dest => dest.techOperationWork, opt => opt.Ignore());


        CreateMap<DiagramPosledov, DiagramPosledov>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.DiagramParalelnoId, opt => opt.Ignore());

        CreateMap<DiagramShag, DiagramShag>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Remark, opt => opt.Ignore())
            .ForMember(dest => dest.Reply, opt => opt.Ignore())
            .ForMember(dest => dest.IsRemarkClosed, opt => opt.Ignore())
            .ForMember(dest => dest.DiagramPosledovId, opt => opt.Ignore());

        CreateMap<DiagramShagToolsComponent, DiagramShagToolsComponent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DiagramShagId, opt => opt.Ignore());
    }
}
