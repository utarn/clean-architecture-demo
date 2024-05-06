using System.Reflection;
using MyAuthorizationDemo.Application.Common.Interfaces;
using MyAuthorizationDemo.Domain.Entities;
using MyAuthorizationDemo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyAuthorizationDemo.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

//  dotnet ef migrations add Initial -p Infrastructure/ -s Web/ -c ApplicationDbContext -o Data/Migrations
//  dotnet ef database update -p Infrastructure/ -s Web/ -c ApplicationDbContext

// dotnet ef dbcontext scaffold "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=vrm;" Npgsql.EntityFrameworkCore.PostgreSQL -o Test -c ApplicationDbContext2 -f
