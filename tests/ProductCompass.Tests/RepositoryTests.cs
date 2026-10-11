using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProductCompass.Domain.Workspaces;
using ProductCompass.Infrastructure.Persistence;
using ProductCompass.Infrastructure.Workspaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ProductCompass.Tests;
public sealed class RepositoryTests
{
    [Fact]
    public async Task Reads_require_matching_organization_subject_and_issuer()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new CompassDbContext(new DbContextOptionsBuilder<CompassDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var organizationA = Guid.NewGuid(); var organizationB = Guid.NewGuid();
        db.Organizations.AddRange(new Organization(organizationA, "A"), new Organization(organizationB, "B"));
        db.Workspaces.AddRange(new Workspace(Guid.NewGuid(), organizationA, "A workspace"), new Workspace(Guid.NewGuid(), organizationB, "B workspace"));
        db.Memberships.Add(new Membership(organizationA, "alice", "issuer-A"));
        await db.SaveChangesAsync();
        var repository = new WorkspaceRepository(db);
        var allowed = await repository.GetForMemberAsync(organizationA, new("alice", "issuer-A"), default);
        Assert.Equal("A workspace", allowed?.WorkspaceName);
        Assert.Null(await repository.GetForMemberAsync(organizationB, new("alice", "issuer-A"), default));
        Assert.Null(await repository.GetForMemberAsync(organizationA, new("bob", "issuer-A"), default));
        Assert.Null(await repository.GetForMemberAsync(organizationA, new("alice", "issuer-B"), default));
        Assert.Null(await repository.GetForMemberAsync(Guid.NewGuid(), new("alice", "issuer-A"), default));
    }

    [Fact]
    public void PostgreSql_migration_matches_model_and_creates_membership_constraint()
    {
        using var db = new CompassDbContext(new DbContextOptionsBuilder<CompassDbContext>()
            .UseNpgsql("Host=localhost;Database=unused").Options);
        Assert.False(db.Database.HasPendingModelChanges());
        var sql = db.GetService<IMigrator>().GenerateScript();
        Assert.Contains("CREATE TABLE workspaces.", sql);
        Assert.Contains("FOREIGN KEY", sql);
        Assert.Contains("Issuer", sql);
    }
}
