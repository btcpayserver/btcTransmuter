using System.Collections.Immutable;
using BtcTransmuter.Extension.NBXplorer.HostedServices;

namespace BtcTransmuter.Extension.NBXplorer.Controllers
{
    public class NBXplorerSummariesViewModel
    {
        public ImmutableDictionary<string, NBXplorerSummary> Summaries { get; set; }
    }
}