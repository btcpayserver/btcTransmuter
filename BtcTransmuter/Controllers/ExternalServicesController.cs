using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BTCPayServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;

namespace BtcTransmuter.Controllers
{
    [Authorize]
    [Route("external-services")]
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
        public async Task<IActionResult> GetServices()
        {
            var externalServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User)
            });
            return View(new GetExternalServicesViewModel()
            {
                ExternalServices = externalServices,
                Descriptors = _externalServiceDescriptors
            });
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreateExternalService(string statusMessage)
        {
            return View(new CreateExternalServiceViewModel()
            {
                StatusMessage = statusMessage,
                Types = new SelectList(_externalServiceDescriptors,
                    nameof(IExternalServiceDescriptor.ExternalServiceType), nameof(IExternalServiceDescriptor.Name))
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateExternalService(CreateExternalServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Types = new SelectList(_externalServiceDescriptors,
                    nameof(IExternalServiceDescriptor.ExternalServiceType), nameof(IExternalServiceDescriptor.Name),
                    model.Type);
                return View(model);
            }

            var externalService = new ExternalServiceData()
            {
                Name = model.Name,
                Type = model.Type,
                UserId = _userManager.GetUserId(User)
            };


            return RedirectToAction("EditExternalServiceInnerData", new
            {
                externalServiceData = externalService
            });
//            await _externalServiceManager.AddOrUpdateExternalServiceData(externalService);
//
//            return RedirectToAction("EditExternalService",
//                new {externalService.Id, statusMessage = "External Service created"});
        }


        [HttpGet("data")]
        public async Task<IActionResult> EditExternalServiceInnerData(ExternalServiceData externalServiceData)
        {
            return View(new EditExternalServiceInnerDataVieModel()
            {
                Data = externalServiceData
            });
        }

        public class EditExternalServiceInnerDataVieModel
        {
            public ExternalServiceData Data { get; set; }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> EditExternalService(string id, string statusMessage)
        {
            var externalServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User),
                ExternalServiceId = id
            });
            if (!EnumerableExtensions.Any(externalServices))
            {
                return RedirectToAction("GetServices", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "External Service  not found",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                });
            }

            var externalService = externalServices.First();
            return View(new EditExternalServiceViewModel()
            {
                Name = externalService.Name,
                Type = externalService.Type,
                ExternalServiceData = externalService,
                StatusMessage = statusMessage
            });
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditExternalService(string id, EditExternalServiceViewModel model)
        {
            var externalServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User),
                ExternalServiceId = id
            });
            if (!EnumerableExtensions.Any(externalServices))
            {
                return RedirectToAction("GetServices", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "External Service  not found",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                });
            }


            var externalService = externalServices.First();


            if (!ModelState.IsValid)
            {
                model.ExternalServiceData = externalService;
                return View(model);
            }

            externalService.Name = model.Name;

            await _externalServiceManager.AddOrUpdateExternalServiceData(externalService);


            return RedirectToAction("EditExternalService", new {id, statusMessage = "External Service updated"});
        }

        [HttpGet("{id}/remove")]
        public async Task<IActionResult> RemoveExternalService(string id)
        {
            var externalServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User),
                ExternalServiceId = id
            });
            if (!EnumerableExtensions.Any(externalServices))
            {
                return RedirectToAction("GetServices", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "External Service  not found",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                });
            }

            var externalService = Enumerable.First(externalServices);
            return View(new RemoveExternalServiceViewModel()
            {
                ExternalService = externalService
            });
        }

        [HttpPost("{id}/remove")]
        public async Task<IActionResult> RemoveExternalServicePost(string id)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User),
                ExternalServiceId = id
            });
            if (!EnumerableExtensions.Any(services))
            {
                return RedirectToAction("GetServices", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "External Service not found",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                });
            }

            await _externalServiceManager.RemoveExternalServiceData(id);
            return RedirectToAction("GetServices", new
            {
                statusMessage = new StatusMessageModel()
                {
                    Message = $"Recipe {services.First().Name} deleted successfully",
                    Severity = StatusMessageModel.StatusSeverity.Success
                }.ToString()
            });
        }
    }
}