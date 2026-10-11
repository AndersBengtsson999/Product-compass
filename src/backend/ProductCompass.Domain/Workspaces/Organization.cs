namespace ProductCompass.Domain.Workspaces;

public sealed class Organization
{
    private Organization() { }
    public Organization(Guid id, string name)
    {
        if (id == Guid.Empty) throw new ArgumentException("An organization ID is required.", nameof(id));
        Id = id;
        Name = Names.Required(name, 120);
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
}

internal static class Names
{
    public static string Required(string? value, int maxLength)
    {
        var name = value?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > maxLength)
            throw new ArgumentException($"A name between 1 and {maxLength} characters is required.");
        return name;
    }
}
