namespace BtcTransmuter.Data.Entities
{
    public class RecipeTrigger : BaseEntity
    {
        public string RecipeId { get; set; }

        public string ExternalServiceId { get; set; }

        public string TriggerId { get; set; }
        public Recipe Recipe { get; set; }
        public ExternalServiceData ExternalService { get; set; }
    }
}