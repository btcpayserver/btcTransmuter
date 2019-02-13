namespace BtcTransmuter.Abstractions.ExternalServices
{
    public interface IExternalServiceDescriptor
    {
        string ExternalServiceType { get;  }
        string Name { get;  }
        string Description { get;}
        string ViewPartial { get; }
    }
}