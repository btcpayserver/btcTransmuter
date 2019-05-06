using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace BtcTransmuter.Abstractions.Helpers
{
    public class InterpolationHelper
    {

        /// <summary>
        /// https://dotnetfiddle.net/MoqJFk
        /// </summary>
        public static string InterpolateString(string value, Dictionary<string, object> data)
        {
            try
            {
            
                var parameterExpressions =
                    data.Select(pair => Expression.Parameter(pair.Value.GetType(), pair.Key)).ToList();
             
                return Regex.Replace(value, @"{{(.+?)}}",
                    match =>
                    {
                        var processed =  JsonFunc(match.Groups[1].Value, data);
                        try
                        {
                            var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(parameterExpressions.ToArray(), null,
                                processed);
                      
                            return (e.Compile().DynamicInvoke(data.Values.ToArray()) ?? "").ToString();
                        }
                        catch (Exception exception)
                        {
                            return processed;
                        }
                       
                    });
            }
            catch (Exception)
            {
                return value;
            }
        }

        private static string JsonFunc(string value, Dictionary<string, object> data)
        {
            try
            {
                return Regex.Replace(value, @"ToJson\({1}(.+?)\){1}",
                    match =>
                    {
                        
                        var parameterExpressions =
                            data.Select(pair => Expression.Parameter(pair.Value.GetType(), pair.Key)).ToList();
             
                      
                                var processed =  JsonFunc(match.Groups[1].Value, data);
                                var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(parameterExpressions.ToArray(), null,
                                    processed);
                      
                        return JsonConvert.SerializeObject(e.Compile().DynamicInvoke(data.Values.ToArray()));
                    });
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
}