using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
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
                return Regex.Replace(value, @"{{(.+?)}}",
                    match =>
                    {
                        var parameterExpressions =
                            data.Select(pair => Expression.Parameter(pair.Value.GetType(), pair.Key)).ToArray();
                        var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(parameterExpressions, null,
                            match.Groups[1].Value);
                        
                        return (e.Compile().DynamicInvoke(data.Values.ToArray()) ?? "").ToString();
                    });
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
}