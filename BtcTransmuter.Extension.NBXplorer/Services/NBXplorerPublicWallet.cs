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
        private readonly TrackedSource _trackedSource;

        public NBXplorerPublicWallet(ExplorerClient explorerClient, TrackedSource trackedSource)
        {
            _explorerClient = explorerClient;
            _trackedSource = trackedSource;
        }
        public async Task<Money> GetBalance()
        {
            return (await GetUTXOs()).GetUnspentUTXOs().Select(c => c.Value).Sum();
        }
        
        public async Task<UTXOChanges> GetUTXOs()
        {
            return await _explorerClient.GetUTXOsAsync(_trackedSource);
        }
    }
}