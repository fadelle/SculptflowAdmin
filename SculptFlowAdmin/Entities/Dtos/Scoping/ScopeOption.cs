namespace SculptFlowAdmin.Entities.Dtos.Scoping;

/// <summary>One choice in the header's scope selector.</summary>
public sealed record ScopeOption(string Id, string Name, bool IsActive = true, string? Sub = null);
