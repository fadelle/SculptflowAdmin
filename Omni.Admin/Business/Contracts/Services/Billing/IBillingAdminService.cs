using Omni.Admin.Entities.Dtos.Billing;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Requests.Billing;
using Omni.Admin.Entities.Responses.Billing;

namespace Omni.Admin.Business.Contracts.Services.Billing;

/// <summary>
/// What the billing pages call. Everything goes through the main app's platform-admin billing API; this portal never
/// reads or writes billing tables. Every successful write is recorded in admin_audit_log.
/// </summary>
public interface IBillingAdminService
{
    Task<List<BillingAccountRow>> AccountsAsync(CancellationToken ct);

    /// <summary>Every client (id and name) as the main app lists it, for client pickers on billing pages.</summary>
    Task<List<ClientOption>> ClientOptionsAsync(CancellationToken ct);

    Task<AdminBillingOverview?> ClientAsync(Guid clientId, CancellationToken ct);

    Task<List<PlanResponse>> PlansAsync(CancellationToken ct);

    Task<PlanResponse?> PlanAsync(string code, CancellationToken ct);

    Task<List<EntitlementDefinition>> EntitlementsAsync(CancellationToken ct);

    Task<List<RateCardResponse>> RateCardsAsync(CancellationToken ct);

    Task<List<RateResponse>?> RatesAsync(string code, bool history, CancellationToken ct);

    Task<ProviderBillingModes> ProviderBillingModesAsync(CancellationToken ct);

    Task<ReconciliationResponse> ReconcileAsync(Guid clientId, CancellationToken ct);

    Task<PagedResponse<AdminUsageRow>> UsageAsync(Guid clientId, string? status, string? eventType, int page, CancellationToken ct);

    Task<PagedResponse<AdminLedgerRow>> LedgerAsync(Guid clientId, int page, CancellationToken ct);

    Task<BillingReport> ReportAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);

    /// <summary>What Omni would charge the client now; null when no rate card prices it.</summary>
    Task<QuoteResponse?> QuoteAsync(Guid clientId, string eventType, string? country, string? provider, string? providerBilling,
        CancellationToken ct);

    Task CreatePlanAsync(PlanRequest r, CancellationToken ct);

    Task UpdatePlanAsync(string code, PlanRequest r, CancellationToken ct);

    Task CreateRateCardAsync(RateCardRequest r, CancellationToken ct);

    Task UpdateRateCardAsync(string code, RateCardUpdateRequest r, CancellationToken ct);

    Task AddRateAsync(string code, AddRateRequest r, CancellationToken ct);

    Task CloseRateAsync(Guid rateId, CancellationToken ct);

    Task StartSubscriptionAsync(Guid clientId, StartSubscriptionApiRequest r, string opId, CancellationToken ct);

    Task CancelSubscriptionAsync(Guid clientId, CancelSubscriptionRequest r, CancellationToken ct);

    Task ResumeSubscriptionAsync(Guid clientId, CancellationToken ct);

    Task RenewAsync(Guid clientId, CancellationToken ct);

    Task TopUpAsync(Guid clientId, TopUpRequest r, string opId, CancellationToken ct);

    Task AdjustAsync(Guid clientId, AdjustmentRequest r, string opId, CancellationToken ct);

    Task RefundAsync(Guid clientId, Guid usageId, RefundRequest r, CancellationToken ct);

    Task SetProviderBillingAsync(Guid clientId, Guid channelIntegrationId, ProviderBillingRequest r, CancellationToken ct);

    Task ResetProviderBillingAsync(Guid clientId, Guid channelIntegrationId, ResetProviderBillingRequest r, CancellationToken ct);

    bool IsConfigured { get; }

    string? BaseUrl { get; }
}
