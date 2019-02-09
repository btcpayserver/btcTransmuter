namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public abstract class BaseEmailServiceData
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}