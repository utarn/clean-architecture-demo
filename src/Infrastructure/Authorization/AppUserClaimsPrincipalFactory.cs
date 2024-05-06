using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyAuthorizationDemo.Domain.Entities;
using MyAuthorizationDemo.Infrastructure.Identity;

namespace MyAuthorizationDemo.Infrastructure.Authorization;

public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public AppUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        // if (!string.IsNullOrEmpty(user.CitizenId))
        // {
        //     identity.AddClaim(new Claim("citizen_id", user.CitizenId));
        // }
        return identity;
    }
}
