namespace BtcTransmuter.Data.Entities
{
    public class RecipeAction : BaseEntity
    {
        public string RecipeId { get; set; }
        public string ExternalServiceId { get; set; }

        public string ActionId { get; set; }
        public Recipe Recipe { get; set; }
        public ExternalServiceData ExternalService { get; set; }
    }
}