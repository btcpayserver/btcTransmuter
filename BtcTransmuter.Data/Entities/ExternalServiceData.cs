using BtcTransmuter.Data;

namespace BtcTransmuter.Abstractions
{
    public class ExternalServiceData : BaseEntity
    {
        public string Type { get; set; }
        public string UserId { get; set; }

        public User User { get; set; }
    }
}