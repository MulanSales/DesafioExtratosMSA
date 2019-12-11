using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Statements.API.EventsHandlers
{
    public static class HostedEventServiceExtension
    {
        public static void RegisterEvents(this IServiceCollection service)
        {
            Assembly backgroundServices = Assembly.GetExecutingAssembly();
            MethodInfo method = typeof(ServiceCollectionHostedServiceExtensions).GetMethod("AddHostedService", new[] {typeof(IServiceCollection)});
            foreach(Type type in backgroundServices.GetTypes().Where(x => typeof(BackgroundService).IsAssignableFrom(x)))
            {
                MethodInfo generic = method.MakeGenericMethod(type);
                generic.Invoke(null, new object[] { service });
            }
        }
    }
}