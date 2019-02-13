using System;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Models;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.ExternalServices
{
    public abstract class BaseExternalService<T>: IExternalServiceValidator
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
        public abstract ValidationResult Validate(string data);
    }
}