using System;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Extension.Email.Actions;
using BtcTransmuter.Extension.Email.Actions.SendEmail;
using BtcTransmuter.Extension.Email.ExternalServices;
using BtcTransmuter.Extension.Email.HostedServices;
using BtcTransmuter.Extension.Email.Triggers;
using BtcTransmuter.Extension.Email.Triggers.ReceivedEmail;
using ExtCore.Infrastructure;
using ExtCore.Infrastructure.Actions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
namespace BtcTransmuter.Extension.Email
{
    public class EmailBtcTransmuterExtension : ExtensionBase, IConfigureAction, IConfigureServicesAction
    {

        public void Execute(IApplicationBuilder applicationBuilder, IServiceProvider serviceProvider)
        {
        }

        public void Execute(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        {
            serviceCollection.AddSingleton<IExtension, EmailBtcTransmuterExtension>();
            serviceCollection.AddSingleton<ITriggerHandler, ReceivedEmailTriggerHandler>();
            serviceCollection.AddSingleton<IActionHandler, SendEmailDataActionHandler>();
            serviceCollection.AddSingleton<IActionDescriptor, SendEmailActionDescriptor >();
            serviceCollection.AddSingleton<SendEmailActionDescriptor >();
            serviceCollection.AddSingleton<ITriggerDescriptor, ReceivedEmailTriggerDescriptor>();
            serviceCollection.AddSingleton<IExternalServiceDescriptor, Pop3ExternalServiceDescriptor>();
            serviceCollection.AddSingleton<IExternalServiceDescriptor, SmtpExternalServiceDescriptor>();
            serviceCollection.AddHostedService<ReceivingEmailHostedService>();
        }

        int IConfigureServicesAction.Priority => 0;

        int IConfigureAction.Priority => 0;
    }
}