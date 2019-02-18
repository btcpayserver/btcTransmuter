using System.Collections.Generic;
using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.ViewComponents
{
    public class ExtensionsViewComponentViewModel
    {
        public IEnumerable<BtcTransmuterExtension> Extensions { get; }

        public ExtensionsViewComponentViewModel(IEnumerable<BtcTransmuterExtension> extensions)
        {
            Extensions = extensions;
        }
    }
}