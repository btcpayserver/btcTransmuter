using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Abstractions.Extensions
{
    public static class ControllerExtensions
    {

        public static bool IsApi(this Controller controller)
        {
            return controller.HttpContext.IsApi();
        }
        
        public static bool IsApi(this HttpContext context)
        {
            if (context.Items.TryGetValue("API", out var val))
            {
                return val is true;
            }

            return false;
        }
        
        public static void SetIsApi(this HttpContext context, bool val)
        {
            context.Items.TryAdd("API", val);
        }
        public static IActionResult ViewOrJson<T>(this Controller controller, string viewName, T payload)
        {
            if (controller.IsApi())
            {
                return controller.Json(payload);
            }

            return controller.View(viewName, payload);
        }
        public static IActionResult ViewOrJson<T>(this Controller controller, T payload)
        {
            if (controller.IsApi())
            {
                return controller.Json(payload);
            }

            return controller.View(payload);
        }
        public static IActionResult ViewOrBadRequest<T>(this Controller controller, T payload, bool usePayloadInBadRequest = false)
        {
            if (controller.IsApi())
            {
                return usePayloadInBadRequest ? controller.BadRequest(payload) : controller.BadRequest(controller.ModelState);
            }

            return controller.View(payload);
        }
    }
}