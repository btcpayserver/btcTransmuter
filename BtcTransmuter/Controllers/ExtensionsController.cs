using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using BtcTransmuter.SystemExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    [Route("extensions")]
    public class ExtensionsController : Controller
    {
        private readonly IEnumerable<BtcTransmuterExtension> _btcTransmuterExtensions;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ExtensionsController(IEnumerable<BtcTransmuterExtension> btcTransmuterExtensions,
            UserManager<User> userManager,
            IWebHostEnvironment hostingEnvironment)
        {
            _btcTransmuterExtensions = btcTransmuterExtensions;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("")]
        public async Task<IActionResult> Extensions(string statusMessage)
        {
            var user = await _userManager.GetUserAsync(User);

            return View(new ExtensionsViewModel()
            {
                StatusMessage = statusMessage,
                Extensions = _btcTransmuterExtensions,
                IsAdmin = user != null &&  await _userManager.IsInRoleAsync(user, "Admin")
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadExtension(List<IFormFile> files)
        {
            var dest = Path.Combine(_hostingEnvironment.ContentRootPath, "Extensions");
            
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filedest = Path.Combine(dest, formFile.FileName);
                    if (Path.GetExtension(filedest) == ".zip")
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filedest));
                        using (var stream = new FileStream(filedest, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }


                        ZipFile.ExtractToDirectory(filedest,
                            filedest.TrimEnd(".zip", StringComparison.InvariantCultureIgnoreCase), true);
                        System.IO.File.Delete(filedest);
                    }
                }
            }

            return RedirectToAction("Extensions", new
            {
                StatusMessage = "Files uploaded, restart server to load plugins"
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("available")]
        public IActionResult BrowseAvailableExtensions()
        {
            var result = new List<RemoteAvailableExtension>();

            var newExtensions = result.Where(extension =>
                !_btcTransmuterExtensions.Any(transmuterExtension =>
                    transmuterExtension.Name == extension.Name && transmuterExtension.Version == extension.Version));

            return View(newExtensions);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("available/{url}")]
        public IActionResult DownloadRemoteExtension(string url)
        {
            using (var client = new System.Net.WebClient())
            {
                var dest = Path.Combine(_hostingEnvironment.ContentRootPath, "Extensions");
                var filename = url.Split("/").Last();
                var filedest = Path.Combine(dest, filename);
                Directory.CreateDirectory(Path.GetDirectoryName(filedest));

                client.DownloadFileAsync(new Uri(url),
                    filedest);

                ZipFile.ExtractToDirectory(filedest,
                    filedest.TrimEnd(".zip", StringComparison.InvariantCultureIgnoreCase), true);
                System.IO.File.Delete(filedest);

                return RedirectToAction("Extensions", new
                {
                    StatusMessage = "Files uploaded, restart server to load plugins"
                });
            }
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("ssh/{command}")]
        public IActionResult SshCommand(string command)
        {
            return Ok(command.Bash());
        }
    }
}