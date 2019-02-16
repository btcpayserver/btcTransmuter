using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    [Route("email-plugin/pop3")]
    [Authorize]
    public class Pop3Controller : Controller
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;

        public Pop3Controller(IExternalServiceManager externalServiceManager, UserManager<User> userManager)
        {
            _externalServiceManager = externalServiceManager;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> EditData(ExternalServiceData data)
        {
            var identifier = Guid.NewGuid().ToString();

            TempData[identifier] = data;
            return View(new EditDataViewModel()
            {
                Identifier = identifier,
                ExternalServiceData = data
            });
        }


        [HttpPost("")]
        public async Task<IActionResult> EditData(EditDataViewModel data)
        {
            data.ExternalServiceData = TempData[data.Identifier] as ExternalServiceData;
            if (data.ExternalServiceData.UserId != _userManager.GetUserId(User))
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(data);
            }

            
        }


        public class EditDataViewModel : Pop3ExternalServiceData
        {
            public string Identifier { get; set; }
            public ExternalServiceData ExternalServiceData { get; set; }
        }
    }
}