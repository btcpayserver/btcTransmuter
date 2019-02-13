using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    [Authorize]
    public class ExternalServicesController : Controller
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IEnumerable<IExternalServiceDescriptor> _externalServiceDescriptors;

        public ExternalServicesController(IExternalServiceManager externalServiceManager, 
            UserManager<User> userManager, IEnumerable<IExternalServiceDescriptor> externalServiceDescriptors)
        {
            _externalServiceManager = externalServiceManager;
            _userManager = userManager;
            _externalServiceDescriptors = externalServiceDescriptors;
        }
        [HttpGet("")]
        public IActionResult GetServices()
        {
            var externalServices = _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User)
            });
            return View(new GetExternalServicesViewModel()
            {
                ExternalServices = externalServices,
                Descriptors = _externalServiceDescriptors
            });
        }
    }

   
}