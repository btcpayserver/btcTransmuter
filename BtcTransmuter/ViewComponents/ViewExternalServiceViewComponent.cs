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

        public Task<IViewComponentResult> InvokeAsync(ExternalServiceData serviceData, bool showAllData)
        {
            return Task.FromResult<IViewComponentResult>(View(new ViewExternalServiceViewModel()
            {
                ShowAllData = showAllData,
                ExternalServiceData = serviceData,
                ExternalServiceDescriptor = _externalServiceDescriptors
                    .Single(descriptor => descriptor.ExternalServiceType == serviceData.Type)
            }));
        }
    }
}