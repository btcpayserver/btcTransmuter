using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class ViewExternalServiceViewModel
    {
        public ExternalServiceData ExternalServiceData { get; set; }
        public IExternalServiceDescriptor ExternalServiceDescriptor { get; set; }
        public bool ShowAllData { get; set; }
    }
}