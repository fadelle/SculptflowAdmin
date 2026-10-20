using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Admins;
using Omni.Admin.Business.Contracts.Services.Auth;
using Omni.Admin.Entities.Dtos.Admins;

namespace Omni.Admin.Business.Services.Admins;

public class AdminAccountService : IAdminAccountService
{
    /// <summary>The main app's rule (AccountRules.AdminMinPasswordLength), copied for the form hint; the main app checks it.</summary>
    public const int MinPasswordLength = 10;

    private readonly IAdminsApiClient _api;
    private readonly IAdminAuthService _auth;
    private readonly IAdminAudit _audit;

    public AdminAccountService(IAdminsApiClient api, IAdminAuthService auth, IAdminAudit audit)
    {
        _api = api;
        _auth = auth;
        _audit = audit;
    }

    public Task<IReadOnlyList<AdminRow>> ListAsync(CancellationToken ct = default) => _api.ListAsync(ct);

    public async Task<AdminRow> CreateAsync(string? email, string? fullName, string? password, CancellationToken ct = default)
    {
        var admin = await _api.CreateAsync(email?.Trim(), fullName?.Trim(), password, ct);
        await _audit.LogAsync("admin.created", "admin_user", admin.Id, details: new { admin.Email }, ct: ct);
        return admin;
    }

    public async Task<AdminRow> SetActiveAsync(string id, bool active, string? actingAdminId, CancellationToken ct = default)
    {
        var admin = await _api.SetActiveAsync(id, active, actingAdminId, ct);
        // Signed out on their next click, not when the remembered "still active" answer runs out.
        if (!active) _auth.Forget(id);
        await _audit.LogAsync(active ? "admin.activated" : "admin.deactivated", "admin_user", id, ct: ct);
        return admin;
    }

    public async Task<AdminRow> SetPasswordAsync(string id, string? password, CancellationToken ct = default)
    {
        var admin = await _api.SetPasswordAsync(id, password, ct);
        await _audit.LogAsync("admin.password_set", "admin_user", id, ct: ct);
        return admin;
    }
}
