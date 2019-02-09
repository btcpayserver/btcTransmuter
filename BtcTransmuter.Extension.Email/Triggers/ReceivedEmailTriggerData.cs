namespace BtcTransmuter.Extension.Email.Triggers
{
    public class ReceivedEmailTriggerData
    {
        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ExternalServiceId { get; set; }
    }
}