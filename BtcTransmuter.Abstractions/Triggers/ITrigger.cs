using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Abstractions.Triggers
{
    public interface ITrigger: IHasJsonData
    {
        string Id { get; }
    }
}