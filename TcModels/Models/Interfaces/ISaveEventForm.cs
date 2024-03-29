using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcModels.Models.Interfaces
{
    public interface ISaveEventForm
    {
        bool GetDontSaveData();
        bool HasChanges { get; }
        Task SaveChanges();

        bool CloseFormsNoSave { get; set; }

    }
}
