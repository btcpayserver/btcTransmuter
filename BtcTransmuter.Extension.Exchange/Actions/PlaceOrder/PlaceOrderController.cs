using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Exchange.Actions.PlaceOrder
{
    [Route("exchange-plugin/actions/place-order")]
    [Authorize]
    public class PlaceOrderController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public PlaceOrderController(
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
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {ExchangeService.ExchangeServiceType},
                UserId = _userManager.GetUserId(User)
            });

            var vm = new PlaceOrderViewModel()
            {
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), result.Data.ExternalServiceId),
            };
            SetValues(result.Data, vm);

            return View(vm);
        }

        private void SetValues(PlaceOrderViewModel from, RecipeAction to)
        {
            to.ExternalServiceId = from.ExternalServiceId;
            to.RecipeId = from.RecipeId;
            to.Set((PlaceOrderData) from);
        }

        private void SetValues(RecipeAction from, PlaceOrderViewModel to)
        {
            
            
            to.RecipeId = from.RecipeId;
            to.ExternalServiceId = from.ExternalServiceId;
            var fromData = from.Get<PlaceOrderData>();
            to.Price = fromData.Price;
            to.Amount = fromData.Amount;
            to.IsBuy = fromData.IsBuy;
            to.IsMargin = fromData.IsMargin;
            to.OrderType = fromData.OrderType;
            to.StopPrice = fromData.StopPrice;
            to.MarketSymbol = fromData.MarketSymbol;
            
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, PlaceOrderViewModel data)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }


            var externalServiceData  = await _externalServiceManager.GetExternalServiceData(data.ExternalServiceId);
            ExchangeService exchangeService = new ExchangeService(externalServiceData);
            
            if (!ModelState.IsValid)
            {
                var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    Type = new[] {ExchangeService.ExchangeServiceType},
                    UserId = _userManager.GetUserId(User)
                });


                data.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId);
                return View(data);
            }

            var recipeAction = result.Data;
            SetValues(data, recipeAction);

            await _recipeManager.AddOrUpdateRecipeAction(recipeAction);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeAction.RecipeId,
                statusMessage = "Place Order Action Updated"
            });
        }

        private async Task<(IActionResult Error, RecipeAction Data )> GetRecipeAction(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeAction data))
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


        public class PlaceOrderViewModel : PlaceOrderData
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required] public string ExternalServiceId { get; set; }
        }
    }
}