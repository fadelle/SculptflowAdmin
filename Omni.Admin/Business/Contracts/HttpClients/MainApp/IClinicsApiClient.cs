using Omni.Admin.Entities.Dtos.Clinics;
using Omni.Admin.Entities.Requests.Clinics;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IClinicsApiClient
{
    Task<PagedResult<ClinicRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct);

    Task<List<ClinicOption>> OptionsAsync(CancellationToken ct);

    Task<ClinicDetail?> GetAsync(Guid id, CancellationToken ct);

    Task<ClinicCounts> CountsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> UpdateAsync(Guid id, ClinicUpdate update, CancellationToken ct);

    Task<PlatformAdminChange> SetActiveAsync(Guid id, bool active, CancellationToken ct);

    /// <summary>Null when the clinic doesn't exist.</summary>
    Task<InboxSettingsDto?> GetInboxSettingsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> UpdateInboxSettingsAsync(Guid id, UpdateInboxSettingsRequest request, CancellationToken ct);
}
