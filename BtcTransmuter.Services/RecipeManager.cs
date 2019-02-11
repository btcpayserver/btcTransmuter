using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data;
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
                        queryable = queryable.Where(x => x.UserId== query.UserId);
                    }

                    return await queryable.ToListAsync();
                }
            }
        }
    }
}