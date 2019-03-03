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

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    [Authorize]
    [Route("nbxplorer-plugin/triggers/new-transaction")]
    public class NBXplorerNewTransactionController : BaseTriggerController<
        NBXplorerNewTransactionController.NBXplorerNewTransactionViewModel,
        NBXplorerNewTransactionTriggerParameters>
    {
        private readonly IOptions<NBXplorerOptions> _options;

        public NBXplorerNewTransactionController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache, IOptions<NBXplorerOptions> options) : base(recipeManager, userManager,
            memoryCache)
        {
            _options = options;
        }

        protected override async Task<NBXplorerNewTransactionViewModel> BuildViewModel(RecipeTrigger data)
        {
            var innerData = data.Get<NBXplorerNewTransactionTriggerParameters>();
            var cryptos = _options.Value.Cryptos?.ToList();
            cryptos?.Insert(0,"Any");
            return new NBXplorerNewTransactionViewModel()
            {
                CryptoCodes = new SelectList(cryptos,  innerData.CryptoCode),

                RecipeId = data.RecipeId,
                CryptoCode = innerData.CryptoCode
            };
        }

        protected override async Task<(RecipeTrigger ToSave, NBXplorerNewTransactionViewModel showViewModel)> BuildModel(
            NBXplorerNewTransactionViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!ModelState.IsValid)
            {
                var cryptos = _options.Value.Cryptos?.ToList();
                cryptos?.Insert(0,"Any");
                
                viewModel.CryptoCodes = new SelectList(cryptos, viewModel.CryptoCode);
                return (null, viewModel);
            }

            var recipeTrigger = mainModel;
            recipeTrigger.Set((NBXplorerNewTransactionTriggerParameters) viewModel);
            return (recipeTrigger, null);
        }

        public class NBXplorerNewTransactionViewModel : NBXplorerNewTransactionTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList CryptoCodes { get; set; }
        }
    }
}