using System.Collections;
using System.Collections.Generic;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using ExtCore.Infrastructure;

namespace BtcTransmuter.Models
{
    public class ExtensionsViewModel
    {
        public IEnumerable<BtcTransmuterExtension> Extensions { get; set; }
    }
}