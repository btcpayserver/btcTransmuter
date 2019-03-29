using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBXplorer;
using NBXplorer.DerivationStrategy;
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
            return GetBalance(await GetUTXOs());
        }

        public Money GetBalance(UTXOChanges utxoChanges)
        {
            return utxoChanges.GetUnspentUTXOs().Select(c => c.Value).Sum();
        }

        public async Task<UTXOChanges> GetUTXOs()
        {
            var x = await _explorerClient.GetUTXOsAsync(TrackedSource);
            return x;
        }

        public async Task<TransactionBuilder> BuildTransaction(Money amount, IDestination destination, bool subtractFee)
        {
            return await BuildTransaction(new List<(Money amount, IDestination destination, bool subtractFee)>()
            {
                (amount, destination, subtractFee)
            });
        }

        public async Task<TransactionBuilder> BuildTransaction(
            IEnumerable<(Money amount, IDestination destination, bool subtractFee)> outgoing)
        {
            var utxos = await GetUTXOs();
            var balance = GetBalance(utxos);
            var outgoingSum = outgoing.Sum(tuple => tuple.amount);
            if (outgoingSum > balance)
            {
                throw new InvalidOperationException("outgoing amount is greater than balance in source.");
            }

            var transactionBuilder = _explorerClient.Network.NBitcoinNetwork
                .CreateTransactionBuilder()
                .AddCoins(utxos.GetUnspentCoins());


            var feesSubtracted = false;
            foreach (var tuple in outgoing)
            {
                transactionBuilder.Send(tuple.destination, tuple.amount);
                if (tuple.subtractFee)
                {
                    transactionBuilder.SubtractFees();
                    feesSubtracted = true;
                }
            }

            switch (TrackedSource)
            {
                case AddressTrackedSource addressTrackedSource:
                    transactionBuilder.SetChange(addressTrackedSource.Address);
                    break;
                case DerivationSchemeTrackedSource derivationSchemeTrackedSource:
                    var changeAddress = _explorerClient.GetUnused(derivationSchemeTrackedSource.DerivationStrategy,
                        DerivationFeature.Change);
                    transactionBuilder.SetChange(changeAddress.ScriptPubKey);
                    break;
            }

            var recommendedFee = await _explorerClient.GetFeeRateAsync(20, new FeeRate(100L, 1));
            transactionBuilder.SendEstimatedFees(recommendedFee.FeeRate);

            return transactionBuilder;
        }

        public async Task SignTransaction(TransactionBuilder transactionBuilder, ExtKey extKey)
        {
            var utxos = await GetUTXOs();
            transactionBuilder.AddKeys(utxos.GetKeys(extKey));
        }

        public Task<BroadcastResult> BroadcastTransaction(Transaction transaction)
        {
            return _explorerClient.BroadcastAsync(transaction);
        }

        public async Task<BitcoinAddress> GetNextAddress(
            DerivationFeature derivationFeature = DerivationFeature.Deposit)
        {
            switch (TrackedSource)
            {
                case AddressTrackedSource addressTrackedSource:
                    return addressTrackedSource.Address;
                case DerivationSchemeTrackedSource derivationSchemeTrackedSource:
                    return BitcoinAddress.Create((await _explorerClient.GetUnusedAsync(
                        derivationSchemeTrackedSource.DerivationStrategy,
                        derivationFeature, 0, true)).Address, _explorerClient.Network.NBitcoinNetwork);
            }

            throw new InvalidOperationException();
        }
    }
}