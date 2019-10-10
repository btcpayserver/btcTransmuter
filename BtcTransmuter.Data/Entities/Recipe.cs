using System.Collections.Generic;

namespace BtcTransmuter.Data.Entities
{
    public class Recipe : BaseIdEntity
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Description { get; set; }

        public RecipeTrigger RecipeTrigger { get; set; }
        public List<RecipeAction> RecipeActions { get; set; } // executes all actions

        public List<RecipeActionGroup>
            RecipeActionGroups
        {
            get;
            set;
        } // executes actions in groups. Action in a group are executed syncrhonously and depending if the  previous action executes

        public List<RecipeInvocation> RecipeInvocations { get; set; } // log
    }
}