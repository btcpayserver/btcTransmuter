using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Models;
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
					var includableQueryable = context.Recipes
						.Include(recipe => recipe.RecipeActionGroups)
						.ThenInclude(group => group.RecipeActions)
						.ThenInclude(action => action.ExternalService)
						.Include(recipe => recipe.RecipeActions)
						.ThenInclude(action => action.ExternalService)
						.Include(recipe => recipe.RecipeActions)
						.Include(recipe => recipe.RecipeTrigger)
						.ThenInclude(trigger => trigger.ExternalService);

					if (query.IncludeRecipeInvocations)
					{
						includableQueryable.Include(recipe => recipe.RecipeInvocations);
					}

					var queryable = includableQueryable.AsQueryable();

					if (query.Enabled.HasValue)
					{
						queryable = queryable.Where(x => x.Enabled == query.Enabled.Value);
					}

					if (!string.IsNullOrEmpty(query.TriggerId))
					{
						queryable = queryable.Where(x => x.RecipeTrigger.TriggerId == query.TriggerId);
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

		public async Task<IEnumerable<RecipeInvocation>> GetRecipeInvocations(RecipeInvocationsQuery query)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var queryable = context.RecipeInvocations
				.Include(invocation => invocation.Recipe)
				.ThenInclude(recipe => recipe.RecipeTrigger)
				.AsEnumerable();
			if (query.OrderBy != null)
			{
				switch (query.OrderBy.Field)
				{
					case RecipeInvocationsQuery.RecipeInvocationsQueryOrderBy.Timestamp:
						queryable = query.OrderBy.Direction == OrderDirection.Ascending
							? queryable.OrderBy(invocation => invocation.Timestamp)
							: queryable.OrderByDescending(invocation => invocation.Timestamp);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return queryable
				.Where(invocation =>
					invocation.RecipeId.Equals(query.RecipeId, StringComparison.InvariantCultureIgnoreCase))
				.Skip(query.Skip).Take(query.Take).ToList();
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
						context.Entry(recipe).State = EntityState.Modified;
					}

					await context.SaveChangesAsync();
				}
			}
		}

		public async Task AddOrUpdateRecipeTrigger(RecipeTrigger trigger)
		{
			await AddOrUpdateRecipeTriggers(new[] {trigger});
		}

		public async Task AddOrUpdateRecipeTriggers(IEnumerable<RecipeTrigger> triggers)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
				{
					foreach (var trigger in triggers)
					{
						if (string.IsNullOrEmpty(trigger.Id))
						{
							await context.RecipeTriggers.AddAsync(trigger);
						}
						else
						{
							context.Entry(trigger).State = EntityState.Modified;
						}
					}

					await context.SaveChangesAsync();
				}
			}
		}

		public async Task AddOrUpdateRecipeAction(RecipeAction action)
		{
			var oldES = action.ExternalService;
			action.ExternalService = null;
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
						
						context.Entry(action).State = EntityState.Modified;
					}

					await context.SaveChangesAsync();
					action.ExternalService = oldES;
				}
			}
		}

		public async Task AddRecipeActionGroup(RecipeActionGroup recipeActionGroup)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
				{
					if (string.IsNullOrEmpty(recipeActionGroup.Id))
					{
						await context.RecipeActionGroups.AddAsync(recipeActionGroup);
					}
					else
					{
						context.Entry(recipeActionGroup).State = EntityState.Modified;
					}

					await context.SaveChangesAsync();
				}
			}
		}

		public async Task ReorderRecipeActionGroupActions(string recipeActionGroupId,
			Dictionary<string, int> actionsOrder)
		{
			if (!(actionsOrder?.Any() ?? false))
			{
				return;
			}
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
				{
					var actionGroup = await context.RecipeActionGroups.Include(group => group.RecipeActions)
						.SingleAsync(group =>
							group.Id == recipeActionGroupId);


					actionGroup.RecipeActions.ForEach(action =>
					{
						if (actionsOrder.ContainsKey(action.Id))
						{
							action.Order = actionsOrder[action.Id];
							foreach (var entityEntry in context.ChangeTracker.Entries()
								.Where(entry => entry.Entity is RecipeAction))
							{
								entityEntry.State = EntityState.Modified;
							}
						}
					});

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

					//TODO: double check if this was needed
//                    var invocations = (await GetRecipeInvocations(new RecipeInvocationsQuery()
//                    {
//                        Skip = 0,
//                        Take = int.MaxValue,
//                        RecipeId = id
//                    })).ToList();

					context.Attach(recipe);
					context.AttachRange(recipe.RecipeActions);
					if (recipe.RecipeTrigger != null)
						context.AttachRange(recipe.RecipeTrigger);
//                    context.AttachRange(invocations);
//                    context.RecipeInvocations.RemoveRange(invocations);
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

		public async Task RemoveRecipeActionGroup(string recipeActionGroupId)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
				{
					var recipe = await context.RecipeActionGroups.FindAsync(recipeActionGroupId);
					if (recipe != null)
					{
						context.RecipeActionGroups.Remove(recipe);
						await context.SaveChangesAsync();
					}
				}
			}
		}

		public async Task<string> GetRecipeName(string recipeId)
		{
			var result = await GetRecipe(recipeId);
			return result?.Name ?? string.Empty;
		}

		public async Task<Recipe> CloneRecipe(string recipeId, bool enable, string newName = null)
		{
			var recipe = await GetRecipe(recipeId);
			if (recipe == null)
			{
				return null;
			}

			recipe.Id = null;
			if (recipe.RecipeTrigger != null)
			{
				recipe.RecipeTrigger.RecipeId = null;
				recipe.RecipeTrigger.Id = null;
				recipe.RecipeTrigger.ExternalService = null;
			}

			recipe.RecipeInvocations?.Clear();
			recipe.RecipeActions = recipe.RecipeActions
				.Where(action => string.IsNullOrEmpty(action.RecipeActionGroupId)).ToList();
			recipe.RecipeActions.ForEach(action =>
			{
				action.RecipeId = null;
				action.Id = null;
				action.ExternalService = null;
			});
			recipe.RecipeActionGroups.ForEach(action =>
			{
				action.RecipeId = null;
				action.Id = null;
				action.RecipeActions.ForEach(action1 =>
				{
					action1.RecipeId = null;
					action1.Id = null;
					action1.ExternalService = null;
				});
			});
			recipe.Enabled = enable;
			recipe.Name = newName ?? $"Clone of {recipe.Name}";
			await AddOrUpdateRecipe(recipe);
			return recipe;
	}
		}
}