using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BtcTransmuter.Data.Entities
{
    public class RecipeActionGroup : BaseIdEntity
    {
        public string RecipeId { get; set; }

        public Recipe Recipe { get; set; }
        public List<RecipeAction> RecipeActions { get; set; }
    }
}