using SculptFlowAdmin.Entities.Responses.Monitoring;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

public interface IMessageTracesApiClient
{
    Task<MessageTracePage> ListAsync(int hours, Guid? clinicId, Guid? conversationId, bool failedOnly, int page, int pageSize, CancellationToken ct);

    /// <summary>Null when nothing is recorded for the trace.</summary>
    Task<MessageTrace?> GetAsync(string traceId, CancellationToken ct);

    /// <summary>Null when the message has no trace.</summary>
    Task<MessageTrace?> ForMessageAsync(Guid messageId, CancellationToken ct);

    /// <summary>Null when nothing matches.</summary>
    Task<TraceReference?> ResolveAsync(string query, CancellationToken ct);

    Task<MessageTraceSummary> SummaryAsync(int hours, CancellationToken ct);
}