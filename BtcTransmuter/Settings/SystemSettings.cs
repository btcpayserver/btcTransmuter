using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Controllers
{
    public class SystemSettings
    {
        [Display(Name = "Disable Registration")]
        public bool DisableRegistration { get; set; } = false;
    }
}