using Microsoft.AspNetCore.Mvc.Rendering;

namespace BtcTransmuter.Models
{
    public class CreateExternalServiceViewModel
    {
        public string StatusMessage { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public SelectList Types { get; set; }
    }
}