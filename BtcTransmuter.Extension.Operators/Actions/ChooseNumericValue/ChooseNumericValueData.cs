using System.Collections.Generic;

namespace BtcTransmuter.Extension.Operators.Actions.ChooseNumericValue
{
    public class ChooseNumericValueData
    {
        public List<ChooseNumericValueDataItem> Items { get; set; } = new List<ChooseNumericValueDataItem>();
        public NumericComparison Comparison { get; set; }

        public class ChooseNumericValueDataItem
        {
            public string ValueToCompare { get; set; }
            public string ValueToChoose { get; set; }
        }

        public enum NumericComparison
        {
            Maximum,
            Minimum
        }
    }
}