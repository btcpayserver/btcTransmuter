using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
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

        public static readonly SelectListItem[] AllowedStatuses = new SelectListItem[]
        {
            new SelectListItem() {Text = "New", Value = Invoice.STATUS_NEW},
            new SelectListItem() {Text = "Paid", Value = Invoice.STATUS_PAID},
            new SelectListItem() {Text = "Invalid", Value = Invoice.STATUS_INVALID},
            new SelectListItem() {Text = "Confirmed", Value = Invoice.STATUS_CONFIRMED},
            new SelectListItem() {Text = "Complete", Value = Invoice.STATUS_COMPLETE},
            new SelectListItem() {Text = "Expired", Value = "expired"},
        };

        public static SelectListItem[] AllowedExceptionStatus = new SelectListItem[]
        {
            new SelectListItem() {Text = "None", Value = Invoice.EXSTATUS_FALSE},
            new SelectListItem() {Text = "Paid partially", Value = Invoice.EXSTATUS_PAID_PARTIAL},
            new SelectListItem() {Text = "Paid over", Value = Invoice.EXSTATUS_PAID_OVER},
            new SelectListItem() {Text = "Paid late", Value = "paidLate"},
            new SelectListItem() {Text = "Marked", Value = "marked"},
        };


        public static SelectListItem[] GetAvailableStatuses(string[] usedKeys)
        {
	        return AllowedStatuses.Where(item => !usedKeys.Contains(item.Value)).ToArray();

        }


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
                Conditions = fromData.Conditions,
            };
        }

        protected override async Task<(RecipeTrigger ToSave, InvoiceStatusChangedTriggerViewModel showViewModel)>
            BuildModel(
                InvoiceStatusChangedTriggerViewModel viewModel, RecipeTrigger mainModel)
        {
	        if (viewModel.Action != "EditData" || !ModelState.IsValid)
            {
                var btcPayServices = await _externalServiceManager.GetExternalServicesData(
                    new ExternalServicesDataQuery()
                    {
                        Type = new[] {BtcPayServerService.BtcPayServerServiceType},
                        UserId = GetUserId()
                    });
                viewModel.ExternalServices = new SelectList(btcPayServices, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);


				if (viewModel.Action.StartsWith("add-condition", StringComparison.InvariantCultureIgnoreCase))
				{
					viewModel.Conditions.Add(new InvoiceStatusChangeCondition()
					{
						Status = GetAvailableStatuses(viewModel.Conditions.Select(condition => condition.Status).ToArray()).FirstOrDefault()?.Value?? Invoice.STATUS_NEW,
						ExceptionStatuses = new List<string>()
						{
							Invoice.EXSTATUS_FALSE
						}
					});
				}

				if (viewModel.Action.StartsWith("remove-condition", StringComparison.InvariantCultureIgnoreCase))
				{
					var index = int.Parse(viewModel.Action.Substring(viewModel.Action.IndexOf(":", StringComparison.InvariantCultureIgnoreCase) + 1));
					viewModel.Conditions.RemoveAt(index);
				}

				return (null, viewModel);
            }

            mainModel.ExternalServiceId = viewModel.ExternalServiceId;
            mainModel.Set((InvoiceStatusChangedTriggerParameters) viewModel);
            return (mainModel, null);
        }

        public class InvoiceStatusChangedTriggerViewModel : InvoiceStatusChangedTriggerParameters
        {
            public string Action { get; set; }
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required][Display(Name = "BtcPay Service")] public string ExternalServiceId { get; set; }
        }
    }
}