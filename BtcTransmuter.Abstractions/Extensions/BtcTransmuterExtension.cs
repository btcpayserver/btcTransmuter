using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.AutoRegisterDi;

namespace BtcTransmuter.Abstractions.Extensions
{
    public abstract class BtcTransmuterExtension: IExtension
    {

        public abstract string Name { get; }

        public virtual string Version => GetType().Assembly.GetName().Version.ToString();

        public virtual string Description { get; } = string.Empty;
        public virtual string Authors { get; } = string.Empty;
        public virtual string HeaderPartial { get; }
        public virtual  string MenuPartial { get; }

        public IEnumerable<IActionDescriptor> Actions => GetInstancesOfTypeInOurAssembly<IActionDescriptor>();
        public IEnumerable<ITriggerDescriptor> Triggers => GetInstancesOfTypeInOurAssembly<ITriggerDescriptor>();

        public IEnumerable<IExternalServiceDescriptor> ExternalServices =>
            GetInstancesOfTypeInOurAssembly<IExternalServiceDescriptor>();


        public virtual void Execute(IApplicationBuilder applicationBuilder, IServiceProvider serviceProvider)
        {
        }

        public virtual void Execute(IServiceCollection serviceCollection)
        {
            RegisterInstances(serviceCollection, new Type[]
            {
                typeof(IActionDescriptor),
                typeof(IActionHandler),
                typeof(ITriggerDescriptor),
                typeof(ITriggerHandler),
                typeof(IHostedService),
                typeof(IExternalServiceDescriptor),
            });
            serviceCollection.AddSingleton(this);
        }

        private void RegisterInstances(IServiceCollection serviceCollection, Type[] validTypes)
        {
            var types = serviceCollection
                .RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(GetType()))
                .Where(type =>
                {
                    return validTypes.Any(type1 => type1.IsAssignableFrom(type)) && !type.IsAbstract &&
                           type.IsClass;
                });

            foreach (var type in types.TypesToConsider)
            {
                Console.WriteLine($"Registering {type.FullName}");
            }

            types.AsPublicImplementedInterfaces();
        }

        private IEnumerable<T> GetInstancesOfTypeInOurAssembly<T>() where T : class
        {
            return Assembly.GetAssembly(GetType()).GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(type => (T) Activator.CreateInstance(type, new object[0]));
        }
    }
}