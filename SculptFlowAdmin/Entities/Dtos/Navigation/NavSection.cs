namespace SculptFlowAdmin.Entities.Dtos.Navigation;

/// <summary>A sidebar meta-section: an optional uppercase caption over its items.</summary>
public sealed record NavSection(string? Title, NavItem[] Items);
