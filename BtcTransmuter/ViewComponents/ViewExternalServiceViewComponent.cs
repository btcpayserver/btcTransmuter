using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.ViewComponents
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
                    .SingleOrDefault(descriptor => descriptor.ExternalServiceType == serviceData.Type)
            }));
        }
    }
}