using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Areas.ViewComponents
{
    public class ViewExternalServiceViewComponent : ViewComponent
    {
        private readonly IEnumerable<IExternalServiceDescriptor> _externalServiceDescriptors;

        public ViewExternalServiceViewComponent(
            IEnumerable<IExternalServiceDescriptor> externalServiceDescriptors)
        {
            _externalServiceDescriptors = externalServiceDescriptors;
        }

        public async Task<IViewComponentResult> InvokeAsync(ExternalServiceData serviceData)
        {
            return View(new ViewExternalServiceViewModel()
            {
                ExternalServiceData = serviceData,
                ExternalServiceDescriptor = _externalServiceDescriptors
                    .Single(descriptor => descriptor.ExternalServiceType == serviceData.Type)
            });
        }
    }
}