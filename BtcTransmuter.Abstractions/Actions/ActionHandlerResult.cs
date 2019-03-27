using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Abstractions.Actions
{
    public class ActionHandlerResult
    {
        public bool Executed { get; set; }
        public string Result { get; set; }
        public object Data { get; set; }

        public virtual string DataJson => JObject.FromObject(Data).ToString();
    }
}