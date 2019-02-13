namespace BtcTransmuter.Controllers
{
    public class GetExternalServicesViewModel
    {
        public Task<IEnumerable<ExternalServiceData>> ExternalServices { get; set; }
        public IEnumerable<IExternalServiceDescriptor> Descriptors { get; set; }
    }
}