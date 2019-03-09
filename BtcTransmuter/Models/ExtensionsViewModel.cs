using System.Collections;
using System.Collections.Generic;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.Models
{
    public class ExtensionsViewModel
    {
        public IEnumerable<BtcTransmuterExtension> Extensions { get; set; }
        public string StatusMessage { get; set; }
        public bool IsAdmin { get; set; }
    }
}