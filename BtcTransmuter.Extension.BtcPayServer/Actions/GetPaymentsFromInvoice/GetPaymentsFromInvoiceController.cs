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

namespace BtcTransmuter.Extension.BtcPayServer.Actions.GetPaymentsFromInvoice
{
    [Route("btcpayserver-plugin/actions/[controller]")]
    [Authorize]
    public class GetPaymentsFromInvoiceController : BaseActionController<
        GetPaymentsFromInvoiceController.GetPaymentsFromInvoiceViewModel,
        GetPaymentsFromInvoiceData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public static readonly SelectListItem[] PaymentTypes =
        {
            new SelectListItem("Any", ""),
            new SelectListItem("On-Chain", "BTCLike"),
            new SelectListItem("Off-Chain", "LightningLike")
        };

        public GetPaymentsFromInvoiceController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager,
            IExternalServiceManager externalServiceManager) : base(
            memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<GetPaymentsFromInvoiceViewModel> BuildViewModel(RecipeAction from)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery
            {
                Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                UserId = GetUserId()
            });
            var fromData = from.Get<GetPaymentsFromInvoiceData>();
            return new GetPaymentsFromInvoiceViewModel
            {
                RecipeId = from.RecipeId,
                ExternalServiceId = from.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), from.ExternalServiceId),
                PaymentTypes = new SelectList(PaymentTypes, nameof(SelectListItem.Value), nameof(SelectListItem.Text),
                    fromData.PaymentType),
                PaymentType =  fromData.PaymentType,
                InvoiceId =  fromData.InvoiceId,
                CryptoCode = fromData.CryptoCode
            };
        }

        protected override async Task<(RecipeAction ToSave, GetPaymentsFromInvoiceViewModel showViewModel)> BuildModel(
            GetPaymentsFromInvoiceViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<GetPaymentsFromInvoiceData>(viewModel);
                return (mainModel, null);
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery
            {
                Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                UserId = GetUserId()
            });
            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
            viewModel.PaymentTypes = new SelectList(PaymentTypes, nameof(SelectListItem.Value),
                nameof(SelectListItem.Text), viewModel.PaymentType);
            return (null, viewModel);
        }

        public class GetPaymentsFromInvoiceViewModel : GetPaymentsFromInvoiceData, IActionViewModel, IUseExternalService
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