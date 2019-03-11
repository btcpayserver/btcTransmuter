using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BtcTransmuter.Abstractions.Extensions;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace BtcTransmuter
{
    public static class ExtensionManager
    {
        public static void AddExtensions(this IServiceCollection serviceCollection, string extensionsFolder,
            IMvcBuilder mvcBuilder)
        {
            var providers = new List<IFileProvider>();
            var extensions = new List<BtcTransmuterExtension>();

            var loadedPluginAssemblies = GetDefaultLoadedPluginAssemblies();
            providers.AddRange(loadedPluginAssemblies.Select(CreateEmbeddedFileProviderForAssembly));
            extensions.AddRange(loadedPluginAssemblies.SelectMany(assembly =>
                GetAllExtensionTypesFromAssembly(assembly).Select(GetExtensionInstanceFromType)));

            Console.WriteLine($"Loading extensions from {extensionsFolder}");
            Directory.CreateDirectory(extensionsFolder);
            foreach (var dir in Directory.GetDirectories(extensionsFolder))
            {
                var pluginName = Path.GetFileName(dir);
                var plugin = PluginLoader.CreateFromAssemblyFile(Path.Combine(dir, pluginName + ".dll"),
                    PluginLoaderOptions.PreferSharedTypes);
                var pluginAssembly = plugin.LoadDefaultAssembly();
                providers.Add(CreateEmbeddedFileProviderForAssembly(pluginAssembly));
                extensions.AddRange(GetAllExtensionTypesFromAssembly(pluginAssembly)
                    .Select(GetExtensionInstanceFromType));
                Console.WriteLine(
                    $"Loading application parts from extension {pluginName}");

                var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
                foreach (var part in partFactory.GetApplicationParts(pluginAssembly))
                {
                    Console.WriteLine($"* {part.Name}");
                    
                    mvcBuilder.PartManager.ApplicationParts.Add(part);
                }

                // This piece finds and loads related parts, such as MvcAppPlugin1.Views.dll.
                var relatedAssembliesAttrs = pluginAssembly.GetCustomAttributes<RelatedAssemblyAttribute>();
                foreach (var attr in relatedAssembliesAttrs)
                {
                    var assembly = plugin.LoadAssembly(attr.AssemblyFileName);
                    partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                    foreach (var part in partFactory.GetApplicationParts(assembly))
                    {
                        Console.WriteLine($"  * {part.Name}");
                        mvcBuilder.PartManager.ApplicationParts.Add(part);
                    }
                }
            }
        
            serviceCollection.Configure<RazorViewEngineOptions>(options =>
            {
                foreach (var embeddedFileProvider in providers)
                {
                    options.FileProviders.Add(embeddedFileProvider);
                }
            });

            foreach (var btcTransmuterExtension in extensions)
            {
                btcTransmuterExtension.Execute(serviceCollection);
            }
        }

        public static void UseExtensions(this IApplicationBuilder applicationBuilder)
        {
            foreach (var btcTransmuterExtension in applicationBuilder.ApplicationServices
                .GetServices<BtcTransmuterExtension>())
            {
                btcTransmuterExtension.Execute(applicationBuilder,
                    applicationBuilder.ApplicationServices);
            }
        }


        private static Assembly[] GetDefaultLoadedPluginAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
                    assembly.FullName.StartsWith("BtcTransmuter.Extension",
                        StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
        }

        private static Type[] GetAllExtensionTypesFromAssembly(Assembly assembly)
        {
            return assembly.GetTypes().Where(type =>
                typeof(BtcTransmuterExtension).IsAssignableFrom(type) &&
                !type.IsAbstract).ToArray();
        }

        private static BtcTransmuterExtension GetExtensionInstanceFromType(Type type)
        {
            return (BtcTransmuterExtension) Activator.CreateInstance(type, new object[0]);
        }

        private static IFileProvider CreateEmbeddedFileProviderForAssembly(Assembly assembly)
        {
            return new EmbeddedFileProvider(assembly);
        }
    }
}