using System;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(BtcTransmuter.Areas.Identity.IdentityHostingStartup))]
namespace BtcTransmuter.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => { services.AddScoped<BTCPayAuthService>(); });
        }
    }
}