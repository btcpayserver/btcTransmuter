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

        public WalletCreatorController(NBXplorerClientProvider nbXplorerClientProvider,
            NBXplorerOptions nbXplorerOptions, DerivationSchemeParser derivationSchemeParser)
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

        [HttpPost("{cryptoCode}")]
        public IActionResult GetWallet(string cryptoCode, [FromForm] string mnemonic)
        {
            return ActionResult(cryptoCode, string.IsNullOrEmpty(mnemonic) ? null : new Mnemonic(mnemonic));
        }

        [HttpGet("{cryptoCode}")]
        public IActionResult GetWallet(string cryptoCode)
        {
            return ActionResult(cryptoCode);
        }

        private IActionResult ActionResult(string cryptoCode, Mnemonic mnemonic = null)
        {
            var network = _nbXplorerClientProvider.GetClient(cryptoCode).Network;

            var addressTypes = new Dictionary<ScriptPubKeyType, GetWalletViewModel.GetWalletViewModelAddressType>();
            var mnemonicSeed = mnemonic ?? new Mnemonic(Wordlist.English);
            var extKey = mnemonicSeed.DeriveExtKey();
            var wif = extKey.GetWif(network.NBitcoinNetwork);
            var privateKey = extKey.PrivateKey;
            var secret = privateKey.GetBitcoinSecret(network.NBitcoinNetwork);
            

            if (network.NBitcoinNetwork.Consensus.SupportSegwit)
            {
                var segwitExtPubkey = extKey
                    .Derive(NBXplorerPublicWallet.GetDerivationKeyPath(ScriptPubKeyType.Segwit, 0, network))
                    .Neuter()
                    .ToString(network.NBitcoinNetwork);
                var p2shExtPubKey = extKey
                    .Derive(NBXplorerPublicWallet.GetDerivationKeyPath(ScriptPubKeyType.SegwitP2SH, 0, network))
                    .Neuter()
                    .ToString(network.NBitcoinNetwork);

                var segwit = _derivationSchemeParser.Parse(network.DerivationStrategyFactory, $"{segwitExtPubkey}");
                var p2sh = _derivationSchemeParser.Parse(network.DerivationStrategyFactory, $"{p2shExtPubKey}-[p2sh]");


                addressTypes.Add(ScriptPubKeyType.Segwit, new GetWalletViewModel.GetWalletViewModelAddressType()
                {
                    Description = "BTCPay / BTCTransmuter compatible xpub for segwit addresses",
                    DerivationScheme = segwit.ToString(),
                    Addresses = GenerateAddresses(segwit, network),
                    RootKeyPath ="m/" + NBXplorerPublicWallet.GetDerivationKeyPath(ScriptPubKeyType.Segwit, 0, network)
                        .ToString()
                });

                addressTypes.Add(ScriptPubKeyType.SegwitP2SH, new GetWalletViewModel.GetWalletViewModelAddressType()
                {
                    Description = "BTCPay / BTCTransmuter compatible xpub for p2sh addresses",
                    DerivationScheme = p2sh.ToString(),
                    Addresses = GenerateAddresses(p2sh, network),
                    RootKeyPath = "m/" + NBXplorerPublicWallet.GetDerivationKeyPath(ScriptPubKeyType.SegwitP2SH, 0, network)
                        .ToString()
                });
            }

            var legacyExtPubkey = extKey
                .Derive(NBXplorerPublicWallet.GetDerivationKeyPath(ScriptPubKeyType.Legacy, 0, network)).Neuter()
                .ToString(network.NBitcoinNetwork);
            var legacy =
                _derivationSchemeParser.Parse(network.DerivationStrategyFactory, $"{legacyExtPubkey}-[legacy]");

            addressTypes.Add(ScriptPubKeyType.Legacy, new GetWalletViewModel.GetWalletViewModelAddressType()
            {
                Description = "BTCPay / BTCTransmuter compatible xpub for legacy addresses",
                DerivationScheme = legacy.ToString(),
                Addresses = GenerateAddresses(legacy, network),
                RootKeyPath = "m/" +NBXplorerPublicWallet.GetDerivationKeyPath(ScriptPubKeyType.Legacy, 0, network).ToString()
            });

            return View(new GetWalletViewModel()
            {
                Mnemonic = mnemonicSeed.ToString(),
                Network = network,
                CryptoCode = cryptoCode,
                CryptoCodes = _nbXplorerOptions.Cryptos,
                PrivateKey = privateKey,
                WIF = wif,
                ExtPubKey = extKey.Neuter().ToString(network.NBitcoinNetwork),
                Fingerprint = extKey.Neuter().PubKey.GetHDFingerPrint().ToString(),
                AddressTypes = addressTypes,
                Address = secret.GetAddress(ScriptPubKeyType.Legacy),
                SegwitAddress = network.NBitcoinNetwork.Consensus.SupportSegwit
                    ? secret.GetAddress(ScriptPubKeyType.Segwit)
                    : null,
                P2SHAddress = network.NBitcoinNetwork.Consensus.SupportSegwit
                    ? secret.GetAddress(ScriptPubKeyType.SegwitP2SH)
                    : null,
                
            });
        }

        private Dictionary<string, string> GenerateAddresses(DerivationStrategyBase derivation,
            NBXplorerNetwork network)
        {
            var result = new Dictionary<string, string>();
            var deposit = new KeyPathTemplates(null).GetKeyPathTemplate(DerivationFeature.Deposit);

            var line = derivation.GetLineFor(deposit);
            for (int i = 0; i < 20; i++)
            {
                result.Add(deposit.GetKeyPath((uint) i).ToString(),
                    line.Derive((uint) i).ScriptPubKey.GetDestinationAddress(network.NBitcoinNetwork).ToString());
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
            public string Fingerprint { get; set; }
            public Dictionary<ScriptPubKeyType, GetWalletViewModelAddressType> AddressTypes { get; set; }
            public BitcoinAddress SegwitAddress { get; set; }
            public BitcoinAddress Address { get; set; }
            public BitcoinAddress P2SHAddress { get; set; }

            public class GetWalletViewModelAddressType
            {
                public string Description { get; set; }
                public string DerivationScheme { get; set; }
                public string RootKeyPath { get; set; }
                public Dictionary<string, string> Addresses { get; set; }
            }
        }
    }
}