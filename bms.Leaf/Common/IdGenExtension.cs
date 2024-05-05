using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace bms.Leaf.Common
{
    public static class IdGenExtension
    {
        public static IServiceCollection AddIdGen(this IServiceCollection services)
        {
            var idGenTypes = Assembly.GetAssembly(typeof(IDGen))
                .GetTypes()
                .Where(p => p.IsClass && !p.IsAbstract && typeof(IDGen).IsAssignableFrom(p));
            foreach (var idGenType in idGenTypes)
            {
                services.AddScoped(idGenType);
            }
            return services;
        }
    }
}
