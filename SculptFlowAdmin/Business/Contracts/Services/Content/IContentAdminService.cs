using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Business.Contracts.Services.Content;

public interface IContentAdminService
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

    /// <summary>Mirrors the main app's CampaignService.CancelAsync: outstanding recipients are failed, sent ones untouched.</summary>
    Task CancelCampaignAsync(Guid id, CancellationToken ct = default);

    /// <summary>Procedures are never hard-deleted in SculptFlow; inactive ones disappear from the AI's get_procedures.</summary>
    Task SetProcedureActiveAsync(Guid id, bool active, CancellationToken ct = default);

    /// <summary>Inactive documents are skipped by the AI's knowledge search (chunks stay; reactivating needs no re-embed).</summary>
    Task SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct = default);

    /// <summary>Only the retrieval knobs (top K, minimum similarity). Chunk size/overlap only apply on re-save, which needs
    /// the main app's embedding pipeline, so they stay a clinic-side setting.</summary>
    Task UpdateSearchSettingsAsync(Guid clinicId, int topK, double minimumSimilarity, CancellationToken ct = default);
}
