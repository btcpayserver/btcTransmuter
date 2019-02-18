using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.ViewComponents
{
    public class ExtensionsViewComponent : ViewComponent
    {
        private readonly IEnumerable<BtcTransmuterExtension> _extensions;

        public ExtensionsViewComponent(IEnumerable<BtcTransmuterExtension> extensions)
        {
            _extensions = extensions;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            return Task.FromResult<IViewComponentResult>(View(new ExtensionsViewComponentViewModel(_extensions)));
        }
    }
}