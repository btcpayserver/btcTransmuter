using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Abstractions.Helpers
{
    public class ValidationHelper
    {
        public static ICollection<ValidationResult> Validate<T>(string data)
        {
            try
            { 
                var parsedData = JsonConvert.DeserializeObject<T>(data);
                var vc = new ValidationContext(parsedData);
                var result = new List<ValidationResult>();
                Validator.TryValidateObject(data, vc, result);
                return result;
            }
            catch (Exception)
            {
                return new List<ValidationResult>()
                {
                    new ValidationResult("Could not parse action data")
                };
            }
        }
    }
}