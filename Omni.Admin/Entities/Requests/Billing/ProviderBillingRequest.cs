namespace Omni.Admin.Entities.Requests.Billing;

public record ProviderBillingRequest(string? ProviderBilling, bool? OmniUsageBilling, string Reason);
