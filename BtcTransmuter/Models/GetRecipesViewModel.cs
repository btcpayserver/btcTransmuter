using System.Collections.Generic;
using System.Text.Json.Serialization;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class GetRecipesViewModel
    {
        [JsonIgnore]
        public string StatusMessage { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; }
        [JsonIgnore]
        public ListMode ViewMode { get; set; } = ListMode.List;

        public enum ListMode
        {
            Cards,
            List
        }
    }
}