using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction;
using BtcTransmuter.Extension.NBXplorer.Models;
using NBitcoin;
using NBitcoin.BIP174;
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

        public async Task<TransactionBuilder> AddTxOutsToTransaction(TransactionBuilder transactionBuilder,
            Money amount, IDestination destination, bool subtractFee)
        {
            return await AddTxOutsToTransaction(transactionBuilder,
                new List<(Money amount, IDestination destination, bool subtractFee)>()
                {
                    (amount, destination, subtractFee)
                });
        }

        public async Task<TransactionBuilder> CreateTransactionBuilder(bool addUtxos = true)
        {
            var builder = _explorerClient.Network.NBitcoinNetwork
                .CreateTransactionBuilder();

            if (addUtxos)
            {
                var utxos = await GetUTXOs();
                builder.AddCoins(utxos.GetUnspentCoins());
            }
            return builder;
        }

        public async Task<Transaction> BuildTransaction(
            IEnumerable<(Money amount, IDestination destination, bool subtractFee)> outgoing,
            IEnumerable<PrivateKeyDetails> privateKeyDetails = null)
        {
            var txBuilder = await CreateTransactionBuilder();
            await AddTxOutsToTransaction(txBuilder, outgoing);
            if (privateKeyDetails?.Any() ?? false)
            {
                foreach (var privateKeyDetail in privateKeyDetails)
                {
                    await SignTransaction(txBuilder, privateKeyDetail);
                }
            }

            return txBuilder.BuildTransaction(true);
        }

        public async Task<TransactionBuilder> AddTxOutsToTransaction(TransactionBuilder transactionBuilder,
            IEnumerable<(Money amount, IDestination destination, bool subtractFee)> outgoing)
        {
            foreach (var tuple in outgoing)
            {
                transactionBuilder.Send(tuple.destination, tuple.amount);
                if (tuple.subtractFee)
                {
                    transactionBuilder.SubtractFees();
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

        public static ExtKey GetKeyFromDetails(PrivateKeyDetails privateKeyDetails, Network network)
        {
            if (!string.IsNullOrEmpty(privateKeyDetails.MnemonicSeed))
            {
                return new Mnemonic(privateKeyDetails.MnemonicSeed).DeriveExtKey(
                    string.IsNullOrEmpty(privateKeyDetails.Passphrase) ? null : privateKeyDetails.Passphrase);
            }

            return ExtKey.Parse(privateKeyDetails.WIF, network);
        }

        public async Task SignTransaction(TransactionBuilder transactionBuilder, PrivateKeyDetails privateKeyDetails)
        {
            await SignTransaction(transactionBuilder,
                GetKeyFromDetails(privateKeyDetails, _explorerClient.Network.NBitcoinNetwork));
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