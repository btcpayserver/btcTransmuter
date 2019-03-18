using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class UpdateActionGroupOrderViewModel
    {
        public RecipeActionGroup RecipeActionGroup { get; set; }
        
        public UpdateActionGroupOrderItem[] UpdateActionGroupOrderItems { get; set; }

        public class UpdateActionGroupOrderItem
        {
            public string RecipeActionId { get; set; }
            public int Order { get; set; }
        }
    }
}