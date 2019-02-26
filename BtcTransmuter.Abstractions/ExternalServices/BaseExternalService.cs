using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Abstractions.ExternalServices
{
    public abstract class BaseExternalService<T> :IExternalServiceDescriptor
    {
        private ExternalServiceData _data;
        public abstract string ExternalServiceType { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string ViewPartial { get; }


        public T GetData()
        {
            return _data.Get<T>();
        }

        public void SetData(T data)
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
        public abstract Task<IActionResult> EditData(ExternalServiceData data);

        protected BaseExternalService()
        {
        }
    }
}