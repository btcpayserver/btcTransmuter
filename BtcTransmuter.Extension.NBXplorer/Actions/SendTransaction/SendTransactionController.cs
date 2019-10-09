using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;

namespace BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction
{
    [Route("nbxplorer-plugin/actions/[controller]")]
    [Authorize]
    public class SendTransactionController : BaseActionController<SendTransactionController.SendTransactionViewModel,
        SendTransactionData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public SendTransactionController(IMemoryCache memoryCache,
            UserManager<User> userManager, 
            IExternalServiceManager externalServiceManager,
            IRecipeManager recipeManager) : base(
            memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<SendTransactionViewModel> BuildViewModel(RecipeAction from)
        {
            var fromData = from.Get<SendTransactionData>();
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                UserId = GetUserId()
            });
            return new SendTransactionViewModel
            {
                RecipeId = from.RecipeId,
                Outputs = fromData.Outputs,
				FeeSatoshiPerByte = fromData.FeeSatoshiPerByte,
				Fee = fromData.Fee,
                ExternalServiceId = from.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), from.ExternalServiceId)
            };
        }

        protected override async Task<(RecipeAction ToSave, SendTransactionViewModel showViewModel)> BuildModel(
            SendTransactionViewModel viewModel, RecipeAction mainModel)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                UserId = GetUserId()
            });
            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);

            if (viewModel.Action.Equals("add-output", StringComparison.InvariantCultureIgnoreCase))
            {
                viewModel.Outputs.Add(new SendTransactionData.TransactionOutput());
                return (null, viewModel);
            }

            if (viewModel.Action.StartsWith("remove-output", StringComparison.InvariantCultureIgnoreCase))
            {
                var index = int.Parse(viewModel.Action.Substring(viewModel.Action.IndexOf(":") + 1));
                viewModel.Outputs.RemoveAt(index);
                return (null, viewModel);
            }

			if(viewModel.Fee.HasValue && viewModel.FeeSatoshiPerByte.HasValue)
			{
				ModelState.AddModelError(nameof(viewModel.FeeSatoshiPerByte),
					"Please choose either a flat fee or a fee rate or leave blank to use the node's fee estimation algorithm");
				ModelState.AddModelError(nameof(viewModel.Fee),
					"Please choose either a flat fee or a fee rate or leave blank to use the node's fee estimation algorithm");
			}

            if (!viewModel.Outputs.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "Please add at least one transaction output");
            }
            else
            {
                var subtractFeesOutputs = viewModel.Outputs.Select((output, i) => (output, i))
                    .Where(tuple => tuple.Item1.SubtractFeesFromOutput);

                if (subtractFeesOutputs.Count() > 1)
                {
                    foreach (var subtractFeesOutput in subtractFeesOutputs)
                    {
                        viewModel.AddModelError(nameof(SendTransactionData.TransactionOutput.SubtractFeesFromOutput),
                            "You can only subtract fees from one output", ModelState);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<SendTransactionData>(viewModel);

                return (mainModel, null);
            }

            return (null, viewModel);
        }

        public class SendTransactionViewModel : SendTransactionData, IActionViewModel, IUseExternalService
        {
            public string Action { get; set; }
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }

            [Display(Name = "NBXplorer Wallet External Service")]
            [Required]
            public string ExternalServiceId { get; set; }
        }
    }
}