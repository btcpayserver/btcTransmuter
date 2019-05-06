using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Extension.Presets
{
    [Route("presets-plugin/[controller]")]
    public class PresetsController: Controller
    {
        private readonly IEnumerable<ITransmuterPreset> _transmuterPresets;

        public PresetsController(IEnumerable<ITransmuterPreset> transmuterPresets)
        {
            _transmuterPresets = transmuterPresets;
        }
        

        public async Task<IActionResult> ChoosePreset()
        {
            return View(new ChoosePresetViewModel()
            {
                Presets = _transmuterPresets
            });
        }


        public class ChoosePresetViewModel
        {
            public IEnumerable<ITransmuterPreset> Presets { get; set; }
        }
    }
}