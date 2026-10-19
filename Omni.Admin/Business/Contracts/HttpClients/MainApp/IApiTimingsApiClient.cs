using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IApiTimingsApiClient
{
    Task<ApiTimingReport> ReportAsync(int hours, string sort, int take, CancellationToken ct);
}
