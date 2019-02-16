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
                    var queryable = context.Recipes.Include(recipe => recipe.RecipeActions)
                        .Include(recipe => recipe.RecipeTrigger)
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

        public async Task RemoveRecipe(string id)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var recipe = await context.Recipes.FindAsync(id);
                    if (recipe == null)
                    {
                        throw new ArgumentException();
                    }

                    context.Remove(recipe);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<Recipe> GetRecipe(string id)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    return await context.Recipes.FindAsync(id);
                }
            }
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
    }
}