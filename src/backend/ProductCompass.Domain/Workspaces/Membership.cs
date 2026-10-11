namespace ProductCompass.Domain.Workspaces;

public sealed class Membership
{
    private Membership() { }
    public Membership(Guid organizationId, string subject, string issuer)
    {
        if (organizationId == Guid.Empty) throw new ArgumentException("An organization ID is required.");
        OrganizationId = organizationId;
        Subject = Names.Required(subject, 200);
        Issuer = Names.Required(issuer, 300);
    }
    public Guid OrganizationId { get; private set; }
    public string Subject { get; private set; } = "";
    public string Issuer { get; private set; } = "";
}
