using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Controllers
{
    public class SystemSettings
    {
        [Display(Name = "Disable Registration")]
        public bool DisableRegistration { get; set; } = false;
        [Display(Name = "Discourage search engines from indexing this site")]
        public bool DiscourageSearchEngines { get; set; } = true;
    }
}