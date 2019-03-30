using BtcTransmuter.Abstractions.Models;

namespace BtcTransmuter.Abstractions.Recipes
{
    public class RecipeInvocationsQuery
    {
        public string RecipeId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

        public OrderBy<RecipeInvocationsQueryOrderBy> OrderBy { get; set; }

        public enum RecipeInvocationsQueryOrderBy
        {
            Timestamp
        }
    }
}