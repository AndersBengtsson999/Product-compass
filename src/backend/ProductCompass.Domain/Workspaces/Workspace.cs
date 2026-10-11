namespace ProductCompass.Domain.Workspaces;

public sealed class Workspace
{
    private Workspace() { }
    public Workspace(Guid id, Guid organizationId, string name)
    {
        if (id == Guid.Empty || organizationId == Guid.Empty)
            throw new ArgumentException("Workspace and organization IDs are required.");
        Id = id;
        OrganizationId = organizationId;
        Name = Names.Required(name, 120);
    }
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = "";
}
