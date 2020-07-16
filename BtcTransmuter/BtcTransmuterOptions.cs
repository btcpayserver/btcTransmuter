using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BtcTransmuter
{
    public class BtcTransmuterOptions : IBtcTransmuterOptions
    {
        public const string configPrefix = "TRANSMUTER_";
        public BtcTransmuterOptions(IConfiguration configuration, IWebHostEnvironment hostingEnvironment, ILogger logger)
        {
            
            RootPath = configuration.GetValue("RootPath", "");
            
            BTCPayAuthServer = configuration.GetValue<Uri>("BTCPayAuthServer", null);
            DatabaseConnectionString = configuration.GetValue<string>("Database");
            DataProtectionDir = configuration.GetValue<string>("DataProtectionDir");
            DataProtectionApplicationName = configuration.GetValue<string>("DataProtectionApplicationName");
            DatabaseType = configuration.GetValue<DatabaseType>("DatabaseType", DatabaseType.Sqlite);
            UseDatabaseColumnEncryption = configuration.GetValue<bool>("UseDatabaseColumnEncryption", false);
            DisableInternalAuth = configuration.GetValue<bool>("DisableInternalAuth", false);
            if (DisableInternalAuth && BTCPayAuthServer == null)
            {
                DisableInternalAuth = false;
                logger.LogWarning($"Cannot disable internal auth while not setting BTCPayAuthServer");
            }
            ExtensionsDir = configuration.GetValue<string>("ExtensionsDir",
                Path.Combine(hostingEnvironment.ContentRootPath, "Extensions"));

            if (DatabaseType == DatabaseType.Sqlite)
            {
                var dbFilePath = DatabaseConnectionString.Substring(DatabaseConnectionString.IndexOf("Data Source=") + "Data Source=".Length);
                dbFilePath = dbFilePath.Substring(0, dbFilePath.IndexOf(";"));
                        
                if (!string.IsNullOrEmpty(Path.GetDirectoryName(dbFilePath)))
                {
                    if (DataProtectionDir == null)
                    {
                        DataProtectionDir = Path.GetDirectoryName(dbFilePath);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(dbFilePath));
                
                }
            }
            logger.LogWarning($"{JObject.FromObject(this)}");
        }
        public string RootPath { get; set; } = "";
        public string DataProtectionApplicationName { get; set; } = "";
        public string ExtensionsDir { get; set; }

        public string DatabaseConnectionString { get; set; }
        public string DataProtectionDir { get; set; }

        public DatabaseType DatabaseType { get; set; }
        public bool UseDatabaseColumnEncryption { get; set; }
        public bool DisableInternalAuth { get; set; }

        public Uri BTCPayAuthServer { get; set; }
        
    }
}
