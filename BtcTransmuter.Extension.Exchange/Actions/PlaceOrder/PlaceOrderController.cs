using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
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
    public class PlaceOrderController : BaseActionController<PlaceOrderController.PlaceOrderViewModel, PlaceOrderData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public PlaceOrderController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<PlaceOrderViewModel> BuildViewModel(RecipeAction from)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {ExchangeService.ExchangeServiceType},
                UserId = GetUserId()
            });
            var fromData = from.Get<PlaceOrderData>();
            return new PlaceOrderViewModel
            {
                RecipeId = @from.RecipeId,
                ExternalServiceId = @from.ExternalServiceId,
                Price = fromData.Price,
                Amount = fromData.Amount,
                IsBuy = fromData.IsBuy,
                IsMargin = fromData.IsMargin,
                OrderType = fromData.OrderType,
                StopPrice = fromData.StopPrice,
                MarketSymbol = fromData.MarketSymbol,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), @from.ExternalServiceId)
            };
        }
        
        protected override async Task<(RecipeAction ToSave, PlaceOrderViewModel showViewModel)> BuildModel(
            PlaceOrderViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<PlaceOrderData>(viewModel);
                return (mainModel, null);
            }
            
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {ExchangeService.ExchangeServiceType},
                UserId = GetUserId()
            });

            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
            return (null, viewModel);
        }

        public class PlaceOrderViewModel : PlaceOrderData, IUseExternalService
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required] public string ExternalServiceId { get; set; }
        }
    }
}