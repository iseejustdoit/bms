using bms.Leaf.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace bms.Leaf.Logging
{
    public static class Extension
    {
        public static void UseSerilog(this IHostBuilder builder, string? applicationName = null)
        {
            builder.UseSerilog((context, loggerConfiguration) =>
            {
                var appOption = context.Configuration.GetOptions<AppOption>("app");
                var serilogOption = context.Configuration.GetOptions<SerilogOption>("serilog");
                if (!Enum.TryParse<LogEventLevel>(serilogOption.Level, true, out var level))
                {
                    level = LogEventLevel.Information;
                }

                applicationName = string.IsNullOrWhiteSpace(applicationName) ? appOption.Name : applicationName;
                loggerConfiguration.Enrich.FromLogContext()
                    .MinimumLevel.Is(level)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("ApplicationName", applicationName);

                Configure(loggerConfiguration, serilogOption);
            });
        }

        private static void Configure(LoggerConfiguration loggerConfiguration, SerilogOption serilogOption)
        {
            if (serilogOption.ConsoleEnabled)
            {
                loggerConfiguration.WriteTo.Console();
            }
        }
    }
}
