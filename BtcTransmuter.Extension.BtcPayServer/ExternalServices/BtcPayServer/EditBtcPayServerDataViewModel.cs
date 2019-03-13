using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer
{
    public class EditBtcPayServerDataViewModel : BtcPayServerExternalServiceData
    {
        public string Action { get; set; }

        public string PairingUrl { get; set; }

        [Display(Name =
            "Pairing code from server initiated pairing. Alternatively, leave blank and click next to generate a pairing request url")]
        public string PairingCode { get; set; }

        public bool Paired { get; set; }
    }
}