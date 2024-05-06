using System.Threading.Tasks;

namespace RedisCache;

public interface ICacheManager
{
    Task ConnectAsync();
    Task RemovePattern(string pattern);
}