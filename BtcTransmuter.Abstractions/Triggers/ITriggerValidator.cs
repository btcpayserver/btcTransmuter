using BtcTransmuter.Abstractions.Models;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface ITriggerValidator
    {
        string TriggerId { get; }
        ValidationResult Validate(string data);
    }
}