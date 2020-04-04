using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using Microsoft.AspNetCore.Mvc;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(
            IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        [Route("swagger/v1/swagger.json")]
        public async Task<IActionResult> Swagger()
        {
            JObject json = new JObject();
            var directoryContents = _webHostEnvironment.WebRootFileProvider.GetDirectoryContents("swagger/v1");
            foreach (IFileInfo fi in directoryContents)
            {
                await using var stream = fi.CreateReadStream();
                using var reader = new StreamReader(fi.CreateReadStream());
                json.Merge(JObject.Parse(await reader.ReadToEndAsync()));
            }
            var servers = new JArray();
            servers.Add(new JObject(new JProperty("url", HttpContext.Request.GetAbsoluteRoot())));
            json["servers"] = servers;
            return Json(json);
        }

        [Route("docs")]
        public IActionResult SwaggerDocs()
        {
            return View();
        }
    }
}
