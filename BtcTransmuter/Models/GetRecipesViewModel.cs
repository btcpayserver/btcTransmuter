using System.Collections.Generic;
using System.Linq;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Controllers
{
    public class GetRecipesViewModel
    {
        public string StatusMessage { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; }
    }
}