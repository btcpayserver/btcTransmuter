using System;
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

namespace BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer
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

            var clientData = client.GetData();
            return View(new EditBtcPayServerDataViewModel()
            {
                Seed = clientData.Seed ?? new Mnemonic(Wordlist.English, WordCount.Twelve).ToString(),
                Server = clientData.Server,
                PairingUrl = await client.GetPairingUrl(),
                Paired = await client.CheckAccess()
            });
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, EditBtcPayServerDataViewModel data, string action)
        {
            var result = await GetExternalServiceData(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            if (action == "unpair")
            {
                data.Seed = null;
            }
            //current External Service data
            var externalServiceData = result.Data;

            //current External Service data
            var oldData = externalServiceData.Get<BtcPayServerExternalServiceData>();


            if (oldData.Seed == data.Seed && oldData.Server == data.Server)
            {
                data.LastCheck = oldData.LastCheck;
                data.MonitoredInvoiceStatuses = oldData.MonitoredInvoiceStatuses;
                data.PairedDate = oldData.PairedDate;
            }
            else
            {
                data.PairedDate = DateTime.Now;
            }

            externalServiceData.Set((BtcPayServerExternalServiceData) data);
            var service = new BtcPayServerService(externalServiceData);

            if (!ModelState.IsValid)
            {
                var serviceData = service.GetData();
                return View(new EditBtcPayServerDataViewModel()
                {
                    Seed = serviceData.Seed ?? new Mnemonic(Wordlist.English, WordCount.Twelve).ToString(),
                    Server = serviceData.Server,
                    PairingUrl = await service.GetPairingUrl(),
                    Paired = await service.CheckAccess()
                });
            }


            if (!await service.CheckAccess())
            {
                data.Seed = data.Seed ?? new Mnemonic(Wordlist.English, WordCount.Twelve).ToString();
                service.SetData(data);
                data.PairingUrl = await service.GetPairingUrl();
                data.Paired = false;
                if (!string.IsNullOrEmpty(data.PairingCode))
                {
                    var client = service.ConstructClient();
                    await client.AuthorizeClient(new PairingCode(data.PairingCode));
                    if (!await service.CheckAccess())
                    {
                        ModelState.AddModelError(string.Empty, "Could not pair with pairing code");
                        return View(data);
                    }
                }

                ModelState.AddModelError(string.Empty, "Cannot proceed until paired");

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