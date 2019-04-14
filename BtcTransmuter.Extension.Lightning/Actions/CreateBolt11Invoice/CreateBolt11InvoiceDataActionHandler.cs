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

namespace BtcTransmuter.Extension.Lightning.Actions.CreateBolt11Invoice
{
    public class CreateBolt11InvoiceDataActionHandler : BaseActionHandler<CreateBolt11InvoiceData, LightningInvoice>
    {
        public override string ActionId => "CreateBolt11Invoice";
        public override string Name => "Create Lightning Network Invoice";

        public override string Description =>
            "Create a Bolt11 Lightning invoice on a connected lightning node";

        public override string ViewPartial => "ViewCreateBolt11InvoiceAction";
        
        public override string ControllerName => "CreateBolt11Invoice";

        protected override async Task<TypedActionHandlerResult<LightningInvoice>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            CreateBolt11InvoiceData actionData)
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
                var invoice  = await client.CreateInvoice(
                    LightMoney.FromUnit(decimal.Parse(InterpolateString(actionData.Amount, data)),
                        actionData.AmountMoneyUnit),
                    InterpolateString(actionData.Description, data),
                    TimeSpan.FromMilliseconds(int.Parse(InterpolateString(actionData.ExpiryMilliseconds, data))));
                
                return new TypedActionHandlerResult<LightningInvoice>()
                {
                    Executed = true,
                    Result =
                        $"Created Bolt11 invoice {invoice.BOLT11}",
                    TypedData = invoice
                };
            }
            
           
        }
    }
}