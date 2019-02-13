namespace BtcTransmuter.Abstractions.Recipes
{
    public class RecipesQuery
    {
        public bool? Enabled { get; set; }
        
        public string RecipeTriggerId { get; set; }
        public string UserId { get; set; }
        public string RecipeId { get; set; }
    }
}