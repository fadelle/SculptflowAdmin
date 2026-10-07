using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Requests.Clinics;
using SculptFlowAdmin.Entities.Requests.Content;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin clinics API (/api/platform-admin/clinics).</summary>
public class ClinicsApiClient : MainAppApiClient, IClinicsApiClient
{
    public const string Prefix = "api/platform-admin/clinics";

    public ClinicsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<ClinicsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Clinics")
    {
    }

    public Task<PagedResult<ClinicRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<ClinicRow>>(Query(("search", search), ("active", active?.ToString().ToLowerInvariant()), ("page", page)), ct);

    public Task<List<ClinicOption>> OptionsAsync(CancellationToken ct) => GetRequiredAsync<List<ClinicOption>>("/options", ct);

    public Task<ClinicDetail?> GetAsync(Guid id, CancellationToken ct) => GetAsync<ClinicDetail>($"/{id}", ct);

    public Task<ClinicCounts> CountsAsync(Guid id, CancellationToken ct) => GetRequiredAsync<ClinicCounts>($"/{id}/counts", ct);

    public Task<PlatformAdminChange> UpdateAsync(Guid id, ClinicUpdate update, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{id}", update, null, false, ct);

    public Task<PlatformAdminChange> SetActiveAsync(Guid id, bool active, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{id}/active", new ActiveBody(active), null, false, ct);
}
