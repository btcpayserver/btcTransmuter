using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class EditRecipeViewModel
    {
        public string Id { get; set; }
        public string StatusMessage { get; set; }
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        [Required] public bool Enabled { get; set; }

        public IEnumerable<RecipeAction> Actions { get; set; }
        public List<RecipeActionGroup> ActionGroups { get; set; }
        public RecipeTrigger Trigger { get; set; }
    }
}