using Omni.Admin.Entities.Dtos.Content;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IContentApiClient
{
    Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clientId, string? status, int page, CancellationToken ct);

    Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct);

    Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct);

    Task<PlatformAdminChange> CancelCampaignAsync(Guid id, CancellationToken ct);

    Task<List<TemplateRow>> ListTemplatesAsync(Guid? clientId, string? status, CancellationToken ct);

    Task<List<ServiceRow>> ListServicesAsync(Guid? clientId, CancellationToken ct);

    Task<PlatformAdminChange> SetServiceActiveAsync(Guid id, bool active, CancellationToken ct);

    Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clientId, string? search, bool? active, int page, CancellationToken ct);

    Task<KnowledgeDocDetail?> GetDocumentAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct);

    Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clientId, CancellationToken ct);

    Task<SearchSettingsDetail?> SearchSettingsAsync(Guid clientId, CancellationToken ct);

    Task<PlatformAdminChange> UpdateSearchSettingsAsync(Guid clientId, int topK, double minimumSimilarity, CancellationToken ct);
}
