using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using ProductCompass.Application.Workspaces;

namespace ProductCompass.Tests;
public sealed class ApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;
    public ApiTests(ApiFactory factory) => client = factory.CreateClient();
    private const string Route = "/api/organizations/11111111-1111-1111-1111-111111111111/workspace";
    [Fact]
    public async Task Anonymous_and_invalid_tokens_are_rejected()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Route)).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", "not-a-token");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Route)).StatusCode);
    }
    [Fact]
    public async Task Members_can_read_but_other_organizations_are_forbidden()
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", ApiFactory.Token("developer"));
        var response = await client.GetAsync(Route);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Acme", await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(Route.Replace("11111111", "33333333"))).StatusCode);
    }
    [Fact]
    public async Task Non_member_is_forbidden()
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", ApiFactory.Token("outsider"));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(Route)).StatusCode);
    }
    [Fact]
    public async Task Expired_tokens_are_rejected()
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", ApiFactory.Token("developer", expired: true));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Route)).StatusCode);
    }
    [Fact]
    public async Task Invalid_scope_returns_validation_problem()
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", ApiFactory.Token("developer"));
        var response = await client.GetAsync("/api/organizations/not-a-guid/workspace");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("organizationId", await response.Content.ReadAsStringAsync());
    }
    [Fact]
    public async Task Failures_do_not_expose_internal_details()
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", ApiFactory.Token("throw"));
        var response = await client.GetAsync(Route);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("private-database-password", body);
        Assert.Contains("traceId", body);
    }
    [Fact]
    public async Task Liveness_is_public() => Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    // Test fixture only; never used by the running application.
    private static readonly SymmetricSecurityKey Key = new(Encoding.UTF8.GetBytes("test-only-signing-key-at-least-thirty-two-bytes-long"));
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        { ["ConnectionStrings:Database"] = "Host=localhost;Database=unused" }));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IWorkspaceRepository>();
            services.AddSingleton<IWorkspaceRepository, TestRepository>();
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, ValidIssuer = "test-issuer", ValidateAudience = true, ValidAudience = "test-audience",
                    ValidateLifetime = true, ClockSkew = TimeSpan.Zero, ValidateIssuerSigningKey = true, IssuerSigningKey = Key
                };
            });
        });
    }
    public static string Token(string subject, bool expired = false) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        issuer: "test-issuer", audience: "test-audience", claims: [new Claim("sub", subject)],
        notBefore: DateTime.UtcNow.AddHours(-2), expires: expired ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddMinutes(5),
        signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)));
    private sealed class TestRepository : IWorkspaceRepository
    {
        public Task<WorkspaceOverview?> GetForMemberAsync(Guid organizationId, UserIdentity user, CancellationToken ct)
        {
            if (user.Subject == "throw") throw new InvalidOperationException("private-database-password");
            return Task.FromResult(user.Subject == "developer" && organizationId == Guid.Parse("11111111-1111-1111-1111-111111111111")
                ? new WorkspaceOverview(organizationId, "Acme", Guid.NewGuid(), "Product workspace") : null);
        }
    }
}
