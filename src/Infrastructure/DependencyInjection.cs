using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyAuthorizationDemo.Application.Common.Interfaces;
using MyAuthorizationDemo.Domain.Constants;
using MyAuthorizationDemo.Domain.Entities;
using MyAuthorizationDemo.Infrastructure.Authorization;
using MyAuthorizationDemo.Infrastructure.Data;
using MyAuthorizationDemo.Infrastructure.Data.Interceptors;
using MyAuthorizationDemo.Infrastructure.Identity;
using RedisCache;

namespace MyAuthorizationDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddAuthentication();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        Guard.Against.Null(redisConnectionString, message: "Connection string 'Redis' not found.");
        services.RegisterRedisService(redisConnectionString);
        
        var secretKey = Encoding.UTF8.GetBytes("2bb80d537b1da3e38bd30361aa855686bde0eacd7162fef6a25fe97bf527a25b");
        services.AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            })
            // .AddJwtBearer("GoogleBearer", options =>
            // {
            //     options.RequireHttpsMetadata = false;
            //     options.SaveToken = true;
            //     options.TokenValidationParameters = new TokenValidationParameters
            //     {
            //         ValidateIssuerSigningKey = true,
            //         ValidIssuer = "https://accounts.google.com",
            //         ValidateIssuer = true,
            //         ValidateAudience = true,
            //         ValidAudience = googleAuthNSection["ClientId"],
            //         ValidateLifetime = true
            //     };
            // });
            ;
        
        services.AddAuthorizationBuilder();

        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
            // .AddApiEndpoints();

        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthorization(options =>
        {
            // options.AddPolicy("Sample", policy =>
            //     policy.RequireRole("Admin")
            //         .RequireRole("Moderator"));

            // options.AddPolicy("Sample", policy =>
            //     policy.RequireClaim("citizen_id"));

            // options.AddPolicy("Sample", policy =>
            //     policy.RequireClaim("head_of_department", "true", "True", "TRUE"));

            foreach( var (name, _) in Policies.Claims)
            {
                // options.AddPolicy(name, policy =>
                //     policy.RequireClaim(name, "true", "True", "TRUE"));
                
                options.AddPolicy(name, policy =>
                {
                    policy.Requirements.Add(new CustomClaimRequirement(name));
                    // policy.Requirements.Add();   
                });
                
                options.AddPolicy("AnyClaims", policy =>
                    policy.Requirements.Add(new OneOfPoliciesRequirement(Policies.CanEdit, Policies.CanDelete)));
            }
            
        });
        
        services.AddScoped<ClaimService>();
        services.AddScoped<IAuthorizationHandler, CustomClaimAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, OneOfPoliciesAuthorizationHandler>();
        
        return services;
    }
}
