using Autofac;
using Autofac.Extensions.DependencyInjection;
using bms.Leaf;
using bms.Leaf.Common;
using bms.Leaf.Initializer;
using bms.Leaf.MySQL;
using bms.Leaf.Redis;
using bms.Leaf.RedisHolder;

namespace bms.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
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
            builder.Services.AddInitializers(typeof(IIDGenInitializer));
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.AddIdGen();
                builder.AddMySQL();
                builder.AddRedis();
                builder.AddRedisHolder();
                builder.AddInititalizer();
            });

            var app = builder.Build();
            var initializer = app.Services.GetService<IStartupInitializer>();
            if (initializer != null)
            {
                await initializer.InitializeAsync();
            }

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
