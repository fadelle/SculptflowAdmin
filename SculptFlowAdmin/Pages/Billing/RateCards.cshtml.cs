using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Requests.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Billing;

/// <summary>Rate cards: the default prices, plan cards and per-clinic custom pricing.</summary>
public class RateCardsModel : BillingPageModel
{
    private readonly IBillingApiClient _api;
    private readonly IClinicAdminService _clinics;

    public RateCardsModel(IBillingApiClient api, IClinicAdminService clinics)
    {
        _api = api;
        _clinics = clinics;
    }

    public List<RateCardResponse> Cards { get; private set; } = new();
    public List<PlanResponse> Plans { get; private set; } = new();
    public List<ClinicOption> Clinics { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Clinics = await _clinics.OptionsAsync(ct);
        await LoadAsync(async () =>
        {
            Cards = await _api.RateCardsAsync(ct);
            Plans = await _api.PlansAsync(ct);
        });
    }

    public string ClinicName(Guid? id) => id is null ? "" : Clinics.FirstOrDefault(c => c.Id == id)?.Name ?? id.Value.ToString()[..8];

    public Task<IActionResult> OnPostCreateAsync(string? code, string? name, string? description, Guid? clinicId, bool isDefault, CancellationToken ct) =>
        RunAsync(() => _api.CreateRateCardAsync(new RateCardRequest((code ?? "").Trim(), (name ?? "").Trim(), Clean(description), clinicId, isDefault), ct),
            "Rate card created. Add its rates next.", new { });
}
