
﻿namespace TcModels.Models.TcContent
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
