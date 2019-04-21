using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Helpers;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace BtcTransmuter.ViewComponents
{
    public class RecipeActionFooterViewComponent : ViewComponent
    {
        private readonly IEnumerable<ITriggerDescriptor> _triggerDescriptors;
        private readonly IEnumerable<IActionDescriptor> _actionDescriptors;
        private readonly IRecipeManager _recipeManager;

        public RecipeActionFooterViewComponent(IEnumerable<ITriggerDescriptor> triggerDescriptors,
            IEnumerable<IActionDescriptor>actionDescriptors,
            IRecipeManager recipeManager)
        {
            _triggerDescriptors = triggerDescriptors;
            _actionDescriptors = actionDescriptors;
            _recipeManager = recipeManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string recipeId, string recipeActionIdInGroupBeforeThisOne)
        {
            var recipe = await _recipeManager.GetRecipe(recipeId);

            var properties = new Dictionary<string, object>();

            if (recipe?.RecipeTrigger != null)
            {
                var descriptor = _triggerDescriptors.FirstOrDefault(triggerDescriptor =>
                    triggerDescriptor.TriggerId == recipe?.RecipeTrigger.TriggerId);

                if (descriptor != null && descriptor.GetType().IsSubclassOfRawGeneric(typeof(BaseTriggerHandler<,>)))
                {
                    var type = descriptor.GetType().BaseType.GetGenericArguments().First();
                    properties.Add("TriggerData", GetRecursiveAvailableProperties(type));
                }
            }

            if (!string.IsNullOrEmpty(recipeActionIdInGroupBeforeThisOne))
            {
                 var previousAction = recipe.RecipeActions.FirstOrDefault(action =>
                    action.Id.Equals(recipeActionIdInGroupBeforeThisOne, StringComparison.InvariantCultureIgnoreCase));

                 if (previousAction != null)
                 {
                     var type = _actionDescriptors.FirstOrDefault(handler =>
                         handler.ActionId == previousAction.ActionId)?.ActionResultDataType;

                     properties.Add("PreviousAction", GetRecursiveAvailableProperties(type));


                     var recipeActionGroupId = previousAction.RecipeActionGroupId;
                     var recipeActionGroup = recipe.RecipeActionGroups.SingleOrDefault(group =>
                         group.Id.Equals(recipeActionGroupId, StringComparison.InvariantCultureIgnoreCase));
                     if (recipeActionGroup != null)
                     {
                         var actions = recipeActionGroup.RecipeActions.OrderBy(action => action.Order);
                         for (var i = 0; i < actions.Count(); i++)
                         {
                             var currentAction = actions.ElementAt(i);
                          
                             if (currentAction.Id.Equals(recipeActionIdInGroupBeforeThisOne,
                                 StringComparison.InvariantCultureIgnoreCase))
                             {
                                 break;
                             }
                             
                             type = _actionDescriptors.FirstOrDefault(handler =>
                                 handler.ActionId == currentAction.ActionId)?.ActionResultDataType;
                             
                             properties.Add($"ActionData{i}", GetRecursiveAvailableProperties(type));

                            
                         }
                     }
                 }
            }

            return View(new RecipeActionFooterViewModel()
            {
                Properties = properties,
                NoRecipeTrigger = recipe?.RecipeTrigger == null
            });
        }
        private static Dictionary<Type, Dictionary<string, object> > cached = new Dictionary<Type, Dictionary<string, object>>();

        private Dictionary<string, object> GetRecursiveAvailableProperties(Type type, int currentDepth = 0)
        {
            if (cached.ContainsKey(type))
            {
                return cached[type];
            }
            var properties = new Dictionary<string, dynamic>();
            if (currentDepth >= 5)
            {
                properties.Add(type.Name, "Too deep, guess the rest bro");
                return properties;
            }
            
            var tProps = type.GetProperties().GroupBy(info => info.Name + info.PropertyType).Select(infos => infos.First());
            foreach (var prop in tProps)
            {
                if (properties.ContainsKey(prop.Name))
                {
                    var currentValue = properties[prop.Name];
                    if (currentValue is Dictionary<string, object> currentValueDict)
                    {
                        properties[prop.Name] = currentValueDict
                            .Concat(GetRecursiveAvailableProperties(prop.PropertyType, currentDepth + 1))
                            .ToDictionary(pair => pair.Key, pair => pair.Value);
                    }
                    continue;
                }
                
                properties.Add(prop.Name,
                    prop.PropertyType.IsPrimitive
                        ? (dynamic) prop.PropertyType.ToString()
                        : GetRecursiveAvailableProperties(prop.PropertyType, currentDepth+1));
            }
            
            if (type.IsArray || (type.IsGenericType && new Type[]{typeof(Dictionary<,>), typeof(List<>)}.Contains(type.GetGenericTypeDefinition() )))
            {

                var genericArguments = type.GetInterfaces()
                    .FirstOrDefault(type1 =>
                    {
                        if (type1.IsGenericType)
                        {
                            var definition = type1.GetGenericTypeDefinition();
                            if (definition == typeof(IDictionary<,>) || definition == typeof(IEnumerable<>))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                         )
                    ?.GetGenericArguments();

                if (genericArguments != null)
                {
                    var indexName = "[index]";
                    if (genericArguments.Length > 1)
                    {
                        var keyType = genericArguments.First();
                        if (keyType == typeof(string))
                        {
                            indexName = "[\"index\"]";
                        }
                        else
                        {
                            indexName = $"[index-{keyType.Name}]";
                        }
                    }
                    properties.Add(indexName, GetRecursiveAvailableProperties(genericArguments.Last(), currentDepth));
                }
            }
            cached.AddOrReplace(type, properties);
            return properties;
        }
    }
}