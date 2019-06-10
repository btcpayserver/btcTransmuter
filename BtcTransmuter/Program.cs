using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BtcTransmuter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddEnvironmentVariables(prefix: BtcTransmuterOptions.configPrefix);
                })
                .ConfigureLogging(builder => 
                    builder
                        .SetMinimumLevel(LogLevel.Information)
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddDebug()
                        .AddConsole())
                .UseStartup<Startup>();
    }
}