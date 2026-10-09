using SculptFlowAdmin.Entities.Dtos.Scoping;

namespace SculptFlowAdmin.Business.Contracts.Managers;

/// <summary>The scope entity's names and choices (clinics), for the header selector and its chip.</summary>
public interface IScopeLookup
{
    /// <summary>The name for an id, or null when no such entity exists (a stale or edited cookie/URL).</summary>
    Task<string?> GetNameAsync(string id, CancellationToken ct);

    /// <summary>Choices whose name contains <paramref name="q"/>, a page at a time.</summary>
    Task<ScopeOptionPage> SearchAsync(string? q, int skip, int take, CancellationToken ct);
}
