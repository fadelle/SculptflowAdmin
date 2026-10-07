using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Billing;
using SculptFlowAdmin.Entities.Requests.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Billing;

/// <summary>One rate card: its details and its versioned rates (add a new version, close one).</summary>
public class RatesModel : MainAppPageModel
{
    private readonly IBillingAdminService _billing;

    public RatesModel(IBillingAdminService billing) => _billing = billing;

    [BindProperty(SupportsGet = true)] public string Code { get; set; } = "";
    [BindProperty(SupportsGet = true)] public bool History { get; set; }
    public RateCardResponse? Card { get; private set; }
    public List<RateResponse> Rates { get; private set; } = new();
    public ProviderBillingModes? Modes { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var found = true;
        await LoadAsync(async () =>
        {
            Card = (await _billing.RateCardsAsync(ct)).FirstOrDefault(c => c.Code == Code.Trim().ToLowerInvariant());
            if (Card is null)
            {
                found = false;
                return;
            }
            Rates = await _billing.RatesAsync(Card.Code, History, ct) ?? new();
            Modes = await _billing.ProviderBillingModesAsync(ct);
        });
        return found ? Page() : NotFound();
    }

    private object Back => new { code = Code, history = History };

    public Task<IActionResult> OnPostUpdateCardAsync(string? name, string? description, bool isActive, bool isDefault, CancellationToken ct) =>
        RunAsync(() => _billing.UpdateRateCardAsync(Code, new RateCardUpdateRequest((name ?? "").Trim(), Clean(description), isActive, isDefault), ct),
            "Rate card saved.", Back);

    public Task<IActionResult> OnPostAddRateAsync(string? eventType, string? countryCode, string? @operator, string? provider, string? providerBilling,
        string? unit, decimal providerCost, decimal clientRate, string? effectiveFrom, string? notes, CancellationToken ct) =>
        RunAsync(() =>
        {
            DateTimeOffset? from = null;
            if (!string.IsNullOrWhiteSpace(effectiveFrom))
            {
                if (!DateTime.TryParse(effectiveFrom, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
                    throw new ArgumentException("Effective from isn't a valid date and time.");
                from = new DateTimeOffset(parsed, TimeSpan.Zero);
            }
            return _billing.AddRateAsync(Code, new AddRateRequest((eventType ?? "").Trim(), Clean(countryCode)?.ToUpperInvariant(), Clean(@operator),
                Clean(provider)?.ToLowerInvariant(), Clean(unit), providerCost, clientRate, from, null, Clean(notes), Clean(providerBilling)), ct);
        }, "Rate added. The previous version of the same rate (if any) ends when this one starts.", Back);

    public Task<IActionResult> OnPostCloseRateAsync(Guid rateId, CancellationToken ct) =>
        RunAsync(() => _billing.CloseRateAsync(rateId, ct), "Rate closed: it no longer prices new usage.", Back);
}
