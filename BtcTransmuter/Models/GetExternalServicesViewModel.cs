using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Controllers
{
    public class GetExternalServicesViewModel
    {
        public IEnumerable<ExternalServiceData> ExternalServices { get; set; }
        public IEnumerable<IExternalServiceDescriptor> Descriptors { get; set; }
    }
}