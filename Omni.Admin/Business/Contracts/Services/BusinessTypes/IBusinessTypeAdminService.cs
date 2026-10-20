using Omni.Admin.Entities.Requests.BusinessTypes;
using Omni.Admin.Entities.Responses.BusinessTypes;

namespace Omni.Admin.Business.Contracts.Services.BusinessTypes;

/// <summary>Business types (names, terminology, AI profile, stages), through the main app's business types API; every
/// change is audited.</summary>
public interface IBusinessTypeAdminService
{
    Task<List<BusinessTypeResponse>> ListAsync(CancellationToken ct = default);

    Task<BusinessTypeResponse?> GetAsync(string code, CancellationToken ct = default);

    Task<BusinessTypeResponse> CreateAsync(SaveBusinessTypeRequest request, CancellationToken ct = default);

    Task<BusinessTypeResponse> UpdateAsync(string code, SaveBusinessTypeRequest request, CancellationToken ct = default);

    Task SetActiveAsync(string code, bool active, CancellationToken ct = default);
}
