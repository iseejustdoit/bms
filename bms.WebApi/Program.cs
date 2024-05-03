
using bms.Leaf;
using bms.Leaf.Common;
using bms.Leaf.Segment;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using bms.Leaf.Snowflake;
using bms.Leaf.SnowFlake;
using bms.WebApi.Service;

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
            builder.Services.AddScoped<IAllocDAL>((provider) =>
            {
                var connectionString = configuration.GetConnectionString("MySql");
                return new AllocDALImpl(connectionString);
            });
            builder.Services.AddSingleton<ISnowflakeRedisHolder>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SnowflakeRedisHolder>>();
                var connectionString = configuration.GetConnectionString("Redis");

                return new SnowflakeRedisHolder(logger, Utils.GetIp(), httpPort, connectionString);
            });

            builder.Services.AddScoped((provider) =>
            {
                var logger = provider.GetRequiredService<ILogger<SegmentIDGenImpl>>();
                var allocDal = provider.GetRequiredService<IAllocDAL>();
                return new SegmentIDGenImpl(logger, allocDal);
            });
            builder.Services.AddScoped<SnowflakeIDGenImpl>();
            builder.Services.AddScoped<ZeroIDGen>();
            builder.Services.AddSingleton<ServiceFactory>();

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
