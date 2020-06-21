using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Extension.NBXplorer.Extensions;
using BtcTransmuter.Extension.NBXplorer.Models;
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
            return new Money(utxoChanges.GetUnspentUTXOs().Select(c => c.Value).Sum(money => money.GetValue(null)), MoneyUnit.BTC);
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
            IEnumerable<PrivateKeyDetails> privateKeyDetails = null, FeeRate feeRate = null, Money fee = null)
        {
            var txBuilder = await CreateTransactionBuilder();
            await AddTxOutsToTransaction(txBuilder, outgoing);
            
            if (feeRate == null && fee == null)
            {
                feeRate = (await _explorerClient.GetFeeRateAsync(20, new FeeRate(100L, 1))).FeeRate;
            }
            
            if (fee != null)
            {
                txBuilder.SendFees(fee);
            }

            if (feeRate != null)
            {
                txBuilder.SendEstimatedFees(feeRate);
            }

            

            if (!(privateKeyDetails?.Any() ?? false)) return txBuilder.BuildTransaction(true);
            foreach (var privateKeyDetail in privateKeyDetails)
            {
                await SignTransaction(txBuilder, privateKeyDetail);
            }

            return txBuilder.BuildTransaction(true);
        }

        private Task<TransactionBuilder> AddTxOutsToTransaction(TransactionBuilder transactionBuilder,
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

            return Task.FromResult(transactionBuilder);
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
            if (TrackedSource is DerivationSchemeTrackedSource derivationSchemeTrackedSource)
            {
                transactionBuilder.AddKeys(utxos.GetKeys(extKey.Derive(GetDerivationKeyPath(
                    GetScriptPubKeyType(derivationSchemeTrackedSource.DerivationStrategy), 0,
                    _explorerClient.Network))));
            }
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
                    return (await _explorerClient.GetUnusedAsync(
                        derivationSchemeTrackedSource.DerivationStrategy,
                        derivationFeature, 0, true)).Address;
            }

            throw new InvalidOperationException();
        }
        
        public  static KeyPath GetDerivationKeyPath(ScriptPubKeyType scriptPubKeyType, int accountNumber,
            NBXplorerNetwork network)
        {
            var keyPath = new KeyPath(scriptPubKeyType == ScriptPubKeyType.Legacy ? "44'" :
                scriptPubKeyType == ScriptPubKeyType.Segwit ? "84'" :
                scriptPubKeyType == ScriptPubKeyType.SegwitP2SH ? "49'" :
                throw new NotSupportedException(scriptPubKeyType.ToString())); // Should never happen
            return keyPath.Derive(network.CoinType)
                .Derive(accountNumber, true);
        }
        
        public ScriptPubKeyType GetScriptPubKeyType(DerivationStrategyBase derivationStrategyBase)
        {
            if (IsSegwitCore(derivationStrategyBase))
            {
                return NBitcoin.ScriptPubKeyType.Segwit;
            }

            return (derivationStrategyBase is P2SHDerivationStrategy p2shStrat && IsSegwitCore(p2shStrat.Inner))
                ? NBitcoin.ScriptPubKeyType.SegwitP2SH
                : NBitcoin.ScriptPubKeyType.Legacy;
            
        }
        
        private static bool IsSegwitCore(DerivationStrategyBase derivationStrategyBase)
        {
            return (derivationStrategyBase is P2WSHDerivationStrategy) ||
                   (derivationStrategyBase is DirectDerivationStrategy direct) && direct.Segwit;
        }
    }
}