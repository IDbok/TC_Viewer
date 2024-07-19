using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcDbConnector.Migrations.OldCore.Models.TcContent.Work
{
    public class MaxEW
    {
        [Key]
        public Guid guid { get; set; }
        public ExecutionWork? executionWork { get; set; }

        public SumEW? sumEW { get; set; }
    }
}
