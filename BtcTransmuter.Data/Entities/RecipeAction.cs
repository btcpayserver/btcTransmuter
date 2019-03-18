using System.Collections.Generic;

namespace BtcTransmuter.Data.Entities
{
    public class RecipeAction : BaseEntity
    {
        public string RecipeId { get; set; }
        public string RecipeActionGroupId { get; set; }

        public string ExternalServiceId { get; set; }

        public string ActionId { get; set; }
        public Recipe Recipe { get; set; }
        public ExternalServiceData ExternalService { get; set; }
        public RecipeActionGroup RecipeActionGroup { get; set; }
        public List<RecipeInvocation> RecipeInvocations { get; set; }
        public int Order { get; set; } = 0;

    }
}