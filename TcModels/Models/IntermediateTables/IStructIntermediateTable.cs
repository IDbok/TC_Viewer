﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.IntermediateTables
{
    public interface IStructIntermediateTable<P, C>: IIntermediateTable<P,C>
    {
        public int ParentId { get; set; }
        public P Parent { get; set; }
        public int ChildId { get; set; }
        public C Child { get; set; }
        public int Order { get; set; }
        public int Quantity { get; set; }
    
    }
}
