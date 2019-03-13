using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.ViewComponents
{
    public class ExtensionHeadersViewComponent : ViewComponent
    {
        private readonly IEnumerable<BtcTransmuterExtension> _extensions;

        public ExtensionHeadersViewComponent(IEnumerable<BtcTransmuterExtension> extensions)
        {
            _extensions = extensions;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            return Task.FromResult<IViewComponentResult>(View(new ExtensionHeadersViewComponentViewModel(_extensions)));
        }
    }
}