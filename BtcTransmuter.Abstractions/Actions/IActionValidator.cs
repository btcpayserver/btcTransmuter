using BtcTransmuter.Abstractions.Models;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionValidator
    {
        string ActionId { get; }
        ValidationResult Validate(string data);
    }
}