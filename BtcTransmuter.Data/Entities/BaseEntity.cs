using System.ComponentModel.DataAnnotations.Schema;
using BtcTransmuter.Data.Encryption;
using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Data.Entities
{
    public abstract class BaseEntity :BaseIdEntity,  IHasJsonData
    {
        [Encrypted] public string DataJson { get; set; }
    }
}