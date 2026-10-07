using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Billing;
using SculptFlowAdmin.Common.Helpers;
using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Entities.Requests.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Billing;

/// <summary>One clinic's billing: subscription, wallet, who pays the provider per messaging account, usage, ledger.</summary>
public class ClinicModel : MainAppPageModel
{
    private readonly IBillingAdminService _billing;

    public ClinicModel(IBillingAdminService billing) => _billing = billing;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "overview";
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? EventType { get; set; }

    // Price check (GET)
    [BindProperty(SupportsGet = true)] public string? QuoteEvent { get; set; }
    [BindProperty(SupportsGet = true)] public string? QuoteCountry { get; set; }
    [BindProperty(SupportsGet = true)] public string? QuoteProvider { get; set; }
    [BindProperty(SupportsGet = true)] public string? QuoteBilling { get; set; }
    public QuoteResponse? Quote { get; private set; }
    public bool QuoteMissing { get; private set; }

    public AdminBillingOverview? Overview { get; private set; }
    public List<PlanResponse> Plans { get; private set; } = new();
    public ProviderBillingModes? Modes { get; private set; }
    public PagedResponse<AdminUsageRow>? Usage { get; private set; }
    public PagedResponse<AdminLedgerRow>? Ledger { get; private set; }

    public string Currency => Overview?.Summary.Currency ?? "";

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Tab = Tab is "usage" or "ledger" ? Tab : "overview";
        PageNumber = Math.Max(1, PageNumber);
        var found = true;
        await LoadAsync(async () =>
        {
            Overview = await _billing.ClinicAsync(Id, ct);
            if (Overview is null)
            {
                found = false;
                return;
            }
            if (Tab == "overview")
            {
                Plans = await _billing.PlansAsync(ct);
                Modes = await _billing.ProviderBillingModesAsync(ct);
                if (!string.IsNullOrWhiteSpace(QuoteEvent))
                {
                    Quote = await _billing.QuoteAsync(Id, QuoteEvent, Clean(QuoteCountry), Clean(QuoteProvider), Clean(QuoteBilling), ct);
                    QuoteMissing = Quote is null;
                }
            }
            else if (Tab == "usage") Usage = await _billing.UsageAsync(Id, Clean(Status), Clean(EventType), PageNumber, ct);
            else Ledger = await _billing.LedgerAsync(Id, PageNumber, ct);
        });
        return found ? Page() : NotFound();
    }

    private object Back => new { id = Id, tab = Tab };

    public Task<IActionResult> OnPostStartPlanAsync(string? planCode, bool chargeFirstPeriod, string? reason, string? opId, CancellationToken ct) =>
        RunAsync(() =>
        {
            if (string.IsNullOrWhiteSpace(planCode)) throw new ArgumentException("Choose a plan.");
            return _billing.StartSubscriptionAsync(Id, new StartSubscriptionApiRequest(planCode.Trim(), chargeFirstPeriod, Clean(reason)), opId ?? "", ct);
        }, chargeFirstPeriod ? "Plan started; its price was charged from the wallet." : "Plan started without charging the first period.", Back);

    public Task<IActionResult> OnPostCancelAsync(bool immediately, string? reason, CancellationToken ct) =>
        RunAsync(() => _billing.CancelSubscriptionAsync(Id, new CancelSubscriptionRequest(immediately, Clean(reason)), ct),
            immediately ? "Subscription cancelled now." : "Subscription will end at the end of the current period.", Back);

    public Task<IActionResult> OnPostResumeAsync(CancellationToken ct) =>
        RunAsync(() => _billing.ResumeSubscriptionAsync(Id, ct), "Subscription resumed: it will renew again.", Back);

    public Task<IActionResult> OnPostRenewAsync(CancellationToken ct) =>
        RunAsync(() => _billing.RenewAsync(Id, ct), "Renewal check ran. The subscription below shows the outcome.", Back);

    public Task<IActionResult> OnPostTopUpAsync(decimal amount, string? reference, string? reason, string? opId, CancellationToken ct) =>
        RunAsync(() => _billing.TopUpAsync(Id, new TopUpRequest(amount, Clean(reference), Clean(reason)), opId ?? "", ct),
            $"Wallet topped up by {BillingUi.Money(amount)}.", Back);

    public Task<IActionResult> OnPostAdjustAsync(decimal amount, string? balanceType, string? reason, string? opId, CancellationToken ct) =>
        RunAsync(() => _billing.AdjustAsync(Id, new AdjustmentRequest(amount, balanceType ?? "wallet", reason ?? ""), opId ?? "", ct),
            $"{BillingUi.BalanceLabel(balanceType ?? "wallet")} adjusted by {BillingUi.Money(amount)}.", Back);

    public Task<IActionResult> OnPostProviderBillingAsync(Guid accountId, string? providerBilling, string? usageBilling, string? reason, CancellationToken ct) =>
        RunAsync(() =>
        {
            bool? charge = usageBilling switch { "on" => true, "off" => false, _ => null };
            return _billing.SetProviderBillingAsync(Id, accountId, new ProviderBillingRequest(Clean(providerBilling), charge, reason ?? ""), ct);
        }, "Billing arrangement saved. It applies to messages from now on; past usage keeps the arrangement it had.", Back);

    public Task<IActionResult> OnPostResetProviderBillingAsync(Guid accountId, string? reason, CancellationToken ct) =>
        RunAsync(() => _billing.ResetProviderBillingAsync(Id, accountId, new ResetProviderBillingRequest(reason ?? ""), ct),
            "Override removed: the account follows the defaults again.", Back);

    public Task<IActionResult> OnPostRefundAsync(Guid usageId, string? reason, CancellationToken ct) =>
        RunAsync(() => _billing.RefundAsync(Id, usageId, new RefundRequest(reason ?? ""), ct),
            "Usage refunded to where it was paid from.", new { id = Id, tab = "usage", p = PageNumber, status = Status, eventType = EventType });
}
