using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Billing;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Requests.Billing;
using Omni.Admin.Entities.Responses.Billing;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Billing;

/// <summary>Rate cards: the default prices, plan cards and per-client custom pricing.</summary>
public class RateCardsModel : MainAppPageModel
{
    private readonly IBillingAdminService _billing;

    public RateCardsModel(IBillingAdminService billing) => _billing = billing;

    public List<RateCardResponse> Cards { get; private set; } = new();
    public List<PlanResponse> Plans { get; private set; } = new();
    public List<ClientOption> Clients { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(async () =>
        {
            Clients = await _billing.ClientOptionsAsync(ct);
            Cards = await _billing.RateCardsAsync(ct);
            Plans = await _billing.PlansAsync(ct);
        });
    }

    public string ClientName(Guid? id) => id is null ? "" : Clients.FirstOrDefault(c => c.Id == id)?.Name ?? id.Value.ToString()[..8];

    public Task<IActionResult> OnPostCreateAsync(string? code, string? name, string? description, Guid? clientId, bool isDefault, CancellationToken ct) =>
        RunAsync(() => _billing.CreateRateCardAsync(new RateCardRequest((code ?? "").Trim(), (name ?? "").Trim(), Clean(description), clientId, isDefault), ct),
            "Rate card created. Add its rates next.", new { });
}
