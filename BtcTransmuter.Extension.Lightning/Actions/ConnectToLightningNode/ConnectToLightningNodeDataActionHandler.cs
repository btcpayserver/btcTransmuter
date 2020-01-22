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

namespace BtcTransmuter.Extension.Lightning.Actions.ConnectToLightningNode
{
    public class ConnectToLightningNodeDataActionHandler : BaseActionHandler<ConnectToLightningNodeData, NodeInfo>
    {
        public override string ActionId => "ConnectToLightningNode";
        public override string Name => "Connect to a Lightning Node";

        public override string Description =>
            "Connect to a Lightning Node with a connected lightning network node";

        public override string ViewPartial => "ViewConnectToLightningNodeAction";

        public override string ControllerName => "ConnectToLightningNode";

        protected override async Task<TypedActionHandlerResult<NodeInfo>> Execute(
            Dictionary<string, object> data, RecipeAction recipeAction,
            ConnectToLightningNodeData actionData)
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
                
                if (! NodeInfo.TryParse(InterpolateString(actionData.NodeInfo, data),
                    out var nodeInfo))
                {
                    return new TypedActionHandlerResult<NodeInfo>()
                    {
                        Executed = false,
                        Result =
                            $"Could not connect because node info was incorrect",
                    };
                }

                await client.ConnectTo(nodeInfo);
                return new TypedActionHandlerResult<NodeInfo>()
                {
                    Executed = true,
                    Result =
                        $"Connected to LN Node {nodeInfo}",
                    TypedData = nodeInfo
                };
            }
        }
    }
}