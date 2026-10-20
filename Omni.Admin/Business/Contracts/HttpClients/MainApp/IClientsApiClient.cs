using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Requests.Clients;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IClientsApiClient
{
    Task<PagedResult<ClientRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct);

    Task<List<ClientOption>> OptionsAsync(CancellationToken ct);

    Task<ClientDetail?> GetAsync(Guid id, CancellationToken ct);

    Task<ClientCounts> CountsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> UpdateAsync(Guid id, ClientUpdate update, CancellationToken ct);

    Task<PlatformAdminChange> SetActiveAsync(Guid id, bool active, CancellationToken ct);

    /// <summary>Null when the client doesn't exist.</summary>
    Task<InboxSettingsDto?> GetInboxSettingsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> UpdateInboxSettingsAsync(Guid id, UpdateInboxSettingsRequest request, CancellationToken ct);

    Task<PlatformAdminChange> SetDefaultLanguageAsync(Guid id, string language, CancellationToken ct);

    /// <summary>What moving the client to another business type would do to its contacts' stages (changes nothing).</summary>
    Task<BusinessTypeChangePreview> PreviewBusinessTypeAsync(Guid id, string toCode, CancellationToken ct);

    Task<PlatformAdminChange> ChangeBusinessTypeAsync(Guid id, string toCode, CancellationToken ct);
}
