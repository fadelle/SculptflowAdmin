using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Configuration;
using SculptFlowAdmin.Entities.Requests.Configuration;
using SculptFlowAdmin.Entities.Responses.Configuration;

namespace SculptFlowAdmin.Business.Services.Configuration;

public class SettingsAdminService : ISettingsAdminService
{
    private readonly ISettingsApiClient _api;
    private readonly IAdminAudit _audit;

    public SettingsAdminService(ISettingsApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<List<SettingResponse>> ListAsync(CancellationToken ct) => _api.ListAsync(ct);

    public async Task SetAsync(string section, string key, string? value, string? note, bool isSecret, CancellationToken ct)
    {
        var request = new SetSettingRequest(value ?? string.Empty, string.IsNullOrWhiteSpace(note) ? null : note.Trim());
        await _api.SetAsync(section.Trim(), key.Trim(), request, ct);
        object details = isSecret ? new { Value = "(secret, not logged)", request.Note } : request;
        await _audit.LogAsync("config.setting_set", "config_setting", $"{section.Trim()}:{key.Trim()}", null, details, ct);
    }

    public async Task ResetAsync(string section, string key, CancellationToken ct)
    {
        await _api.ResetAsync(section.Trim(), key.Trim(), ct);
        await _audit.LogAsync("config.setting_reset", "config_setting", $"{section.Trim()}:{key.Trim()}", null, null, ct);
    }
}
