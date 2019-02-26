
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MailKit.Net.Smtp;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace BtcTransmuter.Extension.Email.ExternalServices.Smtp
{
    public class SmtpService : BaseExternalService<SmtpExternalServiceData>, IExternalServiceDescriptor
    {
        public const string SmtpExternalServiceType = "SmtpExternalService";
        public override string ExternalServiceType => SmtpExternalServiceType;

        public override  string Name => "SMTP External Service";
        public override  string Description => "SMTP External Service to be able to send emails as an action";
        public override  string ViewPartial => "ViewSmtpExternalService";

        public override  Task<IActionResult> EditData(ExternalServiceData externalServiceData)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier = externalServiceData.Id ?? $"new_{Guid.NewGuid()}";
                if (string.IsNullOrEmpty(externalServiceData.Id))
                {
                    var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                    memoryCache.Set(identifier, externalServiceData, new MemoryCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(60)
                    });
                }

                return Task.FromResult<IActionResult>(new RedirectToActionResult(nameof(SmtpController.EditData),
                    "Smtp", new
                    {
                        identifier
                    }));
            }
        }

        public SmtpService() : base()
        {
        }

        public SmtpService(ExternalServiceData data) : base(data)
        {
        }

        public async Task SendEmail(MimeMessage message)
        {
            var data = GetData();
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s,c,h,e) => true;
                await client.ConnectAsync(data.Server, data.Port, data.SSL);
                await client.AuthenticateAsync(data.Username, data.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}