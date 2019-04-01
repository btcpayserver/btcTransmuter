using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BtcTransmuter
{
    public class BtcTransmuterOptions
    {
        public const string configPrefix = "TRANSMUTER_";
        public BtcTransmuterOptions(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {

            DatabaseConnectionString = configuration.GetValue<string>("Database");
            DataProtectionDir = configuration.GetValue<string>("DataProtectionDir");
            DatabaseType = configuration.GetValue<DatabaseType>("DatabaseType", DatabaseType.Sqlite);
            ExtensionsDir = Path.Combine(hostingEnvironment.ContentRootPath, "Extensions");

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
        }

        public string ExtensionsDir { get; set; }

        public string DatabaseConnectionString { get; set; }
        public string DataProtectionDir { get; set; }

        public DatabaseType DatabaseType { get; set; }
       
    }
}