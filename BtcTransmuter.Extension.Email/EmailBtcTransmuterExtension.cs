using BtcTransmuter.Abstractions;

namespace BtcTransmuter.Extension.Email
{
    public class EmailBtcTransmuterExtension: IBtcTransmuterExtension
    {
        public string Name => "Emails";

        public string Version => "0.01";
        
        public const string Pop3ExternalServiceType = "Pop3ExternalService";
    }
}