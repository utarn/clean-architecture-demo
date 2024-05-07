using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyAuthorizationDemo.Application.Common.Interfaces;

namespace MyAuthorizationDemo.Infrastructure.Authorization;

public class OneOfPoliciesRequirement : IAuthorizationRequirement
{
    public string[] Policies { get; private set; }

    public OneOfPoliciesRequirement(params string[] policies)
    {
        Policies = policies;
    }
}

public class OneOfPoliciesAuthorizationHandler(IServiceProvider serviceProvider)
    : AuthorizationHandler<OneOfPoliciesRequirement>
{
    private readonly ClaimService _claimService;
    private readonly IUser _user;

    public OneOfPoliciesAuthorizationHandler(IServiceProvider serviceProvider, ClaimService claimService, IUser user) :
        this(serviceProvider)
    {
        _claimService = claimService;
        _user = user;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OneOfPoliciesRequirement requirement)
    {
        if (_user.Id == null)
        {
            context.Fail();
            return;
        }
        
        var userClaims = await _claimService.GetClaimsAsync(_user.Id);
        if (userClaims.Any(c => requirement.Policies.Contains(c.Type)))
        {
            context.Succeed(requirement);
            return;
        }
    }
}
