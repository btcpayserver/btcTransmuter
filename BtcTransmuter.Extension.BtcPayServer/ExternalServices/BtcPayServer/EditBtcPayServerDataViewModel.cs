namespace BtcTransmuter.Extension.Email.ExternalServices.Pop3
{
    public class EditBtcPayServerDataViewModel : BtcPayServerExternalServiceData
    {
        public string PairingUrl { get; set; }

        public string PairingCode { get; set; }
        public bool Paired { get; set; }
    }
}