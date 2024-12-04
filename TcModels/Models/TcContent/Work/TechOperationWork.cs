using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TcModels.Models.TcContent
{
    public class TechOperationWork
    {
        public int Id { get; set; }

        public TechOperation techOperation { get; set; }
        public int techOperationId { get; set; }

        public TechnologicalCard technologicalCard { get; set; }
        public int TechnologicalCardId { get; set; }

        public  List<ToolWork> ToolWorks { get; set; } =new List<ToolWork>();

        public List<ComponentWork> ComponentWorks { get; set; } = new List<ComponentWork>();

        public ICollection<ExecutionWork> executionWorks { get; set; } = new List<ExecutionWork>();
        /// <summary>
        /// Хранилище переменных для параллельных операций в формате int / int,
        /// где первое число - это индекс параллельность операции, второе - индекс группы последовательных операций
        /// ( ParallelIndex / SequenceGroupIndex )
        /// </summary>
        private string? ParallelIndex { get; set; }
        public List<DiagramParalelno> ListDiagramParalelno { get; set; } = new List<DiagramParalelno>();

        [NotMapped] public bool Delete { get; set; } = false;
        [NotMapped] public bool NewItem { get; set; } = false;
        
       public int Order { get; set; }

        public override string ToString()
        {
            return techOperation.Name;
        }

        public TechOperationWork()
        {
            
        }

        public TechOperationWork(TechnologicalCard technologicalCard, TechOperation techOperation, 
            int order, string? parallelIndex)
        {
            this.technologicalCard = technologicalCard;
            this.techOperation = techOperation;
            Order = order;
            ParallelIndex = parallelIndex;
        }

        public int? GetParallelIndex()
        {
            if (ParallelIndex == null)
                return null;

            var index = ParallelIndex.Split('/');

            if (int.TryParse(index[0], out var result))
                return result;

            return null; // Возвращаем null, если парсинг не удался
        }

        public void SetParallelIndex(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "ParallelIndex cannot be less than 0");

            if (index == 0)
            {
                ParallelIndex = null;
                return;
            }

            var groupIndex = GetSequenceGroupIndex();

            ParallelIndex = groupIndex == null
                ? index.ToString()
                : index + "/" + groupIndex;
        }

        public int? GetSequenceGroupIndex()
        {
            if (ParallelIndex == null)
                return null;

            var index = ParallelIndex.Split('/');

            if (index.Length > 1 && int.TryParse(index[1], out var result))
                return result;

            return null;
        }

        public void SetSequenceGroupIndex(int index)
        {
            if (ParallelIndex == null)
                throw new InvalidOperationException("ParallelIndex is null");

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "SequenceGroupIndex cannot be less than 0");

            var parallelIndex = GetParallelIndex();
            if (parallelIndex == null)
                throw new InvalidOperationException("ParallelIndex is null");

            ParallelIndex = parallelIndex + "/" + index;
        }

        public string? GetParallelIndexString()
        {
            return ParallelIndex;
        }

        public TechOperationWork CopyTOWList()
        {
            TechOperationWork newTOW = new TechOperationWork
            {
                Id = 0,
                TechnologicalCardId = this.TechnologicalCardId,
                techOperation = this.techOperation,
                techOperationId = this.techOperationId,
                Order = this.Order,
                ParallelIndex = this.ParallelIndex,
            };

            foreach(var item in this.ToolWorks)
            {
                item.Id = 0;
                item.techOperationWorkId = newTOW.Id;
                item.techOperationWork = null;
                var newItem = item.CloneJson();
                newTOW.ToolWorks.Add(newItem);
            }

            foreach (var item in this.ComponentWorks)
            {
                item.Id = 0;
                item.techOperationWorkId = newTOW.Id;
                item.techOperationWork = null;
                var newItem = item.CloneJson();
                newTOW.ComponentWorks.Add(newItem);
            }

            //foreach (var item in this.executionWorks)
            //{
            //    item.Id = 0;
            //    item.techOperationWorkId = newTOW.Id;
            //    item.techOperationWork = null;
            //    var newItem = item.CopyEW();
            //    newTOW.executionWorks.Add(newItem);
            //}

            return newTOW;
        }

    }
}
