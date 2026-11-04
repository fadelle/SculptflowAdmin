using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Business.Contracts.Services.Monitoring;

/// <summary>The main app's message traces, through its platform-admin API. Read-only, so nothing is audited.</summary>
public interface IMessageTracesAdminService
{
    Task<MessageTracePage> ListAsync(int hours, Guid? clientId, bool failedOnly, int page, CancellationToken ct);

    /// <summary>A trace by its id, or the trace a message was created in; null when nothing is recorded.</summary>
    Task<MessageTrace?> GetAsync(string? traceId, Guid? messageId, CancellationToken ct);

    /// <summary>The trace id for a trace id, message id or provider message id; null when nothing matches.</summary>
    Task<string?> ResolveAsync(string query, CancellationToken ct);

    /// <summary>Conversations whose last message is the customer's and got no answer within <paramref name="minutes"/> (QA t7-10).</summary>
    Task<UnansweredPage> UnansweredAsync(int hours, int minutes, Guid? clientId, int page, CancellationToken ct);

    /// <summary>AI bot use per client over the last <paramref name="days"/>: runs, tokens, estimated cost, AI reply revenue (QA t7-10).</summary>
    Task<AiUsageReport> AiUsageAsync(int days, Guid? clientId, CancellationToken ct);

    /// <summary>The dashboard tile's counts, or null when the main app can't answer (the dashboard must still load).</summary>
    Task<MessageTraceSummary?> TrySummaryAsync(CancellationToken ct);
}