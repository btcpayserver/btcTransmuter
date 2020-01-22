using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BtcTransmuter.Abstractions.Extensions
{
    public static class ModelStateExtensions
    {
        public static void AddModelError<TModel>(this TModel source,        
                                                            string name,
                                                            string message,
                                                            ModelStateDictionary modelState)
            {
                modelState.AddModelError(name, message);
            }
    }
}