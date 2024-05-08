using Autofac;
using Autofac.Extensions.DependencyInjection;
using bms.Leaf.Common;
using bms.Leaf.Extensions;
using bms.Leaf.Initializer;
using bms.Leaf.MySQL;
using bms.Leaf.Redis;
using bms.Leaf.RedisHolder;
using bms.WebApi.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

namespace bms.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            // Add services to the container.
            builder.Services.AddGrpc();
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

            builder.WebHost.ConfigureKestrel(options =>
            {
                var portOption = new PortOption();
                configuration.GetSection("port").Bind(portOption);
                // gRPC 服务
                options.Listen(IPAddress.Any, portOption.GrpcPort, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });

                // HTTP 服务
                options.Listen(IPAddress.Any, portOption.HttpPort, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
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

            app.MapGrpcService<IdGeneratorService>();

            app.Run();
        }
    }
}
