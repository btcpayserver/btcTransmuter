using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class ViewRecipeActionViewModel
    {
        public ExternalServiceData ExternalServiceData { get; set; }
        public RecipeAction RecipeAction { get; set; }
        public IActionDescriptor ActionDescriptor { get; set; }
    }
}