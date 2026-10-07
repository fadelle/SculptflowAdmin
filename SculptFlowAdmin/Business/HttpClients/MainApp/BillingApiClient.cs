using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Entities.Requests.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>
/// Client for the main app's platform-admin billing API (/api/platform-admin/billing). Billing rules, balances and the
/// ledger live in the main app; the portal only calls it. Every call carries the shared key and the signed-in admin's
/// email (X-Admin-Actor, written to the ledger). Money-moving calls send an Idempotency-Key made from the form's own
/// operation id, so a double-submitted form applies once. Pages don't call this directly: they go through
/// <c>BillingAdminService</c>, which also records each write in admin_audit_log.
/// </summary>
public class BillingApiClient : MainAppApiClient, IBillingApiClient
{
    public const string Prefix = "api/platform-admin/billing/";

    public BillingApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<BillingApiClient> logger)
        : base(http, options, context, logger, Prefix, "Billing")
    {
    }

    // ---- reads -------------------------------------------------------------------------------------------------

    public Task<List<BillingAccountRow>> AccountsAsync(CancellationToken ct) => GetRequiredAsync<List<BillingAccountRow>>("accounts", ct);
    public Task<AdminBillingOverview?> ClinicAsync(Guid clinicId, CancellationToken ct) => GetAsync<AdminBillingOverview>($"clinics/{clinicId}", ct);
    public Task<List<PlanResponse>> PlansAsync(CancellationToken ct) => GetRequiredAsync<List<PlanResponse>>("plans", ct);
    public Task<PlanResponse?> PlanAsync(string code, CancellationToken ct) => GetAsync<PlanResponse>($"plans/{Esc(code)}", ct);
    public Task<List<EntitlementDefinition>> EntitlementsAsync(CancellationToken ct) => GetRequiredAsync<List<EntitlementDefinition>>("entitlements", ct);
    public Task<List<RateCardResponse>> RateCardsAsync(CancellationToken ct) => GetRequiredAsync<List<RateCardResponse>>("rate-cards", ct);
    public Task<List<RateResponse>?> RatesAsync(string code, bool history, CancellationToken ct) =>
        GetAsync<List<RateResponse>>($"rate-cards/{Esc(code)}/rates?history={(history ? "true" : "false")}", ct);
    public Task<ProviderBillingModes> ProviderBillingModesAsync(CancellationToken ct) => GetRequiredAsync<ProviderBillingModes>("provider-billing", ct);
    public Task<ReconciliationResponse> ReconcileAsync(Guid clinicId, CancellationToken ct) =>
        GetRequiredAsync<ReconciliationResponse>($"clinics/{clinicId}/reconciliation", ct);

    public Task<PagedResponse<AdminUsageRow>> UsageAsync(Guid clinicId, string? status, string? eventType, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResponse<AdminUsageRow>>($"clinics/{clinicId}/usage?page={page}&pageSize=50" +
            (string.IsNullOrWhiteSpace(status) ? "" : "&status=" + Esc(status)) +
            (string.IsNullOrWhiteSpace(eventType) ? "" : "&eventType=" + Esc(eventType)), ct);

    public Task<PagedResponse<AdminLedgerRow>> LedgerAsync(Guid clinicId, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResponse<AdminLedgerRow>>($"clinics/{clinicId}/ledger?page={page}&pageSize=50", ct);

    public Task<BillingReport> ReportAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct) =>
        GetRequiredAsync<BillingReport>($"report?from={Esc(from.ToString("o"))}&to={Esc(to.ToString("o"))}", ct);

    /// <summary>What SculptFlow would charge the clinic now; null when no rate card prices it.</summary>
    public Task<QuoteResponse?> QuoteAsync(Guid clinicId, string eventType, string? country, string? provider, string? providerBilling,
        CancellationToken ct) =>
        GetAsync<QuoteResponse>($"clinics/{clinicId}/quote?eventType={Esc(eventType)}" +
            (string.IsNullOrWhiteSpace(country) ? "" : "&country=" + Esc(country)) +
            (string.IsNullOrWhiteSpace(provider) ? "" : "&provider=" + Esc(provider)) +
            (string.IsNullOrWhiteSpace(providerBilling) ? "" : "&providerBilling=" + Esc(providerBilling)), ct);

    // ---- writes ------------------------------------------------------------------------------------------------

    public Task CreatePlanAsync(PlanRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, "plans", r, null, true, ct);

    public Task UpdatePlanAsync(string code, PlanRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Put, $"plans/{Esc(code)}", r, null, false, ct);

    public Task CreateRateCardAsync(RateCardRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, "rate-cards", r, null, true, ct);

    public Task UpdateRateCardAsync(string code, RateCardUpdateRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Put, $"rate-cards/{Esc(code)}", r, null, false, ct);

    public Task AddRateAsync(string code, AddRateRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"rate-cards/{Esc(code)}/rates", r, null, false, ct);

    public Task CloseRateAsync(Guid rateId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"rates/{rateId}/close", new CloseRateRequest(null), null, false, ct);

    public Task StartSubscriptionAsync(Guid clinicId, StartSubscriptionApiRequest r, string opId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/subscription", r, opId, false, ct);

    public Task CancelSubscriptionAsync(Guid clinicId, CancelSubscriptionRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/subscription/cancel", r, null, false, ct);

    public Task ResumeSubscriptionAsync(Guid clinicId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/subscription/resume", null, null, false, ct);

    public Task RenewAsync(Guid clinicId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/subscription/renew", null, null, false, ct);

    public Task TopUpAsync(Guid clinicId, TopUpRequest r, string opId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/wallet/top-ups", r, opId, false, ct);

    public Task AdjustAsync(Guid clinicId, AdjustmentRequest r, string opId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/wallet/adjustments", r, opId, false, ct);

    public Task RefundAsync(Guid clinicId, Guid usageId, RefundRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/usage/{usageId}/refund", r, null, false, ct);

    public Task SetProviderBillingAsync(Guid channelIntegrationId, ProviderBillingRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Put, $"channel-accounts/{channelIntegrationId}/provider-billing", r, null, false, ct);

    public Task ResetProviderBillingAsync(Guid channelIntegrationId, ResetProviderBillingRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"channel-accounts/{channelIntegrationId}/provider-billing/reset", r, null, false, ct);
}
