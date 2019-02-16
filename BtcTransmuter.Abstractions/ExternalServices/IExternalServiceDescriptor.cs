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
        IActionResult EditData(ExternalServiceData data);


    }
}