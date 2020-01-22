using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.BtcPayServer.Actions.GetPaymentsFromInvoice;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged;
using BtcTransmuter.Extension.Email.Actions.SendEmail;
using BtcTransmuter.Extension.Email.ExternalServices.Smtp;
using BtcTransmuter.Extension.Exchange.Actions.PlaceOrder;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using ExchangeSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NBitpayClient;
using Newtonsoft.Json;

namespace BtcTransmuter.Extension.Presets
{
    [Route("presets-plugin/presets/FiatExchangeConversion")]
    [Authorize]
    public class FiatExchangeConversionController : Controller, ITransmuterPreset
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IRecipeManager _recipeManager;
        public string Id { get; } = "FiatExchangeConversion";
        public string Name { get; } = "Fiat Conversion";
        public string Description { get; } = "Convert incoming money to fiat on an exchange by market selling when BTCPay invoice statuses change.";

        public FiatExchangeConversionController(
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

            return View(new CreateFiatExchangeConversionViewModel()
            {
                BTCPayServices = new SelectList(services.btcPayServices, nameof(ExternalServiceData.Id), nameof(ExternalServiceData.Name)),
                ExchangeServices = new SelectList(services.exchangeServices, nameof(ExternalServiceData.Id), nameof(ExternalServiceData.Name)),
                Status = Invoice.STATUS_COMPLETE,
                
            });
        }

        private async Task<(IEnumerable<ExternalServiceData> btcPayServices, IEnumerable<ExternalServiceData> exchangeServices)> GetServices()
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User),
                Type = new[] {Exchange.ExternalServices.Exchange.ExchangeService.ExchangeServiceType, BtcPayServerService.BtcPayServerServiceType}
            });
            var btcPayServices = services.Where(data => data.Type == BtcPayServerService.BtcPayServerServiceType);
            var exchangeServices = services.Where(data => data.Type == Exchange.ExternalServices.Exchange.ExchangeService.ExchangeServiceType);

            return (btcPayServices, exchangeServices);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateFiatExchangeConversionViewModel viewModel)
        {
            var services = await GetServices();

            viewModel.BTCPayServices = new SelectList(services.btcPayServices, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name));
            viewModel.ExchangeServices = new SelectList(services.exchangeServices, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name));

            if (viewModel.Action.Equals("add-item", StringComparison.InvariantCultureIgnoreCase))
            {
                viewModel.Conditions.Add(new CreateFiatExchangeConversionViewModel.ConversionConditionItem());
                return View(viewModel);
            }

            if (viewModel.Action.StartsWith("remove-item", StringComparison.InvariantCultureIgnoreCase))
            {
                var index = int.Parse(viewModel.Action.Substring(viewModel.Action.IndexOf(":") + 1));
                viewModel.Conditions.RemoveAt(index);
                return View(viewModel);
            }
            
            if (!viewModel.Conditions.Any())
            {
                ModelState.AddModelError(nameof(viewModel.Conditions), "You need to add at least one conversion scenario!");
            }         
            if (ModelState.IsValid)
            {
                for (var index = 0; index < viewModel.Conditions.Count; index++)
                {
                    var condition = viewModel.Conditions[index];
                    var serviceData =
                        await _externalServiceManager.GetExternalServiceData(condition.ExchangeServiceId, GetUserId());
                    var exchangeService = new ExchangeService(serviceData);
                    var symbols = (await exchangeService.ConstructClient().GetMarketSymbolsAsync()).ToArray();
                    if (!symbols.Contains(condition.MarketSymbol))
                    {
                        viewModel.AddModelError(
                            $"{nameof(viewModel.Conditions)}[{index}].{nameof(CreateFiatExchangeConversionViewModel.ConversionConditionItem.MarketSymbol)}",
                            $"The market symbols you entered is invalid. Please choose from the following: {string.Join(",", symbols)}", ModelState);
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            return await SetItUp(viewModel);
        }
        
        protected string GetUserId()
        {
            return _userManager.GetUserId(User);
        }

        private async Task<IActionResult> SetItUp(CreateFiatExchangeConversionViewModel vm)
        {
            var presetName = $"Generated_FiatExchangeConversion";
            
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

            foreach (var condition in vm.Conditions)
            {
                await _recipeManager.AddRecipeActionGroup(recipeActionGroup);
                var recipeActionGroupIndex = 0;
                var getPayments = new RecipeAction()
                {
                    RecipeId = recipe.Id,
                    RecipeActionGroupId = recipeActionGroup.Id,
                    ActionId = new GetPaymentsFromInvoiceDataActionHandler().ActionId,
                    ExternalServiceId = vm.SelectedBTCPayExternalServiceId,
                    Order = recipeActionGroupIndex,
                    DataJson = JsonConvert.SerializeObject(new GetPaymentsFromInvoiceData()
                    {
                        CryptoCode = condition.CryptoCode,
                        InvoiceId = "{{TriggerData.Invoice.Id}}",
                        PaymentType = ""
                    })
                };
                recipeActionGroupIndex++;
                await _recipeManager.AddOrUpdateRecipeAction(getPayments);
                var tradeAction = new RecipeAction()
                {
                    RecipeId = recipe.Id,
                    RecipeActionGroupId = recipeActionGroup.Id,
                    ActionId = new PlaceOrderDataActionHandler().ActionId,
                    ExternalServiceId = condition.ExchangeServiceId,
                    Order = recipeActionGroupIndex,
                    DataJson = JsonConvert.SerializeObject(new PlaceOrderData()
                    {
                        Amount = "{{PreviousAction.Sum("+nameof(InvoicePaymentInfo.Value)+")}}",
                        IsBuy = condition.IsBuy,
                        MarketSymbol = condition.MarketSymbol,
                        OrderType = OrderType.Market
                    })
                };

                recipeActionGroupIndex++;
                await _recipeManager.AddOrUpdateRecipeAction(tradeAction);
            }
            
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipe.Id,
                statusMessage =
                    "Preset generated. Recipe is currently disabled for now. Please verify details are correct before enabling!"
            });
        }
    }

    public class CreateFiatExchangeConversionViewModel
    {
        public SelectList BTCPayServices { get; set; }
        public SelectList ExchangeServices { get; set; }
        
        [Display(Name = "Existing BTCPay Store")]
        [Required]
        public string SelectedBTCPayExternalServiceId { get; set; }

        [Display(Name = "When the BTCPay invoice status becomes ")]
        public string Status { get; set; }

        public List<ConversionConditionItem> Conditions { get; set; } = new List<ConversionConditionItem>()
        {
            new ConversionConditionItem()
        };
        public string Action { get; set; }

        public class  ConversionConditionItem
        {
            [Required]
            [Display(Name = "When a payment in the following currency happens on a BTCPay invoice")]
            public string CryptoCode { get; set; }
            
            [Display(Name = "Existing Exchange service")]
            [Required]
            public string ExchangeServiceId { get; set; }
            
            [Display(Name = "The trading pair on the exchange")]
            [Required]
            public string MarketSymbol { get; set; }
            
            [Display(Name = "Is it a buy market order?")]
            [Required]
            public bool  IsBuy { get; set; }
        }
    }
}