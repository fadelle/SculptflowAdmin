using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Auth;
using SculptFlowAdmin.Common.Statics;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Admins;

public class IndexModel : AdminPageModel
{
    private readonly IAdminAuthService _auth;
    private readonly IAdminAudit _audit;

    public IndexModel(IAdminAuthService auth, IAdminAudit audit)
    {
        _auth = auth;
        _audit = audit;
    }

    public List<AdminUser> Admins { get; private set; } = new();
    public Guid? Me => AdminClaims.Id(User);

    public async Task OnGetAsync(CancellationToken ct) => Admins = await _auth.ListAsync(ct);

    public Task<IActionResult> OnPostCreateAsync(string email, string fullName, string password, CancellationToken ct) =>
        RunAsync(async () =>
        {
            var user = await _auth.CreateAsync(email, fullName, password, ct);
            await _audit.LogAsync("admin.created", "admin_user", user.Id, details: new { user.Email }, ct: ct);
        }, "Admin added.");

    public Task<IActionResult> OnPostSetActiveAsync(Guid id, bool active, CancellationToken ct) =>
        RunAsync(async () =>
        {
            await _auth.SetActiveAsync(id, active, Me, ct);
            await _audit.LogAsync(active ? "admin.activated" : "admin.deactivated", "admin_user", id, ct: ct);
        }, active ? "Admin activated." : "Admin deactivated. They are signed out on their next click.");

    public Task<IActionResult> OnPostPasswordAsync(Guid id, string password, CancellationToken ct) =>
        RunAsync(async () =>
        {
            await _auth.SetPasswordAsync(id, password, ct);
            await _audit.LogAsync("admin.password_set", "admin_user", id, ct: ct);
        }, "Password changed.");
}
