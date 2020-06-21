using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Extension.Presets
{
    [Route("presets-plugin/[controller]")]
    [Authorize]
    public class PresetsController: Controller
    {
        private readonly IEnumerable<ITransmuterPreset> _transmuterPresets;

        public PresetsController(IEnumerable<ITransmuterPreset> transmuterPresets)
        {
            _transmuterPresets = transmuterPresets;
        }

        [HttpGet]
        public Task<IActionResult> ChoosePreset()
        {
            return Task.FromResult<IActionResult>(View(new ChoosePresetViewModel()
            {
                Presets = _transmuterPresets
            }));
        }
        public class ChoosePresetViewModel
        {
            public IEnumerable<ITransmuterPreset> Presets { get; set; }
        }
    }
}