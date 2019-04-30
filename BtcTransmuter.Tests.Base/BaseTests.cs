using System.Collections.Generic;
using BtcTransmuter.Abstractions.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Tests.Base
{
    public abstract class BaseTests
    {
        protected static IServiceScopeFactory ScopeFactory;
        protected BaseTests()
        {
        }

        public void ConfigureDependencyHelper()
        {
            if (ScopeFactory == null)
            {


                var ssF = Program.CreateWebHostBuilder(new string[0]).ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddInMemoryCollection(new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("Database", "Data Source=mydb.db;"),
                        new KeyValuePair<string, string>("DatabaseType", "sqlite"),
                        new KeyValuePair<string, string>("DataProtectionDir", "C:/transmuterkey"),
                        new KeyValuePair<string, string>("ENVIRONMENT", "Development"),
                        new KeyValuePair<string, string>("NBXplorer_Cryptos", "btc;ltc"),
                        new KeyValuePair<string, string>("NBXplorer_Uri", "http://127.0.0.1:32838/"),
                        new KeyValuePair<string, string>("NBXplorer_NetworkType", "Regtest"),
                        new KeyValuePair<string, string>("NBXplorer_UseDefaultCookie", "1"),
                    });
                }).Build().Services.GetRequiredService<IServiceScopeFactory>();

                DependencyHelper.ServiceScopeFactory = ssF;
            }
        }
    }
}