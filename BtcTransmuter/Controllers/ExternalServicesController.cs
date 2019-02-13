using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    [Authorize]
    public class ExternalServicesController : Controller
    {
        [Authorize]
        public IActionResult GetServices()
        {
            return Ok();
        }
    }
}