using System;

namespace BtcTransmuter
{
    public interface IBtcTransmuterOptions
    {
        string ExtensionsDir { get; set; }
        string DatabaseConnectionString { get; set; }
        string DataProtectionDir { get; set; }
        DatabaseType DatabaseType { get; set; }
        bool UseDatabaseColumnEncryption { get; set; }
        bool DisableInternalAuth { get; set; }
        Uri BTCPayAuthServer { get; set; }
    }
}