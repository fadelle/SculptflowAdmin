namespace Omni.Admin.Entities.Requests.Billing;

/// <summary>(Copy of the main app's shape.) PUT billing/addons/{code}: creates or changes a catalog add-on. The limit it
/// raises (EntitlementKey) is fixed once the add-on exists.</summary>
public record AddonRequest(string? Name, string? EntitlementKey, decimal UnitPrice, bool IsActive = true, int SortOrder = 0);
