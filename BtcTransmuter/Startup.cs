using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ILogger = Microsoft.VisualStudio.Web.CodeGeneration.ILogger;

namespace BtcTransmuter
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public Startup(IHostingEnvironment hostingEnvironment, IConfiguration configuration,  ILoggerFactory logFactory)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logFactory.CreateLogger(nameof(Startup));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IdentityOptions>(identityOptions =>
            {
                identityOptions.Password.RequireDigit = false;
                identityOptions.Password.RequireLowercase = false;
                identityOptions.Password.RequireUppercase = false;
                identityOptions.Password.RequiredUniqueChars = 0;
                identityOptions.Password.RequireNonAlphanumeric = false;
            });
            
            services.Configure<SecurityStampValidatorOptions>(validatorOptions =>  validatorOptions.ValidationInterval = TimeSpan.FromSeconds(50));

            services.AddHttpClient();
            services.AddOptions();
            services.AddTransmuterServices();
            services.AddMemoryCache();


            var options = new BtcTransmuterOptions(Configuration, _hostingEnvironment, _logger);
            services.AddSingleton(options);
            services.AddSingleton<IBtcTransmuterOptions>(options);
            services.AddDbContext<ApplicationDbContext>(builder =>
            {
                switch (options.DatabaseType)
                {
                    case DatabaseType.Sqlite:
                        builder.UseSqlite(options.DatabaseConnectionString);
                        break;
                    case DatabaseType.Postgres:
                        builder.UseNpgsql(options.DatabaseConnectionString);
                        break;
                    case DatabaseType.SqlServer:
                        builder.UseSqlServer(options.DatabaseConnectionString);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            services.AddDataProtection()
                .SetApplicationName("btctransmuter")
                .PersistKeysToFileSystem(Directory.CreateDirectory(options.DataProtectionDir));

            services.ConfigureApplicationCookie(options => {
                options.Cookie.Name = ".AspNet.Cookie.btctransmuter";
            });
            
            services.AddDefaultIdentity<User>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            var mvcBuilder = services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddExtensions(options.ExtensionsDir, mvcBuilder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, BtcTransmuterOptions options,
            IServiceScopeFactory serviceScopeFactory)
        {
            DependencyHelper.ServiceScopeFactory = serviceScopeFactory;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    if (context.Database.IsSqlite())
                    {                       
                        context.Database.EnsureCreated();
                    }
                    else
                    {
                        context.Database.Migrate();
                    }

                    using (var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>())
                    {
                        if (!roleManager.RoleExistsAsync("Admin").Result)
                        {
                            if (!roleManager.CreateAsync(new IdentityRole("Admin")).Result.Succeeded)
                            {
                                throw new Exception("couldnt create role needed for admin");
                            }
                        }
                    }
                }
            }

            if (env.IsDevelopment())
            {
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            app.UseForwardedHeaders();
            app.UseStaticFiles(options.RootPath);
            app.UsePathBase(options.RootPath);
            app.UseAuthentication();
            app.UseExtensions();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                Converters =
                    app.ApplicationServices.GetServices<BtcTransmuterExtension>()
                        .Where(extension => extension.JsonConverters != null)
                        .SelectMany(extension => extension.JsonConverters).ToList()
            };
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            
        }
    }
}