using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged;
using BtcTransmuter.Extension.Email.Actions.SendEmail;
using BtcTransmuter.Extension.Email.ExternalServices.Smtp;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NBitpayClient;
using Newtonsoft.Json;

namespace BtcTransmuter.Extension.Presets
{
    [Route("presets-plugin/presets/BTCPayEmailReceipts")]
    [Authorize]
    public class BTCPayEmailReceiptsController : Controller, ITransmuterPreset
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IRecipeManager _recipeManager;
        public string Id { get; } = "BTCPayEmailReceipts";
        public string Name { get; } = "BTCPay Email Receipts";
        public string Description { get; } = "Send an email when a BTCPay invoice gets paid";

        public BTCPayEmailReceiptsController(
            IExternalServiceManager externalServiceManager,
            UserManager<User> userManager,
            IRecipeManager recipeManager)
        {
            _externalServiceManager = externalServiceManager;
            _userManager = userManager;
            _recipeManager = recipeManager;
        }

        public (string ControllerName, string ActionName) GetLink()
        {
            return (Id, nameof(Create));
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var services = await GetServices();

            return View(new CreateBTCPayEmailReceiptsViewModel()
            {
                BTCPayServices = new SelectList(services.btcPayServices, nameof(ExternalServiceData.Id), nameof(ExternalServiceData.Name)),
                SMTPServices = new SelectList(services.smtpServices, nameof(ExternalServiceData.Id), nameof(ExternalServiceData.Name)),
                Status = Invoice.STATUS_COMPLETE,
                
            });
        }

        private async Task<(IEnumerable<ExternalServiceData> btcPayServices, IEnumerable<ExternalServiceData> smtpServices)> GetServices()
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User),
                Type = new[] {SmtpService.SmtpExternalServiceType, BtcPayServerService.BtcPayServerServiceType}
            });
            var btcPayServices = services.Where(data => data.Type == BtcPayServerService.BtcPayServerServiceType);
            var smtpServices = services.Where(data => data.Type == SmtpService.SmtpExternalServiceType);
            return (btcPayServices, smtpServices);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateBTCPayEmailReceiptsViewModel viewModel)
        {
            
            var services = await GetServices();

            viewModel.BTCPayServices = new SelectList(services.btcPayServices, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name));
            viewModel.SMTPServices = new SelectList(services.smtpServices, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name));

            if (!viewModel.SendToCustomer && string.IsNullOrEmpty(viewModel.AlternateEmail))
            {
                ModelState.AddModelError(nameof(viewModel.SendToCustomer),
                    "You need to send the email to either the customer or the BCC email address");
                ModelState.AddModelError(nameof(viewModel.AlternateEmail),
                    "You need to send the email to either the customer or the BCC email address");
            }
                

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            return await SetItUp(viewModel);
        }

        private async Task<IActionResult> SetItUp(CreateBTCPayEmailReceiptsViewModel vm)
        {
            var presetName = $"Generated_BTCPayEmailReceipts_{vm.Status}";
            
            var recipe = new Recipe()
            {
                Name = presetName,
                Description = "Generated from a preset",
                UserId = _userManager.GetUserId(User),
                Enabled = false
            };
            await _recipeManager.AddOrUpdateRecipe(recipe);

            var recipeTrigger = new RecipeTrigger()
            {
                ExternalServiceId = vm.SelectedBTCPayExternalServiceId,
                TriggerId = new InvoiceStatusChangedTrigger().Id,
                RecipeId = recipe.Id
            };

            recipeTrigger.Set(new InvoiceStatusChangedTriggerParameters()
            {
                Conditions = new List<InvoiceStatusChangeCondition>()
                {
                    new InvoiceStatusChangeCondition()
                    {
                        Status = vm.Status,
                        ExceptionStatuses = InvoiceStatusChangedController.AllowedExceptionStatus.Select(item => item.Value).ToList()
                    }
                }
            });
            await _recipeManager.AddOrUpdateRecipeTrigger(recipeTrigger);

            var recipeActionGroup = new RecipeActionGroup()
            {
                RecipeId = recipe.Id
            };
            await _recipeManager.AddRecipeActionGroup(recipeActionGroup);
            var recipeActionGroupIndex = 0;
            
            if (vm.SendToCustomer)
            {
                var sendEmailAction = new RecipeAction()
                {
                    RecipeId = recipe.Id,
                    RecipeActionGroupId = recipeActionGroup.Id,
                    ActionId = new SendEmailDataActionHandler().ActionId,
                    ExternalServiceId = vm.SelectedSMTPExternalServiceId,
                    Order = recipeActionGroupIndex,
                    DataJson = JsonConvert.SerializeObject(new SendEmailData()
                    {
                        IsHTML = true,
                        Body = vm.HTMLBody,
                        Subject = vm.Subject,
                        From = vm.From,
                        To = "{{TriggerData.Invoice.Buyer.email}}"
                    })
                };
                await _recipeManager.AddOrUpdateRecipeAction(sendEmailAction);
                recipeActionGroupIndex++;
            }
            if (!string.IsNullOrEmpty(vm.AlternateEmail))
            {

                var sendEmailAction = new RecipeAction()
                {
                    RecipeId = recipe.Id,
                    RecipeActionGroupId = recipeActionGroup.Id,
                    ActionId = new SendEmailDataActionHandler().ActionId,
                    ExternalServiceId = vm.SelectedSMTPExternalServiceId,
                    Order = recipeActionGroupIndex,
                    DataJson = JsonConvert.SerializeObject(new SendEmailData()
                    {
                        IsHTML = true,
                        Body = vm.HTMLBody,
                        Subject = vm.Subject,
                        From = vm.From,
                        To = vm.AlternateEmail
                    })
                };
                await _recipeManager.AddOrUpdateRecipeAction(sendEmailAction);
                recipeActionGroupIndex++;
            }
            

            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipe.Id,
                statusMessage =
                    "Preset generated. Recipe is currently disabled for now. Please verify details are correct before enabling!"
            });
        }
    }

    public class CreateBTCPayEmailReceiptsViewModel
    {
        public SelectList BTCPayServices { get; set; }
        public SelectList SMTPServices { get; set; }

        [Display(Name = "Existing BTCPay Store")]
        [Required]
        public string SelectedBTCPayExternalServiceId { get; set; }
        
        [Display(Name = "Existing SMTP Service")]
        [Required]
        public string SelectedSMTPExternalServiceId { get; set; }

        [Display(Name = "When the BTCPay invoice status becomes ")]
        public string Status { get; set; }
        [Display(Name = "Send email to the address registered on the BTCPay invoice")]
        public bool SendToCustomer { get; set; }
        [EmailAddress]
        
        [Display(Name = "Send email to this address")]
        public string AlternateEmail { get; set; }

        [Display(Name = "Email Subject")]
        public string Subject { get; set; }

        [Display(Name = "Email Body( in HTML)")]
        public string HTMLBody { get; set; } = @"<div style='text-align:center; width:100%'>
   Thank you for your contribution ( via {{TriggerData.Invoice.ItemDesc}}) <br> 
   <p>Please make sure you have the contents of this email available to present at BTCPay Day. </p>
   <p>You can find more information regarding agenda and schedule at <a href='https://day.btcpayserver.org' target='_blank'>day.btcpayserver.org</a></p>
   <p> <img src='https://api.qrserver.com/v1/create-qr-code/?size=150x150&amp;data={{TriggerData.Invoice.Url}}'><br> Invoice Id: <a target='_blank' href='{{TriggerData.Invoice.Url}}'>{{TriggerData.Invoice.Id}}</a> </p>
</div>";

        [Display(Name = "Email to send from")]
        [EmailAddress]
        [Required]
        public string From { get; set; }
    }
}