using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BtcTransmuter.Models
{
    public class EditRecipeTriggerViewModel
    {
        public string RecipeId { get; set; }
        [Display(Name = "Trigger Type")]
        [Required] public string TriggerId { get; set; }
        public SelectList Triggers { get; set; }
        public string StatusMessage { get; set; }
        public RecipeTrigger RecipeTrigger { get; set; }
    }
}