using System.Threading.Tasks;
using BtcTransmuter.Extension.NBXplorer.HostedServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Extension.NBXplorer.Controllers
{
    [Route("nbxplorer-plugin/status")]
    public class NBXplorerStatusController : Controller
    {
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;

        public NBXplorerStatusController(NBXplorerSummaryProvider nbXplorerSummaryProvider)
        {
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
        }

        public async Task<IActionResult> GetSummaries()
        {
            return View(new NBXplorerSummariesViewModel()
            {
                Summaries = _nbXplorerSummaryProvider.GetSummaries()
            });
        }
    }
}