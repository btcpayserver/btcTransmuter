using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.AspNetCore.Mvc;

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
}