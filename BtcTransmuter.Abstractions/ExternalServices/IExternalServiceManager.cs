using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.ExternalServices
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
}