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


        protected override Task<TypedActionHandlerResult<string>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            ConditionData actionData)
        {
            var condition = InterpolateString(actionData.Condition, data);
            
            return Task.FromResult(new TypedActionHandlerResult<string>()
            {
                TypedData = condition,
                Executed = condition.Equals("true", StringComparison.InvariantCultureIgnoreCase),
                Result = $"Data value was: {condition}"
            });
        }
    }
}