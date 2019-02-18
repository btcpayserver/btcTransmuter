using System.Collections.Generic;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class GetExternalServicesViewModel
    {
        public IEnumerable<ExternalServiceData> ExternalServices { get; set; }
        public IEnumerable<IExternalServiceDescriptor> Descriptors { get; set; }
        public string StatusMessage { get; set; }
    }
}