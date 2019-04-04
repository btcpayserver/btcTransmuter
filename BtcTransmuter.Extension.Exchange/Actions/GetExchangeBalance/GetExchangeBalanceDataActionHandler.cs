using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.DynamicServices;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using ExchangeSharp;

namespace BtcTransmuter.Extension.Exchange.Actions.GetExchangeBalance
{
    public class GetExchangeBalanceDataActionHandler : BaseActionHandler<GetExchangeBalanceData, decimal>
    {
        public override string ActionId => "GetExchangeBalance";
        public override string Name => "Get Exchange Balance";

        public override string Description =>
            "get the balance of an asset on an exchange";

        public override string ViewPartial => "ViewGetExchangeBalanceAction";
        public override string ControllerName => "GetExchangeBalance";


        protected override async Task<TypedActionHandlerResult<decimal>> Execute(Dictionary<string, object> data,
            RecipeAction recipeAction,
            GetExchangeBalanceData actionData)
        {
            
            var externalService = await recipeAction.GetExternalService();
            var exchangeService = new ExchangeService(externalService);
            var client = exchangeService.ConstructClient();

            var result = await client.GetAmountsAsync();
            var amount = result.ContainsKey(actionData.Asset) ? result[actionData.Asset] : 0;

            return new TypedActionHandlerResult<decimal>()
            {
                Executed = true,
                Result =
                    $"Got exchange balance {amount}{actionData.Asset}",
                TypedData = amount
            };
        }
    }
}