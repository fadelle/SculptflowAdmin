namespace SculptFlowAdmin.Entities.Dtos.Scoping;

/// <summary>A page of scope choices for the selector's infinite scroll (aurora.js reads items + hasMore).</summary>
public sealed record ScopeOptionPage(IReadOnlyList<ScopeOption> Items, bool HasMore);
