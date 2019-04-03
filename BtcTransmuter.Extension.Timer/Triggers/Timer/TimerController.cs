using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Timer.Triggers.Timer
{
    [Authorize]
    [Route("timer-plugin/triggers/[controller]")]
    public class TimerController : BaseTriggerController<TimerController.TimerTriggerViewModel, TimerTriggerParameters>
    {
        public TimerController(IRecipeManager recipeManager, UserManager<User> userManager, IMemoryCache memoryCache,
            IExternalServiceManager externalServiceManager) :
            base(recipeManager, userManager, memoryCache, externalServiceManager)
        {
        }

        protected override Task<TimerTriggerViewModel> BuildViewModel(RecipeTrigger data)
        {
            return Task.FromResult(new TimerTriggerViewModel(data.Get<TimerTriggerParameters>(), data.RecipeId));
        }

        protected override Task<(RecipeTrigger ToSave, TimerTriggerViewModel showViewModel)> BuildModel(
            TimerTriggerViewModel viewModel, RecipeTrigger mainModel)
        {
            if (viewModel.TriggerEveryAmount <= 0)
            {
                ModelState.AddModelError(nameof(TimerTriggerViewModel.TriggerEveryAmount),
                    "Amount needs to be at least 1");
            }

            if (!ModelState.IsValid)
            {
                return Task.FromResult<(RecipeTrigger ToSave, TimerTriggerViewModel showViewModel)>((null, viewModel));
            }

            mainModel.Set((TimerTriggerParameters) viewModel);

            return Task.FromResult<(RecipeTrigger ToSave, TimerTriggerViewModel showViewModel)>((mainModel, null));
        }

        public class TimerTriggerViewModel : TimerTriggerParameters
        {
            public TimerTriggerViewModel()
            {
            }

            public TimerTriggerViewModel(TimerTriggerParameters data, string recipeId)
            {
                StartOn = data.StartOn;
                TriggerEvery = data.TriggerEvery;
                TriggerEveryAmount = data.TriggerEveryAmount;
                RecipeId = recipeId;
            }

            public string RecipeId { get; private set; }
        }
    }
}