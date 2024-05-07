using Autofac;
using bms.Leaf.Extensions;
using FreeRedis;
using Microsoft.Extensions.Configuration;

namespace bms.Leaf.Redis
{
    public static class Extension
    {
        public static void AddRedis(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var option = configuration.GetOptions<RedisOption>("redis");

                return option;
            }).SingleInstance();

            builder.Register(context =>
            {
                var option = context.Resolve<RedisOption>();

                return new RedisClient(option.ConnectionString);
            }).As<IRedisClient>().SingleInstance();
        }
    }
}
