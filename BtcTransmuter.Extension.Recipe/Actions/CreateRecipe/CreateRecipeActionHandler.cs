using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Recipe.Actions.CreateRecipe
{
    public class CreateRecipeActionHandler : BaseActionHandler<CreateRecipeData, Data.Entities.Recipe>
    {
        public override string ActionId => "CreateRecipe";
        public override string Name => "Create Recipe";

        public override string Description =>
            "Place a new recipe within the system";

        public  override string ViewPartial => "ViewCreateRecipeAction";

        public override string ControllerName => "CreateRecipe";

        protected override async Task<TypedActionHandlerResult<Data.Entities.Recipe>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            CreateRecipeData actionData)
        {
            
            try
            {
                using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
                {
                    var recipeManager = scope.ServiceProvider.GetService<IRecipeManager>();
                    var cloneResult = await recipeManager.CloneRecipe(actionData.RecipeTemplateId, actionData.Enable);
                    if (cloneResult == null)
                    {
                        return new TypedActionHandlerResult<Data.Entities.Recipe>()
                        {
                            Executed = false,
                            Result =
                                $"Could not find recipe to use as template"
                        };
                    }

                    return new TypedActionHandlerResult<Data.Entities.Recipe>()
                    {
                        Executed = true,
                        TypedData = cloneResult,
                        Result =
                            $"Created recipe (id:{cloneResult.Id})",
                        
                    };
                }
            }
            catch (Exception e)
            {
                return new TypedActionHandlerResult<Data.Entities.Recipe>()
                {
                    Executed = false,
                    Result =
                        $"Could not create recipe because {e.Message}"
                };
            }
        }
    }
}