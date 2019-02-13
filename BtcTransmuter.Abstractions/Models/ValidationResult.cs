using System.Collections.Generic;

namespace BtcTransmuter.Abstractions.Models
{
    public class ValidationResult
    {
        public Dictionary<string, string> Errors { get; set; }
    }
}