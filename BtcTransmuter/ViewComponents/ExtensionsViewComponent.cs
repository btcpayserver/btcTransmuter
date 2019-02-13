using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Areas.ViewComponents
{
    public class ExtensionsViewComponent : ViewComponent
    {
        private readonly IEnumerable<BtcTransmuterExtension> _extensions;

        public ExtensionsViewComponent(IEnumerable<BtcTransmuterExtension> extensions)
        {
            _extensions = extensions;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(new ExtensionsViewComponentViewModel(_extensions));
        }
    }

    public class ExtensionsViewComponentViewModel
    {
        public IEnumerable<BtcTransmuterExtension> Extensions { get; }

        public ExtensionsViewComponentViewModel(IEnumerable<BtcTransmuterExtension> extensions)
        {
            Extensions = extensions;
        }
    }
}