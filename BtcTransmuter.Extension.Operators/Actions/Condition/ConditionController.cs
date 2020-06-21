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

namespace BtcTransmuter.Extension.Operators.Actions.Condition
{
    [Route("operators-plugin/actions/[controller]")]
    [Authorize]
    public class ConditionController : BaseActionController<ConditionController.ConditionViewModel, ConditionData>
    {

        public ConditionController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
        }

        protected override Task<ConditionViewModel> BuildViewModel(RecipeAction from)
        {
            var fromData = from.Get<ConditionData>();
            return Task.FromResult(new ConditionViewModel
            {
                RecipeId = @from.RecipeId,
                Condition = fromData.Condition
            });
        }

        protected override async Task<(RecipeAction ToSave, ConditionViewModel showViewModel)> BuildModel(
            ConditionViewModel viewModel, RecipeAction mainModel)
        {
          
            if (ModelState.IsValid)    
            {
                mainModel.Set<ConditionData>(viewModel);
                return (mainModel, null);

            }

            return (null, viewModel);
        }

        public class ConditionViewModel : ConditionData, IActionViewModel
        {
            public string Action { get; set; }
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
        }
    }
}