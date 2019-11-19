using System.Collections.Generic;
using System.Linq;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBXplorer;
using NBXplorer.DerivationStrategy;

namespace BtcTransmuter.Extension.NBXplorer.Controllers
{
    [Route("nbxplorer-plugin/wallet-creator")]
    public class WalletCreatorController : Controller
    {
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly NBXplorerOptions _nbXplorerOptions;
        private readonly DerivationSchemeParser _derivationSchemeParser;

        public WalletCreatorController(NBXplorerClientProvider nbXplorerClientProvider, NBXplorerOptions nbXplorerOptions, DerivationSchemeParser derivationSchemeParser)
        {
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _nbXplorerOptions = nbXplorerOptions;
            _derivationSchemeParser = derivationSchemeParser;
        }

        [HttpGet("")]
        public IActionResult GetWallet()
        {
            return RedirectToAction("GetWallet", new
            {
                cryptoCode = _nbXplorerOptions.Cryptos.First()
            });

        }
        
        [HttpGet("{cryptoCode}/{mnemonic?}")]
        public IActionResult GetWallet(string cryptoCode, string mnemonic)
        {
            if (string.IsNullOrEmpty(mnemonic))
            {
                return RedirectToAction("GetWallet", new
                {
                    cryptoCode = cryptoCode,
                    mnemonic = new Mnemonic(Wordlist.English).ToString()
                });
            }

            var network = _nbXplorerClientProvider.GetClient(cryptoCode).Network;
            

            var mnemonicSeed = new Mnemonic(mnemonic);
            var extKey = mnemonicSeed.DeriveExtKey();
            var wif = extKey.GetWif(network.NBitcoinNetwork);
            var privateKey = extKey.PrivateKey;
            var secret = privateKey.GetBitcoinSecret(network.NBitcoinNetwork);
            var extPubKey = extKey.Neuter().ToString(network.NBitcoinNetwork);
        
            var legacy = _derivationSchemeParser.Parse(network.DerivationStrategyFactory, $"{extPubKey}-[legacy]");
            var segwit = _derivationSchemeParser.Parse(network.DerivationStrategyFactory, $"{extPubKey}");
            var p2sh = _derivationSchemeParser.Parse(network.DerivationStrategyFactory, $"{extPubKey}-[p2sh]");

            var addressTypes = new Dictionary<ScriptPubKeyType, (string, Dictionary<string, string>)>();
            addressTypes.Add(ScriptPubKeyType.Legacy, ($"{extPubKey}-[legacy]", GenerateAddresses(legacy, network)));
            if (network.NBitcoinNetwork.Consensus.SupportSegwit)
            {

                addressTypes.Add(ScriptPubKeyType.Segwit, ($"{extPubKey}", GenerateAddresses(segwit, network)));
                addressTypes.Add(ScriptPubKeyType.SegwitP2SH,
                    ($"{extPubKey}-[p2sh]", GenerateAddresses(p2sh, network)));
            }

            return View(new GetWalletViewModel()
            {
                Mnemonic = mnemonic,
                Network = network,
                CryptoCode =  cryptoCode,
                CryptoCodes = _nbXplorerOptions.Cryptos,
                PrivateKey = privateKey,
                WIF = wif,
                ExtPubKey = extPubKey,
                AddressTypes = addressTypes,
                Address = secret.GetAddress(ScriptPubKeyType.Legacy),
                SegwitAddress = network.NBitcoinNetwork.Consensus.SupportSegwit? secret.GetAddress(ScriptPubKeyType.Segwit): null,
                P2SHAddress = network.NBitcoinNetwork.Consensus.SupportSegwit? secret.GetAddress(ScriptPubKeyType.SegwitP2SH): null
            });
        }
        
        private Dictionary<string, string> GenerateAddresses(DerivationStrategyBase derivation, NBXplorerNetwork network)
        {
            var result = new Dictionary<string, string>();
            var deposit = new KeyPathTemplates(null).GetKeyPathTemplate(DerivationFeature.Deposit);

            var line = derivation.GetLineFor(deposit);
            for (int i = 0; i < 20; i++)
            {
                result.Add(deposit.GetKeyPath((uint)i).ToString(), line.Derive((uint)i).ScriptPubKey.GetDestinationAddress(network.NBitcoinNetwork).ToString());
            }
            return result;
        }

        public class GetWalletViewModel
        {
            public string Mnemonic { get; set; }
            public string CryptoCode { get; set; }
            public NBXplorerNetwork Network { get; set; }
            public string[] CryptoCodes { get; set; }
            public Key PrivateKey { get; set; }
            public BitcoinExtKey WIF { get; set; }
            public string ExtPubKey { get; set; }

            public Dictionary<ScriptPubKeyType, (string, Dictionary<string, string>)> AddressTypes { get; set; }
            public BitcoinAddress SegwitAddress { get; set; }
            public BitcoinAddress Address { get; set; }
            public BitcoinAddress P2SHAddress { get; set; }
        }
    }
}