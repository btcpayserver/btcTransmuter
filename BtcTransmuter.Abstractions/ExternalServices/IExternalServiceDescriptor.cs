using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Abstractions.ExternalServices
{
    public interface IExternalServiceDescriptor
    {
        string ExternalServiceType { get;  }
        string Name { get;  }
        string Description { get;}
        string ViewPartial { get; }
        Task<IActionResult> EditData(ExternalServiceData data);


    }
}