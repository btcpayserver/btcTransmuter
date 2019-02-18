using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class EditExternalServiceViewModel
    {
        public ExternalServiceData ExternalServiceData { get; set; }
        public string StatusMessage { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}