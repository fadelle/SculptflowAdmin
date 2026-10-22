namespace Omni.Admin.Entities.Responses.Billing;

/// <summary>(Copy of the main app's shape, GET /api/platform-admin/billing/addons.) An add-on in the catalog, with how many
/// clients have it.</summary>
public record AddonResponse(string Code, string Name, string EntitlementKey, decimal UnitPrice, string Currency, bool IsActive, int SortOrder, int Clients);
