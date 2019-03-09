using System;
using System.Collections.Generic;
using System.Data.Common;
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
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace BtcTransmuter
{
    public class Startup
    {
        private string extensionsPath;
        private IEnumerable<BtcTransmuterExtension> _extensionsLoadedByDefault;

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


            services.AddHttpClient();
            services.AddOptions();
            services.AddTransmuterServices();
            services.AddMemoryCache();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<User>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
                assembly.FullName.StartsWith("BtcTransmuter.Extension", StringComparison.InvariantCultureIgnoreCase));
            var types = assemblies.SelectMany(assembly =>
                assembly.GetTypes().Where(type =>
                    typeof(BtcTransmuterExtension).IsAssignableFrom(type) &&
                    !type.IsAbstract));

            var fileProviders = types.Select(type => type.Assembly).Distinct().Select(assembly =>
                new EmbeddedFileProvider(
                    assembly));
            _extensionsLoadedByDefault = types.Select(type => (BtcTransmuterExtension) Activator.CreateInstance(type, new object[0]));

            foreach (var btcTransmuterExtension in _extensionsLoadedByDefault)
            {
                btcTransmuterExtension.Execute(services);
            }
            //Add the file provider to the Razor view engine
            services.Configure<RazorViewEngineOptions>(options =>
            {
                foreach (var embeddedFileProvider in fileProviders)
                {
                    options.FileProviders.Add(embeddedFileProvider);
                }
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IServiceScopeFactory serviceScopeFactory)
        {
            DependencyHelper.ServiceScopeFactory = serviceScopeFactory;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                foreach (var btcTransmuterExtension in _extensionsLoadedByDefault)
                {
                    btcTransmuterExtension.Execute(app, scope.ServiceProvider);
                }

                using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
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
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}