using AutoMapper;
using TcDbConnector.Repositories;
using TcModels.Models;
using TcModels.Models.TcContent;

namespace TC_WinForms.Services
{
    public static class CloneObjectService
    {
        private static Dictionary<Type,CloneObjectType> keyValuePairs = new Dictionary<Type, CloneObjectType>
        {
            {typeof(TechnologicalCard),CloneObjectType.TechnologicalCard},
            {typeof(TechOperationWork), CloneObjectType.TechOperationWork},
        };

        public static TechnologicalCard CloneObject(this TechnologicalCard card)
        {
            var newCard = card.DeepCopyObject();

            foreach (var sourceTOW in card.TechOperationWorks)
            {
                sourceTOW.CloneObject(newCard);
            }

            foreach (var sourceDTW in card.DiagamToWork)
            {
                var newDTW = sourceDTW.CloneObject(newCard);
                newCard.DiagamToWork.Add(newDTW);
            }

            return newCard;
        }

        public static TechOperationWork CloneObject(this TechOperationWork sourceTOW, TechnologicalCard parentCard)
        {
            var newTOW = sourceTOW.DeepCopyObject();
            parentCard.TechOperationWorks.Add(newTOW);

            foreach (var exItem in sourceTOW.executionWorks)
            {
                var newEx = exItem.DeepCopyObject(parentCard);
                newTOW.executionWorks.Add(newEx);
            }

            return newTOW;
        }

        public static DiagamToWork CloneObject(this DiagamToWork sourceDTW, TechnologicalCard parentCard)
        {
            return sourceDTW.DeepCopyObject(parentCard);
        }

        #region TCDataClone
        private static T? DeepCopyObject<T>(this T source) where T : class
        {
            if (keyValuePairs.TryGetValue(source.GetType(), out CloneObjectType type))
            {
                T newObject;

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

        private static ExecutionWork DeepCopyObject(this ExecutionWork sourceExecutionWork, TechnologicalCard newtechnologicalCard)
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

        private static DiagamToWork DeepCopyObject(this DiagamToWork sourceDTW, TechnologicalCard newtechnologicalCard)
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
