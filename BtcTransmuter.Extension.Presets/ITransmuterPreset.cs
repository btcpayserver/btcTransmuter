using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Extension.Presets
{
    public interface ITransmuterPreset
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }

        (string ControllerName, string ActionName) GetLink();
    }
}