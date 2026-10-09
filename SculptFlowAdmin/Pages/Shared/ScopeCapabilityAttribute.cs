using SculptFlowAdmin.Common.Enums;

namespace SculptFlowAdmin.Pages.Shared;

/// <summary>Declares that a page's data can be narrowed by the header's scope selector (docs/UI_GUIDE.md §7). Pages
/// without it hide the selector but keep the selection.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ScopeCapabilityAttribute : Attribute
{
    public ScopeCapabilityAttribute(ScopeCapability capability) => Capability = capability;

    public ScopeCapability Capability { get; }
}
