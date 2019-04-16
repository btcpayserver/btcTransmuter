using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Data.Models
{
    public static class IHasJsonDataExtensions
    {
        public static T Get<T>(this IHasJsonData i)
        {
            return JsonConvert.DeserializeObject<T>(i.DataJson ?? "{}");
        }

        public static void Set<T>(this IHasJsonData i, T data)
        {
            i.DataJson =  JsonConvert.SerializeObject(data);
        }
    }
}