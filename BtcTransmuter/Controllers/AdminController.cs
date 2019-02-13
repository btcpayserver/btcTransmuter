using System.Collections.Generic;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Models;
using ExtCore.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IEnumerable<BtcTransmuterExtension> _btcTransmuterExtensions;

        public AdminController(IEnumerable<BtcTransmuterExtension> btcTransmuterExtensions)
        {
            _btcTransmuterExtensions = btcTransmuterExtensions;
        }

        public IActionResult Extensions()
        {
            return View(new ExtensionsViewModel()
            {
                Extensions = _btcTransmuterExtensions
            });
        }
    }
}