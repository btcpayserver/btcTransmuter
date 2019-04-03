using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Lightning.Actions.CreateBolt11Invoice
{
    [Route("lightning-plugin/actions/[controller]")]
    [Authorize]
    public class CreateBolt11InvoiceController : BaseActionController<CreateBolt11InvoiceController.CreateBolt11InvoiceViewModel, CreateBolt11InvoiceData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public CreateBolt11InvoiceController(IMemoryCache memoryCache, UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<CreateBolt11InvoiceViewModel> BuildViewModel(RecipeAction from)
        {
            var fromData = from.Get<CreateBolt11InvoiceData>();
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {LightningNodeService.LightningNodeServiceType},
                UserId = GetUserId()
            });
            return new CreateBolt11InvoiceViewModel
            {
                
                RecipeId = from.RecipeId,
                ExternalServiceId = from.ExternalServiceId,
                Amount = fromData.Amount,
                Description = fromData.Description,
                AmountMoneyUnit= fromData.AmountMoneyUnit,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), from.ExternalServiceId),
            };
        }

        protected override async Task<(RecipeAction ToSave, CreateBolt11InvoiceViewModel showViewModel)> BuildModel(
            CreateBolt11InvoiceViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<CreateBolt11InvoiceData>(viewModel);
                return (mainModel, null);
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {LightningNodeService.LightningNodeServiceType},
                UserId = GetUserId()
            });


            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
            return (null, viewModel);
        }

        public class CreateBolt11InvoiceViewModel : CreateBolt11InvoiceData, IUseExternalService, IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }
            [Display(Name = "Lightning Node External Service")]
            [Required] public string ExternalServiceId { get; set; }
        }
    }
}