using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Abstractions
{
    public interface ITrigger: IHasJsonData
    {
        string Id { get; }
        string Name { get; }
    }
}