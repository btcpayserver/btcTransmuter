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

namespace BtcTransmuter.Extension.Lightning.Actions.GetOnChainLightningDepositAddress
{
    public class GetOnChainLightningDepositAddressDataActionHandler : BaseActionHandler<GetOnChainLightningDepositAddressData, BitcoinAddress>
    {
        public override string ActionId => "GetOnChainLightningDepositAddress";
        public override string Name => "Get Lightning On-chain Deposit Address";

        public override string Description =>
            "Get Deposit Address for lightning node";

        public override string ViewPartial => "ViewGetOnChainLightningDepositAddressAction";

        public override string ControllerName => "GetOnChainLightningDepositAddress";

        protected override async Task<TypedActionHandlerResult<BitcoinAddress>> Execute(
            Dictionary<string, object> data, RecipeAction recipeAction,
            GetOnChainLightningDepositAddressData actionData)
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
                var result = await client.GetDepositAddress();
                return new TypedActionHandlerResult<BitcoinAddress>()
                {
                    Executed = true,
                    Result =
                        $"Got deposit address {result}",
                    TypedData = result
                };
            }
        }
    }
}