using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Services
{
    public class RecipeManager : IRecipeManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RecipeManager(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IEnumerable<Recipe>> GetRecipes(RecipesQuery query)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var queryable = context.Recipes
                        .Include(recipe => recipe.RecipeActions)
                        .ThenInclude(action => action.ExternalService)
                        .Include(recipe => recipe.RecipeTrigger)
                        .ThenInclude(trigger => trigger.ExternalService)
                        .Include(recipe => recipe.RecipeInvocations)
                        
                        .AsQueryable();

                    if (query.Enabled.HasValue)
                    {
                        queryable = queryable.Where(x => x.Enabled == query.Enabled.Value);
                    }

                    if (!string.IsNullOrEmpty(query.RecipeTriggerId))
                    {
                        queryable = queryable.Where(x => x.RecipeTrigger.TriggerId == query.RecipeTriggerId);
                    }

                    if (!string.IsNullOrEmpty(query.UserId))
                    {
                        queryable = queryable.Where(x => x.UserId == query.UserId);
                    }

                    if (!string.IsNullOrEmpty(query.RecipeId))
                    {
                        queryable = queryable.Where(x => x.Id == query.RecipeId);
                    }

                    return await queryable.ToListAsync();
                }
            }
        }

        public async Task AddOrUpdateRecipe(Recipe recipe)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    if (string.IsNullOrEmpty(recipe.Id))
                    {
                        await context.Recipes.AddAsync(recipe);
                    }
                    else
                    {
                        context.Recipes.Attach(recipe).State = EntityState.Modified;
                    }

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task AddOrUpdateRecipeTrigger(RecipeTrigger trigger)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    if (string.IsNullOrEmpty(trigger.Id))
                    {
                        await context.RecipeTriggers.AddAsync(trigger);
                    }
                    else
                    {
                        context.RecipeTriggers.Attach(trigger).State = EntityState.Modified;
                    }

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task AddOrUpdateRecipeAction(RecipeAction action)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    if (string.IsNullOrEmpty(action.Id))
                    {
                        await context.RecipeActions.AddAsync(action);
                    }
                    else
                    {
                        context.RecipeActions.Attach(action).State = EntityState.Modified;
                    }

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveRecipe(string id)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var recipe = await GetRecipe(id);
                    if (recipe == null)
                    {
                        throw new ArgumentException();
                    }

                    context.Attach(recipe);
                    context.AttachRange(recipe.RecipeActions);
                    context.AttachRange(recipe.RecipeTrigger);
                    context.AttachRange(recipe.RecipeInvocations);

                    context.Remove(recipe);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<Recipe> GetRecipe(string id, string userId = null)
        {
            var recipes = await GetRecipes(new RecipesQuery()
            {
                UserId = userId,
                RecipeId = id
            });

            return recipes.FirstOrDefault();
        }

        public async Task AddRecipeInvocation(RecipeInvocation invocation)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var recipe = await context.Recipes.FindAsync(invocation.RecipeId);
                    if (recipe != null)
                    {
                        await context.RecipeInvocations.AddAsync(invocation);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task RemoveRecipeAction(string recipeActionId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var recipe = await context.RecipeActions.FindAsync(recipeActionId);
                    if (recipe != null)
                    {
                        context.RecipeActions.Remove(recipe);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task RemoveRecipeTrigger(string recipeTriggerId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var recipe = await context.RecipeTriggers.FindAsync(recipeTriggerId);
                    if (recipe != null)
                    {
                        context.RecipeTriggers.Remove(recipe);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}