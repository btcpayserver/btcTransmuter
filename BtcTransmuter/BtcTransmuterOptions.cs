using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BtcTransmuter
{
    public class BtcTransmuterOptions : IBtcTransmuterOptions
    {
        public const string configPrefix = "TRANSMUTER_";
        public BtcTransmuterOptions(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILogger logger)
        {
            
            RootPath = configuration.GetValue("RootPath", "");
            DatabaseConnectionString = configuration.GetValue<string>("Database");
            DataProtectionDir = configuration.GetValue<string>("DataProtectionDir");
            DataProtectionApplicationName = configuration.GetValue<string>("DataProtectionApplicationName");
            DatabaseType = configuration.GetValue<DatabaseType>("DatabaseType", DatabaseType.Sqlite);
            UseDatabaseColumnEncryption = configuration.GetValue<bool>("UseDatabaseColumnEncryption", false);
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
    }
}