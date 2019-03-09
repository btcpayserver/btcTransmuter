
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Webhook.Actions.MakeWebRequest
{
    public class MakeWebRequestData
    {
        [Required]
        public string Method { get; set; }
        [Required]
        public string Url { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }
    }
}