using Microsoft.EntityFrameworkCore;
using ProductCompass.Domain.Workspaces;

namespace ProductCompass.Infrastructure.Persistence;

public sealed class CompassDbContext(DbContextOptions<CompassDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.HasDefaultSchema("workspaces");
        model.Entity<Organization>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        });
        model.Entity<Membership>(b =>
        {
            b.HasKey(x => new { x.OrganizationId, x.Subject, x.Issuer });
            b.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            b.Property(x => x.Issuer).HasMaxLength(300).IsRequired();
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<Workspace>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(120).IsRequired();
            // Foundation supports one workspace per organization; this can be expanded later.
            b.HasIndex(x => x.OrganizationId).IsUnique();
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
