using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace bms.Leaf.Initializer
{
    public static class Extension
    {
        public static IServiceCollection AddInitializers(this IServiceCollection services, params Type[] initializers)
           => initializers == null
               ? services
               : services.AddTransient<IStartupInitializer, StartupInitializer>(c =>
               {
                   var startupInitializer = new StartupInitializer();
                   var validInitializers = initializers.Where(t => typeof(IInitializer).IsAssignableFrom(t));
                   foreach (var initializer in validInitializers)
                   {
                       if (c.GetService(initializer) is IInitializer service)
                       {
                           startupInitializer.AddInitializer(service);
                       }
                   }

                   return startupInitializer;
               });

        public static void AddInititalizer(this ContainerBuilder builder)
        {
            builder.RegisterType<IDGenInitializer>()
                .As<IIDGenInitializer>()
                .InstancePerLifetimeScope();
        }

        public static void AddIdGen(this ContainerBuilder builder)
        {
            var idGenAssembly = Assembly.GetAssembly(typeof(IIDGen));
            if (idGenAssembly != null)
            {
                var idGenTypes = idGenAssembly
                    .GetTypes()
                    .Where(p => p.IsClass && !p.IsAbstract && typeof(IIDGen).IsAssignableFrom(p));
                foreach (var idGenType in idGenTypes)
                {
                    builder.RegisterType(idGenType)
                        .As<IIDGen>()
                        .SingleInstance();
                }
            }
        }
    }
}
