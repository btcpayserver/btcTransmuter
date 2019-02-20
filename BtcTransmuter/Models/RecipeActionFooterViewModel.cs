using System.Collections.Generic;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;

namespace BtcTransmuter.ViewComponents
{
    public class RecipeActionFooterViewModel
    {
        public Dictionary<string, object> Properties { get; set; }
        public bool NoRecipeTrigger { get; set; }
    }
}