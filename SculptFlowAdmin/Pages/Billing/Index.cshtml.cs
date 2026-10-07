using SculptFlowAdmin.Business.Contracts.Services.Billing;
using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Billing;

public class IndexModel : MainAppPageModel
{
    private readonly IBillingAdminService _billing;

    public IndexModel(IBillingAdminService billing) => _billing = billing;

    public string? Q { get; set; }
    public string? Status { get; set; }
    public List<BillingAccountRow> Accounts { get; private set; } = new();
    public List<BillingAccountRow> Shown { get; private set; } = new();
    public string Currency => Accounts.FirstOrDefault()?.Currency ?? "";

    public async Task OnGetAsync(string? q, string? status, CancellationToken ct)
    {
        Q = Clean(q);
        Status = Clean(status);
        await LoadAsync(async () => Accounts = await _billing.AccountsAsync(ct));
        Shown = Accounts
            .Where(a => Q is null || a.ClinicName.Contains(Q, StringComparison.OrdinalIgnoreCase) || (a.PlanCode ?? "").Contains(Q, StringComparison.OrdinalIgnoreCase))
            .Where(a => Status is null || (Status == "none" ? a.SubscriptionStatus is null : a.SubscriptionStatus == Status))
            .OrderBy(a => a.ClinicName)
            .ToList();
    }
}
