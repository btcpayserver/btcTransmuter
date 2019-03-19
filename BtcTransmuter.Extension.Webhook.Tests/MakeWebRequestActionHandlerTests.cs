using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Webhook.Actions.MakeWebRequest;
using Moq;
using Xunit;
using MaxKagamine.Moq.HttpClient;

namespace BtcTransmuter.Extension.Webhook.Tests
{
    public class MakeWebRequestActionHandlerTests
    {
        [Fact]
        public async Task MakeWebRequestActionHandler_Execute_ExecuteOnlyWhenCorrect()
        {
            var handler = new Mock<HttpMessageHandler>();
            var factory = handler.CreateClientFactory();


            Mock.Get(factory).Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => handler.CreateClient());

            var actionHandler = new MakeWebRequestActionHandler(factory);
            var recipeAction = new RecipeAction()
            {
                ActionId = actionHandler.ActionId
            };
            recipeAction.Set(new MakeWebRequestData()
            {
                ContentType = "application/json",
                Url = "http://gozo.com",
                Body = "{}",
                Method = "POST"
            });
            var result = await actionHandler.Execute(new Dictionary<string, object>(), recipeAction);
            Assert.True(result.Executed);
            
            recipeAction = new RecipeAction()
            {
                ActionId = "incorrectid"
            };
            recipeAction.Set(new MakeWebRequestData()
            {
                ContentType = "application/json",
                Url = "http://gozo.com",
                Body = "{}",
                Method = "POST"
            });
            result = await actionHandler.Execute(new Dictionary<string, object>(), recipeAction);
            Assert.False(result.Executed);
        }
        
        [Fact]
        public async Task MakeWebRequestActionHandler_Execute_SendHttpRequest()
        {
            var handler = new Mock<HttpMessageHandler>();
            var factory = handler.CreateClientFactory();


            Mock.Get(factory).Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => handler.CreateClient());

            var actionHandler = new MakeWebRequestActionHandler(factory);
            var recipeAction = new RecipeAction()
            {
                ActionId = actionHandler.ActionId
            };
            recipeAction.Set(new MakeWebRequestData()
            {
                ContentType = "application/json",
                Url = "http://gozo.com",
                Body = "{}",
                Method = "POST"
            });
            await actionHandler.Execute(new Dictionary<string, object>(), recipeAction);
            handler.VerifyRequest(HttpMethod.Post, "http://gozo.com", Times.Once());
        }
    }
}