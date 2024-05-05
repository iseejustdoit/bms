using bms.Leaf.Common;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using bms.Leaf.Segment.Entity;
using bms.Leaf.Snowflake;
using bms.Leaf.SnowFlake;
using bms.WebApi.Service;
using Microsoft.EntityFrameworkCore;

namespace bms.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;
            var httpPort = configuration.GetValue<string>("HttpPort");
            // Add services to the container.
            builder.WebHost.UseUrls($"http://*:{httpPort}");
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddLogging((builder) =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var dbConnString = configuration.GetConnectionString("MySql");
            var serverVersion = ServerVersion.AutoDetect(dbConnString);
            builder.Services.AddDbContext<LeafContext>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.UseMySql(dbConnString, serverVersion);
            });
            builder.Services.AddScoped<IAllocDAL, AllocDALImpl>();
            builder.Services.AddSingleton<ISnowflakeRedisHolder>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SnowflakeRedisHolder>>();
                var connectionString = configuration.GetConnectionString("Redis");

                return new SnowflakeRedisHolder(logger, Utils.GetIp(), httpPort, connectionString);
            });
            builder.Services.AddIdGen();
            // ×¢²á·þÎñ
            builder.Services.AddSingleton<IDGenFactory>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
