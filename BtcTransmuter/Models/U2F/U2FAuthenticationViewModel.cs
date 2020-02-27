using System.Collections.Generic;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models.U2F
{
    public class U2FAuthenticationViewModel
    {
        public List<U2FDevice> Devices { get; set; }
    }
}
