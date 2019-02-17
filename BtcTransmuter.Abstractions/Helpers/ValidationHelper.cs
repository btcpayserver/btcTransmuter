using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

public class ValidationHelper
{
    public static ICollection<ValidationResult> Validate<T>(string data)
    {
        try
        {
            var parsedData = JObject.Parse(data).ToObject<T>();
            var vc = new ValidationContext(parsedData);
            var result = new List<ValidationResult>();
            Validator.TryValidateObject(data, vc, result);
            return result;
        }
        catch (Exception e)
        {
            return new List<ValidationResult>()
            {
                new ValidationResult("Could not parse action data")
            };
        }
    }

    
    
}