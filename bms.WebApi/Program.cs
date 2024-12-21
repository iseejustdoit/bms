using Autofac;
using Autofac.Extensions.DependencyInjection;
using bms.Leaf.Initializer;
using bms.Leaf.Kestrel;
using bms.Leaf.Logging;
using bms.Leaf.PostgreSQL;
using bms.Leaf.Redis;
using bms.Leaf.RedisHolder;
using bms.Leaf.Swagger;
using bms.WebApi.Services;

namespace bms.WebApi
{
    /// <summary>
    /// 应用程序的主要入口点。
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 启动应用程序的主方法。
        /// </summary>
        /// <param name="args">命令行参数。</param>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            // 将服务添加到容器中
            builder.Services.AddGrpc();
            builder.Services.AddControllers();
            // 了解有关在 https://aka.ms/aspnetcore/swashbuckle 配置 Swagger/OpenAPI 的更多信息
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerDocs();
            builder.Services.AddPostgreSQL();
            builder.Services.AddInitializers(typeof(IIDGenInitializer));
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.AddIdGen();
                builder.AddRedis();
                builder.AddRedisHolder();
                builder.AddInititalizer();
            });

            builder.WebHost.AddKestrel();
            builder.Host.UseSerilog();
            var app = builder.Build();
            var initializer = app.Services.GetService<IStartupInitializer>();
            if (initializer != null)
            {
                await initializer.InitializeAsync();
            }

            // 配置 HTTP 请求管道
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerDocs();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.MapGrpcService<IdGeneratorService>();

            app.Run();
        }
    }
}
