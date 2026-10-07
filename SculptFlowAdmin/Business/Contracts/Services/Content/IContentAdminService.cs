using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Contracts.Services.Content;

/// <summary>Campaigns, templates, procedures and the knowledge base, through the main app's content API; every change is
/// audited.</summary>
public interface IContentAdminService
{
    Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clinicId, string? status, int page, CancellationToken ct = default);

    Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct = default);

    Task<List<TemplateRow>> ListTemplatesAsync(Guid? clinicId, string? status, CancellationToken ct = default);

    Task<List<ProcedureRow>> ListProceduresAsync(Guid? clinicId, CancellationToken ct = default);

    Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clinicId, string? search, bool? active, int page,
        CancellationToken ct = default);

    Task<KnowledgeDocDetail?> GetDocumentAsync(Guid id, CancellationToken ct = default);

    Task<SearchSettingsDetail?> SearchSettingsAsync(Guid clinicId, CancellationToken ct = default);

    Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clinicId, CancellationToken ct = default);

    Task CancelCampaignAsync(Guid id, CancellationToken ct = default);

    Task SetProcedureActiveAsync(Guid id, bool active, CancellationToken ct = default);

    Task SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct = default);

    Task UpdateSearchSettingsAsync(Guid clinicId, int topK, double minimumSimilarity, CancellationToken ct = default);
}
