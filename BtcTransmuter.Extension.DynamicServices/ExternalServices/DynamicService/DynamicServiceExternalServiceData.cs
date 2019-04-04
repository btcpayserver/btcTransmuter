using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.DynamicService.ExternalServices.DynamicService
{
    public class DynamicServiceExternalServiceData
    {
        public string RecipeId { get; set; }
        public string RecipeActionId { get; set; }
        public string RecipeActionGroupId { get; set; }
        public string Value { get; set; }
    }
}