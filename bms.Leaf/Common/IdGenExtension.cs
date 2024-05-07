using Autofac;
using System.Reflection;

namespace bms.Leaf.Common
{
    public static class IdGenExtension
    {
        public static void AddIdGen(this ContainerBuilder builder)
        {
            var idGenTypes = Assembly.GetAssembly(typeof(IIDGen))
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
