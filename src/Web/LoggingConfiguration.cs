using Serilog;
using Serilog.Events;

namespace MyAuthorizationDemo.Web;

internal static class LoggingConfiguration
{
    internal static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            var serilogSection = context.Configuration.GetSection("SerilogSetting").GetChildren().ToList();
            var seqHost =
                serilogSection.FirstOrDefault(s => s.Key.ToLower() == "hostname")?.Value ?? (
                    context.HostingEnvironment.IsProduction()
                        ? "seq"
                        : "localhost");
            if (!seqHost.StartsWith("http"))
            {
                seqHost = $"http://{seqHost}";
            }

            var seqPort = serilogSection.FirstOrDefault(s => s.Key.ToLower() == "port")?.Value ?? "80";
            var hostname = $"{seqHost}:{seqPort}";
            var apiKey = serilogSection.FirstOrDefault(s => s.Key.ToLower() == "apikey")?.Value ?? "";
            var appName = serilogSection.FirstOrDefault(s => s.Key.ToLower() == "appname")?.Value ?? "Trusty";

#if DEBUG
            configuration
                // .ReadFrom.Configuration(context.Configuration)
                // .ReadFrom.Services(services)
                .MinimumLevel.Information()
                .MinimumLevel.Override("MyAuthorizationDemo.Application", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("MyAuthorizationDemo.Infrastructure", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("Utharn.Library", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("NpgSql", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("MyAuthorizationDemo.Infrastructure.Services", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
                    Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", appName)
                .Destructure.ToMaximumDepth(4)
                .Destructure.ToMaximumStringLength(512)
                .Destructure.ToMaximumCollectionCount(10)
                .WriteTo.Seq(serverUrl: hostname, apiKey: apiKey)
                .WriteTo.Async(configure => configure.Console());
#else
        configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("MyAuthorizationDemo.Infrastructure", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Utharn.Library", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("NpgSql", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("MyAuthorizationDemo.Infrastructure.Services", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", appName)
        .Destructure.ToMaximumDepth(4)
        .Destructure.ToMaximumStringLength(512)
        .Destructure.ToMaximumCollectionCount(10)
        .WriteTo.Seq(serverUrl: hostname, apiKey: apiKey)
        .WriteTo.Async(configure => configure.Console());

#endif
            // var lineNotifyToken = serilogSection.FirstOrDefault(s => s.Key.ToLower() == "linenotifytoken")?.Value ?? "";
            //
            // if (!string.IsNullOrEmpty(lineNotifyToken))
            // {
            //     var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            //     var lineNotifySink =
            //         new LineNotifySink(httpClientFactory, appName, lineNotifyToken);
            //     configuration.WriteTo.Sink(lineNotifySink);
            // }

            if (context.HostingEnvironment.EnvironmentName.ToLower().Contains("Staging"))
            {
                configuration.MinimumLevel.Override("Utharn.Library", LogEventLevel.Information);
            }
        });
    }
}
