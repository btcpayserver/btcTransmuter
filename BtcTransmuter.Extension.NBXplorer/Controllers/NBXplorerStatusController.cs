using System.Linq;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace BtcTransmuter.Extension.NBXplorer.Controllers
{
    [Route("nbxplorer-plugin/status")]
    public class NBXplorerStatusController : Controller
    {
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;
        private readonly NBXplorerOptions _nbXplorerOptions;

        public NBXplorerStatusController(NBXplorerSummaryProvider nbXplorerSummaryProvider, NBXplorerOptions nbXplorerOptions)
        {
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
            _nbXplorerOptions = nbXplorerOptions;
        }

        public IActionResult GetSummaries()
        {
            return View(new NBXplorerSummariesViewModel()
            {
                Options = _nbXplorerOptions,
                Summaries = _nbXplorerSummaryProvider.GetSummaries()
            });
        }
    }
    
    [Route("nbxplorer-plugin/wallet-creator")]
    public class WalletCreatorController : Controller
    {
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly NBXplorerOptions _nbXplorerOptions;

        public WalletCreatorController(NBXplorerClientProvider nbXplorerClientProvider, NBXplorerOptions nbXplorerOptions)
        {
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _nbXplorerOptions = nbXplorerOptions;
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
            return View(new GetWalletViewModel()
            {
                Mnemonic = mnemonic,
                Network = _nbXplorerClientProvider.GetClient(cryptoCode).Network.NBitcoinNetwork,
                CryptoCode =  cryptoCode,
                
                CryptoCodes = _nbXplorerOptions.Cryptos
            });

        }

        public class GetWalletViewModel
        {
            public string Mnemonic { get; set; }
            public string CryptoCode { get; set; }
            public Network Network { get; set; }
            public string[] CryptoCodes { get; set; }
        }
    }
}