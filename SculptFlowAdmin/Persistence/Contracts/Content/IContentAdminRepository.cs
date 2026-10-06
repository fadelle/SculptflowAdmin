using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Persistence.Contracts.Content;

public interface IContentAdminRepository
{
    Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clinicId, string? status, int page, CancellationToken ct = default);

    Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct = default);

    Task<List<TemplateRow>> ListTemplatesAsync(Guid? clinicId, string? status, CancellationToken ct = default);

    Task<List<ProcedureRow>> ListProceduresAsync(Guid? clinicId, CancellationToken ct = default);

    Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clinicId, string? search, bool? active, int page,
        CancellationToken ct = default);

    Task<KnowledgeDocument?> GetDocumentAsync(Guid id, CancellationToken ct = default);

    Task<KnowledgeSearchSettings?> SearchSettingsAsync(Guid clinicId, CancellationToken ct = default);

    Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clinicId, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<Campaign?> GetCampaignForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tracked: recipients still pending or queued.</summary>
    Task<List<CampaignRecipient>> ListOutstandingRecipientsAsync(Guid campaignId, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<Procedure?> GetProcedureForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<KnowledgeDocument?> GetDocumentForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<KnowledgeSearchSettings?> GetSearchSettingsForUpdateAsync(Guid clinicId, CancellationToken ct = default);
}
