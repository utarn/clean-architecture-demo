using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MyAuthorizationDemo.Application.Common.Interfaces;
using RedisCache;

namespace MyAuthorizationDemo.Infrastructure.Authorization;

public class ClaimService
{
    private readonly IDistributedCache _cache;
    private readonly IApplicationDbContext _dbContext;

    public ClaimService(IDistributedCache cache, IApplicationDbContext dbContext)
    {
        _cache = cache;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Claim>> GetClaimsAsync(string userId)
    {
        var cachedClaims = await _cache.GetObjectAsync<IEnumerable<Claim>>($"allActiveClaims:{userId}");
        if (cachedClaims != null)
        {
            return cachedClaims;
        }
        else
        {
            var claims = await _dbContext.UserClaims
                .Where(u => u.UserId == userId 
                            && u.ClaimType != null && u.ClaimValue != null)
                .Select(c => new Claim(c.ClaimType!, c.ClaimValue!))
                .ToListAsync();

            
            await _cache.SetObjectAsync($"allActiveClaims:{userId}", claims);
            return claims;
        }
    }
}
