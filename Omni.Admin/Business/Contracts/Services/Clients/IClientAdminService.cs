using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Requests.Clients;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.Services.Clients;

/// <summary>Clients, through the main app's clients API; every change is audited.</summary>
public interface IClientAdminService
{
    Task<PagedResult<ClientRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct = default);

    Task<List<ClientOption>> OptionsAsync(CancellationToken ct = default);

    Task<ClientDetail?> GetAsync(Guid id, CancellationToken ct = default);

    Task<ClientCounts> CountsAsync(Guid id, CancellationToken ct = default);

    Task UpdateAsync(Guid id, ClientUpdate update, CancellationToken ct = default);

    Task SetActiveAsync(Guid id, bool active, CancellationToken ct = default);

    /// <summary>The client's AI bot switches and whether its plan includes the bot; null when the client doesn't exist.</summary>
    Task<InboxSettingsDto?> GetInboxSettingsAsync(Guid id, CancellationToken ct = default);

    Task UpdateInboxSettingsAsync(Guid id, UpdateInboxSettingsRequest request, CancellationToken ct = default);

    /// <summary>What moving the client to another business type would do to its contacts' stages (changes nothing).</summary>
    Task<BusinessTypeChangePreview> PreviewBusinessTypeAsync(Guid id, string toCode, CancellationToken ct = default);

    Task ChangeBusinessTypeAsync(Guid id, string toCode, CancellationToken ct = default);
}
