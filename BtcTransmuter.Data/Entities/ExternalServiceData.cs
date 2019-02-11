namespace BtcTransmuter.Data.Entities
{
    public class ExternalServiceData : BaseEntity
    {
        public string Type { get; set; }
        public string UserId { get; set; }

        public User User { get; set; }
    }
}