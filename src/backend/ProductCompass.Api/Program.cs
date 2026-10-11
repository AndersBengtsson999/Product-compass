using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using ProductCompass.Application.Workspaces;
using ProductCompass.Infrastructure.Persistence;
using ProductCompass.Infrastructure.Workspaces;
using ProductCompass.Domain.Workspaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<SafeExceptionHandler>();
builder.Services.AddDbContext<CompassDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("Database") is { Length: > 0 } connection
        ? connection : throw new InvalidOperationException("Configure ConnectionStrings:Database using user-secrets or an environment variable.")));
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
    if (!builder.Environment.IsDevelopment())
    {
        options.Authority = builder.Configuration["Authentication:Authority"]
            ?? throw new InvalidOperationException("Configure Authentication:Authority outside Development.");
        options.Audience = builder.Configuration["Authentication:Audience"]
            ?? throw new InvalidOperationException("Configure Authentication:Audience outside Development.");
        options.RequireHttpsMetadata = true;
    }
});
builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(
    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

// Explicit CLI setup only; schema changes and demo memberships never run on ordinary startup.
if (args.Contains("--seed-development"))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Development seeding is disabled outside Development.");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CompassDbContext>();
    await db.Database.MigrateAsync();
    var organizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    if (!await db.Organizations.AnyAsync(x => x.Id == organizationId))
    {
        db.Organizations.Add(new Organization(organizationId, "Acme Studio"));
        db.Workspaces.Add(new Workspace(Guid.Parse("22222222-2222-2222-2222-222222222222"), organizationId, "Product workspace"));
        db.Memberships.Add(new Membership(organizationId, "developer", "dotnet-user-jwts"));
        await db.SaveChangesAsync();
    }
    return;
}

app.MapGet("/health/live", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapGet("/health/ready", async (CompassDbContext db, CancellationToken ct) =>
{
    try
    {
        var ready = await db.Database.CanConnectAsync(ct)
            && !(await db.Database.GetPendingMigrationsAsync(ct)).Any();
        return ready ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
    }
    catch (Exception) when (!ct.IsCancellationRequested) { return Results.StatusCode(503); }
}).AllowAnonymous();

app.MapGet("/api/organizations/{organizationId}/workspace", async (
    string organizationId, ClaimsPrincipal principal, WorkspaceService service, CancellationToken ct) =>
{
    if (!Guid.TryParse(organizationId, out var id) || id == Guid.Empty)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["organizationId"] = ["A non-empty organization UUID is required."] });
    var subject = principal.FindFirstValue("sub");
    var issuer = principal.FindFirstValue("iss");
    if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(issuer)) return Results.Forbid();
    var overview = await service.GetOverviewAsync(id, new UserIdentity(subject, issuer), ct);
    // Same response for a missing organization and a non-member: do not disclose its existence.
    return overview is null ? Results.Forbid() : Results.Ok(overview);
}).RequireAuthorization();
app.Run();

public partial class Program { }
