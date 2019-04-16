using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.BtcPayServer.Actions.GetInvoice
{
    [Route("btcpayserver-plugin/actions/[controller]")]
    [Authorize]
    public class GetInvoiceController : BaseActionController<
        GetInvoiceController.GetInvoiceViewModel,
        GetInvoiceData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public GetInvoiceController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager,
            IExternalServiceManager externalServiceManager) : base(
            memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<GetInvoiceViewModel> BuildViewModel(RecipeAction from)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery
            {
                Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                UserId = GetUserId()
            });
            var fromData = from.Get<GetInvoiceData>();
            return new GetInvoiceViewModel
            {
                RecipeId = from.RecipeId,
                ExternalServiceId = from.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), from.ExternalServiceId),
                InvoiceId =  fromData.InvoiceId,
            };
        }

        protected override async Task<(RecipeAction ToSave, GetInvoiceViewModel showViewModel)> BuildModel(
            GetInvoiceViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<GetInvoiceData>(viewModel);
                return (mainModel, null);
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery
            {
                Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                UserId = GetUserId()
            });
            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
            return (null, viewModel);
        }

        public class GetInvoiceViewModel : GetInvoiceData, IActionViewModel, IUseExternalService
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }
            public SelectList PaymentTypes { get; set; }

            [Display(Name = "BtcPay External Service")]
            [Required]
            public string ExternalServiceId { get; set; }
        }
    }
}