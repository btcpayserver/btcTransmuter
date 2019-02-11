namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionDescriptor
    {
        string ActionId { get;  }
        string Name { get;  }
        string Description { get;}
    }
}