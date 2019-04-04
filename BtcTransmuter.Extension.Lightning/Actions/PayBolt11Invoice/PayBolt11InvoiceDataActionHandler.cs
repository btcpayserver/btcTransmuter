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

namespace BtcTransmuter.Extension.Lightning.Actions.PayBolt11Invoice
{
    public class PayBolt11InvoiceDataActionHandler : BaseActionHandler<PayBolt11InvoiceData, PayResponse>
    {
        public override string ActionId => "PayBolt11Invoice";
        public override string Name => "Pay Lightning Network Invoice";

        public override string Description =>
            "Pay a Bolt11 Lightning invoice with a connected lightning node";

        public override string ViewPartial => "ViewPayBolt11InvoiceAction";

        public override string ControllerName => "PayBolt11Invoice";

        protected override async Task<TypedActionHandlerResult<PayResponse>> Execute(
            Dictionary<string, object> data, RecipeAction recipeAction,
            PayBolt11InvoiceData actionData)
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
                var response = await client.Pay(InterpolateString(actionData.Bolt11, data));
                return new TypedActionHandlerResult<PayResponse>()
                {
                    Executed = response.Result == PayResult.Ok,
                    Result =
                        $"Paying Bolt11 Invoice: {Enum.GetName(typeof(PayResult), response.Result)}",
                    TypedData = response
                };
            }
        }
    }
}