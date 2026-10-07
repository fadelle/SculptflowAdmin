using SculptFlowAdmin.Entities.Responses.Configuration;

namespace SculptFlowAdmin.Business.Contracts.Services.Configuration;

/// <summary>
/// What the Configuration page calls: the main app's settings (by section and key), read and written through its
/// platform-admin settings API. Every change is recorded in admin_audit_log.
/// </summary>
public interface ISettingsAdminService
{
    Task<List<SettingResponse>> ListAsync(CancellationToken ct);

    /// <summary>Saves a value. For a secret setting the audit log records that it was changed, never the value.</summary>
    Task SetAsync(string section, string key, string? value, string? note, bool isSecret, CancellationToken ct);

    Task ResetAsync(string section, string key, CancellationToken ct);
}
