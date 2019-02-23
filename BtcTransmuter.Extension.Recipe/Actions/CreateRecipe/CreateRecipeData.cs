
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Recipe.Actions.CreateRecipe
{
    public class CreateRecipeData
    {
        [Required]
        public string RecipeTemplateId { get; set; }
        public bool Enable { get; set; }
    }
}