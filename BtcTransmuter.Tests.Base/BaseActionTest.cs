using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.Json;
using Moq;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public abstract class BaseActionTest<TActionHandler, TActionData, TActionResultData> : BaseTests
        where TActionHandler : BaseActionHandler<TActionData, TActionResultData>
    {
        [Fact]
        public void BasicActionPropertiesSet()
        {
            var actionHandler = GetActionHandlerInstance();
            Assert.NotNullOrEmpty(actionHandler.Name);
            Assert.NotNullOrEmpty(actionHandler.ActionId);
            Assert.NotNullOrEmpty(actionHandler.ViewPartial);
            Assert.NotNullOrEmpty(actionHandler.ControllerName);
            Assert.NotNullOrEmpty(actionHandler.Description);
            Assert.NotNull(actionHandler.ActionResultDataType);
        }

        [Fact]
        public async Task CanExecute_ExecuteOnlyWhenCorrect()
        {
            var actionHandler = GetActionHandlerInstance();

            var recipeAction = new RecipeAction()
            {
                ActionId = actionHandler.ActionId
            };

            var result =
                await actionHandler.CanExecute(new Dictionary<string, (object data, string json)>(), recipeAction);

            Assert.True(result);

            recipeAction = new RecipeAction()
            {
                ActionId = "incorrectid"
            };
            result = await actionHandler.CanExecute(new Dictionary<string, (object data, string json)>(), recipeAction);
            Assert.False(result);
        }

        [Fact]
        public async Task EditData_GenerateActionResultToEditRecipeAction()
        {
            ConfigureDependencyHelper();
            var actionHandler = GetActionHandlerInstance();

            var newRecipeAction = new RecipeAction()
            {
                ActionId = actionHandler.ActionId,
            };
            var newResult = Assert.IsType<RedirectToActionResult>(await actionHandler.EditData(newRecipeAction));
            Assert.Equal(actionHandler.ControllerName, newResult.ControllerName);

            var existingRecipeAction = new RecipeAction()
            {
                ActionId = actionHandler.ActionId,
                Id = Guid.NewGuid().ToString()
            };
            var existingResult =
                Assert.IsType<RedirectToActionResult>(await actionHandler.EditData(existingRecipeAction));
            Assert.Equal(actionHandler.ControllerName, existingResult.ControllerName);
            Assert.NotEqual(newResult.RouteValues["identifier"], existingResult.RouteValues["identifier"]);
        }


        
        
        protected virtual TActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            return Activator.CreateInstance<TActionHandler>();
        }
    }
}