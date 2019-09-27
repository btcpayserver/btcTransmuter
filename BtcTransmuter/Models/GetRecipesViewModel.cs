using System.Collections.Generic;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class GetRecipesViewModel
    {
        public string StatusMessage { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; }
        public ListMode ViewMode { get; set; } = ListMode.Cards;

        public enum ListMode
        {
            Cards,
            List
        }
    }
}