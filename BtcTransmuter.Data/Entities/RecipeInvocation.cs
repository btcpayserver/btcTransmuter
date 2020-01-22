using System;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Data.Entities
{
    public class RecipeInvocation: BaseIdEntity
    {
        public string RecipeId { get; set; }
        
        public string RecipeAction { get; set; }
        public string TriggerDataJson { get; set; }
        public string ActionResult { get; set; }
        public DateTime Timestamp { get; set; }

        public Recipe Recipe { get; set; }
    }
}