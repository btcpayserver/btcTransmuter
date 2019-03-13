using System.Collections.Generic;
using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.Models
{
    public class ExtensionHeadersViewComponentViewModel
    {
        public IEnumerable<BtcTransmuterExtension> Extensions { get; }

        public ExtensionHeadersViewComponentViewModel(IEnumerable<BtcTransmuterExtension> extensions)
        {
            Extensions = extensions;
        }
    }
}