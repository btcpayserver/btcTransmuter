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
            var cryptos = _options.Value.Cryptos?.ToList();
            cryptos?.Insert(0,"Any");
            return new NBXplorerNewBlockViewModel()
            {
                CryptoCodes = new SelectList(cryptos,  innerData.CryptoCode),

                RecipeId = data.RecipeId,
                CryptoCode = innerData.CryptoCode
            };
        }

        protected override async Task<(RecipeTrigger ToSave, NBXplorerNewBlockViewModel showViewModel)> BuildModel(
            NBXplorerNewBlockViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!ModelState.IsValid)
            {
                var cryptos = _options.Value.Cryptos?.ToList();
                cryptos?.Insert(0,"Any");
                
                viewModel.CryptoCodes = new SelectList(cryptos, viewModel.CryptoCode);
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