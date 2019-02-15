using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public abstract class BaseEmailServiceData
    {
        [Required]
        public string Server { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public bool SSL { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}