using Microsoft.AspNetCore.Identity;
using MyAuthorizationDemo.Domain.Entities;

namespace MyAuthorizationDemo.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<ApplicationUser> Users { get; }
    DbSet<IdentityUserRole<string>> UserRoles { get; }
    DbSet<IdentityRole> Roles { get; }
    DbSet<IdentityUserClaim<string>> UserClaims { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
