using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Settings;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Abstractions.U2F;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Services
{
    public static class TransmuterExtensions
    {
        public static void AddTransmuterServices(this IServiceCollection collection)
        {
            collection.AddSingleton<IExternalServiceManager, ExternalServiceManager>();
            collection.AddSingleton<IRecipeManager, RecipeManager>();
            collection.AddSingleton<IActionDispatcher, ActionDispatcher>();
            collection.AddSingleton<ITriggerDispatcher, TriggerDispatcher>();
            collection.AddSingleton<ISettingsManager, SettingsManager>();
            collection.AddSingleton<IU2FService, U2FService>();
        }
    }
}
