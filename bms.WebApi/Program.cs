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
    /// Ӧ�ó������Ҫ��ڵ㡣
    /// </summary>
    public class Program
    {
        /// <summary>
        /// ����Ӧ�ó������������
        /// </summary>
        /// <param name="args">�����в�����</param>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            // ��������ӵ�������
            builder.Services.AddGrpc();
            builder.Services.AddControllers();
            // �˽��й��� https://aka.ms/aspnetcore/swashbuckle ���� Swagger/OpenAPI �ĸ�����Ϣ
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

            // ���� HTTP ����ܵ�
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
