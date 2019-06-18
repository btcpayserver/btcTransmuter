using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BtcTransmuter
{
    public class BtcTransmuterOptions : IBtcTransmuterOptions
    {
        public const string configPrefix = "TRANSMUTER_";
        public BtcTransmuterOptions(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILogger logger)
        {
            
            DatabaseConnectionString = configuration.GetValue<string>("Database");
            DataProtectionDir = configuration.GetValue<string>("DataProtectionDir");
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
            
            logger.LogInformation($"Connecting to {DatabaseType} db with: {DatabaseConnectionString}");
            logger.LogInformation($"Extensions Dir: {ExtensionsDir}, Data Protection Dir: {DataProtectionDir}");
            logger.LogInformation($"Database Column Encryption is {(UseDatabaseColumnEncryption?"Enabled": "Disabled")}");
        }

        public string ExtensionsDir { get; set; }

        public string DatabaseConnectionString { get; set; }
        public string DataProtectionDir { get; set; }

        public DatabaseType DatabaseType { get; set; }
        public bool UseDatabaseColumnEncryption { get; set; }
    }
}