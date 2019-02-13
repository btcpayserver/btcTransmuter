using BtcTransmuter.Abstractions.Models;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IExternalServiceValidator
    {
        string ExternalServiceType { get; }
        ValidationResult Validate(string data);
    }
}