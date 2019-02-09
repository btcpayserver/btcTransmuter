using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BtcTransmuter.Abstractions
{
    public interface IExternalServiceManager
    {
        Task<ExternalServiceData> GetExternalServiceData(string id);
        Task RemoveExternalServiceData(string id);
        Task AddOrUpdateExternalServiceData(ExternalServiceData externalServiceData);
        Task UpdateInternalData(string id, object data);
        event EventHandler<UpdatedItem<ExternalServiceData>> ExternalServiceDataUpdated;
        Task<IEnumerable<ExternalServiceData>> GetExternalServicesData(ExternalServicesDataQuery query);
    }

    public class ExternalServicesDataQuery
    {
        public string[] Type { get; set; }
    }
}