using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using ExchangeSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

            return View(new EditExchangeExternalServiceDataViewModel(client.GetData(),
                ExchangeService.GetAvailableExchanges()));
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
                return View(new EditExchangeExternalServiceDataViewModel(data,
                    ExchangeService.GetAvailableExchanges()));
            }

            //current External Service data
            var oldData = externalServiceData.Get<ExchangeExternalServiceData>();
            externalServiceData.Set(data);
            var exchangeService = new ExchangeService(externalServiceData);

            if (!await exchangeService.TestAccess())
            {
                ModelState.AddModelError(String.Empty, "Could not connect with current settings");

                return View(new EditExchangeExternalServiceDataViewModel(data,
                    ExchangeService.GetAvailableExchanges()));
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

        public class EditExchangeExternalServiceDataViewModel : ExchangeExternalServiceData
        {
            public SelectList Exchanges { get; set; }

            public EditExchangeExternalServiceDataViewModel(ExchangeExternalServiceData serviceData,
                IEnumerable<IExchangeAPI> exchangeApis)
            {
                OverrideUrl = serviceData.OverrideUrl;
                ExchangeName = serviceData.ExchangeName;
                PublicKey = serviceData.PublicKey;
                PassPhrase = serviceData.PassPhrase;
                PrivateKey = serviceData.PrivateKey;
                LastCheck = serviceData.LastCheck;
                PairedDate = serviceData.PairedDate;
                Exchanges = new SelectList(exchangeApis, nameof(IExchangeAPI.Name), nameof(IExchangeAPI.Name),
                    ExchangeName);
            }

            public EditExchangeExternalServiceDataViewModel()
            {
            }
        }
    }
}