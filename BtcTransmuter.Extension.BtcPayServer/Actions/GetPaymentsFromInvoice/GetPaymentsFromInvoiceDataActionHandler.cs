using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Extension.BtcPayServer.HostedServices;
using BtcTransmuter.Extension.DynamicServices;
using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.Actions.GetPaymentsFromInvoice
{
    public class GetPaymentsFromInvoiceDataActionHandler : BaseActionHandler<GetPaymentsFromInvoiceData, List<InvoicePaymentInfo>>
    {
        public override string ActionId => "GetPaymentsFromInvoice";
        public override string Name => "Get payments on BTCPay invoice";

        public override string Description =>
            "Get the payments made to a specific btcpay invoice";

        public override string ViewPartial => "ViewGetPaymentsFromInvoiceAction";

        public override string ControllerName => "GetPaymentsFromInvoice";

        public GetPaymentsFromInvoiceDataActionHandler()
        {
        }
        protected override async Task<TypedActionHandlerResult<List<InvoicePaymentInfo>>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            GetPaymentsFromInvoiceData actionData)
        {

            var externalService = await recipeAction.GetExternalService();
            var service  = new BtcPayServerService(externalService);
            var invoiceId = InterpolateString(actionData.InvoiceId, data);
            var client = service.ConstructClient();
            var invoice = await client.GetInvoiceAsync<BtcPayInvoice>(invoiceId);

            var payments = invoice.CryptoInfo.Where(info => info.CryptoCode.Equals(actionData.CryptoCode))
                               .SelectMany(info => info.Payments)
                               .Where(x =>
                                   string.IsNullOrEmpty(actionData.PaymentType) ||
                                   x.PaymentType.Equals(actionData.PaymentType))
                               .ToList();
            
            return new BtcPayServerActionHandlerResult<List<InvoicePaymentInfo>>()
            {
                Executed = true,
                TypedData = payments,
                Result = $"found {payments.Count} {actionData.CryptoCode} payments in invoice {invoiceId}"
            };
        }
    }
}