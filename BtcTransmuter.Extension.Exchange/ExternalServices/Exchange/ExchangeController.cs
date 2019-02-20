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

namespace BtcTransmuter.Extension.Exchange.ExternalServices.Exchange
{
    [Route("exchange-plugin/external-services/exchange")]
    [Authorize]
    public class ExchangeController : Controller
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public ExchangeController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
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

            var client = new ExchangeService(result.Data);


            return View(client.GetData());
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, ExchangeExternalServiceData data)
        {
            var result = await GetExternalServiceData(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            //current External Service data
            var externalServiceData = result.Data;
            
            if (!ModelState.IsValid)
            {
                return View(data);
            }
            
            //current External Service data
            var oldData = externalServiceData.Get<ExchangeExternalServiceData>();
            externalServiceData.Set(data);
            var exchangeService = new ExchangeService(externalServiceData);

            if(! await exchangeService.TestAccess())
            {
                ModelState.AddModelError(String.Empty, "Could not connect with current settings");
                
                return View(data);
            }
            
            await _externalServiceManager.AddOrUpdateExternalServiceData(externalServiceData);
            return RedirectToAction("EditExternalService", "ExternalServices", new
            {
                id = externalServiceData.Id,
                statusMessage = "Exchange Data updated"
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
                    Type = new string[] {ExchangeService.ExchangeServiceType},
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