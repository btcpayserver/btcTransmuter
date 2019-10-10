using System.ComponentModel.DataAnnotations.Schema;

namespace BtcTransmuter.Data.Entities
{
    public abstract class BaseIdEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
    }
}