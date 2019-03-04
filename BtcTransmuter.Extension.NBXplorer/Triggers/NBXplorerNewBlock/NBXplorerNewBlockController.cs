using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.NBXplorer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock
{
    [Authorize]
    [Route("nbxplorer-plugin/triggers/new-block")]
    public class NBXplorerNewBlockController : BaseTriggerController<
        NBXplorerNewBlockController.NBXplorerNewBlockViewModel,
        NBXplorerNewBlockTriggerParameters>
    {
        private readonly IOptions<NBXplorerOptions> _options;

        public NBXplorerNewBlockController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache, IOptions<NBXplorerOptions> options) : base(recipeManager, userManager,
            memoryCache)
        {
            _options = options;
        }

        protected override async Task<NBXplorerNewBlockViewModel> BuildViewModel(RecipeTrigger data)
        {
            var innerData = data.Get<NBXplorerNewBlockTriggerParameters>();
            return new NBXplorerNewBlockViewModel()
            {
                CryptoCodes = new SelectList(_options.Value.Cryptos?.ToList() ?? new List<string>(),  innerData.CryptoCode),

                RecipeId = data.RecipeId,
                CryptoCode = innerData.CryptoCode
            };
        }

        protected override async Task<(RecipeTrigger ToSave, NBXplorerNewBlockViewModel showViewModel)> BuildModel(
            NBXplorerNewBlockViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.CryptoCodes = new SelectList(_options.Value.Cryptos?.ToList() ?? new List<string>(), viewModel.CryptoCode);
                return (null, viewModel);
            }

            var recipeTrigger = mainModel;
            recipeTrigger.Set((NBXplorerNewBlockTriggerParameters) viewModel);
            return (recipeTrigger, null);
        }

        public class NBXplorerNewBlockViewModel : NBXplorerNewBlockTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList CryptoCodes { get; set; }
        }
    }
}