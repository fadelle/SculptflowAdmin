using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Requests.Clinics;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

public interface IClinicsApiClient
{
    Task<PagedResult<ClinicRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct);

    Task<List<ClinicOption>> OptionsAsync(CancellationToken ct);

    Task<ClinicDetail?> GetAsync(Guid id, CancellationToken ct);

    Task<ClinicCounts> CountsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> UpdateAsync(Guid id, ClinicUpdate update, CancellationToken ct);

    Task<PlatformAdminChange> SetActiveAsync(Guid id, bool active, CancellationToken ct);
}
