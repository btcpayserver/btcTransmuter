namespace BtcTransmuter.Abstractions.Recipes
{
    public class RecipesQuery
    {
        public bool? Enabled { get; set; }
        
        public string TriggerId { get; set; }
        public string UserId { get; set; }
        public string RecipeId { get; set; }

        public bool IncludeRecipeInvocations { get; set; }
    }
}