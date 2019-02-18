using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BtcTransmuter.Controllers
{
    public class GetExternalServicesViewModel
    {
        public IEnumerable<ExternalServiceData> ExternalServices { get; set; }
        public IEnumerable<IExternalServiceDescriptor> Descriptors { get; set; }
        public string StatusMessage { get; set; }
    }

    public class CreateExternalServiceViewModel
    {
        public string StatusMessage { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public SelectList Types { get; set; }
    }

    public class EditExternalServiceViewModel
    {
        public ExternalServiceData ExternalServiceData { get; set; }
        public string StatusMessage { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}