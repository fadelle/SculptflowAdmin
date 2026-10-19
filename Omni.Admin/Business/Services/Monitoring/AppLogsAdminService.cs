using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Common.Statics;
using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Business.Services.Monitoring;

public class AppLogsAdminService : IAppLogsAdminService
{
    private readonly IAppLogsApiClient _api;

    public AppLogsAdminService(IAppLogsApiClient api)
    {
        _api = api;
    }

    public Task<AppLogGroupReport> GroupsAsync(int hours, string level, Guid? clinicId, string? search, string sort, CancellationToken ct) =>
        _api.GroupsAsync(hours, level, clinicId, search, sort, AppLogOptions.GroupTake, ct);

    public Task<AppLogPage> ListAsync(int hours, string level, Guid? clinicId, string? search, string? fingerprint, string? traceId, int page,
        CancellationToken ct) =>
        _api.ListAsync(hours, level, clinicId, search, fingerprint, traceId, page, AppLogOptions.PageSize, ct);

    public Task<AppLogDetail?> GetAsync(long id, CancellationToken ct) => _api.GetAsync(id, ct);

    public async Task<AppLogSummary?> TrySummaryAsync(CancellationToken ct)
    {
        try
        {
            return await _api.SummaryAsync(AppLogOptions.SummaryHours, ct);
        }
        catch (MainAppApiException)
        {
            return null;
        }
    }
}