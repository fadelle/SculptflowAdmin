using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Requests.Clinics;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Services.Clinics;

public class ClinicAdminService : IClinicAdminService
{
    private readonly IClinicsApiClient _api;
    private readonly IAdminAudit _audit;

    public ClinicAdminService(IClinicsApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<PagedResult<ClinicRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct = default) =>
        _api.ListAsync(search, active, page, ct);

    public Task<List<ClinicOption>> OptionsAsync(CancellationToken ct = default) => _api.OptionsAsync(ct);

    public Task<ClinicDetail?> GetAsync(Guid id, CancellationToken ct = default) => _api.GetAsync(id, ct);

    public Task<ClinicCounts> CountsAsync(Guid id, CancellationToken ct = default) => _api.CountsAsync(id, ct);

    public async Task UpdateAsync(Guid id, ClinicUpdate update, CancellationToken ct = default)
    {
        var before = await _api.GetAsync(id, ct) ?? throw new KeyNotFoundException();
        await _api.UpdateAsync(id, update, ct);
        await _audit.LogAsync("clinic.updated", "clinic", id, id,
            new { before = new { before.Name, before.Phone, before.Email, before.Website, before.CountryCode, before.Timezone }, after = update }, ct);
    }

    public async Task SetActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        await _api.SetActiveAsync(id, active, ct);
        await _audit.LogAsync(active ? "clinic.activated" : "clinic.deactivated", "clinic", id, id, ct: ct);
    }
}
