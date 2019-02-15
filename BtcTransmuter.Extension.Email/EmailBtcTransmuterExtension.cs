using System;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Extensions;
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

    
    public class EmailBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Emails Plugin";
        public override string Version  => "0.001";
        protected override int Priority => 0;
        public override string[] Scripts => new string[0];
        public override string[] Stylesheets => new string[0];
    }
}