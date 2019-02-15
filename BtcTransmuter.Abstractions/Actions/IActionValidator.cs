using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionValidator
    {
        string ActionId { get; }
        ICollection<ValidationResult> Validate(string data);
    }
}