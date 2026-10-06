using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Entities.Requests.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Billing;

/// <summary>One rate card: its details and its versioned rates (add a new version, close one).</summary>
public class RatesModel : BillingPageModel
{
    private readonly IBillingApiClient _api;

    public RatesModel(IBillingApiClient api) => _api = api;

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
            Card = (await _api.RateCardsAsync(ct)).FirstOrDefault(c => c.Code == Code.Trim().ToLowerInvariant());
            if (Card is null)
            {
                found = false;
                return;
            }
            Rates = await _api.RatesAsync(Card.Code, History, ct) ?? new();
            Modes = await _api.ProviderBillingModesAsync(ct);
        });
        return found ? Page() : NotFound();
    }

    private object Back => new { code = Code, history = History };

    public Task<IActionResult> OnPostUpdateCardAsync(string? name, string? description, bool isActive, bool isDefault, CancellationToken ct) =>
        RunAsync(() => _api.UpdateRateCardAsync(Code, new RateCardUpdateRequest((name ?? "").Trim(), Clean(description), isActive, isDefault), ct),
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
            return _api.AddRateAsync(Code, new AddRateRequest((eventType ?? "").Trim(), Clean(countryCode)?.ToUpperInvariant(), Clean(@operator),
                Clean(provider)?.ToLowerInvariant(), Clean(unit), providerCost, clientRate, from, null, Clean(notes), Clean(providerBilling)), ct);
        }, "Rate added. The previous version of the same rate (if any) ends when this one starts.", Back);

    public Task<IActionResult> OnPostCloseRateAsync(Guid rateId, CancellationToken ct) =>
        RunAsync(() => _api.CloseRateAsync(rateId, ct), "Rate closed: it no longer prices new usage.", Back);
}
