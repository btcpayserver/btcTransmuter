using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBXplorer.DerivationStrategy;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    [Authorize]
    [Route("nbxplorer-plugin/triggers/[controller]")]
    public class NBXplorerNewTransactionController : BaseTriggerController<
        NBXplorerNewTransactionController.NBXplorerNewTransactionViewModel,
        NBXplorerNewTransactionTriggerParameters>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public NBXplorerNewTransactionController(IRecipeManager recipeManager,
            UserManager<User> userManager,
            IMemoryCache memoryCache,
            IExternalServiceManager externalServiceManager) : base(recipeManager, userManager,
            memoryCache, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<NBXplorerNewTransactionViewModel> BuildViewModel(RecipeTrigger data)
        {
            var innerData = data.Get<NBXplorerNewTransactionTriggerParameters>();
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                UserId = GetUserId()
            });
            return new NBXplorerNewTransactionViewModel()
            {
                ExternalServiceId = data.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId),
                RecipeId = data.RecipeId,
                ConfirmationsRequired = innerData.ConfirmationsRequired,
            };
        }

        protected override async Task<(RecipeTrigger ToSave, NBXplorerNewTransactionViewModel showViewModel)>
            BuildModel(
                NBXplorerNewTransactionViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!ModelState.IsValid)
            {
                var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                    UserId = GetUserId()
                });
                viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);

                return (null, viewModel);
            }

            var recipeTrigger = mainModel;
            var oldData = recipeTrigger.Get<NBXplorerNewTransactionTriggerParameters>();
            var newData = (NBXplorerNewTransactionTriggerParameters) viewModel;
            newData.Transactions = oldData.Transactions;

            recipeTrigger.ExternalServiceId = viewModel.ExternalServiceId;
            recipeTrigger.Set((NBXplorerNewTransactionTriggerParameters) viewModel);

            return (recipeTrigger, null);
        }

        public class NBXplorerNewTransactionViewModel : NBXplorerNewTransactionTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }

            [Display(Name = "NBXplorer Wallet External Service")]
            [Required]
            public string ExternalServiceId { get; set; }
        }
    }
}