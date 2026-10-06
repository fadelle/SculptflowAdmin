using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Requests.Clinics;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Business.Contracts.Services.Clinics;

public interface IClinicAdminService
{
    Task<PagedResult<ClinicRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct = default);

    /// <summary>Every clinic as (id, name), for the clinic filter on list pages.</summary>
    Task<List<ClinicOption>> OptionsAsync(CancellationToken ct = default);

    Task<Clinic?> GetAsync(Guid id, CancellationToken ct = default);

    Task<ClinicCounts> CountsAsync(Guid id, CancellationToken ct = default);

    Task UpdateAsync(Guid id, ClinicUpdate update, CancellationToken ct = default);

    Task SetActiveAsync(Guid id, bool active, CancellationToken ct = default);
}
