using bms.Leaf.Common;
using bms.Leaf.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

namespace bms.Leaf.Kestrel
{
    public static class Extension
    {
        public static IWebHostBuilder AddKestrel(this IWebHostBuilder builder)
        {
            builder.ConfigureKestrel((context, options) =>
            {
                var portOption = context.Configuration.GetOptions<PortOption>("port");
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
            return builder;
        }
    }
}
