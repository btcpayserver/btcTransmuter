namespace BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer
{
    public class EditBtcPayServerDataViewModel : BtcPayServerExternalServiceData
    {
        public string PairingUrl { get; set; }

        public string PairingCode { get; set; }
        public bool Paired { get; set; }
    }
}