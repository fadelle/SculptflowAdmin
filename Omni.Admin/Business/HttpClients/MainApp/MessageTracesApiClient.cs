using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin message trace (/api/platform-admin/message-traces). Read-only.</summary>
public class MessageTracesApiClient : MainAppApiClient, IMessageTracesApiClient
{
    public const string Prefix = "api/platform-admin/message-traces";

    public MessageTracesApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<MessageTracesApiClient> logger)
        : base(http, options, context, logger, Prefix, "Message trace")
    {
    }

    public Task<MessageTracePage> ListAsync(int hours, Guid? clinicId, Guid? conversationId, bool failedOnly, int page, int pageSize, CancellationToken ct) =>
        GetRequiredAsync<MessageTracePage>(Query(("hours", hours), ("clinicId", clinicId), ("conversationId", conversationId),
            ("failedOnly", failedOnly), ("page", page), ("pageSize", pageSize)), ct);

    public Task<MessageTrace?> GetAsync(string traceId, CancellationToken ct) => GetAsync<MessageTrace>("/" + Esc(traceId), ct);

    public Task<MessageTrace?> ForMessageAsync(Guid messageId, CancellationToken ct) => GetAsync<MessageTrace>("/by-message/" + messageId, ct);

    public Task<TraceReference?> ResolveAsync(string query, CancellationToken ct) => GetAsync<TraceReference>("/resolve" + Query(("q", query)), ct);

    public Task<MessageTraceSummary> SummaryAsync(int hours, CancellationToken ct) =>
        GetRequiredAsync<MessageTraceSummary>("/summary" + Query(("hours", hours)), ct);
}