using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BtcTransmuter.Models
{
    public class EditRecipeActionViewModel
    {
        public string RecipeId { get; set; }
        [Display(Name = "Action Type")]
        public string ActionId { get; set; }
        public SelectList Actions { get; set; }
        [JsonIgnore]
        public string StatusMessage { get; set; }
        public RecipeAction RecipeAction { get; set; }
        public string RecipeActionGroupId { get; set; }
    }
}