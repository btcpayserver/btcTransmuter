using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Email.ExternalServices.Pop3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    [Authorize]
    [Route("btcpayserver-plugin/triggers/invoice-status-changed")]
    public class InvoiceStatusChangedController : Controller
    {
        private readonly (string Text, string Value)[] AllowedStatuses = new []
        {
            ("Any Status", null),
            ("New",  Invoice.STATUS_NEW),
            ("Paid", Invoice.STATUS_PAID),
            ("Invalid", Invoice.STATUS_INVALID),
            ("Confirmed", Invoice.STATUS_CONFIRMED),
            ("Complete", Invoice.STATUS_COMPLETE)
        };
        private readonly IRecipeManager _recipeManager;
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;
        
        

        public InvoiceStatusChangedController(
            IRecipeManager recipeManager,
            IExternalServiceManager externalServiceManager,
            UserManager<User> userManager,
            IMemoryCache memoryCache)
        {
            _recipeManager = recipeManager;
            _externalServiceManager = externalServiceManager;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var btcPayServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                UserId = _userManager.GetUserId(User)
            });

            var vm = new InvoiceStatusChangedTriggerViewModel()
            {
                ExternalServices = new SelectList(btcPayServices, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), result.Data.ExternalServiceId),
            };
            SetValues(result.Data, vm);

            return View(vm);
        }

        private void SetValues(InvoiceStatusChangedTriggerViewModel from, RecipeTrigger to)
        {
            to.ExternalServiceId = from.ExternalServiceId;
            to.RecipeId = from.RecipeId;
            var currentData = to.Get<InvoiceStatusChangedTriggerParameters>();
            currentData.Status = from.Status;
            to.Set(currentData);
        }

        private void SetValues(RecipeTrigger from, InvoiceStatusChangedTriggerViewModel to)
        {
            to.RecipeId = from.RecipeId;
            to.ExternalServiceId = from.ExternalServiceId;
            var fromData = from.Get<InvoiceStatusChangedTriggerParameters>();
            to.Status = fromData.Status;
            to.Statuses = new SelectList(AllowedStatuses, "Value", "Text", fromData.Status);
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, InvoiceStatusChangedTriggerViewModel data)
        {
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }
            
            if (!ModelState.IsValid)
            {
                var btcPayServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                    UserId = _userManager.GetUserId(User)
                });


                data.Statuses = new SelectList(AllowedStatuses, "Value", "Text", data.Status);
                data.ExternalServices = new SelectList(btcPayServices, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId);
                return View(data);
            }

            var recipeTrigger = result.Data;
            SetValues(data, recipeTrigger);

            await _recipeManager.AddOrUpdateRecipeTrigger(recipeTrigger);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeTrigger.RecipeId,
                statusMessage = "Invoice Status Change trigger Updated"
            });
        }

        private async Task<(IActionResult Error, RecipeTrigger Data )> GetRecipeTrigger(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeTrigger data))
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            var recipe = await _recipeManager.GetRecipe(data.RecipeId, _userManager.GetUserId(User));

            if (recipe == null)
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            return (null, data);
        }


        public class InvoiceStatusChangedTriggerViewModel : InvoiceStatusChangedTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required] public string ExternalServiceId { get; set; }
            public SelectList Statuses{ get; set; }
            
            
        }
    }
}