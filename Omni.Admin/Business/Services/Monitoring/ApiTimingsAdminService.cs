using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Business.Services.Monitoring;

public class ApiTimingsAdminService : IApiTimingsAdminService
{
    private readonly IApiTimingsApiClient _api;

    public ApiTimingsAdminService(IApiTimingsApiClient api)
    {
        _api = api;
    }

    public Task<ApiTimingReport> ReportAsync(int hours, string sort, int take, CancellationToken ct) => _api.ReportAsync(hours, sort, take, ct);
}
