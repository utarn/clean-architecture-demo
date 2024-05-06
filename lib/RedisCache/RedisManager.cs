using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisCache;

internal class RedisManager : ICacheManager
{
    private readonly ConfigurationOptions _options;
    private IDatabase? _database;
    private bool? _isConnected;
    private IServer? _server;

    public RedisManager(IConfiguration configuration)
    {
        _isConnected = false;
        _options = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis") ??
                                              throw new InvalidOperationException());
    }

    public static string? InstanceName { get; set; }

    public async Task ConnectAsync()
    {
        ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(_options);
        EndPoint endPoint = connection.GetEndPoints().First();
        _server = connection.GetServer(endPoint);
        _database = connection.GetDatabase();
        _isConnected = true;
    }

    public async Task RemovePattern(string pattern)
    {
        if (!(_isConnected ?? false))
        {
            await ConnectAsync();
        }

        if (InstanceName != null)
        {
            pattern = InstanceName + pattern;
        }

        if (_server != null && _database != null)
        {
            RedisKey[] keys = _server.Keys(pattern: pattern).ToArray();
            await _database.KeyDeleteAsync(keys);
        }
    }
}