using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin.Protocol;

namespace BtcTransmuter.Tests.Base
{
    public class BaseActionControllerTest<TActionController, TViewModel, TRecipeActionData> : BaseTests
        where TActionController : BaseActionController<TViewModel, TRecipeActionData>
        where TViewModel : TRecipeActionData, IActionViewModel
    {
        protected IServiceScope ServiceScope;

        public BaseActionControllerTest()
        {
        }

       

        public virtual TActionController GetController()
        {
            return ServiceScope.ServiceProvider.GetRequiredService<TActionController>();
        }
    }


    public class TestActionController : BaseActionController<TestActionController.TestRecipeActionData, TestActionController.TestRecipeActionData>
    {
        public class TestRecipeActionData : IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
        }

        public TestActionController(IMemoryCache memoryCache, UserManager<User> userManager, IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache, userManager, recipeManager, externalServiceManager)
        {
        }

        protected override Task<TestRecipeActionData> BuildViewModel(RecipeAction recipeAction)
        {
            return Task.FromResult(new TestRecipeActionData());
        }

        protected override Task<(RecipeAction ToSave, TestRecipeActionData showViewModel)> BuildModel(TestRecipeActionData viewModel, RecipeAction mainModel)
        {
            return Task.FromResult<(RecipeAction ToSave, TestRecipeActionData showViewModel)>((mainModel, null));
        }
    }
}