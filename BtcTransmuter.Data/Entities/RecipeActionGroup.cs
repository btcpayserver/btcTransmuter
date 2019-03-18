using System.Collections.Generic;

namespace BtcTransmuter.Data.Entities
{
    public class RecipeActionGroup
    {
        public string Id { get; set; }
        public string RecipeId { get; set; }

        public Recipe Recipe { get; set; }
        public List<RecipeAction> RecipeActions { get; set; }
    }
}