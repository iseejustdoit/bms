using Autofac;
using bms.Leaf.Entity;
using bms.Leaf.Extensions;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace bms.Leaf.MySQL
{
    public static class Extension
    {
        public static void AddMySQL(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var option = configuration.GetOptions<MySQLOption>("mysql");

                return option;
            }).SingleInstance();

            builder.Register(context =>
            {
                var option = context.Resolve<MySQLOption>();
                var optionsBuilder = new DbContextOptionsBuilder<LeafContext>();
                ServerVersion serverVersion;
                if (string.IsNullOrEmpty(option.Version))
                {
                    serverVersion = ServerVersion.AutoDetect(option.ConnectionString);
                }
                else
                    serverVersion = ServerVersion.Parse(option.Version);
                optionsBuilder.UseMySql(option.ConnectionString, serverVersion);
                return new LeafContext(optionsBuilder.Options);
            }).InstancePerLifetimeScope();

            builder.RegisterType<AllocDALImpl>()
                .As<IAllocDAL>()
                .InstancePerLifetimeScope();
        }
    }
}
