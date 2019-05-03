using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Abstractions.ExternalServices
{
    public abstract class BaseExternalService<T> : IExternalServiceDescriptor
    {
        private ExternalServiceData _data;
        public abstract string ExternalServiceType { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string ViewPartial { get; }
        public abstract string ControllerName { get; }


        public virtual T GetData()
        {
            return _data.Get<T>();
        }

        public virtual  void SetData(T data)
        {
            _data.Set(data);
        }

        protected BaseExternalService(ExternalServiceData data)
        {
            if (data.Type != ExternalServiceType)
            {
                throw new ArgumentException("fuck this shit you gave me the wrong external service data");
            }

            _data = data;
        }

        public virtual Task<IActionResult> EditData(ExternalServiceData externalServiceData)
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

                return Task.FromResult<IActionResult>(new RedirectToActionResult("EditData",
                    ControllerName, new
                    {
                        identifier
                    }));
            }
        }

        protected BaseExternalService()
        {
        }
    }
}