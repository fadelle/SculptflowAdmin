using Omni.Admin.Entities.Dtos.Content;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.Services.Content;

/// <summary>Campaigns, templates, services and the knowledge base, through the main app's content API; every change is
/// audited.</summary>
public interface IContentAdminService
{
    Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clientId, string? status, int page, CancellationToken ct = default);

    Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct = default);

    Task<List<TemplateRow>> ListTemplatesAsync(Guid? clientId, string? status, CancellationToken ct = default);

    Task<List<ServiceRow>> ListServicesAsync(Guid? clientId, CancellationToken ct = default);

    Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clientId, string? search, bool? active, int page,
        CancellationToken ct = default);

    Task<KnowledgeDocDetail?> GetDocumentAsync(Guid id, CancellationToken ct = default);

    Task<SearchSettingsDetail?> SearchSettingsAsync(Guid clientId, CancellationToken ct = default);

    Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clientId, CancellationToken ct = default);

    Task CancelCampaignAsync(Guid id, CancellationToken ct = default);

    Task SetServiceActiveAsync(Guid id, bool active, CancellationToken ct = default);

    Task SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct = default);

    Task UpdateSearchSettingsAsync(Guid clientId, int topK, double minimumSimilarity, CancellationToken ct = default);
}
