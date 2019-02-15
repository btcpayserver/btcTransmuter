using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IExternalServiceValidator
    {
        string ExternalServiceType { get; }
        ICollection<ValidationResult> Validate(string data);
    }
}