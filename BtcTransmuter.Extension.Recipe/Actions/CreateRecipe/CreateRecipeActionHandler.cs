using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Recipe.Actions.CreateRecipe
{
    public class CreateRecipeActionHandler : BaseActionHandler<CreateRecipeData>
    {
        public override string ActionId => "CreateRecipe";
        public override string Name => "Create Recipe";

        public override string Description =>
            "Place a new recipe within the system";

        public  override string ViewPartial => "ViewCreateRecipeAction";

        public override string ControllerName => "CreateRecipe";

        protected override Task<bool> CanExecute(object triggerData, RecipeAction recipeAction)
        {
            return Task.FromResult(recipeAction.ActionId == ActionId);
        }

        protected override async Task<ActionHandlerResult> Execute(object triggerData, RecipeAction recipeAction,
            CreateRecipeData actionData)
        {
            
            try
            {
                using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
                {
                    var recipeManager = scope.ServiceProvider.GetService<IRecipeManager>();
                    var recipe = await recipeManager.GetRecipe(actionData.RecipeTemplateId);
                    if (recipe == null)
                    {
                        return new ActionHandlerResult()
                        {
                            Executed = false,
                            Result =
                                $"Could not find recipe to use as template"
                        };
                    }

                    recipe.Id = null;
                    if (recipe.RecipeTrigger != null)
                    {
                        recipe.RecipeTrigger.RecipeId = null;
                        recipe.RecipeTrigger.Id= null;
                    }

                    recipe.RecipeInvocations.Clear();
                    recipe.RecipeActions.ForEach(action =>
                    {
                        action.RecipeId = null;
                        action.Id = null;
                    });
                    recipe.RecipeActionGroups.ForEach(action =>
                    {
                        action.RecipeId = null;
                        action.Id = null;
                    });
                    recipe.Enabled = actionData.Enable;
                    await recipeManager.AddOrUpdateRecipe(recipe);
                    return new ActionHandlerResult()
                    {
                        Executed = true,
                        Result =
                            $"Created recipe (id:{recipe.Id})"
                    };
                }
            }
            catch (Exception e)
            {
                return new ActionHandlerResult()
                {
                    Executed = false,
                    Result =
                        $"Could not create recipe because {e.Message}"
                };
            }
        }
    }
}