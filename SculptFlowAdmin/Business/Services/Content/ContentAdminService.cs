using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Services.Content;

public class ContentAdminService : IContentAdminService
{
    private readonly IContentApiClient _api;
    private readonly IAdminAudit _audit;

    public ContentAdminService(IContentApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clinicId, string? status, int page, CancellationToken ct = default) =>
        _api.ListCampaignsAsync(clinicId, status, page, ct);

    public Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct = default) => _api.GetCampaignAsync(id, ct);

    public Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct = default) =>
        _api.RecipientsAsync(campaignId, status, page, ct);

    public Task<List<TemplateRow>> ListTemplatesAsync(Guid? clinicId, string? status, CancellationToken ct = default) =>
        _api.ListTemplatesAsync(clinicId, status, ct);

    public Task<List<ProcedureRow>> ListProceduresAsync(Guid? clinicId, CancellationToken ct = default) => _api.ListProceduresAsync(clinicId, ct);

    public Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clinicId, string? search, bool? active, int page,
        CancellationToken ct = default) =>
        _api.ListDocumentsAsync(clinicId, search, active, page, ct);

    public Task<KnowledgeDocDetail?> GetDocumentAsync(Guid id, CancellationToken ct = default) => _api.GetDocumentAsync(id, ct);

    public Task<SearchSettingsDetail?> SearchSettingsAsync(Guid clinicId, CancellationToken ct = default) => _api.SearchSettingsAsync(clinicId, ct);

    public Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clinicId, CancellationToken ct = default) => _api.ListWebsitesAsync(clinicId, ct);

    public async Task CancelCampaignAsync(Guid id, CancellationToken ct = default)
    {
        var change = await _api.CancelCampaignAsync(id, ct);
        await _audit.LogAsync("campaign.cancelled", "campaign", id, change.ClinicId, ct: ct);
    }

    public async Task SetProcedureActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var change = await _api.SetProcedureActiveAsync(id, active, ct);
        await _audit.LogAsync(active ? "procedure.activated" : "procedure.deactivated", "procedure", id, change.ClinicId, ct: ct);
    }

    public async Task SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var change = await _api.SetDocumentActiveAsync(id, active, ct);
        await _audit.LogAsync(active ? "knowledge.activated" : "knowledge.deactivated", "knowledge_document", id, change.ClinicId, ct: ct);
    }

    public async Task UpdateSearchSettingsAsync(Guid clinicId, int topK, double minimumSimilarity, CancellationToken ct = default)
    {
        var before = await _api.SearchSettingsAsync(clinicId, ct);
        await _api.UpdateSearchSettingsAsync(clinicId, topK, minimumSimilarity, ct);
        await _audit.LogAsync("knowledge.settings_updated", "knowledge_search_settings", before?.Id, clinicId,
            new { before = before is null ? null : new { before.TopK, before.MinimumSimilarity }, after = new { topK, minimumSimilarity } }, ct);
    }
}
