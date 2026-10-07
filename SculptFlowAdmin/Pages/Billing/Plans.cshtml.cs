using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Billing;
using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Entities.Requests.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Billing;

/// <summary>Subscription plans: price, period, included credit, rate card and features/limits (entitlements).</summary>
public class PlansModel : MainAppPageModel
{
    private readonly IBillingAdminService _billing;

    public PlansModel(IBillingAdminService billing) => _billing = billing;

    [BindProperty(SupportsGet = true)] public string? Edit { get; set; }
    public List<PlanResponse> Plans { get; private set; } = new();
    public List<EntitlementDefinition> Catalog { get; private set; } = new();
    public List<RateCardResponse> RateCards { get; private set; } = new();
    public PlanResponse? Editing { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(async () =>
        {
            Plans = await _billing.PlansAsync(ct);
            Catalog = await _billing.EntitlementsAsync(ct);
            RateCards = await _billing.RateCardsAsync(ct);
            Editing = string.IsNullOrWhiteSpace(Edit) ? null : Plans.FirstOrDefault(p => p.Code == Edit.Trim().ToLowerInvariant());
        });
    }

    public Task<IActionResult> OnPostSaveAsync(string? originalCode, string? code, string? name, string? description, decimal price,
        string? billingPeriod, decimal includedUsageCredit, string? rateCardCode, bool isActive, int sortOrder, CancellationToken ct) =>
        RunAsync(async () =>
        {
            var entitlements = ReadEntitlements(await _billing.EntitlementsAsync(ct));
            var request = new PlanRequest((originalCode ?? code ?? "").Trim(), (name ?? "").Trim(), Clean(description), price,
                billingPeriod ?? "month", includedUsageCredit, Clean(rateCardCode), isActive, sortOrder, entitlements);
            if (string.IsNullOrWhiteSpace(originalCode)) await _billing.CreatePlanAsync(request, ct);
            else await _billing.UpdatePlanAsync(originalCode, request, ct);
        }, string.IsNullOrWhiteSpace(originalCode) ? "Plan created." : "Plan saved. Price and credit changes apply from each subscriber's next renewal.",
        string.IsNullOrWhiteSpace(originalCode) ? new { } : new { edit = originalCode });

    /// <summary>Feature checkboxes post "ent:{key}=true"; limits post a number or "unlimited" (blank = 0, left out).</summary>
    private Dictionary<string, string> ReadEntitlements(List<EntitlementDefinition> catalog)
    {
        var result = new Dictionary<string, string>();
        foreach (var d in catalog)
        {
            var raw = Request.Form["ent:" + d.Key].ToString().Trim().ToLowerInvariant();
            if (d.Kind == "feature") result[d.Key] = raw == "true" ? "true" : "false";
            else if (raw.Length > 0) result[d.Key] = raw;
        }
        return result;
    }
}
