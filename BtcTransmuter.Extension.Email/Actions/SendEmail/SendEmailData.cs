using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    public class SendEmailData
    {
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public bool IsHTML { get; set; } = false;
    }
}