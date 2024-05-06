using MyAuthorizationDemo.Domain.Entities;
using MyAuthorizationDemo.Infrastructure.Identity;

namespace MyAuthorizationDemo.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<ApplicationUser>();
    }
}
