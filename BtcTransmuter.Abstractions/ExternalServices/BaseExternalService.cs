using System;

namespace BtcTransmuter.Abstractions
{
    public abstract class BaseExternalService<T>
    {
        private ExternalServiceData _data;
        protected abstract string ExternalServiceType { get; }
        public T Data { get; set; }

        public BaseExternalService(ExternalServiceData data)
        {
            if (data.Type != ExternalServiceType)
            {
                throw new ArgumentException("fuck this shit you gave me the wrong external service data");
            }
            _data = data;
        }
    }
}