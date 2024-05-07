using Autofac;
using bms.Leaf.Common;
using bms.Leaf.Extensions;
using bms.Leaf.Snowflake;
using bms.Leaf.SnowFlake;
using FreeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace bms.Leaf.RedisHolder
{
    public static class Extension
    {
        public static void AddRedisHolder(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var option = configuration.GetOptions<RedisHolderOption>("redisholder");
                if (option != null)
                {
                    option.HttpPort = configuration["httpport"];
                }
                return option;
            }).SingleInstance();

            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var option = context.Resolve<RedisHolderOption>();
                string ip = option.Ip;
                if (string.IsNullOrEmpty(ip))
                {
                    ip = Utils.GetIp(option.Interface);
                }
                var logger = context.Resolve<ILogger<SnowflakeRedisHolder>>();
                var redisClient = context.Resolve<IRedisClient>();
                return new SnowflakeRedisHolder(logger, redisClient, ip, option.HttpPort);
            }).As<ISnowflakeRedisHolder>().SingleInstance();
        }
    }
}
