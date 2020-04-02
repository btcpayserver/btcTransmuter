using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public async Task<IActionResult> GetServices(string statusMessage)
        {
            var externalServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User)
            });
            return View(new GetExternalServicesViewModel()
            {
                ExternalServices = externalServices,
                StatusMessage = statusMessage,
                Descriptors = _externalServiceDescriptors
            });
        }

        [HttpGet("create")]
        public IActionResult CreateExternalService(string statusMessage, string selectedType = null)
        {
            return View(new CreateExternalServiceViewModel()
            {
                StatusMessage = statusMessage,
                Type = selectedType,
                Types = new SelectList(_externalServiceDescriptors,
                    nameof(IExternalServiceDescriptor.ExternalServiceType), nameof(IExternalServiceDescriptor.Name), selectedType)
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

            var serviceDescriptor =
                _externalServiceDescriptors.Single(descriptor =>
                    descriptor.ExternalServiceType == externalService.Type);

            return await serviceDescriptor.EditData(externalService);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> EditExternalService(string id, string statusMessage)
        {
            var externalService =
                await _externalServiceManager.GetExternalServiceData(id, _userManager.GetUserId(User));
            if (externalService == null)
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
            
            return View(new EditExternalServiceViewModel()
            {
                Name = externalService.Name,
                Type = externalService.Type,
                ExternalServiceData = externalService,
                StatusMessage = statusMessage
            });
        }


        [HttpGet("{id}/data")]
        public async Task<IActionResult> EditExternalServiceInnerData(string id)
        {
            var externalService =
                await _externalServiceManager.GetExternalServiceData(id, _userManager.GetUserId(User));
            if (externalService == null)
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

            var serviceDescriptor =
                _externalServiceDescriptors.Single(descriptor =>
                    descriptor.ExternalServiceType == externalService.Type);
            return await serviceDescriptor.EditData(externalService);
        }


        [HttpPost("{id}")]
        public async Task<IActionResult> EditExternalService(string id, EditExternalServiceViewModel model)
        {
            var externalService =
                await _externalServiceManager.GetExternalServiceData(id, _userManager.GetUserId(User));
            if (externalService == null)
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
            var data = await GetDataToDelete(id);
            if (data.Error != null)
            {
                return data.Error;
            }

            return View(new RemoveExternalServiceViewModel()
            {
                ExternalService = data.Data
            });
        }

        [HttpPost("{id}/remove")]
        public async Task<IActionResult> RemoveExternalServicePost(string id)
        {
            var data = await GetDataToDelete(id);
            if (data.Error != null)
            {
                return data.Error;
            }

            await _externalServiceManager.RemoveExternalServiceData(id);
            return RedirectToAction("GetServices", new
            {
                statusMessage = new StatusMessageModel()
                {
                    Message = $"Recipe {data.Data.Name} deleted successfully",
                    Severity = StatusMessageModel.StatusSeverity.Success
                }.ToString()
            });
        }

        private async Task<(ExternalServiceData Data, IActionResult Error)> GetDataToDelete(string id)
        {
            var externalService =
                await _externalServiceManager.GetExternalServiceData(id, _userManager.GetUserId(User));
            if (externalService == null)
            {
                return (null, RedirectToAction("GetServices", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "External Service not found",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                }));
            }

            if (externalService.RecipeActions.Any() || externalService.RecipeTriggers.Any())
            {
                return (null, RedirectToAction("GetServices", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "Cannot remove external service, it is being used by recipe actions/triggers",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                }));
            }

            return (externalService, null);
        }
    }
}