using System;
using System.IO;
using System.Linq;
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

namespace BtcTransmuter
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _hostingEnvironment = hostingEnvironment;
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

            services.AddHttpClient();
            services.AddOptions();
            services.AddTransmuterServices();
            services.AddMemoryCache();


            var options = new BtcTransmuterOptions(Configuration, _hostingEnvironment);
            services.AddSingleton(options);
            Console.WriteLine($"Connecting to {options.DatabaseType} db with: {options.DatabaseConnectionString}");
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
                .PersistKeysToFileSystem(Directory.CreateDirectory(options.DataProtectionDir));

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

                        if (context.Database.GetPendingMigrations().Any())
                        {
                            context.Database.EnsureDeleted();
                        }
                        
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
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseExtensions();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}