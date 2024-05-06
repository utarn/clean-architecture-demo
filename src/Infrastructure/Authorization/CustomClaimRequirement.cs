using Microsoft.AspNetCore.Authorization;

namespace MyAuthorizationDemo.Infrastructure.Authorization;

public class CustomClaimRequirement : IAuthorizationRequirement
{
    public string ClaimType { get; }

    public CustomClaimRequirement(string claimType)
    {
        ClaimType = claimType;
    }
}
