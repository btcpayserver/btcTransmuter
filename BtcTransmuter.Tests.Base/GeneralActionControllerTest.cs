using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public class GeneralActionControllerTest : BaseActionControllerTest<TestActionController,
        TestActionController.TestRecipeActionData, TestActionController.TestRecipeActionData>
    {
        [Fact]
        public async Task CanLoadEditView()
        {
            ConfigureDependencyHelper();
            ServiceScope = ScopeFactory.CreateScope();
            var controller = GetController();

            var wrongId = Assert.IsType<RedirectToActionResult>(await controller.EditData("wrong"));
            Assert.Equal("Recipes", wrongId.ControllerName);
            Assert.True(wrongId.RouteValues.ContainsKey("statusMessage"));
            Assert.Equal(StatusMessageModel.StatusSeverity.Error,
                new StatusMessageModel(wrongId.RouteValues["statusMessage"].ToString()).Severity);

            var memoryCache = ServiceScope.ServiceProvider.GetRequiredService<IMemoryCache>();
            var recipeManager = ServiceScope.ServiceProvider.GetRequiredService<IRecipeManager>();

            var identifier = "correctidbutnorecipe";
            memoryCache.Set(identifier, new RecipeAction()
            {
                RecipeId = "bad",
            });

            wrongId = Assert.IsType<RedirectToActionResult>(await controller.EditData(identifier));
            Assert.Equal("Recipes", wrongId.ControllerName);
            Assert.True(wrongId.RouteValues.ContainsKey("statusMessage"));
            Assert.Equal(StatusMessageModel.StatusSeverity.Error,
                new StatusMessageModel(wrongId.RouteValues["statusMessage"].ToString()).Severity);


            identifier = "correctid";
            var recipe = new Recipe()
            {
                Name = "A",
                Description = "B",
                Enabled = true
            };

            await recipeManager.AddOrUpdateRecipe(recipe);
            var recipeActionGroup = new RecipeActionGroup();
            await recipeManager.AddRecipeActionGroup(recipeActionGroup);
            var recipeAction = new RecipeAction()
            {
                RecipeId = recipe.Id,
                RecipeActionGroupId = recipeActionGroup.Id,
                Order = 1
            };

            var recipeAction2 = new RecipeAction()
            {
                RecipeId = recipe.Id,
                RecipeActionGroupId = recipeActionGroup.Id,
                Order = 0
            };
            await recipeManager.AddOrUpdateRecipeAction(recipeAction);
            await recipeManager.AddOrUpdateRecipeAction(recipeAction2);
            memoryCache.Set(identifier, recipeAction);

            var correct =
                Assert.IsAssignableFrom<IActionViewModel>(Assert
                    .IsType<ViewResult>(await controller.EditData(identifier)).Model);
            Assert.Equal(recipe.Id, correct.RecipeId);
            Assert.Equal(recipeAction2.Id, correct.RecipeActionIdInGroupBeforeThisOne);
        }

        public override TestActionController GetController()
        {
            return new TestActionController(
                ServiceScope.ServiceProvider.GetRequiredService<IMemoryCache>(),
                ServiceScope.ServiceProvider.GetRequiredService<UserManager<User>>(),
                ServiceScope.ServiceProvider.GetRequiredService<IRecipeManager>(),
                ServiceScope.ServiceProvider.GetRequiredService<IExternalServiceManager>());
        }
    }
}