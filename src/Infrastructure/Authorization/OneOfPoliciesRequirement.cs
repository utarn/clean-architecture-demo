using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

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
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OneOfPoliciesRequirement requirement)
    {
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        foreach (var policy in requirement.Policies)
        {
            var policyResult = await authorizationService.AuthorizeAsync(context.User, policy);
            if (policyResult.Succeeded)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
