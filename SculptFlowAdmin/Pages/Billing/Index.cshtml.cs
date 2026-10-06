using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Billing;

public class IndexModel : BillingPageModel
{
    private readonly IBillingApiClient _api;

    public IndexModel(IBillingApiClient api) => _api = api;

    public string? Q { get; set; }
    public string? Status { get; set; }
    public List<BillingAccountRow> Accounts { get; private set; } = new();
    public List<BillingAccountRow> Shown { get; private set; } = new();
    public string Currency => Accounts.FirstOrDefault()?.Currency ?? "";

    public async Task OnGetAsync(string? q, string? status, CancellationToken ct)
    {
        Q = Clean(q);
        Status = Clean(status);
        await LoadAsync(async () => Accounts = await _api.AccountsAsync(ct));
        Shown = Accounts
            .Where(a => Q is null || a.ClinicName.Contains(Q, StringComparison.OrdinalIgnoreCase) || (a.PlanCode ?? "").Contains(Q, StringComparison.OrdinalIgnoreCase))
            .Where(a => Status is null || (Status == "none" ? a.SubscriptionStatus is null : a.SubscriptionStatus == Status))
            .OrderBy(a => a.ClinicName)
            .ToList();
    }
}
