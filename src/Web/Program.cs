using System.Net;
using System.Security.Claims;
using MyAuthorizationDemo.Infrastructure;
using MyAuthorizationDemo.Infrastructure.Data;
using MyAuthorizationDemo.Web;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureSerilog();
builder.WebHost.UseKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1073741824; // 1GB
});
builder.Services.Configure<HostOptions>(hostOptions =>
{
    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// Add services to the container.
builder.Services.AddKeyVaultIfConfigured(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.Use((context, next) =>
{
    if (context.Request.Headers["x-forwarded-proto"] == "https")
    {
        context.Request.Scheme = "https";
    }

    return next();
});
app.UseHttpsRedirection();
app.UseSerilogRequestLogging(
    options =>
    {
        options.MessageTemplate =
            "{RemoteIpAddress} {RequestScheme} {RequestHost} {RequestMethod} {RequestPath} {UserName} {Email} {Role} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (
            diagnosticContext,
            httpContext) =>
        {
            string? header =
                (httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                 httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()) ??
                httpContext.Connection.RemoteIpAddress?.ToString();
            if (IPAddress.TryParse(header, out IPAddress? ip))
            {
                diagnosticContext.Set("RemoteIpAddress", ip);
            }

            diagnosticContext.Set("Email", httpContext.User.FindFirstValue(ClaimTypes.Email));
            diagnosticContext.Set("UserName",
                httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-");
            diagnosticContext.Set("Role", httpContext.User.FindFirstValue(ClaimTypes.Role) ?? "-");
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });
app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.UseExceptionHandler(options => { });

// app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

app.Run();

public partial class Program { }
