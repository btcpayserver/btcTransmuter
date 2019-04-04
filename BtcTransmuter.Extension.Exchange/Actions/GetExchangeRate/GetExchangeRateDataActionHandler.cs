using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.DynamicServices;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using ExchangeSharp;

namespace BtcTransmuter.Extension.Exchange.Actions.GetExchangeRate
{
    public class GetExchangeRateDataActionHandler : BaseActionHandler<GetExchangeRateData, ExchangeTicker>
    {
        public override string ActionId => "GetExchangeRate";
        public override string Name => "Get Exchange Rate";

        public override string Description =>
            "get the bid/ask rate on an exchange for a market symbol";

        public override string ViewPartial => "ViewGetExchangeRateAction";
        public override string ControllerName => "GetExchangeRate";

        protected override async Task<TypedActionHandlerResult<ExchangeTicker>> Execute(Dictionary<string, object> data,
            RecipeAction recipeAction,
            GetExchangeRateData actionData)
        {
            var externalService = await recipeAction.GetExternalService();
            var exchangeService = new ExchangeService(externalService);
            var client = exchangeService.ConstructClient();

            var result = await client.GetTickerAsync(actionData.MarketSymbol);

            return new TypedActionHandlerResult<ExchangeTicker>()
            {
                Executed = true,
                Result =
                    $"{result}",
                TypedData = result
            };
        }
    }
}