using System;
using BtcTransmuter.Abstractions;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class Pop3ExternalServiceData : BaseEmailServiceData
    {
        public DateTime? LastCheck { get; set; }
    }

    public class Pop3Service
    {
        private ExternalServiceData _data;

        public Pop3ExternalServiceData Data
        {
            get => _data.GetData<Pop3ExternalServiceData>();
            set => _data.SetData(value);
        }

        public Pop3Service(ExternalServiceData data)
        {
            if (data.Type != EmailBtcTransmuterExtension.Pop3ExternalServiceType)
            {
                throw new ArgumentException("fuck this shit you gave me the wrong external service data");
            }

            _data = data;
        }
    }
}