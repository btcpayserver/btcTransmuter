using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using BtcTransmuter.Abstractions.Extensions;
using Newtonsoft.Json;

namespace BtcTransmuter.Abstractions.Helpers
{
    public class TransmuterInterpolationTypeProvider : DefaultDynamicLinqCustomTypeProvider
    {
        private readonly Type[] _types;

        public TransmuterInterpolationTypeProvider(params Type[] types)
        {
            _types = types;
        }
        public override HashSet<Type> GetCustomTypes()
        {
            return _types.ToHashSet();
        }
    }
    
    public sealed class InterpolationTypeProvider : DefaultDynamicLinqCustomTypeProvider {
        private readonly IEnumerable<DefaultDynamicLinqCustomTypeProvider> _typeProviders;

        public InterpolationTypeProvider(IEnumerable<TransmuterInterpolationTypeProvider> typeProviders)
        {
            _typeProviders = typeProviders;
            ParsingConfig.Default.CustomTypeProvider = this;
            
        }

        public override HashSet<Type> GetCustomTypes()
        {
            return _typeProviders.SelectMany(provider => provider.GetCustomTypes()).ToHashSet();
        }
    }
    public static class InterpolationHelper
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
                        var processed = match.Groups[1].Value;
                        try
                        {
                            var e = DynamicExpressionParser.ParseLambda(parameterExpressions.ToArray(), null,
                                processed);
                      
                            return (e.Compile().DynamicInvoke(data.Values.ToArray()) ?? "").ToString();
                        }
                        catch (Exception)
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
    }
}