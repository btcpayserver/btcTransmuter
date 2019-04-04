using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BtcTransmuter.Extension.DynamicService.ExternalServices.DynamicService
{        
    public class EditDynamicServiceDataViewModel : DynamicServiceExternalServiceData
    {
        public string Action { get; set; }
        public SelectList Recipes { get; set; }
        public SelectList RecipeActions { get; set; }
        public SelectList RecipeActionGroups { get; set; }
    }
}