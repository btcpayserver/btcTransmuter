using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Controllers
{
    public class EditRecipeViewModel
    {
        public string StatusMessage { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Description { get; set; }
        [Required] public bool Enabled { get; set; }

        public IEnumerable<RecipeAction> Actions { get; set; }
        public RecipeTrigger Trigger { get; set; }
    }
    
}