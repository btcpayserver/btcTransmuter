using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Abstractions.ExternalServices
{
    [Authorize]
    public abstract class BaseExternalServiceController<TViewModel> : Controller
    {
        protected abstract string ExternalServiceType { get; }
        protected readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        protected BaseExternalServiceController(IExternalServiceManager externalServiceManager,
            UserManager<User> userManager,
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
            
            return View(await BuildViewModel(result.Data));
        }

        protected abstract Task<TViewModel> BuildViewModel(ExternalServiceData data);

        protected abstract Task<(ExternalServiceData ToSave, TViewModel showViewModel)> BuildModel(
            TViewModel viewModel, ExternalServiceData mainModel);

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, TViewModel data)
        {
            var result = await GetExternalServiceData(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var externalServiceData = result.Data;

            var modelResult = await BuildModel(data, externalServiceData);

            if (modelResult.showViewModel != null)
            {
                return View(modelResult.showViewModel);
            }

            await _externalServiceManager.AddOrUpdateExternalServiceData(modelResult.ToSave);
            return RedirectToAction("EditExternalService", "ExternalServices", new
            {
                id = externalServiceData.Id,
                statusMessage = "Data updated"
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
                    Type = new string[] {ExternalServiceType},
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

        protected async Task<bool> IsAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            return user!= null &&  await _userManager.IsInRoleAsync(user, "Admin");
        }
    }
}