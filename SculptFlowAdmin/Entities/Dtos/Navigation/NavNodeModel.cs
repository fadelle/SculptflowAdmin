namespace SculptFlowAdmin.Entities.Dtos.Navigation;

/// <summary>What the recursive _NavNode partial renders: one node, the current path and the labels above it.</summary>
public sealed record NavNodeModel(NavItem Item, string CurrentPath, string[] Trail, int Depth);
