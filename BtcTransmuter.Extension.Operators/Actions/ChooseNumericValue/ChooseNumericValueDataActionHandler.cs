using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Operators.Actions.ChooseNumericValue
{
    public class ChooseNumericValueDataActionHandler : BaseActionHandler<ChooseNumericValueData, string>
    {
        public override string ActionId => "ChooseNumericValue";
        public override string Name => "Choose Value Through Numeric Operation";

        public override string Description =>
            "Choose a value through a numeric condition";

        public override string ViewPartial => "ViewChooseNumericValueAction";
        public override string ControllerName => "ChooseNumericValue";


        protected override Task<TypedActionHandlerResult<string>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            ChooseNumericValueData actionData)
        {
            ChooseNumericValueData.ChooseNumericValueDataItem selectedItem = null;
            actionData.Items.ForEach(item =>
            {
                if (selectedItem == null)
                {
                    selectedItem = item;
                }
                else
                {
                    var selectedItemValueToCompare = decimal.Parse(selectedItem.ValueToCompare);
                    var valueToCompare = decimal.Parse(item.ValueToCompare);

                    switch (actionData.Comparison)
                    {
                        case ChooseNumericValueData.NumericComparison.Maximum:
                            if (valueToCompare > selectedItemValueToCompare)
                            {
                                selectedItem = item;
                            }
                            break;
                        case ChooseNumericValueData.NumericComparison.Minimum:
                            if (valueToCompare < selectedItemValueToCompare)
                            {
                                selectedItem = item;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

            });
            return Task.FromResult(new TypedActionHandlerResult<string>()
            {
                TypedData = selectedItem.ValueToChoose,
                Executed = true,
                Result = $"chose {selectedItem.ValueToChoose}"
            });
        }
    }
}