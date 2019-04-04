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

namespace BtcTransmuter.Extension.Lightning.Actions.GetLightningNodeInfo
{
    public class GetLightningNodeInfoDataActionHandler : BaseActionHandler<GetLightningNodeInfoData, LightningNodeInformation>
    {
        public override string ActionId => "GetLightningNodeInfo";
        public override string Name => "Get Lightning Node Info";

        public override string Description =>
            "get Node info of a connected lightning network node";

        public override string ViewPartial => "ViewGetLightningNodeInfoAction";

        public override string ControllerName => "GetLightningNodeInfo";

        protected override async Task<TypedActionHandlerResult<LightningNodeInformation>> Execute(
            Dictionary<string, object> data, RecipeAction recipeAction,
            GetLightningNodeInfoData actionData)
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
                var result = await client.GetInfo();
                return new TypedActionHandlerResult<LightningNodeInformation>()
                {
                    Executed = true,
                    Result =
                        $"Got lightning node info block height:{result.BlockHeight}, node: {result.NodeInfoList.First()}",
                    TypedData = result
                };
            }
        }
    }
}