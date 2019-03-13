using System.Collections.Generic;
using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.Models
{
    public class ExtensionMenusViewComponentViewModel
    {
        public IEnumerable<BtcTransmuterExtension> Extensions { get; }

        public ExtensionMenusViewComponentViewModel(IEnumerable<BtcTransmuterExtension> extensions)
        {
            Extensions = extensions;
        }
    }
}