using System;
using NBitcoin;

namespace BtcTransmuter.Extension.NBXplorer.Models
{
    public class NBXplorerOptions
    {
        public Uri Uri { get; set; }
        public NetworkType NetworkType { get; set; }
        public string CookieFile { get; set; }
        public bool UseDefaultCookie { get; set; }
        public string[] Cryptos { get; set; }
    }
}