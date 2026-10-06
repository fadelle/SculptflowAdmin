namespace SculptFlowAdmin.Entities.Requests.Billing;

// Request/response shapes of the main app's platform-admin billing API (/api/platform-admin/billing/*). A copy of the
// admin part of SculptFlowApp's PlasticSurgery/Dtos/BillingDtos.cs (+ ChannelAccountBilling from Billing/ProviderBilling.cs).
// The main app owns billing: this portal never writes billing.* tables, it calls the API. If a shape changes there,
// change it here too (MAIN_APP_SYNC.md).

// ---- plans ----------------------------------------------------------------------------------------------------

public record PlanRequest(
    string Code,
    string Name,
    string? Description,
    decimal Price,
    string BillingPeriod,
    decimal IncludedUsageCredit,
    string? RateCardCode,
    bool IsActive = true,
    int SortOrder = 0,
    Dictionary<string, string>? Entitlements = null);
