namespace RedisCache;

public interface INotificationHandlerSemaphore
{
    Task<bool> WaitAsync(CancellationToken cancellationToken = default);
    Task ReleaseAsync(CancellationToken cancellationToken = default);

}
