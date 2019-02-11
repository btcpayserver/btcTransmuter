using System;

namespace BtcTransmuter.Data.Entities
{
    public class RecipeInvocation
    {
        public string Id { get; set; }    
        public int RecipeId { get; set; }
        public int RecipeActionId { get; set; }
        public int RecipeTriggerId { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public Recipe Recipe { get; set; }
        public RecipeAction RecipeAction { get; set; }    
        public RecipeTrigger RecipeTrigger { get; set; }
        
        public string ActionResult { get; set; }
    }
}