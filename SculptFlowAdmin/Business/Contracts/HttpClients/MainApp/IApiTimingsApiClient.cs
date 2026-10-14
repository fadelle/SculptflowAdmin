using SculptFlowAdmin.Entities.Responses.Monitoring;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

public interface IApiTimingsApiClient
{
    Task<ApiTimingReport> ReportAsync(int hours, string sort, int take, CancellationToken ct);
}
