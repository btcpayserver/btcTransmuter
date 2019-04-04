using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.DynamicServices;
using BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode;
using BtcTransmuter.Extension.NBXplorer.Services;
using BTCPayServer.Lightning;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;

namespace BtcTransmuter.Extension.Lightning.Actions.GetLightningChannels
{
    public class GetLightningChannelsDataActionHandler : BaseActionHandler<GetLightningChannelsData, LightningChannel[]>
    {
        public override string ActionId => "GetLightningChannels";
        public override string Name => "Get Lightning Node Channels";

        public override string Description =>
            "get channels of a connected lightning network node";

        public override string ViewPartial => "ViewGetLightningChannelsAction";

        public override string ControllerName => "GetLightningChannels";

        protected override async Task<TypedActionHandlerResult<LightningChannel[]>> Execute(
            Dictionary<string, object> data, RecipeAction recipeAction,
            GetLightningChannelsData actionData)
        {
            using (var serviceScope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var externalService = await recipeAction.GetExternalService();
                var service = new LightningNodeService(externalService, 
                    serviceScope.ServiceProvider.GetService<NBXplorerClientProvider>(),
                    serviceScope.ServiceProvider.GetService<NBXplorerSummaryProvider>(),
                    serviceScope.ServiceProvider.GetService<SocketFactory>()
                );

                var client = service.ConstructClient();
                var result = await client.ListChannels();
                return new TypedActionHandlerResult<LightningChannel[]>()
                {
                    Executed = true,
                    Result =
                        $"Found {result.Length} channels",
                    TypedData = result
                };
            }
        }
    }
}