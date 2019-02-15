using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface ITriggerValidator
    {
        string TriggerId { get; }
        ICollection<ValidationResult> Validate(string data);
    }
}