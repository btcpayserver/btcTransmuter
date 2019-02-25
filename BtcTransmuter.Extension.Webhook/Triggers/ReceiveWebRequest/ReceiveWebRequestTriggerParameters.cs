using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace BtcTransmuter.Extension.Webhook.Triggers.ReceiveWebRequest
{
    public class ReceiveWebRequestTriggerParameters
    {
        public string Method { get; set; }
        public string RelativeUrl { get; set; }
        public string Body { get; set; }
        public FieldComparer BodyComparer { get; set; } = FieldComparer.None;

        public enum FieldComparer
        {
            None,
            Equals,
            Contains
        }
        
    }
}