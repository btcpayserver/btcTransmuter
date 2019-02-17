using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using ExtCore.Infrastructure;
using ExtCore.Infrastructure.Actions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.AutoRegisterDi;

namespace BtcTransmuter.Abstractions.Extensions
{
    public abstract class BtcTransmuterExtension : ExtensionBase, IConfigureAction, IConfigureServicesAction
    {
        int IConfigureServicesAction.Priority => Priority;

        int IConfigureAction.Priority => Priority;
        protected abstract int Priority { get; }
        public abstract string[] Scripts { get; }
        public abstract string[] Stylesheets { get; }

        public IEnumerable<IActionDescriptor> Actions => GetInstancesOfTypeInOurAssembly<IActionDescriptor>();
        public IEnumerable<ITriggerDescriptor> Triggers => GetInstancesOfTypeInOurAssembly<ITriggerDescriptor>();

        public IEnumerable<IExternalServiceDescriptor> ExternalServices =>
            GetInstancesOfTypeInOurAssembly<IExternalServiceDescriptor>();


        public virtual void Execute(IApplicationBuilder applicationBuilder, IServiceProvider serviceProvider)
        {
        }

        public virtual void Execute(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
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
            return ExtensionManager.GetInstances<T>(assembly => assembly == Assembly.GetAssembly(GetType()));
        }
    }
}