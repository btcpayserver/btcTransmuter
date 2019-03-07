using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBXplorer;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Services
{
    public class NBXplorerPublicWallet
    {
        private readonly ExplorerClient _explorerClient;
        public readonly TrackedSource TrackedSource;

        public NBXplorerPublicWallet(ExplorerClient explorerClient, TrackedSource trackedSource)
        {
            _explorerClient = explorerClient;
            TrackedSource = trackedSource;
        }
        public async Task<Money> GetBalance()
        {
            return (await GetUTXOs()).GetUnspentUTXOs().Select(c => c.Value).Sum();
        }
        
        public async Task<UTXOChanges> GetUTXOs()
        {
            var x  = await _explorerClient.GetUTXOsAsync(TrackedSource);
            return x;
        }
    }
}