using System.Collections.Generic;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Controllers
{
    public class GetRecipeLogsViewModel
    {
        public List<RecipeInvocation> RecipeInvocations { get; set; }
        public string Name { get; set; }
    }
}