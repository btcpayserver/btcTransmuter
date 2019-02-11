namespace BtcTransmuter.Abstractions.Triggers
{
    public interface ITriggerDescriptor
    {
        string TriggerId{ get;  }
        string Name{ get;}
        string Description{ get;  }
    }
}