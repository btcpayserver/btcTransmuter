
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Recipe.Actions.ToggleRecipe
{
    public class ToggleRecipeData
    {
        [Required]
        public string TargetRecipeId { get; set; }
        public ToggleRecipeOption Option { get; set; }

        public enum ToggleRecipeOption
        {
            Enable,
            Disable,
            Toggle
        }
    }
}