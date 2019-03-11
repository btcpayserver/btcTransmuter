using System.Collections.Immutable;

namespace BtcTransmuter.Extension.NBXplorer.Models
{
    public class NBXplorerSummariesViewModel
    {
        public NBXplorerOptions Options { get; set; }
        public ImmutableDictionary<string, NBXplorerSummary> Summaries { get; set; }
    }
}