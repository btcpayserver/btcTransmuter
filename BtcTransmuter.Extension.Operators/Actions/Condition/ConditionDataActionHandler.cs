using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Operators.Actions.Condition
{
    public class ConditionDataActionHandler : BaseActionHandler<ConditionData, string>
    {
        public override string ActionId => "Condition";
        public override string Name => "Check a condition on a value";

        public override string Description =>
            "Check if a value matches your condition";

        public override string ViewPartial => "ViewConditionAction";
        public override string ControllerName => "Condition";


        protected override async Task<TypedActionHandlerResult<string>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            ConditionData actionData)
        {

            var dataToCompare = InterpolateString(actionData.Data, data);
            var condition = InterpolateString(actionData.Condition, data);
            var executed = condition.Equals(dataToCompare, StringComparison.InvariantCultureIgnoreCase);
            
            return new TypedActionHandlerResult<string>()
            {
                TypedData = dataToCompare,
                Executed = executed,
                Result = $"Data value was: {dataToCompare}"
            };
        }
    }
}