using SculptFlowAdmin.Common.Enums;

namespace SculptFlowAdmin.Pages.Shared;

/// <summary>The current request's scope (registered Scoped), set by <see cref="ScopePageFilter"/>. Pages and the header
/// read it; they never re-derive it.</summary>
public sealed class ScopeContext
{
    public ScopeCapability Capability { get; internal set; }
    public string? EntityId { get; internal set; }
    /// <summary>The selected clinic's name; null when nothing is selected or the main app couldn't be asked.</summary>
    public string? EntityName { get; internal set; }
    public bool IsGlobal => EntityId is null;

    /// <summary>The id to filter by: null means all (always null on pages without a scope capability).</summary>
    public string? FilterId => Capability == ScopeCapability.None ? null : EntityId;

    /// <summary><see cref="FilterId"/> as the pages' Guid clinic id.</summary>
    public Guid? FilterGuid => Guid.TryParse(FilterId, out var id) ? id : null;
}
