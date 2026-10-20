using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.BusinessTypes;
using Omni.Admin.Entities.Requests.BusinessTypes;
using Omni.Admin.Entities.Responses.BusinessTypes;

namespace Omni.Admin.Business.Services.BusinessTypes;

public class BusinessTypeAdminService : IBusinessTypeAdminService
{
    private readonly IBusinessTypesApiClient _api;
    private readonly IAdminAudit _audit;

    public BusinessTypeAdminService(IBusinessTypesApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<List<BusinessTypeResponse>> ListAsync(CancellationToken ct = default) => _api.ListAsync(ct);

    public Task<BusinessTypeResponse?> GetAsync(string code, CancellationToken ct = default) => _api.GetAsync(code, ct);

    public async Task<BusinessTypeResponse> CreateAsync(SaveBusinessTypeRequest request, CancellationToken ct = default)
    {
        var created = await _api.CreateAsync(request, ct);
        await _audit.LogAsync("business_type.created", "business_type", created.Code,
            details: new { created.Code, created.Name, stages = created.Stages.Count }, ct: ct);
        return created;
    }

    public async Task<BusinessTypeResponse> UpdateAsync(string code, SaveBusinessTypeRequest request, CancellationToken ct = default)
    {
        var before = await _api.GetAsync(code, ct) ?? throw new KeyNotFoundException();
        var saved = await _api.UpdateAsync(code, request, ct);
        await _audit.LogAsync("business_type.updated", "business_type", code,
            details: new
            {
                before = new { before.Name, before.Terminology, before.AiProfile, before.SortOrder, stages = before.Stages.Select(s => s.Key) },
                after = new { saved.Name, saved.Terminology, saved.AiProfile, saved.SortOrder, stages = saved.Stages.Select(s => s.Key) }
            }, ct: ct);
        return saved;
    }

    public async Task SetActiveAsync(string code, bool active, CancellationToken ct = default)
    {
        await _api.SetActiveAsync(code, active, ct);
        await _audit.LogAsync(active ? "business_type.activated" : "business_type.deactivated", "business_type", code, ct: ct);
    }
}
