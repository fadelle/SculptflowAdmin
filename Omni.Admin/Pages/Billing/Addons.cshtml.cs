using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Billing;
using Omni.Admin.Entities.Dtos.Billing;
using Omni.Admin.Entities.Requests.Billing;
using Omni.Admin.Entities.Responses.Billing;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Billing;

/// <summary>Paid add-ons: extra units of a plan limit (users, channels, AI bots, WhatsApp numbers) that a client can have on
/// top of its plan. Each client's units are set on its billing page.</summary>
public class AddonsModel : MainAppPageModel
{
    private readonly IBillingAdminService _billing;

    public AddonsModel(IBillingAdminService billing) => _billing = billing;

    [BindProperty(SupportsGet = true)] public string? Edit { get; set; }
    public List<AddonResponse> Addons { get; private set; } = new();
    public List<EntitlementDefinition> Limits { get; private set; } = new();
    public AddonResponse? Editing { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(async () =>
        {
            Addons = await _billing.AddonsAsync(ct);
            Limits = (await _billing.EntitlementsAsync(ct)).Where(d => d.Kind != "feature").ToList();
            Editing = string.IsNullOrWhiteSpace(Edit) ? null : Addons.FirstOrDefault(a => a.Code == Edit.Trim().ToLowerInvariant());
        });
    }

    public string LimitLabel(string key) => Limits.FirstOrDefault(l => l.Key == key)?.Label ?? key;

    public Task<IActionResult> OnPostSaveAsync(string? originalCode, string? code, string? name, string? entitlementKey, decimal unitPrice,
        bool isActive, int sortOrder, CancellationToken ct) =>
        RunAsync(() =>
        {
            var key = string.IsNullOrWhiteSpace(originalCode) ? Clean(entitlementKey) : null;
            return _billing.SaveAddonAsync((originalCode ?? code ?? "").Trim().ToLowerInvariant(),
                new AddonRequest((name ?? "").Trim(), key, unitPrice, isActive, sortOrder), ct);
        }, string.IsNullOrWhiteSpace(originalCode) ? "Add-on created." : "Add-on saved. A new price applies from each client's next period.",
        string.IsNullOrWhiteSpace(originalCode) ? new { } : new { edit = originalCode });
}
