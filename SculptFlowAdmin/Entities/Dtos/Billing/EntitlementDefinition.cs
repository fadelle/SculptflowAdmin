namespace SculptFlowAdmin.Entities.Dtos.Billing;

/// <summary>An entitlement key plans can set. Kind "feature" = true/false; "limit" = a whole number or "unlimited".
/// A key a plan doesn't set means off / 0.</summary>
public record EntitlementDefinition(string Key, string Kind, string Label);

// ---- rate cards & rates ----------------------------------------------------------------------------------------
