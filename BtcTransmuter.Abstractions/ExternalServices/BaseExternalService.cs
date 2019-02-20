using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Abstractions.ExternalServices
{
    public abstract class BaseExternalService<T> 
    {
        private ExternalServiceData _data;
        public abstract string ExternalServiceType { get; }

        internal T Data
        {
            get => _data.Get<T>();
            set => _data.Set(value);
        }

        public T GetData()
        {
            return Data;
        }

        public void SetData(T data)
        {
            Data = data;
        }

        protected BaseExternalService(ExternalServiceData data)
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
    }
}