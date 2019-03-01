using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    [Authorize]
    
    [Route("extensions")]
    public class ExtensionsController : Controller
    {
        private readonly IEnumerable<BtcTransmuterExtension> _btcTransmuterExtensions;
        private readonly UserManager<User> _userManager;

        public ExtensionsController(IEnumerable<BtcTransmuterExtension> btcTransmuterExtensions, UserManager<User> userManager)
        {
            _btcTransmuterExtensions = btcTransmuterExtensions;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Extensions(string statusMessage)
        {
            var user = await _userManager.GetUserAsync(User);
            
            return View(new ExtensionsViewModel()
            {
                StatusMessage = statusMessage,
                Extensions = _btcTransmuterExtensions,
                IsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
            });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadExtension(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return RedirectToAction("Extensions", new
            {
                StatusMessage = "Files uploaded, restart server to load plugins"
            });
        }
    }
}