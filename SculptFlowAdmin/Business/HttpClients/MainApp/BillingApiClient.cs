using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Common.Statics;
using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Entities.Requests.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>
/// Client for the main app's platform-admin billing API (/api/platform-admin/billing). Billing rules, balances and the
/// ledger live in the main app; the portal only calls it. Every call carries the shared key and the signed-in admin's
/// email (X-Admin-Actor, written to the ledger). Money-moving calls send an Idempotency-Key made from the form's own
/// operation id, so a double-submitted form applies once. Every successful write is also recorded in this portal's
/// admin_audit_log, like every other admin change.
/// </summary>
public class BillingApiClient : IBillingApiClient
{
    public const string Prefix = "api/platform-admin/billing/";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly MainAppApiOptions _options;
    private readonly IHttpContextAccessor _context;
    private readonly IAdminAudit _audit;
    private readonly ILogger<BillingApiClient> _logger;

    public BillingApiClient(HttpClient http, Microsoft.Extensions.Options.IOptions<MainAppApiOptions> options, IHttpContextAccessor context,
        IAdminAudit audit, ILogger<BillingApiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _context = context;
        _audit = audit;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.BaseUrl) && !string.IsNullOrWhiteSpace(_options.PlatformAdminApiKey);
    public string? BaseUrl => _options.BaseUrl;

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
        WriteAsync(HttpMethod.Post, "plans", r, null, new("billing.plan_create", "billing_plan", r.Code, null, r), ct);

    public Task UpdatePlanAsync(string code, PlanRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Put, $"plans/{Esc(code)}", r, null, new("billing.plan_update", "billing_plan", code, null, r), ct);

    public Task CreateRateCardAsync(RateCardRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, "rate-cards", r, null, new("billing.rate_card_create", "billing_rate_card", r.Code, r.ClinicId, r), ct);

    public Task UpdateRateCardAsync(string code, RateCardUpdateRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Put, $"rate-cards/{Esc(code)}", r, null, new("billing.rate_card_update", "billing_rate_card", code, null, r), ct);

    public Task AddRateAsync(string code, AddRateRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"rate-cards/{Esc(code)}/rates", r, null, new("billing.rate_add", "billing_rate_card", code, null, r), ct);

    public Task CloseRateAsync(Guid rateId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"rates/{rateId}/close", new CloseRateRequest(null), null, new("billing.rate_close", "billing_rate", rateId, null, null), ct);

    public Task StartSubscriptionAsync(Guid clinicId, StartSubscriptionApiRequest r, string opId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/subscription", r, opId, new("billing.subscription_start", "billing_subscription", clinicId, clinicId, r), ct);

    public Task CancelSubscriptionAsync(Guid clinicId, CancelSubscriptionRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/subscription/cancel", r, null, new("billing.subscription_cancel", "billing_subscription", clinicId, clinicId, r), ct);

    public Task ResumeSubscriptionAsync(Guid clinicId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/subscription/resume", null, null, new("billing.subscription_resume", "billing_subscription", clinicId, clinicId, null), ct);

    public Task RenewAsync(Guid clinicId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/subscription/renew", null, null, new("billing.subscription_renew", "billing_subscription", clinicId, clinicId, null), ct);

    public Task TopUpAsync(Guid clinicId, TopUpRequest r, string opId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/wallet/top-ups", r, opId, new("billing.wallet_top_up", "billing_account", clinicId, clinicId, r), ct);

    public Task AdjustAsync(Guid clinicId, AdjustmentRequest r, string opId, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/wallet/adjustments", r, opId, new("billing.wallet_adjust", "billing_account", clinicId, clinicId, r), ct);

    public Task RefundAsync(Guid clinicId, Guid usageId, RefundRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"clinics/{clinicId}/usage/{usageId}/refund", r, null, new("billing.usage_refund", "billing_usage", usageId, clinicId, r), ct);

    public Task SetProviderBillingAsync(Guid clinicId, Guid channelIntegrationId, ProviderBillingRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Put, $"channel-accounts/{channelIntegrationId}/provider-billing", r, null,
            new("billing.provider_billing_set", "channel_integration", channelIntegrationId, clinicId, r), ct);

    public Task ResetProviderBillingAsync(Guid clinicId, Guid channelIntegrationId, ResetProviderBillingRequest r, CancellationToken ct) =>
        WriteAsync(HttpMethod.Post, $"channel-accounts/{channelIntegrationId}/provider-billing/reset", r, null,
            new("billing.provider_billing_reset", "channel_integration", channelIntegrationId, clinicId, r), ct);

    // ------------------------------------------------------------------------------------------------------------

    private sealed record AuditInfo(string Action, string EntityType, object? EntityId, Guid? ClinicId, object? Details);

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct) where T : class
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, null, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null; // no such record (or the API is off)
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(Json, ct);
    }

    /// <summary>For endpoints that always exist (lists, catalogs): a 404 can only mean the main app has the API off.</summary>
    private async Task<T> GetRequiredAsync<T>(string path, CancellationToken ct) where T : class =>
        await GetAsync<T>(path, ct) ?? throw ApiOff();

    private async Task WriteAsync(HttpMethod method, string path, object? body, string? opId, AuditInfo audit, CancellationToken ct)
    {
        string? key = null;
        if (opId is not null)
        {
            if (!Guid.TryParse(opId, out var op)) throw new BillingApiException("This form expired. Reload the page and try again.");
            key = $"admin-portal:{op:N}";
        }
        using var response = await SendAsync(method, path, body, key, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // A create (no id in the path) can only 404 when the API is off; otherwise the record is gone.
            if (!path.Contains('/')) throw ApiOff();
            throw new KeyNotFoundException();
        }
        await EnsureSuccessAsync(response, ct);
        await _audit.LogAsync(audit.Action, audit.EntityType, audit.EntityId, audit.ClinicId, audit.Details, ct);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, string? idempotencyKey, CancellationToken ct)
    {
        if (!IsConfigured)
        {
            throw new BillingApiException("Billing isn't connected to the main app yet: set MainApp:PlatformAdminApiKey (and MainApp:ApiBaseUrl " +
                "or MainApp:PublicBaseUrl) for this portal, and the same key as PlatformAdmin:ApiKey on the main app.");
        }

        var request = new HttpRequestMessage(method, new Uri(new Uri(_options.BaseUrl + "/"), Prefix + path));
        request.Headers.Add("X-Platform-Admin-Key", _options.PlatformAdminApiKey);
        var user = _context.HttpContext?.User;
        request.Headers.Add("X-Admin-Actor", user?.Identity?.IsAuthenticated == true ? AdminClaims.Email(user) : "admin-portal");
        if (idempotencyKey is not null) request.Headers.Add("Idempotency-Key", idempotencyKey);
        if (body is not null) request.Content = JsonContent.Create(body, body.GetType(), options: Json);
        else if (method != HttpMethod.Get) request.Content = JsonContent.Create(new { }, options: Json);

        try
        {
            return await _http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Billing API call {Method} {Path} failed.", method, path);
            throw new BillingApiException($"Couldn't reach the main app at {_options.BaseUrl}. Try again in a moment.");
        }
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new BillingApiException("The main app rejected this portal's key: MainApp:PlatformAdminApiKey must equal the main app's PlatformAdmin:ApiKey.", response.StatusCode);

        var text = await response.Content.ReadAsStringAsync(ct);
        var message = ReadError(text);
        _logger.LogInformation("Billing API answered {Status}: {Message}", (int)response.StatusCode, message);
        throw new BillingApiException(message ?? $"The main app answered {(int)response.StatusCode} {response.ReasonPhrase}.", response.StatusCode);
    }

    /// <summary>{ "error": "…" } from the billing controller, or ASP.NET's validation problem details.</summary>
    private static string? ReadError(string text)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            if (root.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String) return e.GetString();
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var all = errors.EnumerateObject().SelectMany(p => p.Value.ValueKind == JsonValueKind.Array
                    ? p.Value.EnumerateArray().Select(v => v.GetString()) : [p.Value.ToString()]).Where(s => !string.IsNullOrEmpty(s));
                var joined = string.Join(" ", all);
                if (joined.Length > 0) return joined;
            }
            if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String) return t.GetString();
        }
        catch (JsonException)
        {
        }
        return null;
    }

    private static BillingApiException ApiOff() =>
        new("The main app's platform-admin API is off: PlatformAdmin:ApiKey isn't set on the main app.", HttpStatusCode.NotFound);

    private static string Esc(string value) => Uri.EscapeDataString(value.Trim());
}
