using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Data.Entities
{
    public class U2FDevice
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [Required] public byte[] KeyHandle { get; set; }

        [Required] public byte[] PublicKey { get; set; }

        [Required] public byte[] AttestationCert { get; set; }

        [Required] public int Counter { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}