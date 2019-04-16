using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Extension.BtcPayServer.HostedServices;
using BtcTransmuter.Extension.DynamicServices;
using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.Actions.GetInvoice
{
    public class GetInvoiceDataActionHandler : BaseActionHandler<GetInvoiceData, BtcPayInvoice>
    {
        public override string ActionId => "GetInvoice";
        public override string Name => "Get BTCPay invoice";

        public override string Description =>
            "Get a specific btcpay invoice";

        public override string ViewPartial => "ViewGetInvoiceAction";

        public override string ControllerName => "GetInvoice";

        public GetInvoiceDataActionHandler()
        {
        }
        protected override async Task<TypedActionHandlerResult<BtcPayInvoice>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            GetInvoiceData actionData)
        {

            var externalService = await recipeAction.GetExternalService();
            var service  = new BtcPayServerService(externalService);
            var invoiceId = InterpolateString(actionData.InvoiceId, data);
            var client = service.ConstructClient();
            var invoice = await client.GetInvoiceAsync<BtcPayInvoice>(invoiceId);
            
            return new BtcPayServerActionHandlerResult<BtcPayInvoice>()
            {
                Executed = true,
                TypedData = invoice,
                Result = $"fetched invoice {invoiceId}"
            };
        }
    }
}