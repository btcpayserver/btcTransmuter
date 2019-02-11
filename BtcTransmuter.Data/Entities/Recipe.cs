using System.Collections.Generic;

namespace BtcTransmuter.Data.Entities
{
    public class Recipe
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Description { get; set; }

        public RecipeTrigger RecipeTrigger { get; set; }
        public List<RecipeAction> RecipeActions { get; set; } // executes all actions
        public List<RecipeInvocation> RecipeInvocations { get; set; } // log
    }
}