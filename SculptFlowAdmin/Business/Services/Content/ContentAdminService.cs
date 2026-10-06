using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Persistence.Contracts;
using SculptFlowAdmin.Persistence.Contracts.Content;

namespace SculptFlowAdmin.Business.Services.Content;

/// <summary>Campaigns, WhatsApp templates, procedures and the Knowledge Base across clinics.</summary>
public class ContentAdminService : IContentAdminService
{
    private readonly IContentAdminRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminAudit _audit;

    public ContentAdminService(IContentAdminRepository repository, IUnitOfWork unitOfWork, IAdminAudit audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clinicId, string? status, int page, CancellationToken ct = default) =>
        _repository.ListCampaignsAsync(clinicId, status, page, ct);

    public Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetCampaignAsync(id, ct);

    public Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct = default) =>
        _repository.RecipientsAsync(campaignId, status, page, ct);

    public Task<List<TemplateRow>> ListTemplatesAsync(Guid? clinicId, string? status, CancellationToken ct = default) =>
        _repository.ListTemplatesAsync(clinicId, status, ct);

    public Task<List<ProcedureRow>> ListProceduresAsync(Guid? clinicId, CancellationToken ct = default) =>
        _repository.ListProceduresAsync(clinicId, ct);

    public Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clinicId, string? search, bool? active, int page,
        CancellationToken ct = default) =>
        _repository.ListDocumentsAsync(clinicId, search, active, page, ct);

    public Task<KnowledgeDocument?> GetDocumentAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetDocumentAsync(id, ct);

    public Task<KnowledgeSearchSettings?> SearchSettingsAsync(Guid clinicId, CancellationToken ct = default) =>
        _repository.SearchSettingsAsync(clinicId, ct);

    public Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clinicId, CancellationToken ct = default) =>
        _repository.ListWebsitesAsync(clinicId, ct);

    // ------------------------------------------------------------------ campaigns

    /// <summary>Mirrors the main app's CampaignService.CancelAsync: outstanding recipients are failed, sent ones untouched.</summary>
    public async Task CancelCampaignAsync(Guid id, CancellationToken ct = default)
    {
        var campaign = await _repository.GetCampaignForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (campaign.Status is CampaignStatus.Completed or CampaignStatus.Cancelled)
            throw new AdminRuleException($"Campaign is already '{campaign.Status}'.");

        var now = DateTimeOffset.UtcNow;
        var outstanding = await _repository.ListOutstandingRecipientsAsync(id, ct);
        foreach (var r in outstanding)
        {
            r.Status = CampaignRecipientStatus.Failed;
            r.FailureReason = "Campaign cancelled.";
            r.FailedAt = now;
            r.UpdatedAt = now;
        }
        var previous = campaign.Status;
        campaign.Status = CampaignStatus.Cancelled;
        campaign.UpdatedAt = now;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("campaign.cancelled", "campaign", id, campaign.ClinicId, new { from = previous, recipientsStopped = outstanding.Count }, ct);
    }

    // ------------------------------------------------------------------ templates

    // ------------------------------------------------------------------ procedures

    /// <summary>Procedures are never hard-deleted in SculptFlow; inactive ones disappear from the AI's get_procedures.</summary>
    public async Task SetProcedureActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var p = await _repository.GetProcedureForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (p.IsActive == active) return;
        p.IsActive = active;
        p.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync(active ? "procedure.activated" : "procedure.deactivated", "procedure", id, p.ClinicId, new { p.Name }, ct);
    }

    // ------------------------------------------------------------------ knowledge base

    /// <summary>Inactive documents are skipped by the AI's knowledge search (chunks stay; reactivating needs no re-embed).</summary>
    public async Task SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var d = await _repository.GetDocumentForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (d.IsActive == active) return;
        d.IsActive = active;
        d.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync(active ? "knowledge.activated" : "knowledge.deactivated", "knowledge_document", id, d.ClinicId, new { d.Title }, ct);
    }

    /// <summary>Only the retrieval knobs (top K, minimum similarity). Chunk size/overlap only apply on re-save, which needs
    /// the main app's embedding pipeline, so they stay a clinic-side setting.</summary>
    public async Task UpdateSearchSettingsAsync(Guid clinicId, int topK, double minimumSimilarity, CancellationToken ct = default)
    {
        if (topK is < 1 or > 20) throw new AdminRuleException("Top K must be between 1 and 20.");
        if (minimumSimilarity is < 0 or > 1) throw new AdminRuleException("Minimum similarity must be between 0 and 1.");
        var s = await _repository.GetSearchSettingsForUpdateAsync(clinicId, ct)
                ?? throw new AdminRuleException("This clinic has no knowledge search settings row yet.");
        var before = new { s.TopK, s.MinimumSimilarity };
        s.TopK = topK;
        s.MinimumSimilarity = minimumSimilarity;
        s.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("knowledge.settings_updated", "knowledge_search_settings", s.Id, clinicId,
            new { before, after = new { topK, minimumSimilarity } }, ct);
    }

}
