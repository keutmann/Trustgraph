using Microsoft.Practices.Unity;
using System.Linq;
using System.Reflection;
using TrustchainCore.IOC;

namespace TrustgraphServer
{
    public static class UnityContainerExtensions
    {
        public static void RegisterTypesFromAssemblies(this IUnityContainer container, Assembly[] assemblies)
        {
            container.RegisterTypes(
               AllClasses.FromAssemblies(assemblies),
               WithMappings.FromMatchingInterface,
               WithName.Default,
               (instanceType) => {
                   LifetimeManager result = null;
                   var ioc = instanceType.GetCustomAttributes(false).OfType<IOCAttribute>().FirstOrDefault();
                   if (ioc == null)
                       result = WithLifetime.Hierarchical(instanceType);
                   else
                       switch (ioc.LifeCycle)
                       {
                           case IOCLifeCycleType.Singleton: result = WithLifetime.ContainerControlled(instanceType); break;
                           case IOCLifeCycleType.PerThread: result = WithLifetime.PerThread(instanceType); break;
                           case IOCLifeCycleType.PerResolve: result = WithLifetime.PerResolve(instanceType); break;
                           case IOCLifeCycleType.PerRequest: result = WithLifetime.Hierarchical(instanceType); break;
                           default: result = WithLifetime.Hierarchical(instanceType); break;
                       }
                   return result;
               });

        }
    }
}
