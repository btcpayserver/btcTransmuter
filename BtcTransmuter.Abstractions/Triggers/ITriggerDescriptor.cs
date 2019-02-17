using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Abstractions.Triggers
{
    public interface ITriggerDescriptor
    {
        string TriggerId{ get;  }
        string Name{ get;}
        string Description{ get;  }
        string ViewPartial { get; }
        Task<IActionResult> EditData(RecipeTrigger data);
    }
}