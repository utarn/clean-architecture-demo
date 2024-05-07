using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using MyAuthorizationDemo.Application.TodoLists.Commands.CreateTodoList;
using MyAuthorizationDemo.Application.TodoLists.Commands.DeleteTodoList;
using MyAuthorizationDemo.Application.TodoLists.Commands.UpdateTodoList;
using MyAuthorizationDemo.Application.TodoLists.Queries.GetTodos;
using MyAuthorizationDemo.Domain.Constants;
using RedisCache;

namespace MyAuthorizationDemo.Web.Endpoints;

public class TodoLists : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetTodoLists)
            .MapPut(UpdateTodoList, "{id}")
            .MapDelete(DeleteTodoList, "{id}");

        app.MapGroup(this)
            .RequireAuthorization(new AuthorizeAttribute( Policies.CanEdit)
            {
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            })
            .MapPost(CreateTodoList)
            ;

        app.MapGroup("/api/cache")
            .MapGet(ClearCache, "/clear");
    }

    public Task<TodosVm> GetTodoLists(ISender sender)
    {
        return  sender.Send(new GetTodosQuery());
    }

    public async Task<int> CreateTodoList(ISender sender, CreateTodoListCommand command)
    {
        return await sender.Send(command);
    }

    public async Task<IResult> ClearCache(ICacheManager cacheManager)
    {
        await cacheManager.RemovePattern("allActiveClaims:*");
        return Results.StatusCode(600); 
    }

    public async Task<IResult> UpdateTodoList(ISender sender, int id, UpdateTodoListCommand command)
    {
        if (id != command.Id) return Results.BadRequest();
        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> DeleteTodoList(ISender sender, int id)
    {
        await sender.Send(new DeleteTodoListCommand(id));
        return Results.NoContent();
    }
}
