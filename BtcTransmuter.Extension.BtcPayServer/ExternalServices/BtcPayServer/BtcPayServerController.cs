using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using NBitpayClient;

namespace BtcTransmuter.Extension.Email.ExternalServices.Pop3
{
    [Route("btcpayserver-plugin/external-services/btcpayserver")]
    [Authorize]
    public class BtcPayServerController : Controller
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public BtcPayServerController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache)
        {
            _externalServiceManager = externalServiceManager;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetExternalServiceData(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var client = new BtcPayServerService(result.Data);


            return View(new EditBtcPayServerDataViewModel()
            {
                Seed = client.Data.Seed?? new Mnemonic(Wordlist.English, WordCount.Twelve).ToString(),
                Server = client.Data.Server,
                PairingUrl = client.GetPairingUrl(),
                LastCheck = client.Data.LastCheck,
                Paired = client.CheckAccess()
            });
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, EditBtcPayServerDataViewModel data)
        {
            var result = await GetExternalServiceData(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var externalServiceData = result.Data;

            externalServiceData.Set(data);

            var service = new BtcPayServerService(externalServiceData);


            if (!ModelState.IsValid)
            {
                data.PairingUrl = service.GetPairingUrl();
                return View(data);
            }

            if (!service.CheckAccess())
            {
                data.PairingUrl = service.GetPairingUrl();
                if (!string.IsNullOrEmpty(data.PairingCode))
                {
                    var client = service.ConstructClient();
                    await client.AuthorizeClient(new PairingCode(data.PairingCode));
                    if (!service.CheckAccess())
                    {
                        ModelState.AddModelError(string.Empty, "Could not pair with pairing code");

                        data.PairingUrl = service.GetPairingUrl();
                        return View(data);
                    }
                }
                ModelState.AddModelError(string.Empty, "Cannot proceed until paired");

                data.PairingUrl = service.GetPairingUrl();
                return View(data);
               
            }


            await _externalServiceManager.AddOrUpdateExternalServiceData(externalServiceData);
            return RedirectToAction("EditExternalService", "ExternalServices", new
            {
                id = externalServiceData.Id,
                statusMessage = "Btcpayserver Data updated"
            });
        }

        private async Task<(IActionResult Error, ExternalServiceData Data )> GetExternalServiceData(string identifier)
        {
            ExternalServiceData data = null;
            if (identifier.StartsWith("new"))
            {
                if (!_memoryCache.TryGetValue(identifier, out data))
                {
                    return (RedirectToAction("GetServices", "ExternalServices", new
                    {
                        statusMessage = "Error:Data could not be found or data session expired"
                    }), null);
                }

                if (data.UserId != _userManager.GetUserId(User))
                {
                    return (RedirectToAction("GetServices", "ExternalServices", new
                    {
                        statusMessage = "Error:Data could not be found or data session expired"
                    }), null);
                }
            }
            else
            {
                var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    UserId = _userManager.GetUserId(User),
                    Type = new string[] {BtcPayServerService.BtcPayServerServiceType},
                    ExternalServiceId = identifier
                });
                if (!services.Any())
                {
                    return (
                        RedirectToAction("GetServices", "ExternalServices", new
                        {
                            statusMessage = "Error:Data could not be found"
                        }), null);
                }

                data = services.First();
            }

            return (null, data);
        }
    }
}