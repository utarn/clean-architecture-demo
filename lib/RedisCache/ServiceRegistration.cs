using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RedisCache;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterRedisService(this IServiceCollection services, string connectionString,
        string instanceName = "redis")
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = instanceName;
        });
        RedisManager.InstanceName = instanceName;
        services.AddSingleton<ICacheManager, RedisManager>();

        services.AddHealthChecks()
            .AddRedis(connectionString, "cache");
        return services;
    }

    public static IServiceCollection RegisterSignalRRedisBackPlaneService(this IServiceCollection services,
        string connectionString, string instanceName = "redis", IMessagePackFormatter[]? formatters = null)
    {
        services.AddHealthChecks()
            .AddRedis(connectionString, "signalr");

        if (formatters == null)
        {
            services.AddSignalR()
                .AddMessagePackProtocol(options =>
                {
                    options.SerializerOptions = MessagePackSerializerOptions.Standard
                        .WithSecurity(MessagePackSecurity.TrustedData);
                })
                .AddStackExchangeRedis(connectionString, options =>
                {
                    options.Configuration.ChannelPrefix = new RedisChannel(instanceName, RedisChannel.PatternMode.Auto);
                });
        }
        else
        {
            var resolver = CompositeResolver.Create(formatters, new[] { StandardResolver.Instance });
            var resolverOptions = MessagePackSerializerOptions.Standard
                .WithResolver(resolver);
            services.AddSignalR()
                .AddMessagePackProtocol(options =>
                {
                    options.SerializerOptions = resolverOptions;
                })
                .AddStackExchangeRedis(connectionString, options =>
                {
                    options.Configuration.ChannelPrefix = new RedisChannel(instanceName, RedisChannel.PatternMode.Auto);
                });
        }

        return services;
    }

    public static void AddHangfireSemaphore(this IServiceCollection services, string key, int limit)
    {
        services.AddKeyedSingleton<INotificationHandlerSemaphore>(key, (provider, _) =>
        {
            var cache = provider.GetRequiredService<IDistributedCache>();
            var logger = provider.GetRequiredService<ILogger<NotificationHandlerSemaphore>>();
            return new NotificationHandlerSemaphore(logger, cache, key, limit);
        });
    }
}
