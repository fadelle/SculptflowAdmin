using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Common.Statics;
using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Business.Services.Monitoring;

public class MessageTracesAdminService : IMessageTracesAdminService
{
    private readonly IMessageTracesApiClient _api;

    public MessageTracesAdminService(IMessageTracesApiClient api)
    {
        _api = api;
    }

    public Task<MessageTracePage> ListAsync(int hours, Guid? clinicId, bool failedOnly, int page, CancellationToken ct) =>
        _api.ListAsync(hours, clinicId, null, failedOnly, page, MessageTraceOptions.PageSize, ct);

    public Task<MessageTrace?> GetAsync(string? traceId, Guid? messageId, CancellationToken ct) =>
        messageId is { } id ? _api.ForMessageAsync(id, ct)
        : string.IsNullOrWhiteSpace(traceId) ? Task.FromResult<MessageTrace?>(null)
        : _api.GetAsync(traceId.Trim(), ct);

    public async Task<string?> ResolveAsync(string query, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(query) ? null : (await _api.ResolveAsync(query.Trim(), ct))?.TraceId;

    public async Task<MessageTraceSummary?> TrySummaryAsync(CancellationToken ct)
    {
        try
        {
            return await _api.SummaryAsync(MessageTraceOptions.SummaryHours, ct);
        }
        catch (MainAppApiException)
        {
            return null;
        }
    }
}