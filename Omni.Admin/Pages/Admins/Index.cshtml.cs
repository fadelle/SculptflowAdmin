using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Admins;
using Omni.Admin.Business.Contracts.Services.Auth;
using Omni.Admin.Business.Services.Auth;
using Omni.Admin.Common.Statics;
using Omni.Admin.Entities.Dtos.Admins;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Admins;

/// <summary>Admin users, kept by the main app (identity.users with role admin) through its platform-admin admins API.</summary>
public class IndexModel : AdminPageModel
{
    private readonly IAdminAccountService _admins;
    private readonly IAdminAuthService _auth;

    public IndexModel(IAdminAccountService admins, IAdminAuthService auth)
    {
        _admins = admins;
        _auth = auth;
    }

    public IReadOnlyList<AdminRow> Admins { get; private set; } = [];
    public string? Me => AdminClaims.Id(User);

    public async Task OnGetAsync(CancellationToken ct) => Admins = await _admins.ListAsync(ct);

    public Task<IActionResult> OnPostCreateAsync(string? email, string? fullName, string? password, CancellationToken ct) =>
        RunAsync(() => _admins.CreateAsync(email, fullName, password, ct), "Admin added.");

    public Task<IActionResult> OnPostSetActiveAsync(string id, bool active, CancellationToken ct) =>
        RunAsync(() => _admins.SetActiveAsync(id, active, Me, ct),
            active ? "Admin activated." : "Admin deactivated. They are signed out on their next click.");

    /// <summary>A new password signs that admin out everywhere (QA t6-admin-13). Changing your own keeps this session: its cookie is
    /// renewed with the new session stamp.</summary>
    public async Task<IActionResult> OnPostPasswordAsync(string id, string? password, CancellationToken ct)
    {
        var own = id == Me;
        var result = await RunAsync(() => _admins.SetPasswordAsync(id, password, ct),
            own ? "Password changed. Your other sessions are signed out." : "Password changed. They are signed out of every open session.");
        if (own && await _auth.GetActiveAdminAsync(id, ct) is { } me)
        {
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, AdminAuthService.ToPrincipal(me));
        }
        return result;
    }
}
