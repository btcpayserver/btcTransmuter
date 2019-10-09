using System;
using System.Collections.Generic;
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

namespace BtcTransmuter.Extension.Lightning.Actions.OpenLightningChannel
{
    public class OpenLightningChannelDataActionHandler : BaseActionHandler<OpenLightningChannelData, OpenChannelResponse>
    {
        public override string ActionId => "OpenLightningChannel";
        public override string Name => "Open a Lightning Network Channel";

        public override string Description =>
            "Open a Lightning Network Channel on a connected lightning node";

        public override string ViewPartial => "ViewOpenLightningChannelAction";
        
        public override string ControllerName => "OpenLightningChannel";

        protected override async Task<TypedActionHandlerResult<OpenChannelResponse>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            OpenLightningChannelData actionData)
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
                if (!NodeInfo.TryParse(InterpolateString(actionData.NodeInfo, data),
                    out var nodeInfo))
                {
                    return new TypedActionHandlerResult<OpenChannelResponse>()
                    {
                        Executed = false,
                        Result =
                            $"Could not open channel because node info was incorrect",
                    };
                }

                var result = await client.OpenChannel(new OpenChannelRequest()
                {
                    NodeInfo = nodeInfo,
                    ChannelAmount = Money.FromUnit(decimal.Parse(InterpolateString(actionData.Amount, data)),
                        actionData.AmountMoneyUnit)
                });
                
                return new TypedActionHandlerResult<OpenChannelResponse>()
                {
                    Executed = result.Result == OpenChannelResult.Ok,
                    Result =
                        $"Open LN Channel with response {Enum.GetName(typeof(OpenChannelResult), result.Result)}",
                    TypedData = result
                };
            }
            
           
        }
    }
}