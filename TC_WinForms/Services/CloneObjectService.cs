using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.Services
{
    

    public class CloneObjectService
    {
        public Dictionary<Type,CloneObjectType> keyValuePairs = new Dictionary<Type, CloneObjectType>
        {
            {typeof(TechnologicalCard),CloneObjectType.TechnologicalCard},
            {typeof(TechOperationWork), CloneObjectType.TechOperationWork},
        };

        public async Task<TechnologicalCard> CloneTC(int tcId)
        {
            TechnologicalCardRepository technologicalCardRepository = new TechnologicalCardRepository();

            var card = await technologicalCardRepository.GetTCDataAsync(tcId, false);
            card.TechOperationWorks = await technologicalCardRepository.GetTOWDataAsync(tcId, false);
            card.DiagamToWork = await technologicalCardRepository.GetDTWDataAsync(tcId, false);

            var newCard = DeepCopyObject(card);
            newCard.Article += "(copy)";

            foreach (var item in card.TechOperationWorks)
            {
                var newTOW = DeepCopyObject(item);
                newCard.TechOperationWorks.Add(newTOW);
                foreach (var exItem in item.executionWorks)
                {
                    var newEx = DeepCopyEWData(exItem, newCard);
                    newTOW.executionWorks.Add(newEx);
                }
            }

            foreach (var item in card.DiagamToWork)
            {
                var newDTW = DeepCopyDTW(item, newCard);
                newCard.DiagamToWork.Add(newDTW);
            }

           return newCard;
        } 

        private T? DeepCopyObject<T>(T source) where T : class
        {
            if(keyValuePairs.TryGetValue(source.GetType(), out CloneObjectType type))
            {
                T newObject;

                if (type == CloneObjectType.TechnologicalCard || type == CloneObjectType.TechOperationWork)
                {
                    var config = new MapperConfiguration(cfg =>
                    {
                        cfg.AddProfile(new OpenProfile(type));
                    });

                    var mapper = config.CreateMapper();

                    newObject = mapper.Map<T>(source);

                    return newObject;
                }
                else
                    return null;
            }
            else
                return null;
        }

        #region TCDataClone

        public ExecutionWork DeepCopyEWData(ExecutionWork sourceExecutionWork, TechnologicalCard newtechnologicalCard)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OpenProfile(CloneObjectType.ExecutionWork));
            });
            var newExecutionWork = new ExecutionWork();

            var mapper = config.CreateMapper();

            newExecutionWork = mapper.Map<ExecutionWork>(sourceExecutionWork);

            newExecutionWork.Staffs.AddRange
                (newtechnologicalCard.Staff_TCs.Where(s => sourceExecutionWork.Staffs.Exists(staffs => staffs.ChildId == s.ChildId && staffs.Order == s.Order && staffs.Symbol == s.Symbol))
                                               .ToList());

            newExecutionWork.Protections.AddRange
                (newtechnologicalCard.Protection_TCs.Where(s => sourceExecutionWork.Protections.Exists(staffs => staffs.ChildId == s.ChildId && staffs.Order == s.Order))
                                                    .ToList());

            newExecutionWork.Machines.AddRange
                (newtechnologicalCard.Machine_TCs.Where(s => sourceExecutionWork.Machines.Exists(staffs => staffs.ChildId == s.ChildId && staffs.Order == s.Order))
                                                 .ToList());

            if (sourceExecutionWork.Repeat)
            {
                var repeats = sourceExecutionWork.ExecutionWorkRepeats.Select(e => e.ChildExecutionWork).ToList();
                var repeatsTOWs = repeats.Select(e => e.techOperationWork).Distinct().ToList();

                foreach (var tow in repeatsTOWs)
                {
                    var currentEWs = newtechnologicalCard.TechOperationWorks.Where(e => e.techOperationId == tow.techOperationId && e.Order == tow.Order).FirstOrDefault()?.executionWorks.ToList();

                    var currentRepeats = currentEWs.Where(e => repeats.Exists(ex => ex.techTransitionId == e.techTransitionId && ex.Order == e.Order && ex.techOperationWorkId == tow.Id)).ToList();

                    foreach (var repeat in currentRepeats)
                    {
                        newExecutionWork.ExecutionWorkRepeats.Add(
                            new ExecutionWorkRepeat
                            {
                                ChildExecutionWorkId = repeat.Id,
                                ChildExecutionWork = repeat,
                                ParentExecutionWork = newExecutionWork,
                                ParentExecutionWorkId = newExecutionWork.Id,
                            });
                    }
                }
            }

            return newExecutionWork;
        }

        public DiagamToWork DeepCopyDTW(DiagamToWork sourceDTW, TechnologicalCard newtechnologicalCard)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OpenProfile(CloneObjectType.DiagamToWork));
            });

            var newDTW = new DiagamToWork();

            var mapper = config.CreateMapper();

            var currentTOW = newtechnologicalCard.TechOperationWorks.Where(e => e.techOperationId == sourceDTW.techOperationWork.techOperationId && e.Order == sourceDTW.techOperationWork.Order).FirstOrDefault();

            foreach (var paral in sourceDTW.ListDiagramParalelno)
            {
                foreach (var posled in paral.ListDiagramPosledov)
                {
                    foreach (var shag in posled.ListDiagramShag)
                    {
                        foreach (var ShagToolsComponent in shag.ListDiagramShagToolsComponent)
                        {
                            if (ShagToolsComponent.toolWorkId == null)
                            {
                                var component = currentTOW.ComponentWorks.Where(w => w.componentId == ShagToolsComponent.componentWork.componentId).FirstOrDefault();
                                ShagToolsComponent.componentWork = component;
                                ShagToolsComponent.componentWorkId = component.Id;
                            }
                            else
                            {
                                var tool = currentTOW.ToolWorks.Where(w => w.toolId == ShagToolsComponent.toolWork.toolId).FirstOrDefault();
                                ShagToolsComponent.toolWork = tool;
                                ShagToolsComponent.toolWorkId = tool.Id;
                            }
                        }
                    }
                }
            }

            newDTW = mapper.Map<DiagamToWork>(sourceDTW);

            newDTW.techOperationWork = currentTOW;
            newDTW.techOperationWorkId = currentTOW.Id;
            newDTW.ListDiagramParalelno.ForEach(e => e.techOperationWorkId = currentTOW.Id);
            newDTW.ListDiagramParalelno.ForEach(e => e.techOperationWork = currentTOW);

            return newDTW;
        }

        #endregion

    }
}
