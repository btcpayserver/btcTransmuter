using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Operators.Actions.ChooseNumericValue
{
    [Route("operators-plugin/actions/[controller]")]
    [Authorize]
    public class ChooseNumericValueController : BaseActionController<ChooseNumericValueController.ChooseNumericValueViewModel, ChooseNumericValueData>
    {

        public ChooseNumericValueController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
        }

        protected override Task<ChooseNumericValueViewModel> BuildViewModel(RecipeAction from)
        {
            var fromData = from.Get<ChooseNumericValueData>();
            return Task.FromResult(new ChooseNumericValueViewModel
            {
                RecipeId = @from.RecipeId,
                Items =  fromData.Items
            });
        }

        protected override async Task<(RecipeAction ToSave, ChooseNumericValueViewModel showViewModel)> BuildModel(
            ChooseNumericValueViewModel viewModel, RecipeAction mainModel)
        {
            if (viewModel.Action.Equals("add-item", StringComparison.InvariantCultureIgnoreCase))
            {
                viewModel.Items.Add(new ChooseNumericValueData.ChooseNumericValueDataItem());
                return (null, viewModel);
            }

            if (viewModel.Action.StartsWith("remove-item", StringComparison.InvariantCultureIgnoreCase))
            {
                var index = int.Parse(viewModel.Action.Substring(viewModel.Action.IndexOf(":") + 1));
                viewModel.Items.RemoveAt(index);
                return (null, viewModel);
            }
            if (ModelState.IsValid)    
            {
                mainModel.Set<ChooseNumericValueData>(viewModel);
                return (mainModel, null);

            }

            return (null, viewModel);
        }

        public class ChooseNumericValueViewModel : ChooseNumericValueData, IActionViewModel
        {
            public string Action { get; set; }
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
        }
    }
}