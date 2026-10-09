namespace SculptFlowAdmin.Entities.Dtos.Navigation;

/// <summary>
/// A node of the ONE navigation tree (<see cref="SculptFlowAdmin.Common.Statics.NavTree"/>). Leaf = has Path; group = has
/// Children. Aliases are extra path prefixes that also count as this leaf (e.g. /Billing/Clinic is "Billing accounts").
/// </summary>
public sealed record NavItem(string Label, string Icon, string? Path = null, NavItem[]? Children = null, bool Exact = false,
    string[]? Aliases = null)
{
    public bool IsLeaf => Path is not null;

    /// <summary>A group whose children are all leaves → one sidebar row; its children become header tabs.</summary>
    public bool IsLeafGroup => Children is { Length: > 0 } && Children.All(c => c.IsLeaf);

    public string? FirstLeafPath => Path ?? Children?.Select(c => c.FirstLeafPath).FirstOrDefault(p => p is not null);

    /// <summary>Segment-aware: "/Clinics" matches "/Clinics" and "/Clinics/Details", not "/ClinicsArchive". Aliases always
    /// match as prefixes; Exact applies to Path only.</summary>
    public bool Matches(string path) =>
        Path is not null && (Covers(Path, path, Exact) || (Aliases?.Any(a => Covers(a, path, exact: false)) ?? false));

    public bool IsActive(string path) => Matches(path) || (Children?.Any(c => c.IsActive(path)) ?? false);

    private static bool Covers(string prefix, string path, bool exact) =>
        string.Equals(path, prefix, StringComparison.OrdinalIgnoreCase) ||
        (!exact && path.StartsWith(prefix.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase));
}
