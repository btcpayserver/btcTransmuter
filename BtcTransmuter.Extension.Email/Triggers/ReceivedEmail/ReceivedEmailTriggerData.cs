using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Abstractions.ExternalServices;

namespace BtcTransmuter.Extension.Email.Triggers.ReceivedEmail
{
    public class ReceivedEmailTriggerData: IUseExternalService
    {
        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ExternalServiceId { get; set; }
    }
}