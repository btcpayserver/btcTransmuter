using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Extension.Presets
{
    [Route("presets-plugin/presets/PaymentForwarder")]

    public class PaymentForwarderController : Controller, ITransmuterPreset
    {
        public string Id { get; } = "PaymentForwarder";
        public string Name { get; } = "On-chain Forwarder";
        public string Description { get; } = "Forward funds from a wallet elsewhere";

        public PaymentForwarderController()
        {
            
        }
        
        public string GetLink()
        {
            return Url.Action(nameof(Create));
        }


        public async Task<IActionResult> Create()
        {
            return View(new CreatePaymentForwarderViewModel());
        }
        
    }
    
    public class CreatePaymentForwarderViewModel{}
}