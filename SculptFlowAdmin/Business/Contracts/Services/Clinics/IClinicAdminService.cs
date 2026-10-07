using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Requests.Clinics;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Contracts.Services.Clinics;

/// <summary>Clinics, through the main app's clinics API; every change is audited.</summary>
public interface IClinicAdminService
{
    Task<PagedResult<ClinicRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct = default);

    Task<List<ClinicOption>> OptionsAsync(CancellationToken ct = default);

    Task<ClinicDetail?> GetAsync(Guid id, CancellationToken ct = default);

    Task<ClinicCounts> CountsAsync(Guid id, CancellationToken ct = default);

    Task UpdateAsync(Guid id, ClinicUpdate update, CancellationToken ct = default);

    Task SetActiveAsync(Guid id, bool active, CancellationToken ct = default);
}
