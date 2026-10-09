namespace SculptFlowAdmin.Entities.Dtos.Navigation;

/// <summary>One step of the root→leaf chain for a request path, with the siblings it can switch to (breadcrumb, tabs).</summary>
public sealed record NavStep(NavItem Node, NavItem[] Siblings);
