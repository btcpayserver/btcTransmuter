using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Extension.Webhook.Triggers.ReceiveWebRequest
{
    public class ReceiveWebRequestTriggerData
    {
        public string Method { get; set; }= "";
        public string RelativeUrl { get; set; } = "";
        public string Body { get; set; }= "";
        
        public JObject BodyJson { get; set; }
        
    }
}