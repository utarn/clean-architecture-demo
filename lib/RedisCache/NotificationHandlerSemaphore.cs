using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace RedisCache;

public class NotificationHandlerSemaphore : INotificationHandlerSemaphore
{
    private readonly ILogger<NotificationHandlerSemaphore> _logger;
    private readonly string _key;
    private readonly int _maxCount;
    private readonly IDistributedCache _cache;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly Random Random = new Random();
    
    private readonly DistributedCacheEntryOptions CacheEntryOptions = new DistributedCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromDays(30)
    };

    public NotificationHandlerSemaphore(ILogger<NotificationHandlerSemaphore> logger, IDistributedCache cache,
        string key, int maxCount = 1)
    {
        _logger = logger;
        _key = key;
        _cache = cache;
        _maxCount = maxCount;
    }

    public async Task<bool> WaitAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _semaphore.WaitAsync(cancellationToken);
            var time = new Stopwatch();
            time.Start();
            var currentCount = int.Parse(await _cache.GetStringAsync(_key, token: cancellationToken) ?? "0");
            if (currentCount < _maxCount)
            {
                // _logger.LogInformation("Lock acquired for accessing. New count {Count}", currentCount + 1);
                await _cache.SetStringAsync(_key, (currentCount + 1).ToString(), CacheEntryOptions, cancellationToken);
                _semaphore.Release();
                time.Stop();
                // _logger.LogInformation("Lock released. Time elapsed {TimeElapsed}", time.ElapsedMilliseconds);
                return true;
            }

            _semaphore.Release();
            time.Stop();
            // _logger.LogInformation("Lock released. Time elapsed {TimeElapsed}", time.ElapsedMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(300, 1001)), cancellationToken);
        }

        _semaphore.Release();
        return true;
    }

    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        // _logger.LogInformation("Waiting for Releasing lock");
        await _semaphore.WaitAsync(cancellationToken);
        // _logger.LogInformation("Lock acquired for releasing");
        var currentCount = int.Parse(await _cache.GetStringAsync(_key, token: cancellationToken) ?? "0");
        var newValue = Math.Max(currentCount - 1, 0);
        // _logger.LogInformation("Releasing lock. New count {Count}", newValue);
        await _cache.SetStringAsync(_key, newValue.ToString(), CacheEntryOptions, cancellationToken);
        _semaphore.Release();
        // _logger.LogInformation("Released lock");
    }
}
