using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Abstractions
{
    public abstract class BaseTrigger<TData> : ITrigger<TData>
    {
        public virtual string Id => GetType().FullName;
        public abstract string Name { get; }
        public string ParametersJson { get; set; }

        public virtual TData Data
        {
            get => JObject.Parse(ParametersJson ?? "{}").ToObject<TData>();
            set => ParametersJson = value == null ? "{}" : JObject.FromObject(value).ToString();
        }
    }
}