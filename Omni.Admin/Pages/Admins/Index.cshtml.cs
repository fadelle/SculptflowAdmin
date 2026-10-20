using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Admins;
using Omni.Admin.Common.Statics;
using Omni.Admin.Entities.Dtos.Admins;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Admins;

/// <summary>Admin users, kept by the main app (identity.users with role admin) through its platform-admin admins API.</summary>
public class IndexModel : AdminPageModel
{
    private readonly IAdminAccountService _admins;

    public IndexModel(IAdminAccountService admins)
    {
        _admins = admins;
    }

    public IReadOnlyList<AdminRow> Admins { get; private set; } = [];
    public string? Me => AdminClaims.Id(User);

    public async Task OnGetAsync(CancellationToken ct) => Admins = await _admins.ListAsync(ct);

    public Task<IActionResult> OnPostCreateAsync(string? email, string? fullName, string? password, CancellationToken ct) =>
        RunAsync(() => _admins.CreateAsync(email, fullName, password, ct), "Admin added.");

    public Task<IActionResult> OnPostSetActiveAsync(string id, bool active, CancellationToken ct) =>
        RunAsync(() => _admins.SetActiveAsync(id, active, Me, ct),
            active ? "Admin activated." : "Admin deactivated. They are signed out on their next click.");

    public Task<IActionResult> OnPostPasswordAsync(string id, string? password, CancellationToken ct) =>
        RunAsync(() => _admins.SetPasswordAsync(id, password, ct), "Password changed.");
}
