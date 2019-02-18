using System.Collections;
using System.Collections.Generic;

namespace BtcTransmuter.Data.Entities
{
    public class ExternalServiceData : BaseEntity
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }

        public User User { get; set; }
        
        
        public ICollection<RecipeTrigger> RecipeTriggers { get; set; }
        public ICollection<RecipeAction> RecipeActions { get; set; }
    }
}