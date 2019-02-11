using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Abstractions
{
    public abstract class BaseEntity: IHasJsonData
    {
        public string Id { get; set; }

        public string DataJson { get; set; }
    }
}