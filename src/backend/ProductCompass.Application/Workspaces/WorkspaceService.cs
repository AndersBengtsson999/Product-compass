namespace ProductCompass.Application.Workspaces;

public sealed record UserIdentity(string Subject, string Issuer);
public sealed record WorkspaceOverview(Guid OrganizationId, string OrganizationName, Guid WorkspaceId, string WorkspaceName);

// There is deliberately no unscoped read method. Membership must match both issuer and subject.
public interface IWorkspaceRepository
{
    Task<WorkspaceOverview?> GetForMemberAsync(Guid organizationId, UserIdentity user, CancellationToken ct);
}

public sealed class WorkspaceService(IWorkspaceRepository repository)
{
    public Task<WorkspaceOverview?> GetOverviewAsync(Guid organizationId, UserIdentity user, CancellationToken ct = default)
    {
        if (organizationId == Guid.Empty) throw new ArgumentException("An organization ID is required.");
        if (string.IsNullOrWhiteSpace(user.Subject) || string.IsNullOrWhiteSpace(user.Issuer))
            throw new UnauthorizedAccessException("A verified identity is required.");
        return repository.GetForMemberAsync(organizationId, user, ct);
    }
}
