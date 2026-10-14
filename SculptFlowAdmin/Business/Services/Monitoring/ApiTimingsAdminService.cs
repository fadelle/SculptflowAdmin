using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Services.Monitoring;
using SculptFlowAdmin.Entities.Responses.Monitoring;

namespace SculptFlowAdmin.Business.Services.Monitoring;

public class ApiTimingsAdminService : IApiTimingsAdminService
{
    private readonly IApiTimingsApiClient _api;

    public ApiTimingsAdminService(IApiTimingsApiClient api)
    {
        _api = api;
    }

    public Task<ApiTimingReport> ReportAsync(int hours, string sort, int take, CancellationToken ct) => _api.ReportAsync(hours, sort, take, ct);
}
