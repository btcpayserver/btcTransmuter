using System;
using System.Collections.Generic;
using BtcTransmuter.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace BtcTransmuter.Data
{
    public class User : IdentityUser
    {
        public List<Recipe> Recipes { get; set; }
    }

    public class Recipe
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Description { get; set; }

        public RecipeTrigger RecipeTrigger { get; set; }
        public List<RecipeAction> RecipeActions { get; set; } // executes all actions
        public List<RecipeInvocation> RecipeInvocations { get; set; } // log
    }

    public class RecipeTrigger : BaseEntity
    {
        public string RecipeId { get; set; }

        public string ExternalServiceId { get; set; }


        public Recipe Recipe { get; set; }
        public ExternalServiceData ExternalService { get; set; }
    }

    public class RecipeAction : BaseEntity
    {
        public string RecipeId { get; set; }
        public string ExternalServiceId { get; set; }

        public Recipe Recipe { get; set; }
        public ExternalServiceData ExternalService { get; set; }
    }

    public class RecipeInvocation
    {
        public string Id { get; set; }    
        public int RecipeId { get; set; }
        public int RecipeActionId { get; set; }
        public int RecipeTriggerId { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public Recipe Recipe { get; set; }
        public RecipeAction RecipeAction { get; set; }    
        public RecipeTrigger RecipeTrigger { get; set; }
        
        public string ActionResult { get; set; }
    }
}