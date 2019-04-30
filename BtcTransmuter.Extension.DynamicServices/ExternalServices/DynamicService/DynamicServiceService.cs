using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
namespace BtcTransmuter.Extension.DynamicService.ExternalServices.DynamicService
{
    public class DynamicServiceService : BaseExternalService<DynamicServiceExternalServiceData>
    {
        private readonly ExternalServiceData _data;
        private readonly IRecipeManager _recipeManager;
        private readonly IActionDispatcher _actionDispatcher;
        private readonly IExternalServiceManager _externalServiceManager;
        public const string DynamicServiceServiceType = "DynamicServiceExternalService";
        public override string ExternalServiceType => DynamicServiceServiceType;

        public override string Name => "DynamicService External Service";

        public override string Description =>
            "DynamicService External Service to be able to resolve other external services using actions";

        public override string ViewPartial => "ViewDynamicServiceExternalService";
        public override string ControllerName => "DynamicService";


        public DynamicServiceService() : base()
        {
        }

        public DynamicServiceService(ExternalServiceData data, IRecipeManager recipeManager,
            IActionDispatcher actionDispatcher, IExternalServiceManager externalServiceManager) : base(data)
        {
            _data = data;
            _recipeManager = recipeManager;
            _actionDispatcher = actionDispatcher;
            _externalServiceManager = externalServiceManager;
        }

        public async Task<ExternalServiceData> ExecuteWitchcraftToComputeExternalService()
        {
            var data = GetData();
            var recipe = await _recipeManager.GetRecipe(data.RecipeId);
            if (recipe == null)
            {
                return null;
            }

            var actionData = new Dictionary<string, (object data, string json)>();
            if (!string.IsNullOrEmpty(data.RecipeActionId))
            {
                var recipeAction = recipe.RecipeActions.SingleOrDefault(action =>
                    action.Id.Equals(data.RecipeActionId, StringComparison.InvariantCultureIgnoreCase));
                if (recipeAction == null)
                {
                    return null;
                }

                var dispatchResult = await _actionDispatcher.Dispatch(actionData,
                    recipeAction);
                actionData.Add("PreviousAction", (dispatchResult.First().Data, dispatchResult.First().DataJson));
                actionData.Add("ActionData0", (dispatchResult.First().Data, dispatchResult.First().DataJson));
                
            }

            if (!string.IsNullOrEmpty(data.RecipeActionGroupId))
            {
                var recipeActionGroup = recipe.RecipeActionGroups.SingleOrDefault(action =>
                    action.Id.Equals(data.RecipeActionGroupId, StringComparison.InvariantCultureIgnoreCase));
                if (recipeActionGroup == null)
                {
                    return null;
                }

                await _actionDispatcher.Dispatch(actionData,
                    recipeActionGroup);
            }

            var value = InterpolationHelper.InterpolateString(data.Value,
                actionData.ToDictionary(pair => pair.Key, pair => pair.Value.data));

            return await _externalServiceManager.GetExternalServiceData(value, _data.UserId);
        }
        
        
    }
}