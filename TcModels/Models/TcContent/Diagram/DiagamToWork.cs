using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.TcContent
{
    public class DiagamToWork
    {
        public int Id { get; set; }
        public int techOperationWorkId { get; set; }
        public TechOperationWork techOperationWork { get; set; }
        public int technologicalCardId { get; set; }
        public TechnologicalCard technologicalCard { get; set; }
        // Временно делаю поле в формате индекс параллельности/Индекс последовательности
        /// <summary>
        /// Записывается в формате: 
        /// Индекс параллельности/Индекс последовательности.
        /// Где индекс параллельности указывает на параллельность ТО, 
        /// а индекс последовательности принадлежность
        /// к последовательности внутри одной параллельной группы
        /// </summary>
        public string? ParallelIndex { get; set; } 
        public List<DiagramParalelno> ListDiagramParalelno { get; set; } = new List<DiagramParalelno>();
        public int Order { get; set; }

        public DiagamToWork DeepCopyDTW(DiagamToWork sourceDTW, TechnologicalCard newtechnologicalCard)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OpenProfile(3));
            });

            var newDTW = new DiagamToWork();

            var mapper = config.CreateMapper();

            var currentTow = newtechnologicalCard.TechOperationWorks.Where(e => e.techOperationId == sourceDTW.techOperationWork.techOperationId && e.Order == sourceDTW.techOperationWork.Order).FirstOrDefault();
            
            

            foreach(var paral in sourceDTW.ListDiagramParalelno)
            {
                foreach(var posled in paral.ListDiagramPosledov)
                {
                    foreach(var shag in posled.ListDiagramShag)
                    {
                        foreach(var ShagToolsComponent in shag.ListDiagramShagToolsComponent)
                        {
                            if(ShagToolsComponent.toolWorkId == null)
                            {
                                var component = currentTow.ComponentWorks.Where(w => w.componentId == ShagToolsComponent.componentWork.componentId).FirstOrDefault();
                                ShagToolsComponent.componentWork = component;
                                ShagToolsComponent.componentWorkId = component.Id;
                            }
                            else
                            {
                                var tool = currentTow.ToolWorks.Where(w => w.toolId == ShagToolsComponent.toolWork.toolId).FirstOrDefault();
                                ShagToolsComponent.toolWork = tool;
                                ShagToolsComponent.toolWorkId = tool.Id;
                            }
                        }
                    }
                }
            }

            newDTW = mapper.Map<DiagamToWork>(sourceDTW);

            newDTW.techOperationWork = currentTow;
            newDTW.techOperationWorkId = currentTow.Id;
            newDTW.ListDiagramParalelno.ForEach(e => e.techOperationWorkId = currentTow.Id);
            newDTW.ListDiagramParalelno.ForEach(e => e.techOperationWork = currentTow);

            return newDTW;
        }

        public string? GetParallelIndex()
        {
            return ParallelIndex?.Split('/')[0];
        }
        public string? GetSequenceIndex()
        {
            return ParallelIndex?.Split('/').Length > 1 ? ParallelIndex?.Split('/')[1] : null;
        }
    }
}
