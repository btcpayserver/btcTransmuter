using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Operators.Actions.Condition
{
    public class ConditionData
    {
        [Required]
        public string Condition { get; set; }
    }
}