using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

public interface IContentApiClient
{
    Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clinicId, string? status, int page, CancellationToken ct);

    Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct);

    Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct);

    Task<PlatformAdminChange> CancelCampaignAsync(Guid id, CancellationToken ct);

    Task<List<TemplateRow>> ListTemplatesAsync(Guid? clinicId, string? status, CancellationToken ct);

    Task<List<ProcedureRow>> ListProceduresAsync(Guid? clinicId, CancellationToken ct);

    Task<PlatformAdminChange> SetProcedureActiveAsync(Guid id, bool active, CancellationToken ct);

    Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clinicId, string? search, bool? active, int page, CancellationToken ct);

    Task<KnowledgeDocDetail?> GetDocumentAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct);

    Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clinicId, CancellationToken ct);

    Task<SearchSettingsDetail?> SearchSettingsAsync(Guid clinicId, CancellationToken ct);

    Task<PlatformAdminChange> UpdateSearchSettingsAsync(Guid clinicId, int topK, double minimumSimilarity, CancellationToken ct);
}
