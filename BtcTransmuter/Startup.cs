using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
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
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace BtcTransmuter
{
    public class Startup
    {
        private string extensionsPath;
        private string dbFilePath;

        public Startup(IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Configuration = configuration;

            extensionsPath = Path.Combine(hostingEnvironment.ContentRootPath, "Extensions");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.AddDataProtection();
            services.AddHttpClient();
            services.AddOptions();
            services.AddTransmuterServices();
            services.AddMemoryCache();
            var dbConnString = Configuration.GetValue<string>("ConnectionStrings_Database");

            dbFilePath = dbConnString.Substring(dbConnString.IndexOf("Data Source=") + "Data Source=".Length);
            dbFilePath = dbFilePath.Substring(0, dbFilePath.IndexOf(";"));
            if (!string.IsNullOrEmpty(Path.GetDirectoryName(dbFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbFilePath));
            }

            Console.WriteLine($"Connecting to sqlite db with: {dbConnString}");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(dbConnString));

            services.AddDefaultIdentity<User>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //Add the file provider to the Razor view engine

            var mvcBuilder = services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddExtensions(extensionsPath, mvcBuilder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IServiceScopeFactory serviceScopeFactory)
        {
            DependencyHelper.ServiceScopeFactory = serviceScopeFactory;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    if (File.Exists(dbFilePath) && context.Database.IsSqlite() &&
                        context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.EnsureDeleted();
                    }

                    context.Database.Migrate();

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