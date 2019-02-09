using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Abstractions
{
    public abstract class BaseEntity
    {
        public string Id { get; set; }
        public string DataJson { get; set; }

        public virtual TData GetData<TData>()
        {
            return JObject.Parse(DataJson ?? "{}").ToObject<TData>();
        }

        public virtual void SetData<TData>(TData value)
        {
            DataJson = value == null ? "{}" : JObject.FromObject(value).ToString();
        }
    }
}