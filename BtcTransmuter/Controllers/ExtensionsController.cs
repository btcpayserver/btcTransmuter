using System.Collections.Generic;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    [Authorize]
    
    [Route("extensions")]
    public class ExtensionsController : Controller
    {
        private readonly IEnumerable<BtcTransmuterExtension> _btcTransmuterExtensions;

        public ExtensionsController(IEnumerable<BtcTransmuterExtension> btcTransmuterExtensions)
        {
            _btcTransmuterExtensions = btcTransmuterExtensions;
        }

        [HttpGet("")]
        public IActionResult Extensions()
        {
            return View(new ExtensionsViewModel()
            {
                Extensions = _btcTransmuterExtensions
            });
        }
    }
}