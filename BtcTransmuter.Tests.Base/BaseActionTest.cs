using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
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


        protected abstract TActionHandler GetActionHandlerInstance(params object[] setupArgs);
    }


    public abstract class BaseTests
    {
        protected static IServiceScopeFactory ScopeFactory;
        protected BaseTests()
        {
            if (ScopeFactory == null)
            {


                var ssF = Program.CreateWebHostBuilder(new string[0]).ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddInMemoryCollection(new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("Database", "Data Source=mydb.db;"),
                        new KeyValuePair<string, string>("DatabaseType", "sqlite"),
                        new KeyValuePair<string, string>("DataProtectionDir", "C:/transmuterkey"),
                        new KeyValuePair<string, string>("ENVIRONMENT", "Development"),
                        new KeyValuePair<string, string>("NBXplorer_Cryptos", "btc;ltc"),
                        new KeyValuePair<string, string>("NBXplorer_Uri", "http://127.0.0.1:32838/"),
                        new KeyValuePair<string, string>("NBXplorer_NetworkType", "Regtest"),
                        new KeyValuePair<string, string>("NBXplorer_UseDefaultCookie", "1"),
                    });
                }).Build().Services.GetRequiredService<IServiceScopeFactory>();

                DependencyHelper.ServiceScopeFactory = ssF;
            }
            else
            {
                Console.Write("sss");
            }
        }
    }
}