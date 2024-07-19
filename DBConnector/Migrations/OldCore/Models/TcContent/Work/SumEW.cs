using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcDbConnector.Migrations.OldCore.Models.TcContent.Work
{

    public class SumEW
    {
        [Key]
        public Guid guid { get; set; }
        public ExecutionWork executionWork { get; set; }
    }
}
