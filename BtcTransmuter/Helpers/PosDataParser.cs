using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Helpers
{
    public class PosDataParser
    {
        public static Dictionary<string, object> ParsePosData(string posData)
        {
            var result = new Dictionary<string,object>();
            if (string.IsNullOrEmpty(posData))
            {
                return result;
            }
            
            try
            {
                var jObject =JObject.Parse(posData);
                foreach (var item in jObject)
                {
                    
                    switch (item.Value.Type)
                    {
                        case JTokenType.Array:
                            var items = item.Value.AsEnumerable().ToList();
                            for (var i = 0; i < items.Count(); i++)
                            {
                                result.Add($"{item.Key}[{i}]", ParsePosData(items[i].ToString()));
                            }
                            break;
                        case JTokenType.Object:
                            result.Add(item.Key, ParsePosData(item.Value.ToString()));
                            break;
                        default:
                            result.Add(item.Key, item.Value.ToString());
                            break;
                    }
                    
                }
            }
            catch
            {
                result.Add(string.Empty, posData);
            }
            return result;
        }
    }
}