using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.ExternalServices
{
    public abstract class BaseExternalService<T> : IExternalServiceValidator
    {
        private ExternalServiceData _data;
        public abstract string ExternalServiceType { get; }
        public T Data { get; set; }

        public BaseExternalService(ExternalServiceData data)
        {
            if (data.Type != ExternalServiceType)
            {
                throw new ArgumentException("fuck this shit you gave me the wrong external service data");
            }

            _data = data;
        }

        protected BaseExternalService()
        {
        }

        public virtual ICollection<ValidationResult> Validate(string data)
        {
            return ValidationHelper.Validate<T>(data);
        }
    }
}