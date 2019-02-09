using System;
using System.Collections;

namespace BtcTransmuter.Abstractions
{
    public interface IBtcTransmuterExtension
    {
        string Name { get; }
        string Version { get; }
        
    }
}