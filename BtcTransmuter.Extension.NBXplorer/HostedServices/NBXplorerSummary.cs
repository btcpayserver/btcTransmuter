using BtcTransmuter.Extension.NBXplorer.Models;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.HostedServices
{
    public class NBXplorerSummary
    {
        public NBXplorerState State { get; set; }
        public StatusResult Status { get; set; }
        public string Error { get; set; }
    }
}