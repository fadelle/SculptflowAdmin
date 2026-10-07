using SculptFlowAdmin.Entities.Requests.Configuration;
using SculptFlowAdmin.Entities.Responses.Configuration;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin configuration API (/api/platform-admin/settings). HTTP only.</summary>
public interface ISettingsApiClient
{
    Task<List<SettingResponse>> ListAsync(CancellationToken ct);

    Task SetAsync(string section, string key, SetSettingRequest request, CancellationToken ct);

    Task ResetAsync(string section, string key, CancellationToken ct);

    bool IsConfigured { get; }
}
