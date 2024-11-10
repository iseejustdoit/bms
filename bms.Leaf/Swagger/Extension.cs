using bms.Leaf.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace bms.Leaf.Swagger
{
    public static class Extension
    {
        public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
        {
            SwaggerOption option;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>() ?? throw new InvalidOperationException("IConfiguration service is not available.");
                option = configuration.GetOptions<SwaggerOption>("swagger");
            }

            if (!option.Enabled)
            {
                return services;
            }

            return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(option.Name, new OpenApiInfo
                {
                    Title = option.Title,
                    Version = option.Version,
                    Description = "分布式ID生成服务"
                });
                // 为 Swagger JSON and UI设置xml文档注释路径
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "bms.WebApi.xml");
                c.IncludeXmlComments(xmlPath);
                if (option.IncludeSecurity)
                {
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });
                }
            });
        }

        public static IApplicationBuilder UseSwaggerDocs(this IApplicationBuilder builder)
        {
            var configuration = builder.ApplicationServices.GetService<IConfiguration>() ?? throw new InvalidOperationException("IConfiguration service is not available.");
            var option = configuration.GetOptions<SwaggerOption>("swagger");
            if (!option.Enabled)
            {
                return builder;
            }

            var routePrefix = string.IsNullOrWhiteSpace(option.RoutePrefix) ? "swagger" : option.RoutePrefix;

            builder.UseSwagger(c => c.RouteTemplate = routePrefix + "/{documentName}/swagger.json");

            return option.ReDocEnabled
                ? builder.UseReDoc(c =>
                {
                    c.RoutePrefix = routePrefix;
                    c.SpecUrl = $"{option.Name}/swagger.json";
                })
                : builder.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/{routePrefix}/{option.Name}/swagger.json", option.Title);
                    c.RoutePrefix = routePrefix;
                });
        }
    }
}
