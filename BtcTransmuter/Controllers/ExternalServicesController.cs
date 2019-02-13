using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    public class ExternalServicesController : Controller
    {
        [Authorize]
        public IActionResult GetServices()
        {
            return Ok();
        }
    }
}