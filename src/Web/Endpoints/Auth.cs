using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAuthorizationDemo.Application.Auth.Commands.AuthenticateCommand;
using MyAuthorizationDemo.Application.Common.Interfaces;

namespace MyAuthorizationDemo.Web.Endpoints;

public class Auth : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup("/api/auth")
            .MapPost(Authenticate, "/authenticate")
            ;

        app.MapGroup("/api/auth")
            .RequireAuthorization(new AuthorizeAttribute()
            {
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            })
            .MapGet(Verify, "/verify");

    }
    
    private async Task<IResult> Authenticate(ISender sender, AuthenticateCommand command)
    {
        var token = await sender.Send(command);
        return Results.Ok(token);
    }
    
    private async Task<IResult> Verify(IUser currentUser)
    {
        var data = currentUser.Id + ":" + currentUser.Name + ":" + currentUser.Email;
        return Results.Ok(data);
    }
}
