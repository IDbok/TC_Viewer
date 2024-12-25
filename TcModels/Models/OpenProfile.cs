using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TcModels.Models
{
    public enum CloneObjectType
    {
        TechnologicalCard,
        TechOperationWork,
        ExecutionWork,
        DiagamToWork
    }

    public class OpenProfile: Profile
    {
          public OpenProfile(CloneObjectType type)//В зависимости от значения создаем профиль карт нужного объекта
        {
            switch(type)
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
                        .ForMember(dest => dest.executionWorks, opt => opt.Ignore());

            CreateMap<ToolWork, ToolWork>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.techOperationWorkId, opt => opt.Ignore());

            CreateMap<ComponentWork, ComponentWork>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
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
                        .ForMember(dest => dest.ExecutionWorkRepeats, opt => opt.Ignore());
        }

        public void TCProfile()
        {
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
                        .ForMember(dest => dest.DiagamToWork, opt => opt.Ignore());
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
                        .ForMember(dest => dest.DiagramPosledovId, opt => opt.Ignore());
                        //.ForMember(dest => dest.ListDiagramShagToolsComponent, opt => opt.Ignore());


            CreateMap<DiagramShagToolsComponent, DiagramShagToolsComponent>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        //.ForMember(dest => dest.componentWorkId, opt => opt.Ignore())
                        //.ForMember(dest => dest.componentWork, opt => opt.Ignore())
                        //.ForMember(dest => dest.toolWorkId, opt => opt.Ignore())
                        //.ForMember(dest => dest.toolWork, opt => opt.Ignore())
                        .ForMember(dest => dest.DiagramShagId, opt => opt.Ignore());

        }
    }
}
