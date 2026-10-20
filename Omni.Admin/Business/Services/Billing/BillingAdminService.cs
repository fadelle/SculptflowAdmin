using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Billing;
using Omni.Admin.Entities.Dtos.Billing;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Requests.Billing;
using Omni.Admin.Entities.Responses.Billing;

namespace Omni.Admin.Business.Services.Billing;

/// <summary>
/// Billing for the portal's pages: reads and writes go to the main app's platform-admin billing API
/// (<see cref="IBillingApiClient"/>), and each successful write is then recorded in admin_audit_log.
/// </summary>
public class BillingAdminService : IBillingAdminService
{
    private readonly IBillingApiClient _api;
    private readonly IAdminAudit _audit;

    public BillingAdminService(IBillingApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public bool IsConfigured => _api.IsConfigured;
    public string? BaseUrl => _api.BaseUrl;

    // ---- reads -------------------------------------------------------------------------------------------------

    public Task<List<BillingAccountRow>> AccountsAsync(CancellationToken ct) => _api.AccountsAsync(ct);

    public async Task<List<ClientOption>> ClientOptionsAsync(CancellationToken ct) =>
        (await _api.AccountsAsync(ct)).Select(a => new ClientOption(a.ClientId, a.ClientName)).OrderBy(c => c.Name).ToList();

    public Task<AdminBillingOverview?> ClientAsync(Guid clientId, CancellationToken ct) => _api.ClientAsync(clientId, ct);
    public Task<List<PlanResponse>> PlansAsync(CancellationToken ct) => _api.PlansAsync(ct);
    public Task<PlanResponse?> PlanAsync(string code, CancellationToken ct) => _api.PlanAsync(code, ct);
    public Task<List<EntitlementDefinition>> EntitlementsAsync(CancellationToken ct) => _api.EntitlementsAsync(ct);
    public Task<List<RateCardResponse>> RateCardsAsync(CancellationToken ct) => _api.RateCardsAsync(ct);
    public Task<List<RateResponse>?> RatesAsync(string code, bool history, CancellationToken ct) => _api.RatesAsync(code, history, ct);
    public Task<ProviderBillingModes> ProviderBillingModesAsync(CancellationToken ct) => _api.ProviderBillingModesAsync(ct);
    public Task<ReconciliationResponse> ReconcileAsync(Guid clientId, CancellationToken ct) => _api.ReconcileAsync(clientId, ct);

    public Task<PagedResponse<AdminUsageRow>> UsageAsync(Guid clientId, string? status, string? eventType, int page, CancellationToken ct) =>
        _api.UsageAsync(clientId, status, eventType, page, ct);

    public Task<PagedResponse<AdminLedgerRow>> LedgerAsync(Guid clientId, int page, CancellationToken ct) => _api.LedgerAsync(clientId, page, ct);

    public Task<BillingReport> ReportAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct) => _api.ReportAsync(from, to, ct);

    public Task<QuoteResponse?> QuoteAsync(Guid clientId, string eventType, string? country, string? provider, string? providerBilling,
        CancellationToken ct) =>
        _api.QuoteAsync(clientId, eventType, country, provider, providerBilling, ct);

    // ---- writes ------------------------------------------------------------------------------------------------

    public async Task CreatePlanAsync(PlanRequest r, CancellationToken ct)
    {
        await _api.CreatePlanAsync(r, ct);
        await _audit.LogAsync("billing.plan_create", "billing_plan", r.Code, null, r, ct);
    }

    public async Task UpdatePlanAsync(string code, PlanRequest r, CancellationToken ct)
    {
        await _api.UpdatePlanAsync(code, r, ct);
        await _audit.LogAsync("billing.plan_update", "billing_plan", code, null, r, ct);
    }

    public async Task CreateRateCardAsync(RateCardRequest r, CancellationToken ct)
    {
        await _api.CreateRateCardAsync(r, ct);
        await _audit.LogAsync("billing.rate_card_create", "billing_rate_card", r.Code, r.ClientId, r, ct);
    }

    public async Task UpdateRateCardAsync(string code, RateCardUpdateRequest r, CancellationToken ct)
    {
        await _api.UpdateRateCardAsync(code, r, ct);
        await _audit.LogAsync("billing.rate_card_update", "billing_rate_card", code, null, r, ct);
    }

    public async Task AddRateAsync(string code, AddRateRequest r, CancellationToken ct)
    {
        await _api.AddRateAsync(code, r, ct);
        await _audit.LogAsync("billing.rate_add", "billing_rate_card", code, null, r, ct);
    }

    public async Task CloseRateAsync(Guid rateId, CancellationToken ct)
    {
        await _api.CloseRateAsync(rateId, ct);
        await _audit.LogAsync("billing.rate_close", "billing_rate", rateId, null, null, ct);
    }

    public async Task StartSubscriptionAsync(Guid clientId, StartSubscriptionApiRequest r, string opId, CancellationToken ct)
    {
        await _api.StartSubscriptionAsync(clientId, r, opId, ct);
        await _audit.LogAsync("billing.subscription_start", "billing_subscription", clientId, clientId, r, ct);
    }

    public async Task CancelSubscriptionAsync(Guid clientId, CancelSubscriptionRequest r, CancellationToken ct)
    {
        await _api.CancelSubscriptionAsync(clientId, r, ct);
        await _audit.LogAsync("billing.subscription_cancel", "billing_subscription", clientId, clientId, r, ct);
    }

    public async Task ResumeSubscriptionAsync(Guid clientId, CancellationToken ct)
    {
        await _api.ResumeSubscriptionAsync(clientId, ct);
        await _audit.LogAsync("billing.subscription_resume", "billing_subscription", clientId, clientId, null, ct);
    }

    public async Task RenewAsync(Guid clientId, CancellationToken ct)
    {
        await _api.RenewAsync(clientId, ct);
        await _audit.LogAsync("billing.subscription_renew", "billing_subscription", clientId, clientId, null, ct);
    }

    public async Task TopUpAsync(Guid clientId, TopUpRequest r, string opId, CancellationToken ct)
    {
        await _api.TopUpAsync(clientId, r, opId, ct);
        await _audit.LogAsync("billing.wallet_top_up", "billing_account", clientId, clientId, r, ct);
    }

    public async Task AdjustAsync(Guid clientId, AdjustmentRequest r, string opId, CancellationToken ct)
    {
        await _api.AdjustAsync(clientId, r, opId, ct);
        await _audit.LogAsync("billing.wallet_adjust", "billing_account", clientId, clientId, r, ct);
    }

    public async Task RefundAsync(Guid clientId, Guid usageId, RefundRequest r, CancellationToken ct)
    {
        await _api.RefundAsync(clientId, usageId, r, ct);
        await _audit.LogAsync("billing.usage_refund", "billing_usage", usageId, clientId, r, ct);
    }

    public async Task SetProviderBillingAsync(Guid clientId, Guid channelIntegrationId, ProviderBillingRequest r, CancellationToken ct)
    {
        await _api.SetProviderBillingAsync(channelIntegrationId, r, ct);
        await _audit.LogAsync("billing.provider_billing_set", "channel_integration", channelIntegrationId, clientId, r, ct);
    }

    public async Task ResetProviderBillingAsync(Guid clientId, Guid channelIntegrationId, ResetProviderBillingRequest r, CancellationToken ct)
    {
        await _api.ResetProviderBillingAsync(channelIntegrationId, r, ct);
        await _audit.LogAsync("billing.provider_billing_reset", "channel_integration", channelIntegrationId, clientId, r, ct);
    }
}
