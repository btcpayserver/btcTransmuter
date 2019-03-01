using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IApplicationLifetime _applicationLifetime;

        public ExtensionsController(IEnumerable<BtcTransmuterExtension> btcTransmuterExtensions,
            UserManager<User> userManager,
            IHostingEnvironment hostingEnvironment, IApplicationLifetime applicationLifetime)
        {
            _btcTransmuterExtensions = btcTransmuterExtensions;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _applicationLifetime = applicationLifetime;
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
            var dest = Path.Combine(_hostingEnvironment.ContentRootPath, "Extensions");


            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filedest = Path.Combine(dest, formFile.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filedest));
                    using (var stream = new FileStream(filedest, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    if (Path.GetExtension(filedest) == ".zip")
                    {
                        ZipFile.ExtractToDirectory(filedest,
                            filedest.TrimEnd(".zip", StringComparison.InvariantCultureIgnoreCase), true);
                        System.IO.File.Delete(filedest);
                    }
                }
            }

//            _applicationLifetime.StopApplication();
            return RedirectToAction("Extensions", new
            {
                StatusMessage = "Files uploaded, restart server to load plugins"
            });
        }
    }
}