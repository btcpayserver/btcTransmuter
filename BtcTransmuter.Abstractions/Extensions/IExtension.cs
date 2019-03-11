namespace BtcTransmuter.Abstractions.Extensions
{
    public interface IExtension
    {
        string Name { get; }
        string Version { get;  }
        string Description { get; }
        string Authors { get; }
    }
}