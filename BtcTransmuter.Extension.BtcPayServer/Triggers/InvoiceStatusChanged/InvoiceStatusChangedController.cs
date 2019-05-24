using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    [Authorize]
    [Route("btcpayserver-plugin/triggers/[controller]")]
    public class InvoiceStatusChangedController : BaseTriggerController<
        InvoiceStatusChangedController.InvoiceStatusChangedTriggerViewModel, InvoiceStatusChangedTriggerParameters>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        private readonly SelectListItem[] AllowedStatuses = new SelectListItem[]
        {
            new SelectListItem() {Text = "Any Status", Value = null},
            new SelectListItem() {Text = "New", Value = Invoice.STATUS_NEW},
            
            new SelectListItem() {Text = "New(Paid Partially)", Value = $"{Invoice.STATUS_NEW}_{Invoice.EXSTATUS_PAID_PARTIAL}"},
            new SelectListItem() {Text = "Paid", Value = Invoice.STATUS_PAID},
            new SelectListItem() {Text = "Paid(Paid Partially)", Value = $"{Invoice.STATUS_PAID}_{Invoice.EXSTATUS_PAID_PARTIAL}"},
            new SelectListItem() {Text = "Paid(Paid Over)", Value = $"{Invoice.STATUS_PAID}_{Invoice.EXSTATUS_PAID_OVER}"},
            new SelectListItem() {Text = "Invalid", Value = Invoice.STATUS_INVALID},
            
            new SelectListItem() {Text = "Invalid(Marked)", Value = $"{Invoice.STATUS_INVALID}_marked"},
            new SelectListItem() {Text = "Invalid(Paid Partially)", Value = $"{Invoice.STATUS_INVALID}_{Invoice.EXSTATUS_PAID_PARTIAL}"},
            new SelectListItem() {Text = "Invalid(Paid Over)", Value = $"{Invoice.STATUS_INVALID}_{Invoice.EXSTATUS_PAID_OVER}"},
            new SelectListItem() {Text = "Confirmed", Value = Invoice.STATUS_CONFIRMED},
            new SelectListItem() {Text = "Confirmed(Paid Partially)", Value = $"{Invoice.STATUS_CONFIRMED}_{Invoice.EXSTATUS_PAID_PARTIAL}"},
            new SelectListItem() {Text = "Confirmed(Paid Over)", Value = $"{Invoice.STATUS_CONFIRMED}_{Invoice.EXSTATUS_PAID_OVER}"},
            new SelectListItem() {Text = "Complete", Value = Invoice.STATUS_COMPLETE},
            new SelectListItem() {Text = "Complete(Marked)", Value = $"{Invoice.STATUS_COMPLETE}_marked"},
            new SelectListItem() {Text = "Expired", Value = "expired"},
            new SelectListItem() {Text = "Complete(Marked)", Value = $"expired_paidLate"},
        };


        public InvoiceStatusChangedController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache, IExternalServiceManager externalServiceManager) : base(recipeManager, userManager,
            memoryCache, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<InvoiceStatusChangedTriggerViewModel> BuildViewModel(RecipeTrigger data)
        {
            var btcPayServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                UserId = GetUserId()
            });

            var fromData = data.Get<InvoiceStatusChangedTriggerParameters>();

            return new InvoiceStatusChangedTriggerViewModel
            {
                ExternalServices = new SelectList(btcPayServices, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId),
                RecipeId = data.RecipeId,
                ExternalServiceId = data.ExternalServiceId,
                Status = fromData.Status,
                Statuses = new SelectList(AllowedStatuses, "Value", "Text", fromData.Status),
            };
        }

        protected override async Task<(RecipeTrigger ToSave, InvoiceStatusChangedTriggerViewModel showViewModel)>
            BuildModel(
                InvoiceStatusChangedTriggerViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!ModelState.IsValid)
            {
                var btcPayServices = await _externalServiceManager.GetExternalServicesData(
                    new ExternalServicesDataQuery()
                    {
                        Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                        UserId = GetUserId()
                    });


                viewModel.Statuses = new SelectList(AllowedStatuses, "Value", "Text", viewModel.Status);
                viewModel.ExternalServices = new SelectList(btcPayServices, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
                return (null, viewModel);
            }

            mainModel.ExternalServiceId = viewModel.ExternalServiceId;
            mainModel.Set((InvoiceStatusChangedTriggerParameters) viewModel);
            return (mainModel, null);
        }

        public class InvoiceStatusChangedTriggerViewModel : InvoiceStatusChangedTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required][Display(Name = "BtcPay Service")] public string ExternalServiceId { get; set; }
            public SelectList Statuses { get; set; }
        }
    }
}