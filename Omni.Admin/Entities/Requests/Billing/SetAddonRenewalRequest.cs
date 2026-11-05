namespace Omni.Admin.Entities.Requests.Billing;

/// <summary>PUT /api/platform-admin/billing/clients/{id}/addons/{code}/renewal: whether the add-on renews with the plan (true / false,
/// or null = the main app's Billing:AddonsRenewByDefault setting). Copied from the main app.</summary>
public record SetAddonRenewalRequest(bool? AutoRenew);
