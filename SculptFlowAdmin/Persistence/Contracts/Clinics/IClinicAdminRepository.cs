using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Persistence.Contracts.Clinics;

public interface IClinicAdminRepository
{
    Task<PagedResult<ClinicRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct = default);

    /// <summary>Every clinic as (id, name), for the clinic filter on list pages.</summary>
    Task<List<ClinicOption>> OptionsAsync(CancellationToken ct = default);

    Task<Clinic?> GetAsync(Guid id, CancellationToken ct = default);

    Task<ClinicCounts> CountsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<Clinic?> GetForUpdateAsync(Guid id, CancellationToken ct = default);
}
