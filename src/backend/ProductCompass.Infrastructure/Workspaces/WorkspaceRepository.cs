using Microsoft.EntityFrameworkCore;
using ProductCompass.Application.Workspaces;
using ProductCompass.Infrastructure.Persistence;

namespace ProductCompass.Infrastructure.Workspaces;

public sealed class WorkspaceRepository(CompassDbContext db) : IWorkspaceRepository
{
    public Task<WorkspaceOverview?> GetForMemberAsync(Guid organizationId, UserIdentity user, CancellationToken ct) =>
        (from workspace in db.Workspaces.AsNoTracking()
         join organization in db.Organizations on workspace.OrganizationId equals organization.Id
         where workspace.OrganizationId == organizationId
             && db.Memberships.Any(m => m.OrganizationId == organizationId
                 && m.Subject == user.Subject && m.Issuer == user.Issuer)
         select new WorkspaceOverview(organization.Id, organization.Name, workspace.Id, workspace.Name))
        .SingleOrDefaultAsync(ct);
}
