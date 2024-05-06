using Microsoft.AspNetCore.Authorization;
using MyAuthorizationDemo.Application.Common.Interfaces;

namespace MyAuthorizationDemo.Infrastructure.Authorization;

public class CustomClaimAuthorizationHandler : AuthorizationHandler<CustomClaimRequirement>
{
    private readonly ClaimService _claimService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CustomClaimAuthorizationHandler(ClaimService claimService, IApplicationDbContext context,
        IUser user)
    {
        _claimService = claimService;
        _context = context;
        _user = user;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CustomClaimRequirement requirement)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            context.Fail();
            return;
        }

        // switch (requirement.ClaimType)
        // {
        //     case SystemClaim.RmHead:
        //         var isRmHead =
        //             await _context.UserInHeadPositions.AnyAsync(h => h.Email == _currentUserService.Email);
        //         if (isRmHead)
        //         {
        //             context.Succeed(requirement);
        //         }
        //
        //         return;
        //     default:
        //         var userClaims = await _claimService.GetClaimsAsync(_currentUserService.UserId);
        //         if (userClaims.Any(c => c.Type == requirement.ClaimType))
        //         {
        //             context.Succeed(requirement);
        //         }
        //
        //         return;
        // }
    }
}
