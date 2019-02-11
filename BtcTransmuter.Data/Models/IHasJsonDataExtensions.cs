using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Data.Models
{
    public static class IHasJsonDataExtensions
    {
        public static T Get<T>(this IHasJsonData i)
        {
            return JObject.Parse(i.DataJson ?? "{}").ToObject<T>();
        }

        public static void Set<T>(this IHasJsonData i, T data)
        {
            i.DataJson = JObject.FromObject(data).ToString();
        }
    }
}