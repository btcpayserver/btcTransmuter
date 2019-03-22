using System.Net;

namespace BtcTransmuter.Extension.Lightning.Tor
{
    public class OnionEndpoint : DnsEndPoint
    {
        public OnionEndpoint(string host, int port): base(host, port)
        {

        }
    }
}
