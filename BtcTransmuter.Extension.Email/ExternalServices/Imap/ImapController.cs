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

namespace BtcTransmuter.Extension.Email.ExternalServices.Imap
{
    [Route("email-plugin/external-services/imap")]
    [Authorize]
    public class ImapController : Controller
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public ImapController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
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

            var imapService = new ImapService(result.Data);

            return View(imapService.GetData());
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, ImapExternalServiceData data)
        {
            var result = await GetExternalServiceData(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var externalServiceData = result.Data;
            if (!ModelState.IsValid)
            {
                return View(data);
            }
            data.PairedDate = DateTime.Now;
            externalServiceData.Set(data);

            var imapService = new ImapService(externalServiceData);
            var testConnection = await imapService.CreateClientAndConnect();
            if (testConnection == null)
            { 
                ModelState.AddModelError(string.Empty, "Could not connect successfully");

                return View(data);
            }

            testConnection.Dispose();


            await _externalServiceManager.AddOrUpdateExternalServiceData(externalServiceData);
            return RedirectToAction("EditExternalService", "ExternalServices", new
            {
                id = externalServiceData.Id,
                statusMessage = "Imap Data updated"
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
                    Type = new string[] {ImapService.ImapExternalServiceType},
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