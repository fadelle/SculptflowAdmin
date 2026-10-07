using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Requests.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;

namespace SculptFlowAdmin.Business.Contracts.Services.Billing;

/// <summary>
/// What the billing pages call. Everything goes through the main app's platform-admin billing API; this portal never
/// reads or writes billing tables. Every successful write is recorded in admin_audit_log.
/// </summary>
public interface IBillingAdminService
{
    Task<List<BillingAccountRow>> AccountsAsync(CancellationToken ct);

    /// <summary>Every clinic (id and name) as the main app lists it, for clinic pickers on billing pages.</summary>
    Task<List<ClinicOption>> ClinicOptionsAsync(CancellationToken ct);

    Task<AdminBillingOverview?> ClinicAsync(Guid clinicId, CancellationToken ct);

    Task<List<PlanResponse>> PlansAsync(CancellationToken ct);

    Task<PlanResponse?> PlanAsync(string code, CancellationToken ct);

    Task<List<EntitlementDefinition>> EntitlementsAsync(CancellationToken ct);

    Task<List<RateCardResponse>> RateCardsAsync(CancellationToken ct);

    Task<List<RateResponse>?> RatesAsync(string code, bool history, CancellationToken ct);

    Task<ProviderBillingModes> ProviderBillingModesAsync(CancellationToken ct);

    Task<ReconciliationResponse> ReconcileAsync(Guid clinicId, CancellationToken ct);

    Task<PagedResponse<AdminUsageRow>> UsageAsync(Guid clinicId, string? status, string? eventType, int page, CancellationToken ct);

    Task<PagedResponse<AdminLedgerRow>> LedgerAsync(Guid clinicId, int page, CancellationToken ct);

    Task<BillingReport> ReportAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);

    /// <summary>What SculptFlow would charge the clinic now; null when no rate card prices it.</summary>
    Task<QuoteResponse?> QuoteAsync(Guid clinicId, string eventType, string? country, string? provider, string? providerBilling,
        CancellationToken ct);

    Task CreatePlanAsync(PlanRequest r, CancellationToken ct);

    Task UpdatePlanAsync(string code, PlanRequest r, CancellationToken ct);

    Task CreateRateCardAsync(RateCardRequest r, CancellationToken ct);

    Task UpdateRateCardAsync(string code, RateCardUpdateRequest r, CancellationToken ct);

    Task AddRateAsync(string code, AddRateRequest r, CancellationToken ct);

    Task CloseRateAsync(Guid rateId, CancellationToken ct);

    Task StartSubscriptionAsync(Guid clinicId, StartSubscriptionApiRequest r, string opId, CancellationToken ct);

    Task CancelSubscriptionAsync(Guid clinicId, CancelSubscriptionRequest r, CancellationToken ct);

    Task ResumeSubscriptionAsync(Guid clinicId, CancellationToken ct);

    Task RenewAsync(Guid clinicId, CancellationToken ct);

    Task TopUpAsync(Guid clinicId, TopUpRequest r, string opId, CancellationToken ct);

    Task AdjustAsync(Guid clinicId, AdjustmentRequest r, string opId, CancellationToken ct);

    Task RefundAsync(Guid clinicId, Guid usageId, RefundRequest r, CancellationToken ct);

    Task SetProviderBillingAsync(Guid clinicId, Guid channelIntegrationId, ProviderBillingRequest r, CancellationToken ct);

    Task ResetProviderBillingAsync(Guid clinicId, Guid channelIntegrationId, ResetProviderBillingRequest r, CancellationToken ct);

    bool IsConfigured { get; }

    string? BaseUrl { get; }
}
