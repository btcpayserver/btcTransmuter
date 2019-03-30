using System.Collections.Generic;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class GetRecipeLogsViewModel
    {
        public List<RecipeInvocation> RecipeInvocations { get; set; }
        public string Name { get; set; }
        public int  Skip { get; set; }
        public int  Take { get; set; }
        public object Id { get; set; }
    }
}