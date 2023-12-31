﻿using TcModels.Models.TcContent;

namespace TcModels.Models.IntermediateTables
{
    public class Machine_TC : IStructIntermediateTable<TechnologicalCard, Machine>
    {
        public int ChildId { get; set; }
        public Machine Child { get; set; }

        public int ParentId { get; set; }
        public TechnologicalCard Parent { get; set; }

        public int Order { get; set; }
        public int Quantity { get; set; }

        public override string ToString()
        {
            return $"{Order}.{Child.Name} (id: {ChildId}) {Quantity}";
        }
    }
}
