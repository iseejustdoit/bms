using Autofac;
using Microsoft.Extensions.DependencyInjection;

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
                       startupInitializer.AddInitializer(c.GetService(initializer) as IInitializer);
                   }

                   return startupInitializer;
               });

        public static void AddInititalizer(this ContainerBuilder builder)
        {
            builder.RegisterType<IDGenInitializer>()
                .As<IIDGenInitializer>()
                .InstancePerLifetimeScope();
        }
    }
}
