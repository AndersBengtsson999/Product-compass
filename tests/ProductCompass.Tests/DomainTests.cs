using ProductCompass.Domain.Workspaces;
using ProductCompass.Application.Workspaces;

namespace ProductCompass.Tests;
public sealed class DomainTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Names_cannot_be_blank(string? name) =>
        Assert.Throws<ArgumentException>(() => new Organization(Guid.NewGuid(), name!));

    [Fact]
    public void Names_are_trimmed_and_length_is_bounded()
    {
        Assert.Equal("Acme", new Organization(Guid.NewGuid(), "  Acme  ").Name);
        Assert.Throws<ArgumentException>(() => new Workspace(Guid.NewGuid(), Guid.NewGuid(), new string('x', 121)));
    }

    [Fact]
    public void Entities_require_organization_scope()
    {
        Assert.Throws<ArgumentException>(() => new Organization(Guid.Empty, "Acme"));
        Assert.Throws<ArgumentException>(() => new Workspace(Guid.NewGuid(), Guid.Empty, "Product"));
        Assert.Throws<ArgumentException>(() => new Membership(Guid.Empty, "user", "issuer"));
    }

    [Theory]
    [InlineData("", "issuer")]
    [InlineData("user", "")]
    public async Task Unverified_identity_never_reaches_persistence(string subject, string issuer)
    {
        var repository = new NeverReadRepository();
        var service = new WorkspaceService(repository);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetOverviewAsync(Guid.NewGuid(), new(subject, issuer)));
        Assert.False(repository.WasCalled);
    }
    [Fact]
    public async Task Empty_scope_never_reaches_persistence()
    {
        var repository = new NeverReadRepository();
        await Assert.ThrowsAsync<ArgumentException>(() => new WorkspaceService(repository).GetOverviewAsync(Guid.Empty, new("user", "issuer")));
        Assert.False(repository.WasCalled);
    }
    private sealed class NeverReadRepository : IWorkspaceRepository
    {
        public bool WasCalled { get; private set; }
        public Task<WorkspaceOverview?> GetForMemberAsync(Guid organizationId, UserIdentity user, CancellationToken ct)
        { WasCalled = true; return Task.FromResult<WorkspaceOverview?>(null); }
    }
}
