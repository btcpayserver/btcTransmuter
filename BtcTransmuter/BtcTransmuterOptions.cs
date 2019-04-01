using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BtcTransmuter
{
    public class BtcTransmuterOptions
    {
        public const string configPrefix = "TRANSMUTER_";
        public BtcTransmuterOptions(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILogger logger)
        {

            DatabaseConnectionString = configuration.GetValue<string>("Database");
            DataProtectionDir = configuration.GetValue<string>("DataProtectionDir");
            DatabaseType = configuration.GetValue<DatabaseType>("DatabaseType", DatabaseType.Sqlite);
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
            
            logger.LogWarning($"Connecting to {DatabaseType} db with: {DatabaseConnectionString}");
            logger.LogWarning($"Extensions Dir: {ExtensionsDir}, Data Protection Dir: {DataProtectionDir}");
        }

        public string ExtensionsDir { get; set; }

        public string DatabaseConnectionString { get; set; }
        public string DataProtectionDir { get; set; }

        public DatabaseType DatabaseType { get; set; }
       
    }
}