using Omni.Admin.Entities.Requests.BusinessTypes;
using Omni.Admin.Entities.Responses.BusinessTypes;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IBusinessTypesApiClient
{
    Task<List<BusinessTypeResponse>> ListAsync(CancellationToken ct);

    /// <summary>Null when there is no such business type.</summary>
    Task<BusinessTypeResponse?> GetAsync(string code, CancellationToken ct);

    Task<BusinessTypeResponse> CreateAsync(SaveBusinessTypeRequest request, CancellationToken ct);

    Task<BusinessTypeResponse> UpdateAsync(string code, SaveBusinessTypeRequest request, CancellationToken ct);

    Task<BusinessTypeResponse> SetActiveAsync(string code, bool active, CancellationToken ct);
}
