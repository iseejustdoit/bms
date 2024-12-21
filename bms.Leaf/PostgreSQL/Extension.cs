using bms.Leaf.Entity;
using bms.Leaf.Extensions;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace bms.Leaf.PostgreSQL
{
    public static class Extension
    {
        public static void AddPostgreSQL(this IServiceCollection services)
        {
            IConfiguration? configuration;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }
            if (configuration == null)
            {
                throw new InvalidOperationException("IConfiguration service is not available.");
            }
            var option = configuration.GetOptions<PostgreSQLOption>("postgresql");

            var fsql = new FreeSqlBuilder()
                    .UseConnectionString(DataType.PostgreSQL, option.Leaf)
                    .Build();

            services.AddSingleton(fsql);
            services.AddFreeDbContext<LeafContext>(options => options.UseFreeSql(fsql));

            services.AddScoped<IAllocDAL, AllocDALImpl>();
        }
    }
}
